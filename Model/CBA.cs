using BotPrecios.Helpers;
using Dapper;
using OpenQA.Selenium.DevTools.V121.Tracing;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BotPrecios.Model
{
    internal class CBA
    {
        public string superMarket { get; set; }
        public string category { get; set; }
        public string product { get; set; }
        public decimal totalPrice { get; set; }
        public decimal variation { get; set; }

        public void GetAccumCBABySupermarket(string superMarket)
        {
            List<CBAData> CBAProducts = Utilities.LoadJSONFile<CBAData>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Categories\\{superMarket}CBAData.json"));
            this.superMarket = superMarket;
            CBA cbaYesterday = new() { superMarket = superMarket};

            DateTime firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string sql = "SELECT Supermarket,SUM(price) as totalPrice FROM Products " +
                         "WHERE Supermarket = @superMarket " +
                         "AND Name IN @names " +
                         "AND UPPER(Category) = @category " +
                         "AND PriceDate = @priceDate " +
                         "GROUP BY Supermarket";

            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            foreach (CBAData item in CBAProducts)
            {
                
                CBA? resultToday = con.QueryFirstOrDefault<CBA>(sql, 
                    new
                    {
                        superMarket,
                        category = item.category.ToUpper(),
                        item.names,
                        priceDate = DateTime.Now.ToString(Constants.dateFormat)
                    });
                
                CBA? resultYesterday = con.QueryFirstOrDefault<CBA>(sql,
                    new
                    {
                        superMarket,
                        category = item.category.ToUpper(),
                        item.names,
                        priceDate = firstDay.ToString(Constants.dateFormat)
                    });
                
                //Si el producto no se encuentra en alguna de las 2 consultas, se excluye para no interferir en la medicion
                if (resultToday == null || resultYesterday == null)
                    continue;
                
                if(resultToday != null )
                    totalPrice += resultToday.totalPrice / item.names.Length;
                if (resultYesterday != null)
                    cbaYesterday.totalPrice += resultYesterday.totalPrice / item.names.Length;
            }
            variation = CalculateVariation(totalPrice, cbaYesterday.totalPrice);
        }

        public static List<CBA> GetCategoriesVariation()
        {
            DateTime firstDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string sql = "SELECT Supermarket,Category, " +
                         "CAST(((SUM(CASE WHEN PriceDate = @today THEN Price ELSE 0 END)  -" +
                         " SUM(CASE WHEN PriceDate = @firstDate THEN Price ELSE 0 END)) * 100) / " +
                         " SUM(CASE WHEN PriceDate = @today THEN Price ELSE 0 END) AS REAL) as variation " +
                         "FROM Products " +
                         "WHERE PriceDate IN (@firstDate, @today) " +
                         "GROUP BY Supermarket, Category " +
                         "HAVING COUNT(DISTINCT PriceDate) = 2;";
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            List<CBA> result = con.Query<CBA>(sql, new { today = DateTime.Now.ToString(Constants.dateFormat), firstDate = firstDate.ToString(Constants.dateFormat) }).ToList();
            return result;
        }

        public static List<CBA> GetProductsVariation()
        {
            DateTime firstDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string sql = "SELECT Supermarket,Name as product, " +
                         "CAST(((SUM(CASE WHEN PriceDate = @today THEN Price ELSE 0 END)  -" +
                         " SUM(CASE WHEN PriceDate = @firstDate THEN Price ELSE 0 END)) * 100) / " +
                         " SUM(CASE WHEN PriceDate = @today THEN Price ELSE 0 END) AS REAL) as variation " +
                         "FROM Products " +
                         "WHERE PriceDate IN (@firstDate, @today) " +
                         "GROUP BY Supermarket, Name " +
                         "HAVING COUNT(DISTINCT PriceDate) = 2;";
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            List<CBA> result = con.Query<CBA>(sql, new { today = DateTime.Now.ToString(Constants.dateFormat), firstDate = firstDate.ToString(Constants.dateFormat) }).ToList();
            return result;
        }

        private decimal CalculateVariation(decimal actualPrice, decimal previousPrice)
        {
            return actualPrice == 0 ? 0 : decimal.Divide((previousPrice - actualPrice) *100,actualPrice);
        }
    }

    internal class CBAData
    {
        public string category { get; set; }
        public string[] names { get; set; }
    }
}
