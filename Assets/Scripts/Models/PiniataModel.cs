public class PiniataModel
{
    public int ClicksRequired { get; set; } = 1;
    public int ClickCount { get; set; }
    public int CurrentPiniataNum { get; set; }
    public bool IsOnCooldown { get; set; }
    public bool IsOpened { get; set; }
}