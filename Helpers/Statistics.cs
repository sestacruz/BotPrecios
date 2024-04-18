using BotPrecios.Model;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPrecios.Helpers
{
    internal class Statistics
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
                cba.GetDiaryCBABySupermarket(supermarket);
                actualCBA.Add(cba);
                Utilities.WriteColor($"[{(supermarket.Length <7 ? supermarket+"\t" : supermarket)}]\t|\t${cba.totalPrice:0.00}\t|\t{cba.variation:0.00}%",ConsoleColor.Magenta);
            }

            return actualCBA;
        }
    }
}
