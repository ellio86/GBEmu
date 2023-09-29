using GBEmulator.Core.Options;

namespace GBEmulator.App;

using System;
using Hardware.Components;
using Hardware.Cartridges;
using System.Diagnostics;
using Core.Interfaces;
using ITimer = GBEmulator.Core.Interfaces.ITimer;

public class GameBoy
{
    private readonly AppSettings _appSettings;
    
    // Application window
    private Form _window;

    // Hardware that gets connected to the BUS
    private readonly ICpu _cpu;
    private readonly ITimer _timer;
    private readonly IPpu _ppu;
    public readonly IController Controller;
    
    // BUS
    private Bus? _bus;

    // Extra properties
    private bool _poweredOn = false;
    private const int CyclesPerFrame = 70224/2;

    public GameBoy(IPpu ppu, ICpu cpu, ITimer timer, IController controller, AppSettings appSettings)
    {
        _ppu = ppu ?? throw new ArgumentNullException(nameof(ppu));
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    private string _romPath = "..\\..\\..\\..\\GBEmulator.Tests\\Test Roms\\zelda.gb";
    private string GameName => Path.GetFileName(_romPath).Replace(".gb", "");
    private CancellationTokenSource? _mainLoopCancellationTokenSource;
    private bool _loadRequest;
    private string _newRomPath = "";


    public void Initialise(Form window)
    {
        _window = window;
        var windowObj = new Window(_window);

        
        // Create Cartridge
        var cartridge = new Cartridge(_romPath);

        if (cartridge.SavesEnabled)
        {
            var saveLocation = Path.Join(_appSettings.SaveDirectory, $"{GameName}.sav");
            if (Path.Exists(saveLocation))
            {
                cartridge.LoadSaveFile(saveLocation);
            }
        }

        // Create new BUS
        _bus ??= new Bus(_cpu, _timer, _ppu, windowObj, Controller, _appSettings);
        _bus.Reset();
        
        // Load Cartridge
        _bus.LoadCartridge(cartridge);
        
        // Power on GameBoy
        _poweredOn = true;

        _mainLoopCancellationTokenSource = new CancellationTokenSource();

        // Start task on new thread for main loop
        Task.Factory.StartNew(delegate { StartClock(); }, _mainLoopCancellationTokenSource.Token, TaskCreationOptions.LongRunning);
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
                if (frameTimer.ElapsedMilliseconds <= 1000 / 60)
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

        if (_loadRequest)
        {
            _romPath = _newRomPath;
            Initialise(_window);
        }
    }

    public void Save()
    {
        _bus.DumpExternalMemory(GameName);
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

    public void LoadNewRom(string file)
    {
        _mainLoopCancellationTokenSource?.Cancel();
        _poweredOn = false;
        _loadRequest = true;
        _newRomPath = file;
    }
}
