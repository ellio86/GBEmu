namespace GBEmulator.Tests.Abstractions;
using Core.Options;
using Hardware.Components;
using Hardware.Components.Cpu;
using Timer = GBEmulator.Hardware.Components.Timer;

public class AbstractionHelper
{
    public static Bus CreateBus()
    {
        var appSettings = new AppSettings();
        var lcd = new TestLcd();
        var registers = new Registers();
        var cpu = new Cpu(registers);
        var timer = new Timer();
        var ppu = new Ppu(appSettings, lcd);
        var window = new TestImageControl();
        var controller = new Controller();
        
        return new Bus(cpu, timer, ppu, window, controller, appSettings);
    }
}