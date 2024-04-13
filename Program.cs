using BotPrecios.Bots;
using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using System.Net.Http.Headers;


string market = string.Empty;
if (args.Length > 0)
    market = args[0];

ChromeOptions options= new() { BrowserVersion = "123" };
options.AddArgument("--start-maximized");
options.AddArgument("--log-level=3");

switch (market)
{
    case Constants.Jumbo:
        Jumbo jumbo = new (options);
        jumbo.GetProductsData();
        break;
    case Constants.ChangoMas:
        ChangoMas chango = new (options);
        chango.GetProductsData();
        break;
    case Constants.Carrefour:
        Carrefour carrefour = new (options);
        carrefour.GetProductsData();
        break;
    case Constants.Coto:
        Coto coto = new (options);
        coto.GetProductsData();
        break;
    default:
        Jumbo jumboAll = new (options);
        jumboAll.GetProductsData();
        options = new() { BrowserVersion = "123" };
        options.AddArgument("--start-maximized");
        options.AddArgument("--log-level=3");
        ChangoMas changoAll = new (options);
        changoAll.GetProductsData();
        options = new() { BrowserVersion = "123" };
        options.AddArgument("--start-maximized");
        options.AddArgument("--log-level=3");
        Carrefour carrefourAll = new (options);
        carrefourAll.GetProductsData();
        options = new() { BrowserVersion = "123" };
        options.AddArgument("--start-maximized");
        options.AddArgument("--log-level=3");
        Coto cotoAll = new (options);
        cotoAll.GetProductsData();
        break;
}

Console.WriteLine("Fin de la obtención de datos");