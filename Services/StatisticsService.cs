using BotPrecios.Logging;
using BotPrecios.Model;
using Dapper;
using System.Data.SQLite;

namespace BotPrecios.Services
{
    internal class StatisticsService : IStatisticsService
    {
        public List<CBA> GetCBAStatistics(ILogService logger)
        {
            List<CBA> actualCBA = new List<CBA>();
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            List<string> supermarkets = con.Query<string>("SELECT name FROM Supermarkets").ToList();

            logger.ConsoleLog(new string('_', 62));
            logger.ConsoleLog(LogService.GetCenteredLegend("Resultados CBA", 62));
            logger.ConsoleLog(new string('_', 62));
            logger.ConsoleLog($"{LogService.GetCenteredLegend("Supermercado", 20)}|{LogService.GetCenteredLegend("Precio", 20)} |{LogService.GetCenteredLegend("Variación", 20)}");
            logger.ConsoleLog(new string('_', 62));

            foreach (string supermarket in supermarkets)
            {
                CBA cba = new();
                cba.GetAccumCBABySupermarket(supermarket);
                actualCBA.Add(cba);
                logger.ConsoleLog($"[{LogService.GetCenteredLegend(supermarket, 20)}]|" +
                    $"${LogService.GetCenteredLegend(cba.TotalPrice.ToString("0.00"), 19)}|" +
                    $"{LogService.GetCenteredLegend(cba.Variation.ToString("0.00"), 19)}%");
            }

            return actualCBA;
        }

        public void GetMostsCBAs(ILogService logger, List<CBA> cbas, out string mostExpensive, out string cheapest)
        {
            cbas = cbas.OrderBy(x => x.TotalPrice).ToList();
            mostExpensive = cbas.LastOrDefault()?.SuperMarket;
            cheapest = cbas.FirstOrDefault()?.SuperMarket;

            logger.ConsoleLog(new string('_', 60));
            logger.ConsoleLog($"El supermercado [{mostExpensive}] tiene la CBA más cara", foreColor: ConsoleColor.Red);
            logger.ConsoleLog($"El supermercado [{cheapest}] tiene la CBA más barata", foreColor: ConsoleColor.Green);
            logger.ConsoleLog(new string('_', 60));
        }

        public void GetTop5Products(ILogService logger, out List<CBA> top5positive, out List<CBA> top5negative, out List<CBA> categoriesVariation)
        {
            top5positive = [];
            top5negative = [];

            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog(LogService.GetCenteredLegend("Top 5 Productos", 83));
            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog($"{LogService.GetCenteredLegend("#", 20)}|" +
                $"{LogService.GetCenteredLegend("Supermercado", 20)}|" +
                $"{LogService.GetCenteredLegend("Producto", 20)}|" +
                $"{LogService.GetCenteredLegend("Variación", 20)}");
            logger.ConsoleLog(new string('_', 83));

            List<CBA> actualCBA = CBA.GetProductsVariation(out categoriesVariation);
            actualCBA = actualCBA.Where(x => x.Variation < 1000).OrderBy(x => x.Variation).ToList();

            for (int i = 0; i < 5; i++)
            {
                logger.ConsoleLog($"[{LogService.GetCenteredLegend((i + 1).ToString(), 20)}]|" +
                    $"{LogService.GetCenteredLegend(actualCBA[i].SuperMarket, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[i].Product, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[i].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5positive.Add(actualCBA[i]);
            }

            logger.ConsoleLog(new string('=', 83));

            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                logger.ConsoleLog($"[{LogService.GetCenteredLegend(((i * (-1)) + 6).ToString(), 20)}]|" +
                    $"{LogService.GetCenteredLegend(actualCBA[index].SuperMarket, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[index].Product, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[index].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5negative.Add(actualCBA[index]);
            }
        }

        public void GetTop5Categories(ILogService logger, List<CBA> categoriesVariation, out List<CBA> top5positive, out List<CBA> top5negative)
        {
            top5positive = [];
            top5negative = [];

            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog(LogService.GetCenteredLegend("Top 5 Categorías", 83));
            logger.ConsoleLog(new string('_', 83));
            logger.ConsoleLog($"{LogService.GetCenteredLegend("#", 20)}|" +
                $"{LogService.GetCenteredLegend("Supermercado", 20)}|" +
                $"{LogService.GetCenteredLegend("Categoría", 20)}|" +
                $"{LogService.GetCenteredLegend("Variación", 20)}");
            logger.ConsoleLog(new string('_', 83));

            List<CBA> actualCBA = categoriesVariation;
            actualCBA = actualCBA.Where(x => x.Variation < 1000).OrderBy(x => x.Variation).ToList();

            for (int i = 0; i < 5; i++)
            {
                logger.ConsoleLog($"[{LogService.GetCenteredLegend((i + 1).ToString(), 20)}]|" +
                    $"{LogService.GetCenteredLegend(actualCBA[i].SuperMarket, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[i].Category, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[i].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5positive.Add(actualCBA[i]);
            }

            logger.ConsoleLog(new string('=', 83));

            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                logger.ConsoleLog($"[{LogService.GetCenteredLegend(((i * (-1)) + 6).ToString(), 20)}]|" +
                    $"{LogService.GetCenteredLegend(actualCBA[index].SuperMarket, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[index].Category, 20)}|" +
                    $"{LogService.GetCenteredLegend(actualCBA[index].Variation.ToString("+0.00;-0.00;0.00"), 20)}%");
                top5negative.Add(actualCBA[index]);
            }
        }
    }
}
