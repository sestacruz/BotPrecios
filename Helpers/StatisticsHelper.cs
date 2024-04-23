using BotPrecios.Interfaces;
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
                Console.WriteLine($"[{(supermarket.Length <7 ? supermarket+"\t" : supermarket)}]\t|\t${cba.totalPrice:0.00}\t|\t{cba.variation:0.00}%");
            }

            return actualCBA;
        }

        public static void GetMostsCBAs(List<CBA> cbas, out string mostExpensive, out string cheapest)
        {
            cbas = cbas.OrderBy(x => x.totalPrice).ToList();
            mostExpensive = cbas.LastOrDefault().superMarket;
            cheapest = cbas.FirstOrDefault().superMarket;
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"El supermercado [{mostExpensive}] tiene la CBA mas cara");
            Console.WriteLine($"El supermercado [{mostExpensive}] tiene la CBA mas barata");
            Console.WriteLine(new string('-', 60));
        }

        public static void GetTop5Categories(out List<CBA>top5postive, out List<CBA>top5negative)
        {
            top5postive = new List<CBA>();
            top5negative = new List<CBA>();

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
                Console.WriteLine($"[{i + 1}]\t|\t{superMarket}\t|\t{category}\t|\t{actualCBA[i].variation:0.00}%");
                top5postive.Add(actualCBA[i]);                
            }
            Console.WriteLine(new string('-', 90));
            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                string category = actualCBA[index].category.Length <= 7 ? actualCBA[index].category + "\t\t" : actualCBA[index].category;
                string superMarket = actualCBA[index].superMarket.Length <= 7 ? actualCBA[index].superMarket + "\t" : actualCBA[index].superMarket;
                Console.WriteLine($"[{(i*(-1))+6}]\t|\t{superMarket}\t|\t{category}\t|\t{actualCBA[index].variation:0.00}%");
                top5negative.Add(actualCBA[index]);
            }
        }

        public static void GetTop5Products(out List<CBA> top5postive, out List<CBA> top5negative)
        {
            top5postive = new List<CBA>();
            top5negative = new List<CBA>();

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
                Console.WriteLine($"[{i + 1}]\t|\t{superMarket}\t|\t{product}\t|\t{actualCBA[i].variation:0.00}%");
                top5postive.Add(actualCBA[i]);
            }
            Console.WriteLine(new string('-', 90));
            for (int i = 5; i >= 1; i--)
            {
                int index = actualCBA.Count - i;
                string product = actualCBA[index].product.Length <= 7 ? actualCBA[index].product + "\t\t" : actualCBA[index].product;
                string superMarket = actualCBA[index].superMarket.Length <= 7 ? actualCBA[index].superMarket + "\t" : actualCBA[index].superMarket;
                Console.WriteLine($"[{(i * (-1)) + 6}]\t|\t{superMarket}\t|\t{product}\t|\t{actualCBA[index].variation:0.00}%");
                top5negative.Add(actualCBA[index]);
            }
        }
    }
}
