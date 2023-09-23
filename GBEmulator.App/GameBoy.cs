namespace GBEmulator.App;

using System;
using GBEmulator.Core.Interfaces;
using Hardware;
using System.Diagnostics;
using System.CodeDom.Compiler;

public class GameBoy
{
    // Application window
    Form _window;

    // Hardware
    private Cpu _cpu;
    private Timer _timer;
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
        _bus = new Bus(_cpu, _timer);

        _bus.LoadRom("..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\09-op r,r.gb");
        PoweredOn = true;

        Task.Factory.StartNew(StartClock, TaskCreationOptions.LongRunning);
    }

    private void StartClock()
    {
        var stopwatch = Stopwatch.StartNew();
        using var logWriter = new StreamWriter(@"..\..\..\log.txt");
        var totalCycles = 0;
        var _fps = 0;

        while (PoweredOn)
        {
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                SetWindowText($"Elliot Kempson GB Emulator | FPS: {_fps}");
                stopwatch.Restart();
                _fps = 0;
            }
            
            while (totalCycles < CyclesPerFrame)
            {
                // Tick the clock
                var cycleNum = _cpu.Clock(logWriter);

                totalCycles += cycleNum;
                
                // Update Timer, PPU and joypad

                _cpu.HandleInterrupts();

                // Listen to serial io port for test results
                if (_bus.ReadMemory(0xff02) == 0x81)
                {
                    var c = (char)_bus.ReadMemory(0xff01);
                    Console.Write(c);
                    _bus.WriteMemory(0xff02, 0x00);
                }
            }

            totalCycles -= CyclesPerFrame;
            _fps++;
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
