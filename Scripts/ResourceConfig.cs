using Godot;
using System;

namespace FiestaMC;

[GlobalClass]
public partial class ResourceConfig : Resource
{
    [Export] public string ModsFolderPath { get; set; }

    public bool IsModsFolderPathSet()
    {
        return !string.IsNullOrWhiteSpace(ModsFolderPath);
    }

    public void Save()
    {
        Error error = ResourceSaver.Save(resource: this, path: "user://config.tres");

        if (error != Error.Ok)
            GD.Print(error);
    }

    public void Load()
    {
        bool fileExists = FileAccess.FileExists("user://config.tres");

        if (fileExists)
        {
            ResourceConfig config = GD.Load<ResourceConfig>("user://config.tres");
            ModsFolderPath = config.ModsFolderPath;
        }
    }
}
