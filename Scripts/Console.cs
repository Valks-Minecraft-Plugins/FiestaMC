namespace FiestaMC;

public partial class Console : RichTextLabel
{
    static Console instance;

    public override void _Ready()
    {
        instance = this;
    }

    public static void Log(object obj)
    {
        instance.Text += obj + "\n";
    }

    public static void LogWarning(object obj)
    {
        Log($"[color=f9ff84]{obj}[/color]");
    }

    public static void ClearText() => instance.Text = "";
}
