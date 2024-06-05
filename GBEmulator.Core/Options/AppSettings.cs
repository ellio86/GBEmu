namespace GBEmulator.Core.Options;

public class AppSettings
{
    public int Scale { get; set; } = 1;
    public string? SaveDirectory { get; set; } = ".";
    public bool ForceConsole { get; set; }
    public bool AudioEnabled { get; set; }
}
