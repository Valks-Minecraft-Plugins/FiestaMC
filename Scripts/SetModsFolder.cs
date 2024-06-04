using Godot;
using System;

namespace FiestaMC;

public partial class SetModsFolder : FileDialog
{
    [Export] public ResourceConfig Config { get; set; }

    public override void _Ready()
    {
        CurrentDir = Config.ModsFolderPath;

        DirSelected += (dir) =>
        {
            Config.ModsFolderPath = dir;
            Config.Save();
        };
    }
}
