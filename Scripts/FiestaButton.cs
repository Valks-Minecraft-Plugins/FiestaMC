namespace FiestaMC;

public partial class FiestaButton : Button
{
    [Export] public ResourceConfig Config { get; set; }

    public void Setup(Action action)
    {
        Pressed += () =>
        {
            if (!Config.IsModsFolderPathSet())
            {
                GetNode<PopupPanel>("%PopupSetModsFolder").Popup();
                return;
            }

            SetButtonsDisabled(true);

            action();

            SetButtonsDisabled(false);
        };
    }

    void SetButtonsDisabled(bool disabled)
    {
        foreach (Button btn in GetNode("%Buttons").GetChildren())
            btn.Disabled = disabled;
    }
}
