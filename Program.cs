using BotPrecios.Bots;
using BotPrecios.Interfaces;
using BotPrecios.Model;
using BotPrecios.Logging;
using BotPrecios.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.IdentityModel.Logging;

var builder = new ServiceCollection();

// Configuración del logger
builder.AddSingleton<ILogService, LogService>(sp => new LogService("General"));

// Configuración del archivo de configuración
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
IConfiguration config = configurationBuilder.Build();
builder.AddSingleton<IConfiguration>(config);

// Configuración de bots
builder.AddSingleton<IBot, Jumbo>(sp =>
    new Jumbo(
        sp.GetRequiredService<ILogService>(),
        sp.GetRequiredService<IConfiguration>(),
        GetChromeVersion()
    )
);
builder.AddSingleton<IBot, ChangoMas>(sp =>
    new ChangoMas(
        sp.GetRequiredService<ILogService>(),
        sp.GetRequiredService<IConfiguration>(),
        GetChromeVersion()
    )
);
builder.AddSingleton<IBot, Carrefour>(sp =>
    new Carrefour(
        sp.GetRequiredService<ILogService>(),
        sp.GetRequiredService<IConfiguration>(),
        GetChromeVersion()
    )
);
builder.AddSingleton<IBot, Coto>(sp =>
    new Coto(
        sp.GetRequiredService<ILogService>(),
        sp.GetRequiredService<IConfiguration>(),
        GetChromeVersion()
    )
);


// Configuración de servicios
builder.AddSingleton<IStatisticsService, StatisticsService>();
builder.AddSingleton<IPostsService, PostsService>();

// Configuración del BotService
builder.AddSingleton<IBotService, BotService>();

var serviceProvider = builder.BuildServiceProvider();

// Obtención de argumentos
var botService = serviceProvider.GetRequiredService<IBotService>();
await botService.RunAsync(args);

Console.WriteLine("Fin de la obtención de datos");

static string GetChromeVersion()
{
    object path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null);
    return path != null ? FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion.Split('.')[0] : string.Empty;
}
