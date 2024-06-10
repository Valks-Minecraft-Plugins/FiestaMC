namespace FiestaMC;

public partial class ConsoleCommands : LineEdit
{
    [Export] ResourceConfig config;

    public override void _Ready()
    {
        GrabFocus();

        TextSubmitted += text =>
        {
            Clear();

            string cmd = text.ToLower().Split(" ")[0].Trim();

            if (cmd == "clear")
            {
                Console.ClearText();
            }
            else if (cmd == "export")
            {
                IEnumerable<string> mod_list = config.ModInfo.FolderMods.Select(x => x.Value.Name)
                    .Where(x => !x.ToLower().Contains("api"))
                    .Where(x => !x.ToLower().Contains("config"))
                    .Where(x => !x.ToLower().Contains("lib"));

                string mod_list_str = mod_list.Print();

                DisplayServer.ClipboardSet(mod_list_str);

                Console.Log(mod_list_str);

                string feedback = 
                    $"{mod_list.Count()} mods (excluding API mods) have been copied " +
                    $"to the clipboard ({mod_list_str.Length} characters)";

                Console.Log(feedback, "77ff77", 20);
            }
            else
            {
                Console.Log($"'{cmd}' is not a valid command");
            }
        };
    }
}
