using GBEmulator.Core.Interfaces;
using GBEmulator.Hardware;

namespace GBEmulator.App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UI;
using Core.Options;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        Application.Run(new Emulator((GameBoy)host.Services.GetService(typeof(GameBoy))));
    }

    public static IServiceProvider ServiceProvider { get; private set; }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                
                services.AddScoped<AppSettings>(provider => new AppSettings()
                {
                    Scale = context.Configuration.GetValue<int>(nameof(AppSettings.Scale))
                });
                services.AddScoped<IPpu, Ppu>();
                services.AddScoped<ICpu, Cpu>();
                services.AddScoped<Core.Interfaces.ITimer, Hardware.Timer>();
                services.AddScoped<Lcd>();
                services.AddScoped<GameBoy>();
            });
    }
}