using System.IO;
using System.IO.Compression;

namespace FiestaMC;

public class ModInfo
{
    public Dictionary<string, JsonModInfo> FolderMods { get; private set; }
    public Dictionary<string, JsonModInfo> FolderTemp { get; private set; }
    public Dictionary<string, JsonModInfo> FolderNotCulprit { get; private set; }

    Program program;

    FileSystemWatcher fileSystemWatcher;

    public bool ModsFolderWasModified;

    public ModInfo(Program program)
    {
        this.program = program;
    }

    public void ObtainAllModInformation()
    {
        Console.Log("[i]The mods folder has changed, obtaining new mod information...[/i]");
        ModsFolderWasModified = false;
        FolderMods = GetAllModInfo(program.Config.ModsFolderPath);
        FolderTemp = GetAllModInfo(program.Config.ModsFolderPath + "/temp");
        FolderNotCulprit = GetAllModInfo(program.Config.ModsFolderPath + "/not culprit");
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
            "fabric-renderer-api-v1",
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

            if (modInfo.Id == "sodium")
            {
                modInfo.Depends.Add("indium", "*");
            }

            if (!modInfoDict.ContainsKey(modInfo.Id))
            {
                modInfoDict.Add(modInfo.Id, modInfo);
            }
            else
            {
                Console.LogWarning($"[color=f4ff00]{modInfo.Id}[/color] is a duplicate mod so it will be skipped.");
            }
        }

        return modInfoDict;
    }

    public int DependenciesFound;

    public void GetDependenciesForMod(string modId, JsonModInfo mod)
    {
        Dictionary<string, object> dependencies = mod.Depends;

        foreach (KeyValuePair<string, object> dependency in dependencies)
        {
            if (!FolderMods.ContainsKey(dependency.Key))
            {
                if (FolderTemp.ContainsKey(dependency.Key))
                {
                    DependenciesFound++;

                    string fileName = FolderTemp[dependency.Key].FileName;

                    program.MoveMod(fileName, $@"{program.Config.ModsFolderPath}\temp", program.Config.ModsFolderPath);
                    GetDependenciesForMod(dependency.Key, FolderTemp[dependency.Key]);

                    FolderMods.Add(dependency.Key, FolderTemp[dependency.Key]);
                    FolderTemp.Remove(dependency.Key);
                }
                else if (FolderNotCulprit.ContainsKey(dependency.Key))
                {
                    DependenciesFound++;

                    string fileName = FolderNotCulprit[dependency.Key].FileName;

                    program.MoveMod(fileName, $@"{program.Config.ModsFolderPath}\not culprit", program.Config.ModsFolderPath);
                    GetDependenciesForMod(dependency.Key, FolderNotCulprit[dependency.Key]);

                    FolderMods.Add(dependency.Key, FolderNotCulprit[dependency.Key]);
                    FolderNotCulprit.Remove(dependency.Key);
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
                        if (FolderTemp.ContainsKey(the_key))
                        {
                            foundMod = true;

                            DependenciesFound++;

                            string fileName = FolderTemp[the_key].FileName;

                            program.MoveMod(fileName, $@"{program.Config.ModsFolderPath}\temp", program.Config.ModsFolderPath);
                            GetDependenciesForMod(the_key, FolderTemp[the_key]);

                            FolderMods.Add(the_key, FolderTemp[the_key]);
                            FolderTemp.Remove(the_key);
                            break;
                        }
                        else if (FolderNotCulprit.ContainsKey(the_key))
                        {
                            foundMod = true;

                            DependenciesFound++;

                            string fileName = FolderNotCulprit[the_key].FileName;

                            program.MoveMod(fileName, $@"{program.Config.ModsFolderPath}\not culprit", program.Config.ModsFolderPath);
                            GetDependenciesForMod(the_key, FolderNotCulprit[the_key]);

                            FolderMods.Add(the_key, FolderNotCulprit[the_key]);
                            FolderNotCulprit.Remove(the_key);
                            break;
                        }
                        else if (FolderMods.ContainsKey(the_key))
                        {
                            // The mod exists in the main mods folder already
                            foundMod = true;
                            break;
                        }

                        // Remove one character from the end of the string
                        the_key = the_key.Substring(0, the_key.Length - 1);
                    }

                    if (!foundMod)
                        Console.LogWarning($"Dependency '{dependency.Key}' could not be found for mod '{modId}'");
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
                    entry.ExtractToFile($@"{program.Config.ModsFolderPath}/fabric.mod.json");
                    break;
                }
            }
        }

        string file_contents;

        try
        {
            file_contents = File.ReadAllText($@"{program.Config.ModsFolderPath}/fabric.mod.json");
        }
        catch (FileNotFoundException)
        {
            Console.LogWarning($"[color=f4ff00]{mod_name}[/color] has no fabric.mod.json so it will be skipped.");
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

            //modInfo.FolderName = Path.GetFileName(Path.GetDirectoryName(modFilePath));
            modInfo.FileName = Path.GetFileName(modFilePath);
        }
        catch (JsonException e)
        {
            Console.LogWarning($"Failed to read fabric.mod.json for {mod_name}: {e.Message}");
        }

        File.Delete($@"{program.Config.ModsFolderPath}/fabric.mod.json");

        return modInfo;
    }

    #region File Watcher
    void OnRenamed(object sender, RenamedEventArgs e)
    {
        ModsFolderWasModified = true;
        /*GD.Print($"Renamed:");
        GD.Print($"    Old: {e.OldFullPath}");
        GD.Print($"    New: {e.FullPath}");*/
    }

    void OnCreated(object sender, FileSystemEventArgs e)
    {
        ModsFolderWasModified = true;
        //GD.Print($"Created: {e.FullPath}");
    }

    void OnDeleted(object sender, FileSystemEventArgs e)
    {
        ModsFolderWasModified = true;
        //GD.Print($"Deleted: {e.FullPath}");
    }

    public void StopFileWatcher()
    {
        fileSystemWatcher.Dispose();
    }

    public void StartFileWatcher()
    {
        fileSystemWatcher = new FileSystemWatcher(program.Config.ModsFolderPath)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        fileSystemWatcher.Created += OnCreated;
        fileSystemWatcher.Deleted += OnDeleted;
        fileSystemWatcher.Renamed += OnRenamed;
    }
    #endregion
}
