using GBEmulator.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddScoped<IRegisters, Registers>();
builder.Services.AddScoped<ICpu, Cpu>();

var host = builder.Build();

await host.RunAsync();