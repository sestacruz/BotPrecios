using BotPrecios;
using BotPrecios.Bots;
using BotPrecios.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V121.Network;
using System.Text;
using System.Text.Json.Nodes;

string market = string.Empty;
if (args.Length > 0)
    market = args[0];

ChromeOptions options= new() { BrowserVersion = "123"};

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
    default:
        Jumbo jumboAll = new (options);
        ChangoMas changoAll = new (options);
        jumboAll.GetProductsData();
        changoAll.GetProductsData();
        break;
}

Console.WriteLine("Fin de la obtención de datos");