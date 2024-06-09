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

    public ModInfo ModInfo { get; private set; }

    public override void _Ready()
    {
        ModInfo = new(this);

        Config.Load();
        GetNode<FileDialog>("%FileDialog").CurrentDir = Config.ModsFolderPath;

        ModInfo.ObtainAllModInformation();
        ModInfo.StartFileWatcher();

        Node buttons = GetNode("%Buttons");
        
        buttons.GetNode<FiestaButton>("BtnRemove").Setup(OnBtnRemovePressed);
        buttons.GetNode<FiestaButton>("BtnNotCulprit").Setup(OnBtnNotCulpritPressed);
        buttons.GetNode<FiestaButton>("BtnRestore").Setup(OnBtnRestorePressed);
        buttons.GetNode<Button>("BtnSetModsFolder").Pressed += OnBtnSetModsFolderPressed;
    }

    void OnBtnRemovePressed() // Remove Half of Mods Button
    {
        ModInfo.StopFileWatcher();

        if (ModInfo.ModsFolderWasModified)
        {
            ModInfo.ObtainAllModInformation();
        }

        // Move half of mods to temp
        MoveHalfOfModsToTemp(out int modsMoved);

        // Check mod dependencies

        // This needs to be a for loop and not a foreach because the collection gets modified later
        for (int i = 0; i < ModInfo.FolderMods.Count; i++)
        {
            KeyValuePair<string, JsonModInfo> mod = ModInfo.FolderMods.ElementAt(i);

            ModInfo.GetDependenciesForMod(mod.Key, mod.Value);
        }

        Console.Log($"[color=77ff77][color=00ff03]{modsMoved}[/color] mods were moved to \"temp\" and [color=00ff03]{ModInfo.DependenciesFound}[/color] dependencies were moved back to \"mods\"[/color]");
        ModInfo.DependenciesFound = 0;

        ModInfo.StartFileWatcher();
    }

    void OnBtnNotCulpritPressed()
    {
        Console.Log("The 'Mark Removed Mods as Not Culprit' button has yet to be implemented");
    }

    void OnBtnRestorePressed()
    {
        Console.Log("The 'Restore All Mods' button has yet to be implemented");
    }

    void OnBtnSetModsFolderPressed()
    {
        GetNode<FileDialog>("%FileDialog").Popup();
    }

    void MoveHalfOfModsToTemp(out int modsMoved)
    {
        var half_of_mods = ModInfo.FolderMods.Take(ModInfo.FolderMods.Count() / 2);
        
        foreach (KeyValuePair<string, JsonModInfo> mod in half_of_mods)
        {
            string fileName = ModInfo.FolderMods[mod.Key].FileName;

            ModInfo.FolderTemp.Add(mod.Key, mod.Value);
            ModInfo.FolderMods.Remove(mod.Key);

            MoveMod(fileName, Config.ModsFolderPath, $@"{Config.ModsFolderPath}\temp");
        }

        modsMoved = half_of_mods.Count();
    }

    public void MoveMod(string fileName, string from, string to)
    {
        File.Move($@"{from}\{fileName}", $@"{to}\{fileName}");
    }
}
