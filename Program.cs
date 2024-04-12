using BotPrecios.Bots;
using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using System.Net.Http.Headers;


string market = string.Empty;
if (args.Length > 0)
    market = args[0];

ChromeOptions options= new() { BrowserVersion = "123" };
options.AddArgument("--start-maximized");

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
    default:
        Jumbo jumboAll = new (options);
        ChangoMas changoAll = new (options);
        Carrefour carrefourAll = new (options);
        jumboAll.GetProductsData();
        changoAll.GetProductsData();
        carrefourAll.GetProductsData();
        break;
}

Console.WriteLine("Fin de la obtención de datos");