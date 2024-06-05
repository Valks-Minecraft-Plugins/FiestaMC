namespace FiestaMC;

public partial class ConsoleCommands : LineEdit
{
    public override void _Ready()
    {
        Program program = GetTree().Root.GetNode<Program>("Program");

        TextSubmitted += text =>
        {
            Clear();

            string cmd = text.ToLower().Split(" ")[0].Trim();

            switch (cmd)
            {
                case "clear":
                    program.Clear();
                    break;
                default:
                    program.Log($"'{cmd}' is not a valid command");
                    break;
            }
        };
    }
}
