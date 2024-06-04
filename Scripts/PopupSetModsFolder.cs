using Godot;
using System;

namespace FiestaMC;

public partial class PopupSetModsFolder : PopupPanel
{
    void OnButtonPressed()
    {
        Hide();
        GetNode<FileDialog>("%FileDialog").Popup();
    }
}
