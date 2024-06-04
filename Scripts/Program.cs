using Godot;
using System;

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

    void OnBtnRemovePressed()
    {
        if (!Config.IsModsFolderPathSet())
        {
            popupSetModsFolder.Popup();
            return;
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
}
