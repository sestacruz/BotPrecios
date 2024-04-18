using BotPrecios.Helpers;
using Dapper;
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
        public decimal totalPrice { get; set; }
        public decimal variation { get; set; }

        public void GetDiaryCBABySupermarket(string superMarket)
        {
            List<CBAData> CBAProducts = Utilities.LoadJSONFile<CBAData>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Categories\\{superMarket}CBAData.json"));
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            this.superMarket = superMarket;
            CBA cbaYesterday = new() { superMarket = superMarket};
            foreach (CBAData item in CBAProducts)
            {
                
                CBA? resultToday = con.QueryFirstOrDefault<CBA>("SELECT Supermarket,SUM(price) as totalPrice FROM Products " +
                                                    "WHERE Supermarket = @superMarket " +
                                                    "AND Name IN @names " +
                                                    "AND UPPER(Category) = @category " +
                                                    "AND PriceDate = @priceDate " +
                                                    "GROUP BY Supermarket",
                                                    new { superMarket, category = item.category.ToUpper(), item.names, priceDate = DateTime.Now.ToString(Constants.dateFormat) });
                
                CBA? resultYesterday = con.QueryFirstOrDefault<CBA>("SELECT Supermarket,SUM(price) as totalPrice FROM Products " +
                                                    "WHERE Supermarket = @superMarket " +
                                                    "AND Name IN @names " +
                                                    "AND UPPER(Category) = @category " +
                                                    "AND PriceDate = @priceDate " +
                                                    "GROUP BY Supermarket",
                                                    new { superMarket, category = item.category.ToUpper(), item.names, priceDate = DateTime.Now.AddDays(-1).ToString(Constants.dateFormat) });
                
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
