using GBEmulator.Hardware.Components.Apu;

namespace GBEmulator.App;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UI;
using Core.Options;
using Core.Interfaces;
using Hardware.Components;
using Hardware.Components.Cpu;

internal static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Windows forms settings
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Create Host for DI
        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        // Get Emulator Form from services so that it gets injected with appSettings/GameBoy correctly
        var emulator = host.Services.GetService<Emulator>() ?? throw new NullReferenceException(nameof(Emulator));
        
        // Run application
        Application.Run(emulator);
    }

    /// <summary>
    /// Create Host DI container
    /// </summary>
    /// <returns></returns>
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddJsonFile("appsettings.json", false);
                builder.AddJsonFile("appsettings.Local.json", true);
            })
            .ConfigureServices((context, services) =>
            {
                // App Settings
                services.AddScoped(_ => new AppSettings()
                {
                    Scale = context.Configuration.GetValue<int>(nameof(AppSettings.Scale)),
                    SaveDirectory = context.Configuration.GetValue<string>(nameof(AppSettings.SaveDirectory)),
                    ForceConsole = context.Configuration.GetValue<bool>(nameof(AppSettings.ForceConsole))
                });
                
                // Hardware Components
                services.AddScoped<IPpu, Ppu>();
                services.AddScoped<ICpu, Cpu>();
                services.AddScoped<ITimer, Timer>();
                services.AddScoped<ILcd, Lcd>();
                services.AddScoped<IController, Controller>();
                services.AddScoped<IApu, Apu>();
                services.AddScoped<IAudioDriver, AudioSdl>();
                
                services.AddScoped<GameBoy>();
                services.AddScoped<Emulator>();
            });
    }
}