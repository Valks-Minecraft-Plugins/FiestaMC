namespace FiestaMC;

public partial class Console : RichTextLabel
{
    static Console instance;

    public override void _Ready()
    {
        instance = this;
    }

    public static void Log(object obj, string color = "dddddd", int size = 16)
    {
        instance.Text += $"[font_size={size}][color={color}]{obj}[/color][/font_size]\n";
        instance.ScrollToLine(instance.GetLineCount());
    }

    public static void LogWarning(object obj)
    {
        Log($"{obj}", "f9ff84", 17);
    }

    public static void ClearText() => instance.Text = "";
}
