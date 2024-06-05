global using Godot;
global using GodotUtils;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.Json;

using System.IO;

namespace FiestaMC;

public partial class Program : Node
{
    [Export] public ResourceConfig Config { get; set; }

    PopupPanel popupSetModsFolder;

    public override void _Ready()
    {
        Config.Load();
        GetNode<FileDialog>("%FileDialog").CurrentDir = Config.ModsFolderPath;
        popupSetModsFolder = GetNode<PopupPanel>("%PopupSetModsFolder");
    }

    void OnBtnRemovePressed() // Remove Half of Mods Button
    {
        if (!Config.IsModsFolderPathSet())
        {
            popupSetModsFolder.Popup();
            return;
        }

        // Move half of mods to temp
        MoveHalfOfModsToTemp();

        // Obtain all mod information
        Dictionary<string, JsonModInfo> all_mods_info = GetAllModInfo(Config.ModsFolderPath);
        all_mods_info.Merge(GetAllModInfo(Config.ModsFolderPath + "/temp"));

        // Check mod dependencies
        Dictionary<string, JsonModInfo> modsMainFolderInfo = all_mods_info
            .Where(x => x.Value.FolderName == "mods")
            .ToDictionary(x => x.Key, x => x.Value);

        Dictionary<string, JsonModInfo> modsTempFolderInfo = all_mods_info
            .Where(x => x.Value.FolderName == "temp")
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (KeyValuePair<string, JsonModInfo> mod in modsMainFolderInfo)
        {
            Dictionary<string, string> dependencies = mod.Value.Depends;
            
            foreach (KeyValuePair<string, string> dependency in dependencies)
            {
                if (!modsMainFolderInfo.ContainsKey(dependency.Key))
                {
                    if (modsTempFolderInfo.ContainsKey(dependency.Key))
                    {
                        string modFileName = modsTempFolderInfo[dependency.Key].FileName;

                        File.Move(
                            $"{Config.ModsFolderPath}/temp/{modFileName}",
                            $"{Config.ModsFolderPath}/{modFileName}");
                    }
                    else
                    {
                        GD.Print($"Could not find dependency '{dependency.Key}' in temp folder for mod '{mod.Key}'");
                    }
                }
            }
        }
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
        return Directory.GetFiles(directory).Where(x => x.Contains(".jar"));
    }

    Dictionary<string, JsonModInfo> GetAllModInfo(string folderPath)
    {
        string[] ignored_dependencies = new string[]
        {
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

            foreach (string ignored_dependency in ignored_dependencies)
            {
                modInfo.Depends.Remove(ignored_dependency);
            }

            modInfoDict.Add(modInfo.Id, modInfo);
        }

        return modInfoDict;
    }

    JsonModInfo GetModInfo(string modFilePath)
    {
        string mod_name = Path.GetFileName(modFilePath).Replace(".jar", "");
        string extract_to_path = $@"{Config.ModsFolderPath}/delete_me/{mod_name}";

        System.IO.Compression.ZipFile.ExtractToDirectory(
            sourceArchiveFileName: modFilePath,
            destinationDirectoryName: extract_to_path);

        string file_contents = File.ReadAllText($"{extract_to_path}/fabric.mod.json");

        JsonModInfo modInfo = JsonSerializer.Deserialize<JsonModInfo>(file_contents, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        modInfo.FolderName = Path.GetFileName(Path.GetDirectoryName(modFilePath));
        modInfo.FileName = Path.GetFileName(modFilePath);

        Directory.Delete($@"{Config.ModsFolderPath}/delete_me", recursive: true);

        return modInfo;
    }

    void MoveHalfOfModsToTemp()
    {
        IEnumerable<string> full_mod_list = GetModFiles(Config.ModsFolderPath);
        IEnumerable<string> half_mod_list = full_mod_list.Take(full_mod_list.Count() / 2);

        foreach (string modFilePath in half_mod_list)
        {
            File.Move(modFilePath, $@"{Config.ModsFolderPath}/temp/{Path.GetFileName(modFilePath)}");
        }
    }
}
