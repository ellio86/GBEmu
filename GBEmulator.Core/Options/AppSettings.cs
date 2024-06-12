namespace GBEmulator.Core.Options;

public class AppSettings
{
    public int Scale { get; init; } = 1;
    public string? SaveDirectory { get; init; } = ".";
    public bool ForceConsole { get; init; } = false;
    public bool AudioEnabled { get; init; } = true;
    public uint AudioSampleRate { get; init; } = 44100;
    public uint AudioBufferSize { get; init; } = 3072;
    public bool LoggingEnabled { get; init; } = true;
    public float TargetFps { get; init; } = 60f;

}
