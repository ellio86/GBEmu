﻿namespace GBEmulator.App;

using System;
using Hardware.Components;
using Hardware.Cartridges;
using System.Diagnostics;
using Core.Interfaces;
using Core.Options;
using ITimer = Core.Interfaces.ITimer;
using System.IO;

public class GameBoy(IPpu ppu, ICpu cpu, ITimer timer, IController controller, IAudioDriver audioDriver, IApu apu, AppSettings appSettings, TextWriter logger)
{
    private readonly AppSettings _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    
    // Application window
    private Form? _window;

    // Hardware that gets connected to the BUS
    private readonly ICpu _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
    private readonly ITimer _timer = timer ?? throw new ArgumentNullException(nameof(timer));
    private readonly IPpu _ppu = ppu ?? throw new ArgumentNullException(nameof(ppu));
    private readonly IApu _apu = apu ?? throw new ArgumentNullException(nameof(apu));
    public readonly IController Controller = controller ?? throw new ArgumentNullException(nameof(controller));
    private readonly IAudioDriver _audioDriver = audioDriver ?? throw new ArgumentNullException(nameof(audioDriver));
    private readonly TextWriter? _logger = appSettings.LoggingEnabled ? logger : null;
    
    
    // BUS
    private Bus? _bus;

    private ICartridge? _loadedCartridge;

    // Extra properties
    private bool _poweredOn = false;
    private const int CyclesPerFrame = 70224/2;
    private string _romPath = string.Empty; 
    private CancellationTokenSource? _mainLoopCancellationTokenSource;
    private bool _loadRequest;
    private string _newRomPath = "";
    private string? _savePath;

    private string GameName => Path.GetFileName(_romPath).Replace(".gb", "");

    /// <summary>
    /// Populates _bus with new instance if it needs one, creates and inserts a cartridge and 
    /// </summary>
    /// <param name="window"></param>
    public void Initialise(Form window)
    {
        _logger?.WriteLine("INF: Beginning Initialization");
        
        // Associate game boy instance with provided window
        _window = window;

        if (string.IsNullOrEmpty(_romPath))
        {
            return;
        }
        
        _savePath = Path.Join(string.IsNullOrEmpty(_appSettings.SaveDirectory) ? "./" : _appSettings.SaveDirectory, $"{GameName}.sav");
        
        _logger?.WriteLine($"INF: Save path generated: {_savePath}");
        _logger?.WriteLine($"INF: Creating Cartridge");

        // Create Cartridge
        _loadedCartridge = new Cartridge(_romPath, _savePath);
        
        _logger?.WriteLine($"INF: Cartridge Created");
        
        // Create new BUS
        if (_bus is null)
        {
            _logger?.WriteLine($"INF: Creating Bus");
            // Image control is responsible for flipping the screen
            var imageControl = new ImageControl(_window);
            _bus = new Bus(_cpu, _timer, _ppu, _apu, imageControl, Controller, _appSettings);
            
            _logger?.WriteLine($"INF: Bus Created");
        }
        
        if (_appSettings.AudioEnabled)
        {
            if (_apu.AudioDriverBound)
            {
                _audioDriver.Pause(false);
            }
            else
            {
                _logger?.WriteLine($"INF: Binding audio driver");
                
                _apu.BindAudioDriver(_audioDriver);
                
                _logger?.WriteLine($"INF: Audio driver bound");
                _logger?.WriteLine($"INF: Starting audio driver");
                _audioDriver.Start(appSettings.AudioSampleRate, appSettings.AudioBufferSize);
                _logger?.WriteLine($"INF: Audio driver successfully started");
            }
        }
        
        // Reset Hardware registers and memory
        _bus.Reset();
        _logger?.WriteLine($"INF: Bus reset");
        
        // Load Cartridge
        _bus.LoadCartridge(_loadedCartridge);
        _logger?.WriteLine($"INF: Cartridge loaded");
        
        // Power on GameBoy
        _poweredOn = true;
        _logger?.WriteLine($"INF: GameBoy Powered on");

        // Cancellation token for main loop thread for when we want to swap rom files etc.
        _mainLoopCancellationTokenSource = new CancellationTokenSource();

        _logger?.WriteLine($"INF: Starting Clock");
        // Start task on new thread for main loop
        Task.Factory.StartNew( delegate { StartClock(); }, _mainLoopCancellationTokenSource.Token, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Sets up and runs the main loop
    /// </summary>
    private void StartClock()
    {
        // debug - pass to bus.ClockCpu(StreamWriter w) below to enable
        //using var logWriter = new StreamWriter(@"log.txt");
        
        // FPS counter + variables
        var totalCycles = 0;
        var fps = 0;
        var stopwatch = Stopwatch.StartNew();
        var frameTimer = Stopwatch.StartNew();

        // Limiter Settings (TODO: Move to app setting)
        var limiterEnabled = true;
        var limiter = false;
        
        _logger?.WriteLine($"INF: Clock Started");
        
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
                var cycleNum = _bus!.ClockCpu(null);

                if (appSettings.AudioEnabled)
                {
                    // Tick APU  - adjusting how many ticks are ticked by changing the multiplier (i.e. 6) can improve audio quality at the cost of performance
                    for (var i = 0; i < cycleNum * 6; i++)
                    {
                        _apu.Tick(); 
                    }
                }


                totalCycles += cycleNum * 4;
                
                Controller.Update();

                // Handle interrupts after every instruction
                _bus.HandleInterrupts();
            }

            // Frame limiter logic
            if (limiterEnabled)
            {
                // Limit FPS
                if (frameTimer.ElapsedMilliseconds <= 1000 / (120))
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
            _audioDriver.Pause(true);
            _romPath = _newRomPath;
            Initialise(_window!);
        }
    }

    /// <summary>
    /// Dumps contents of external memory to the save directory from app settings
    /// </summary>
    public void Save(string? destinationFilePath = null)
    {
        if (_loadedCartridge is null) return;
        _bus!.DumpExternalMemory(_loadedCartridge!.GameName, destinationFilePath);
    }

    /// <summary>
    /// Thread safe function for writing to the window's text
    /// </summary>
    /// <param name="text"></param>
    private void SetWindowText(string text)
    {
        try
        {
            if (_window!.InvokeRequired)
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

    /// <summary>
    /// Request to stop the emulation and load new rom file
    /// </summary>
    /// <param name="file"></param>
    public void LoadNewRom(string file)
    {
        if (!_poweredOn)
        {
            _romPath = file;
            Initialise(_window!);
        }
        _mainLoopCancellationTokenSource?.Cancel();
        _poweredOn = false;
        _loadRequest = true;
        _newRomPath = file;
        
    }

    public void LoadSaveFile(string file)
    {
        _mainLoopCancellationTokenSource?.Cancel();
        _poweredOn = false;
        _loadRequest = true;
        _newRomPath = _romPath;
        _savePath = file;
    }
}
