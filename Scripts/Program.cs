global using Godot;
global using GodotUtils;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.Json;
using System.IO;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace FiestaMC;

public partial class Program : Node
{
    [Export] public ResourceConfig Config { get; set; }

    Dictionary<string, JsonModInfo> allModInfo;

    FileSystemWatcher fileSystemWatcher;

    bool modsFolderWasModified;

    RichTextLabel console;

    public override void _Ready()
    {
        Config.Load();
        GetNode<FileDialog>("%FileDialog").CurrentDir = Config.ModsFolderPath;

        console = GetNode<RichTextLabel>("%Console");

        ObtainAllModInformation();
        StartFileWatcher();

        Node buttons = GetNode("%Buttons");
        
        buttons.GetNode<FiestaButton>("BtnRemove").Setup(OnBtnRemovePressed);
        buttons.GetNode<FiestaButton>("BtnNotCulprit").Setup(OnBtnNotCulpritPressed);
        buttons.GetNode<FiestaButton>("BtnRestore").Setup(OnBtnRestorePressed);
        buttons.GetNode<Button>("BtnSetModsFolder").Pressed += OnBtnSetModsFolderPressed;
    }

    void OnBtnRemovePressed() // Remove Half of Mods Button
    {
        StopFileWatcher();

        if (modsFolderWasModified)
        {
            ObtainAllModInformation();
        }

        // Move half of mods to temp
        MoveHalfOfModsToTemp(out int modsMoved);

        // Check mod dependencies
        Dictionary<string, JsonModInfo> modsMainFolderInfo = new Dictionary<string, JsonModInfo>(allModInfo)
            .Where(x => x.Value.FolderName == "mods")
            .ToDictionary(x => x.Key, x => x.Value);

        Dictionary<string, JsonModInfo> modsTempFolderInfo = new Dictionary<string, JsonModInfo>(allModInfo)
            .Where(x => x.Value.FolderName == "temp")
            .ToDictionary(x => x.Key, x => x.Value);

        // This needs to be a for loop and not a foreach because the collection gets modified later
        for (int i = 0; i < modsMainFolderInfo.Count; i++)
        {
            KeyValuePair<string, JsonModInfo> mod = modsMainFolderInfo.ElementAt(i);

            GetDependenciesForMod(modsMainFolderInfo, modsTempFolderInfo, mod.Key, mod.Value);
        }

        Log($"[color=77ff77][color=00ff03]{modsMoved}[/color] mods were moved to \"temp\" and [color=00ff03]{dependenciesFound}[/color] dependencies were moved back to \"mods\"[/color]");
        dependenciesFound = 0;

        StartFileWatcher();
    }

    void OnBtnNotCulpritPressed()
    {
        Log("The 'Mark Removed Mods as Not Culprit' button has yet to be implemented");
    }

    void OnBtnRestorePressed()
    {
        Log("The 'Restore All Mods' button has yet to be implemented");
    }

    void OnBtnSetModsFolderPressed()
    {
        GetNode<FileDialog>("%FileDialog").Popup();
    }
    
    #region File Watcher
    void OnRenamed(object sender, RenamedEventArgs e)
    {
        modsFolderWasModified = true;
        /*GD.Print($"Renamed:");
        GD.Print($"    Old: {e.OldFullPath}");
        GD.Print($"    New: {e.FullPath}");*/
    }

    void OnCreated(object sender, FileSystemEventArgs e)
    {
        modsFolderWasModified = true;
        //GD.Print($"Created: {e.FullPath}");
    }

    void OnDeleted(object sender, FileSystemEventArgs e)
    {
        modsFolderWasModified = true;
        //GD.Print($"Deleted: {e.FullPath}");
    }

    public void StopFileWatcher()
    {
        fileSystemWatcher.Dispose();
    }

    public void StartFileWatcher()
    {
        fileSystemWatcher = new FileSystemWatcher(Config.ModsFolderPath)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        fileSystemWatcher.Created += OnCreated;
        fileSystemWatcher.Deleted += OnDeleted;
        fileSystemWatcher.Renamed += OnRenamed;
    }
    #endregion

    #region Helper Functions
    public void ObtainAllModInformation()
    {
        Log("[i]The mods folder has changed, obtaining new mod information...[/i]");
        modsFolderWasModified = false;
        allModInfo = GetAllModInfo(Config.ModsFolderPath);
        allModInfo.Merge(GetAllModInfo(Config.ModsFolderPath + "/temp"));
    }

    IEnumerable<string> GetModFiles(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return Directory.GetFiles(directory).Where(x => x.Contains(".jar"));
    }

    Dictionary<string, JsonModInfo> GetAllModInfo(string folderPath)
    {
        string[] ignored_dependencies = new string[]
        {
            "fabric-resource-loader-v0",
            "fabricloader",
            "fabric",
            "minecraft",
            "java",
            "terraform-wood-api-v1",
            "fabric-screen-api-v1",
            "fabric-key-binding-api-v1",
            "fabric-lifecycle-events-v1",
            "fabric-networking-api-v1",
            "fabric-screen-api-v1",
            "fabric-rendering-v1",
            "fabric-lifecycle-events-v1",
            "fabric-networking-api-v1",
            "fabric-events-interaction-v0",
            "optiglue",
            "apoli",
            "calio",
            "playerabilitylib",
            "team_reborn_energy",
            "fabric-biome-api-v1",
            "fabric-key-binding-api-v1",
            "fabric-rendering-data-attachment-v1",
            "fabric-rendering-fluids-v1",
            "fabric-command-api-v2",
            "fabric-lifecycle-events-v1",
            "team_reborn_energy",
            "fabric-biome-api-v1",
            "porting_lib_accessors",
            "porting_lib_base",
            "porting_lib_entity",
            "porting_lib_extensions",
            "porting_lib_networking",
            "porting_lib_obj_loader",
            "porting_lib_tags",
            "porting_lib_transfer",
            "porting_lib_models",
            "porting_lib_client_events",
            "milk",
            "reach-entity-attributes",
        };
        
        IEnumerable<string> mods = GetModFiles(folderPath);

        Dictionary<string, JsonModInfo> modInfoDict = new();

        foreach (string mod in mods)
        {
            JsonModInfo modInfo = GetModInfo(mod);

            if (modInfo == null)
                continue;

            foreach (string ignored_dependency in ignored_dependencies)
            {
                modInfo.Depends.Remove(ignored_dependency);
            }

            if (!modInfoDict.ContainsKey(modInfo.Id))
            {
                modInfoDict.Add(modInfo.Id, modInfo);
            }
            else
            {
                LogWarning($"[color=f4ff00]{modInfo.Id}[/color] is a duplicate mod so it will be skipped.");
            }
        }

        return modInfoDict;
    }

    public void Clear() => console.Text = "";

    public void Log(object obj)
    {
        console.Text += obj + "\n";
    }

    void LogWarning(object obj)
    {
        Log($"[color=f9ff84]{obj}[/color]");
    }

    int dependenciesFound;

    void GetDependenciesForMod(
        Dictionary<string, JsonModInfo> modsMainFolderInfo,
        Dictionary<string, JsonModInfo> modsTempFolderInfo,
        string modId,
        JsonModInfo mod)
    {
        Dictionary<string, object> dependencies = mod.Depends;

        foreach (KeyValuePair<string, object> dependency in dependencies)
        {
            if (!modsMainFolderInfo.ContainsKey(dependency.Key))
            {
                if (modsTempFolderInfo.ContainsKey(dependency.Key))
                {
                    dependenciesFound++;
                    MoveMod(dependency.Key, $@"{Config.ModsFolderPath}\temp", Config.ModsFolderPath);
                    GetDependenciesForMod(modsMainFolderInfo, modsTempFolderInfo, dependency.Key, modsTempFolderInfo[dependency.Key]);

                    // Need to update these dicts. Their removed at the end of this function. It's a bit
                    // messy but it works.
                    modsMainFolderInfo.Add(dependency.Key, modsTempFolderInfo[dependency.Key]);
                    modsTempFolderInfo.Remove(dependency.Key);
                }
                else
                {
                    // Some dependency names listed in "depends" are not the same as the mod ids
                    // So lets do the brute force approach. Keep removing characters from the name
                    // until the mod ID is found.
                    string the_key = dependency.Key;
                    bool foundMod = false;

                    while (the_key.Length > 2)
                    {
                        if (modsTempFolderInfo.ContainsKey(the_key))
                        {
                            foundMod = true;

                            dependenciesFound++;
                            MoveMod(the_key, $@"{Config.ModsFolderPath}\temp", Config.ModsFolderPath);
                            GetDependenciesForMod(modsMainFolderInfo, modsTempFolderInfo, the_key, modsTempFolderInfo[the_key]);

                            // Need to update these dicts. Their removed at the end of this function. It's a bit
                            // messy but it works.
                            modsMainFolderInfo.Add(the_key, modsTempFolderInfo[the_key]);
                            modsTempFolderInfo.Remove(the_key);
                            break;
                        }
                        else if (modsMainFolderInfo.ContainsKey(the_key))
                        {
                            // The mod exists in the main mods folder already
                            foundMod = true;
                            break;
                        }

                        // Remove one character from the end of the string
                        the_key = the_key.Substring(0, the_key.Length - 1);
                    }

                    if (!foundMod)
                        LogWarning($"Dependency '{dependency.Key}' could not be found in temp folder for mod '{modId}'");
                }
            }
        }
    }

    JsonModInfo GetModInfo(string modFilePath)
    {
        string mod_name = Path.GetFileName(modFilePath).Replace(".jar", "");

        using (ZipArchive zip = ZipFile.Open(modFilePath, ZipArchiveMode.Read))
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                if (entry.Name == "fabric.mod.json")
                {
                    entry.ExtractToFile($@"{Config.ModsFolderPath}/fabric.mod.json");
                    break;
                }
            }
        }

        string file_contents;

        try
        {
            file_contents = File.ReadAllText($@"{Config.ModsFolderPath}/fabric.mod.json");
        } catch (FileNotFoundException)
        {
            LogWarning($"[color=f4ff00]{mod_name}[/color] has no fabric.mod.json so it will be skipped.");
            return null;
        }
        
        // I am really starting to hate json
        file_contents = file_contents.Replace("\n", "").Replace("\r", "");

        JsonModInfo modInfo = null;

        try
        {
            modInfo = JsonSerializer.Deserialize<JsonModInfo>(file_contents, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            modInfo.FolderName = Path.GetFileName(Path.GetDirectoryName(modFilePath));
            modInfo.FileName = Path.GetFileName(modFilePath);
        }
        catch (JsonException e)
        {
            LogWarning($"Failed to read fabric.mod.json for {mod_name}: {e.Message}");
        }

        File.Delete($@"{Config.ModsFolderPath}/fabric.mod.json");

        return modInfo;
    }

    void MoveHalfOfModsToTemp(out int modsMoved)
    {
        var modsMainFolderInfo = allModInfo.Where(x => x.Value.FolderName == "mods");
        var half_of_mods = modsMainFolderInfo.Take(modsMainFolderInfo.Count() / 2);
        
        foreach (KeyValuePair<string, JsonModInfo> modInfo in half_of_mods)
        {
            MoveMod(modInfo.Key, Config.ModsFolderPath, $@"{Config.ModsFolderPath}\temp");
        }

        modsMoved = half_of_mods.Count();
    }

    void MoveMod(string modId, string from, string to)
    {
        string fileName = allModInfo[modId].FileName;
        string folderName = Path.GetFileName(to);

        allModInfo[modId].FolderName = folderName;
        File.Move($@"{from}\{fileName}", $@"{to}\{fileName}");
    }
    #endregion
}
