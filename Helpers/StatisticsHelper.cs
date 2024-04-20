using BotPrecios.Model;
using Dapper;
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
        public static List<CBA> GetCBAStatistics()
        {
            List<CBA> actualCBA = new ();
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            List<string> supermarkets = con.Query<string>("SELECT name FROM Supermarkets").ToList();
            Console.WriteLine(new string('-',60));
            Console.WriteLine("Resultados CBA");
            Console.WriteLine(new string('-',60));
            Console.WriteLine("Supermercado\t|\tPrecio\t\t|\tVariación");
            Console.WriteLine(new string('-',60));

            foreach(string supermarket in supermarkets) 
            { 
                CBA cba = new();
                cba.GetAccumCBABySupermarket(supermarket);
                actualCBA.Add(cba);
                Utilities.WriteColor($"[{(supermarket.Length <7 ? supermarket+"\t" : supermarket)}]\t|\t${cba.totalPrice:0.00}\t|\t{cba.variation:0.00}%",ConsoleColor.Magenta);
            }

            return actualCBA;
        }

        public static void GetMostsCBAs(List<CBA> cbas)
        {
            cbas = cbas.OrderBy(x => x.totalPrice).ToList();
            Console.WriteLine(new string('-', 60));
            Utilities.WriteColor($"El supermercado [{cbas.LastOrDefault().superMarket}] tiene la CBA mas cara", ConsoleColor.Red);
            Utilities.WriteColor($"El supermercado [{cbas.FirstOrDefault().superMarket}] tiene la CBA mas barata", ConsoleColor.Green);
            Console.WriteLine(new string('-', 60));
        }

        public static List<CBA> GetTop5Categories()
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("Tops 5 Categorias");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("Puesto\t|\tSupermercado\t|\tCategoria\t\t|\tVariación");
            Console.WriteLine(new string('-', 90));

            List<CBA> actualCBA = CBA.GetCategoriesVariation();
            actualCBA = actualCBA.OrderBy(x => x.variation).ToList();

            for (int i = 0; i < 5; i++)
            {
                string category = actualCBA[i].category.Length <= 7 ? actualCBA[i].category + "\t\t" : actualCBA[i].category;
                string superMarket = actualCBA[i].superMarket.Length <= 7 ? actualCBA[i].superMarket + "\t" : actualCBA[i].superMarket;
                Utilities.WriteColor($"[{i + 1}]\t|\t{superMarket}\t|\t{category}\t|\t{actualCBA[i].variation:0.00}%", ConsoleColor.Magenta);
            }
            Console.WriteLine(new string('-', 90));
            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                string category = actualCBA[index].category.Length <= 7 ? actualCBA[index].category + "\t\t" : actualCBA[index].category;
                string superMarket = actualCBA[index].superMarket.Length <= 7 ? actualCBA[index].superMarket + "\t" : actualCBA[index].superMarket;
                Utilities.WriteColor($"[{(i*(-1))+6}]\t|\t{superMarket}\t|\t{category}\t|\t{actualCBA[index].variation:0.00}%", ConsoleColor.Magenta);
            }

            return actualCBA;
        }

        public static List<CBA> GetTop5Products()
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("Tops 5 Productos");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("Puesto\t|\tSupermercado\t|\tProducto\t\t|\tVariación");
            Console.WriteLine(new string('-', 90));

            List<CBA> actualCBA = CBA.GetProductsVariation();
            actualCBA = actualCBA.OrderBy(x => x.variation).ToList();

            for (int i = 0; i < 5; i++)
            {
                string product = actualCBA[i].product.Length <= 7 ? actualCBA[i].product + "\t\t" : actualCBA[i].product;
                string superMarket = actualCBA[i].superMarket.Length <= 7 ? actualCBA[i].superMarket + "\t" : actualCBA[i].superMarket;
                Utilities.WriteColor($"[{i + 1}]\t|\t{superMarket}\t|\t{product}\t|\t{actualCBA[i].variation:0.00}%", ConsoleColor.Magenta);
            }
            Console.WriteLine(new string('-', 90));
            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                string product = actualCBA[index].product.Length <= 7 ? actualCBA[index].product + "\t\t" : actualCBA[index].product;
                string superMarket = actualCBA[index].superMarket.Length <= 7 ? actualCBA[index].superMarket + "\t" : actualCBA[index].superMarket;
                Utilities.WriteColor($"[{(i * (-1)) + 6}]\t|\t{superMarket}\t|\t{product}\t|\t{actualCBA[index].variation:0.00}%", ConsoleColor.Magenta);
            }

            return actualCBA;
        }
    }
}
