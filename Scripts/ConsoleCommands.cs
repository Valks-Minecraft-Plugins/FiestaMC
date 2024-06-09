namespace FiestaMC;

public partial class ConsoleCommands : LineEdit
{
    public override void _Ready()
    {
        TextSubmitted += text =>
        {
            Clear();

            string cmd = text.ToLower().Split(" ")[0].Trim();

            switch (cmd)
            {
                case "clear":
                    Console.ClearText();
                    break;
                default:
                    Console.Log($"'{cmd}' is not a valid command");
                    break;
            }
        };
    }
}
