namespace FiestaMC;

public partial class SetModsFolder : FileDialog
{
    [Export] public ResourceConfig Config { get; set; }

    public override void _Ready()
    {
        DirSelected += (dir) =>
        {
            Config.ModsFolderPath = dir;
            Config.Save();

            ModInfo modInfo = GetTree().Root.GetNode<Program>("Program").ModInfo;
            modInfo.StopFileWatcher();
            modInfo.ObtainAllModInformation();
            modInfo.StartFileWatcher();
        };
    }
}
