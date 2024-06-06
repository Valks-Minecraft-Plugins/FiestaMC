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

            Program program = GetTree().Root.GetNode<Program>("Program");
            program.StopFileWatcher();
            program.ObtainAllModInformation();
            program.StartFileWatcher();
        };
    }
}
