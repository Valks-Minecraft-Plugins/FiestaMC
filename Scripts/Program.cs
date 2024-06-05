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

// Current To Do List
// - The dependencies should also be checked for the mod dependency that was moved from temp to mods.
// In other words, need to check to see if the dependency has any of its own dependencies.
public partial class Program : Node
{
    [Export] public ResourceConfig Config { get; set; }

    PopupPanel popupSetModsFolder;

    Dictionary<string, JsonModInfo> allModInfo;

    FileSystemWatcher fileSystemWatcher;

    bool modsFolderWasModified;

    public override void _Ready()
    {
        Config.Load();
        GetNode<FileDialog>("%FileDialog").CurrentDir = Config.ModsFolderPath;
        popupSetModsFolder = GetNode<PopupPanel>("%PopupSetModsFolder");

        ObtainAllModInformation();
        StartFileWatcher();
    }

    void OnRenamed(object sender, RenamedEventArgs e) => modsFolderWasModified = true;
    void OnCreated(object sender, FileSystemEventArgs e) => modsFolderWasModified = true;
    void OnDeleted(object sender, FileSystemEventArgs e) => modsFolderWasModified = true;

    void StopFileWatcher()
    {
        fileSystemWatcher.Dispose();
    }

    void StartFileWatcher()
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

    void ObtainAllModInformation()
    {
        modsFolderWasModified = false;
        allModInfo = GetAllModInfo(Config.ModsFolderPath);
        allModInfo.Merge(GetAllModInfo(Config.ModsFolderPath + "/temp"));
    }

    void OnBtnRemovePressed() // Remove Half of Mods Button
    {
        if (!Config.IsModsFolderPathSet())
        {
            popupSetModsFolder.Popup();
            return;
        }

        if (modsFolderWasModified)
        {
            ObtainAllModInformation();
        }

        StopFileWatcher();

        // Move half of mods to temp
        MoveHalfOfModsToTemp();

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
            var mod = modsMainFolderInfo.ElementAt(i);

            Dictionary<string, object> dependencies = mod.Value.Depends;

            foreach (KeyValuePair<string, object> dependency in dependencies)
            {
                if (!modsMainFolderInfo.ContainsKey(dependency.Key))
                {
                    if (modsTempFolderInfo.ContainsKey(dependency.Key))
                    {
                        MoveMod(dependency.Key, $@"{Config.ModsFolderPath}\temp", Config.ModsFolderPath);
                    }
                    else
                    {
                        // Some dependency names listed in "depends" are not the same as the mod ids
                        // So lets do the brute force approach. Keep removing characters from the name
                        // until the mod ID is found.
                        string key = dependency.Key;
                        bool foundMod = false;

                        while (key.Length > 2)
                        {
                            if (modsTempFolderInfo.ContainsKey(key))
                            {
                                foundMod = true;

                                // Need to update these dicts. Their removed at the end of this function. It's a bit
                                // messy but it works.
                                modsMainFolderInfo.Add(key, modsTempFolderInfo[key]);
                                modsTempFolderInfo.Remove(key);

                                MoveMod(key, $@"{Config.ModsFolderPath}\temp", Config.ModsFolderPath);
                                break;
                            }
                            else if (modsMainFolderInfo.ContainsKey(key))
                            {
                                // The mod exists in the main mods folder already
                                foundMod = true;
                                break;
                            }

                            // Remove one character from the end of the string
                            key = key.Substring(0, key.Length - 1);
                        }

                        if (!foundMod)
                            GD.Print($"Could not find dependency '{dependency.Key}' in temp folder for mod '{mod.Key}'");
                    }
                }
            }
        }

        StartFileWatcher();
    }

    void OnBtnNotCulpritPressed()
    {
        if (!Config.IsModsFolderPathSet())
        {
            popupSetModsFolder.Popup();
            return;
        }
    }

    void OnBtnRestorePressed()
    {
        if (!Config.IsModsFolderPathSet())
        {
            popupSetModsFolder.Popup();
            return;
        }
    }

    void OnBtnSetModsFolderPressed()
    {
        GetNode<FileDialog>("%FileDialog").Popup();
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
            "java"
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
                GD.Print($"There is a duplicate mod named '{modInfo.Id}'. It will be skipped.");
            }
        }

        return modInfoDict;
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
            GD.Print($"The mod named {mod_name} has no 'fabric.mod.json' so it will be skipped.");
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
            GD.Print($"Failed to read fabric.mod.json for mod '{mod_name}': {e.Message}");
        }

        File.Delete($@"{Config.ModsFolderPath}/fabric.mod.json");

        return modInfo;
    }

    void MoveHalfOfModsToTemp()
    {
        var modsMainFolderInfo = allModInfo.Where(x => x.Value.FolderName == "mods");
        var half_of_mods = modsMainFolderInfo.Take(modsMainFolderInfo.Count() / 2);
        
        foreach (KeyValuePair<string, JsonModInfo> modInfo in half_of_mods)
        {
            MoveMod(modInfo.Key, Config.ModsFolderPath, $@"{Config.ModsFolderPath}\temp");
        }
    }

    void MoveMod(string modId, string from, string to)
    {
        string fileName = allModInfo[modId].FileName;
        string folderName = Path.GetFileName(to);

        allModInfo[modId].FolderName = folderName;
        File.Move($@"{from}\{fileName}", $@"{to}\{fileName}");
    }
}
