namespace GBEmulator.App;

using System;
using Hardware;
using System.Diagnostics;

public class GameBoy
{
    // Application window
    Form _window;

    // Hardware
    private Cpu _cpu;
    private Timer _timer;
    private Ppu _ppu;
    private Bus _bus;


    private bool PoweredOn = false;

    private const int CyclesPerFrame = 70224;

    public GameBoy(Form window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public void Initialise()
    {
        _timer = new Timer();
        _cpu = new Cpu();
        _ppu = new Ppu();
        var windowObj = new Window(_window);
        _bus = new Bus(_cpu, _timer, _ppu, windowObj, false);

        //_bus.LoadRom("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\pkmnblue.gb");
        //_bus.LoadRom("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\cpu_instrs.gb");
        _bus.LoadRom("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\09-op r,r.gb");
        //_bus.LoadRom("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\02-interrupts.gb");
        //_bus.LoadRom("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\zelda.gb");
        //_bus.LoadRom("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\tetris.gb");
        
        PoweredOn = true;

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

        while (PoweredOn)
        {
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                SetWindowText($"Elliot Kempson GB Emulator | FPS: {fps}");
                stopwatch.Restart();
                fps = 0;
            }

            
            while (totalCycles < CyclesPerFrame && !limiter)
            {
                // Tick the clock
                var cycleNum = _cpu.Clock(logWriter);

                totalCycles += cycleNum;
                
                // Update Timer, PPU and joypad

                _timer.Clock(cycleNum);
                _ppu.Clock(cycleNum);
                
                _cpu.HandleInterrupts();

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
                if (frameTimer.ElapsedMilliseconds < 1000 / 60)
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
                Action writeText = delegate { SetWindowText(text); };
                _window.Invoke(writeText);
            }
            else
            {
                _window.Text = text;
            }
            _window.Text = text;
        }
        catch
        {
            // ignored
        }
    }
}
