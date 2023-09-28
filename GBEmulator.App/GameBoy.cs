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

    // Hardware that gets connected to the BUS
    private readonly ICpu _cpu;
    private readonly ITimer _timer;
    private readonly IPpu _ppu;
    public readonly IController Controller;
    
    // BUS
    private Bus _bus;

    // Extra properties
    private bool _poweredOn = false;
    private const int CyclesPerFrame = 70224/2;

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

        var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\09-op r,r.gb");
        //var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\tetris.gb");
        //var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\pkmnblue.gb");
        
        // Create Cartridge
        //var cartridge = new Cartridge("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\zelda.gb");

        // Create new BUS
        _bus = new Bus(_cpu, _timer, _ppu, windowObj, Controller);
        
        // Load Cartridge
        _bus.LoadCartridge(cartridge);
        
        // Power on GameBoy
        _poweredOn = true;

        // Start task on new thread for main loop
        Task.Factory.StartNew(StartClock, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Sets up and runs the main loop
    /// </summary>
    private void StartClock()
    {
        // debug - pass to bus.ClockCpu(StreamWriter w) below to enable
        //using var logWriter = new StreamWriter(@"..\..\..\log.txt");
        
        // FPS counter + variables
        var totalCycles = 0;
        var fps = 0;
        var stopwatch = Stopwatch.StartNew();
        var frameTimer = Stopwatch.StartNew();

        // Limiter Settings (TODO: Move to app setting)
        var limiterEnabled = true;
        var limiter = false;
        
        while (_poweredOn)
        {
            // Update window text every second for live FPS counter
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                SetWindowText($"KempoGB | FPS: {fps}");
                stopwatch.Restart();
                fps = 0;
            }
            
            // Execute 1 frame's worth of instructions
            while (totalCycles < CyclesPerFrame && !limiter)
            {
                // Execute CPU instruction and get how many cycles it took
                var cycleNum = _bus.ClockCpu();

                totalCycles += cycleNum;
                
                Controller.Update();
                
                // Handle interrupts after every instruction
                _bus.HandleInterrupts();
            }

            // Frame limiter logic
            if (limiterEnabled)
            {
                // Limit FPS
                if (frameTimer.ElapsedMilliseconds <= 10)
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
