using BotPrecios.Interfaces;
using BotPrecios.Model;
using Dapper;
using OpenQA.Selenium.DevTools.V121.CSS;
using OpenQA.Selenium.DevTools.V121.Target;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPrecios.Helpers
{
    internal class StatisticsHelper
    {
        public static List<CBA> GetCBAStatistics(ILogHelper logger)
        {
            List<CBA> actualCBA = [];
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            List<string> supermarkets = con.Query<string>("SELECT name FROM Supermarkets").ToList();
            logger.ConsoleLog(new string('_', 62));
            logger.ConsoleLog(LogHelper.GetCenteredLegend("Resultados CBA",62));
            logger.ConsoleLog(new string('_',62));
            logger.ConsoleLog($"{LogHelper.GetCenteredLegend("Supermercado",20)}|{LogHelper.GetCenteredLegend("Precio",20)} |{LogHelper.GetCenteredLegend("Variación",20)}");
            logger.ConsoleLog(new string('_',62));

            foreach(string supermarket in supermarkets) 
            { 
                CBA cba = new();
                cba.GetAccumCBABySupermarket(supermarket);
                actualCBA.Add(cba);
                logger.ConsoleLog($"[{LogHelper.GetCenteredLegend(supermarket, 20)}]|" +
                    $"${LogHelper.GetCenteredLegend(cba.TotalPrice.ToString("0.00"),19)}|" +
                    $"{LogHelper.GetCenteredLegend(cba.Variation.ToString("0.00"),19)}%");
            }

            return actualCBA;
        }

        public static void GetMostsCBAs(ILogHelper logger,List<CBA> cbas, out string mostExpensive, out string cheapest)
        {
            cbas = [.. cbas.OrderBy(x => x.TotalPrice)];
            mostExpensive = cbas.LastOrDefault().SuperMarket;
            cheapest = cbas.FirstOrDefault().SuperMarket;
            logger.ConsoleLog(new string('_', 60));
            logger.ConsoleLog($"El supermercado [{mostExpensive}] tiene la CBA mas cara",foreColor:ConsoleColor.Red);
            logger.ConsoleLog($"El supermercado [{cheapest}] tiene la CBA mas barata", foreColor:ConsoleColor.Green);
            logger.ConsoleLog(new string('_', 60));
        }

        public static void GetTop5Categories(ILogHelper logger, out List<CBA>top5postive, out List<CBA>top5negative)
        {
            top5postive = [];
            top5negative = [];

            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog(LogHelper.GetCenteredLegend("Tops 5 Categorias",83));
            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog($"{LogHelper.GetCenteredLegend("#",20)}|" +
                $"{LogHelper.GetCenteredLegend("Supermercado",20)}|" +
                $"{LogHelper.GetCenteredLegend("Categoria",20)}|" +
                $"{LogHelper.GetCenteredLegend("Variación",20)}");
            logger.ConsoleLog(new string('_', 83));

            List<CBA> actualCBA = CBA.GetCategoriesVariation();
            //Se quitan las variaciones demasiado grandes, pueden ser causadas por errores en los datos
            actualCBA = [.. actualCBA.Where(x => x.Variation < 1000).OrderBy(x => x.Variation)];

            for (int i = 0; i < 5; i++)
            {
                logger.ConsoleLog($"[{LogHelper.GetCenteredLegend((i + 1).ToString(),20)}]|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[i].SuperMarket,20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[i].Category, 20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[i].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5postive.Add(actualCBA[i]);                
            }
            logger.ConsoleLog(new string('=', 83));
            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                logger.ConsoleLog($"[{LogHelper.GetCenteredLegend(((i * (-1)) + 6).ToString(), 20)}]|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[index].SuperMarket, 20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[index].Category, 20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[index].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5negative.Add(actualCBA[index]);
            }
        }

        public static void GetTop5Products(ILogHelper logger,out List<CBA> top5postive, out List<CBA> top5negative)
        {
            top5postive = [];
            top5negative = [];

            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog(LogHelper.GetCenteredLegend("Tops 5 Productos", 83));
            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog($"{LogHelper.GetCenteredLegend("#", 20)}|" +
                $"{LogHelper.GetCenteredLegend("Supermercado", 20)}|" +
                $"{LogHelper.GetCenteredLegend("Producto", 20)}|" +
                $"{LogHelper.GetCenteredLegend("Variación", 20)}");
            logger.ConsoleLog(new string('_', 83));

            List<CBA> actualCBA = CBA.GetProductsVariation();
            //Se quitan las variaciones demasiado grandes, pueden ser causadas por errores en los datos
            actualCBA = [.. actualCBA.Where(x => x.Variation < 1000).OrderBy(x => x.Variation)];

            for (int i = 0; i < 5; i++)
            {
                logger.ConsoleLog($"[{LogHelper.GetCenteredLegend((i + 1).ToString(), 20)}]|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[i].SuperMarket, 20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[i].Product, 20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[i].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5postive.Add(actualCBA[i]);
            }
            logger.ConsoleLog(new string('=', 83));
            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                logger.ConsoleLog($"[{LogHelper.GetCenteredLegend(((i * (-1)) + 6).ToString(), 20)}]|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[index].SuperMarket, 20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[index].Product, 20)}|" +
                    $"{LogHelper.GetCenteredLegend(actualCBA[index].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5negative.Add(actualCBA[index]);
            }
        }
    }
}
