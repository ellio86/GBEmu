namespace GBEmulator.App;

using System;
using Hardware.Components;
using System.Diagnostics;
using Core.Interfaces;
using ITimer = GBEmulator.Core.Interfaces.ITimer;

public class GameBoy
{
    // Application window
    private Form _window;

    // Hardware
    private readonly ICpu _cpu;
    private readonly ITimer _timer;
    private readonly IPpu _ppu;
    private Bus _bus;
    public IController Controller;
    
    private bool _poweredOn = false;
    private const int CyclesPerFrame = 70224;

    public GameBoy(IPpu ppu, ICpu cpu, ITimer timer, IController controller)
    {
        _ppu = ppu ?? throw new ArgumentNullException(nameof(ppu));
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    public void Initialise(Form window)
    {
        _window = window;
        var windowObj = new Window(_window);

        //var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\01-special.gb");
        //var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\tetris.gb");
        //var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\pkmnblue.gb");
        var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\zelda.gb");


        _bus = new Bus(_cpu, _timer, _ppu, windowObj, Controller);
        _bus.LoadCartridge(cartridge);
        
        _poweredOn = true;

        Task.Factory.StartNew(StartClock, TaskCreationOptions.LongRunning);
    }

    private void StartClock()
    {
        var stopwatch = Stopwatch.StartNew();
        using var logWriter = new StreamWriter(@"..\..\..\log.txt");
        var totalCycles = 0;
        var fps = 0;

        var limiterEnabled = true;
        var limiter = false;
        
        var frameTimer = Stopwatch.StartNew();

        while (_poweredOn)
        {
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                SetWindowText($"KempoGB | FPS: {fps}");
                stopwatch.Restart();
                fps = 0;
            }
            
            while (totalCycles < CyclesPerFrame && !limiter)
            {
                // Tick the clock
                var cycleNum = _bus.ClockCpu(logWriter);

                totalCycles += cycleNum * 4;
                
                // Update Timer, PPU and joypad
                _bus.ClockPpu(cycleNum * 2);
                _bus.ClockTimer(cycleNum * 4);
                
                Controller.Update();
                
                _bus.HandleInterrupts();

                // Listen to serial io port for test results
                if (_bus.ReadMemory(0xff02) == 0x81)
                {
                    var c = (char)_bus.ReadMemory(0xff01);
                    Console.Write(c);
                    _bus.WriteMemory(0xff02, 0x00);
                }
            }

            if (limiterEnabled)
            {
                // Limit FPS
                if (frameTimer.ElapsedMilliseconds < 1000 / (60 * 2))
                {
                    limiter = true;
                }
                else
                {
                    totalCycles -= CyclesPerFrame;
                    fps++;
                    limiter = false;
                    frameTimer.Restart();
                }
            }
            else
            {
                totalCycles -= CyclesPerFrame;
                fps++;
            }
        }
    }

    /// <summary>
    /// Thread safe function for writing to the window's text
    /// </summary>
    /// <param name="text"></param>
    private void SetWindowText(string text)
    {
        try
        {
            if (_window.InvokeRequired)
            {
                void WriteText() => SetWindowText(text);
                _window.Invoke(WriteText);
            }
            else
            {
                _window.Text = text;
            }
        }
        catch
        {
            // ignored
        }
    }
}
