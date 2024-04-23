﻿using BotPrecios.Bots;
using BotPrecios.Helpers;
using BotPrecios.Interfaces;
using BotPrecios.Model;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

string option = string.Empty;
ILogHelper logger = new LogHelper("General");

if (args.Length > 0)
    option = args[0];

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

List<Product> products = [];
IBot[] bots =
    [
        new Jumbo(new LogHelper(Constants.Jumbo)),
        new ChangoMas(new LogHelper(Constants.ChangoMas)),
        new Carrefour(new LogHelper(Constants.Carrefour)),
        new Coto(new LogHelper(Constants.Coto))
    ];

if (!string.IsNullOrEmpty(option))
{
    var botsToDispose = bots.Where(b => option != b.GetType().Name || string.IsNullOrEmpty(option)).ToArray();
    foreach (var bot in botsToDispose)
        bot.Dispose();
    bots = bots.Where(b => option == b.GetType().Name || string.IsNullOrEmpty(option)).ToArray();
}

List<Task<List<Product>>> tasks = new List<Task<List<Product>>>();
foreach (var bot in bots)
{
    logger.ConsoleLog($"Inciando bot {bot.GetType().Name}");
    tasks.Add(Task.Run(() =>
    {
        List<Product> botProducts = bot.GetProductsData();
        bot.Dispose();
        return botProducts;
    }));
}

await Task.WhenAll(tasks);
products.AddRange(tasks.SelectMany(t => t.Result));

Console.WriteLine("Fin de la obtención de datos");
Console.WriteLine(new string('-', 120));

Console.WriteLine("Se obtuvieron los siguientes productos según las categorías:");
List<Category> categories = Category.GetAllCategories();
foreach (Category category in categories)
{
    int cant = products.Count(p => p.category == category.name);
    logger.ConsoleLog($"{category.name}: [{cant}]", foreColor: ConsoleColor.Cyan);
}
if (option == "statistics" || string.IsNullOrEmpty(option))
{
    List<CBA> CBAs = StatisticsHelper.GetCBAStatistics();
    StatisticsHelper.GetTop5Categories(out List<CBA> topPositiveCat, out List<CBA> topNegativeCat);
    StatisticsHelper.GetTop5Products(out List<CBA> topPositiveProd, out List<CBA> topNegativeProd);
    StatisticsHelper.GetMostsCBAs(CBAs,out string expensive,out string cheapest);
    if (args.Length > 1 /*|| args.Length == 0*/)
    {
        logger.ConsoleLog("Posteando en X");
        string apiUrl = config["ApiURL"];
        PostsHelper postsHelper = new(apiUrl);
        postsHelper.PublishMontlyCBA(CBAs);
        postsHelper.PublishTop5CheapestCategory(topPositiveCat);
        postsHelper.PublishTop5MostExpensiveCategory(topNegativeCat);
        postsHelper.PublishTop5CheapestProduct(topPositiveProd);
        postsHelper.PublishTop5MostExpensiveProduct(topNegativeProd);
        DateTime today = DateTime.Now;
        DateTime lastDayOfMonth = new (today.Year,today.Month,DateTime.DaysInMonth(today.Year,today.Month));
        if (today.Date == lastDayOfMonth.Date)
            postsHelper.PublisTopMonthCBAs(expensive, cheapest);
        logger.ConsoleLog("Posteos terminados");
    }
}

