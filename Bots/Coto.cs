﻿using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using BotPrecios.Interfaces;


namespace BotPrecios.Bots
{
    public class Coto : IDisposable, IBot
    {
        private ChromeOptions _co;
        private IWebDriver driver;
        private const string _superMarket = Constants.Coto;

        public Coto() 
        {
            _co = new() { BrowserVersion = "123" };
            _co.AddArgument("--start-maximized");
            _co.AddArgument("--log-level=3");
            driver = new ChromeDriver(_co);
        }

        public List<Product> GetProductsData()
        {
            Helper.WriteColor("Comenzando la lectura de los productos de la CBA de [COTO]", ConsoleColor.White,ConsoleColor.Red);
            Console.WriteLine("Leyendo categorias");
            List<Category> cotoCategories = Helper.LoadJSONFile<Category>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\Coto.json"));
            List<Product> products = new List<Product>();

            Console.WriteLine("Configurando Navegador");
            foreach (var category in cotoCategories)
            {
                products.AddRange(GetProducts(category));
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Coto_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            Helper.WriteColor($"Fin de la carga de datos. El archivo se encuentra en [{filePath}]", ConsoleColor.DarkBlue);

            return products;
        }

        public List<Product> GetProducts(Category category)
        {
            driver.Navigate().GoToUrl(category.url);
            Thread.Sleep(100);

            int totalProducts = 0;
            int attemps = 0;
            Console.WriteLine();
            Helper.WriteColor($"Buscando productos de la categoria [{category.name}]", ConsoleColor.White);
            Console.WriteLine();
            while (totalProducts == 0 && attemps < 3)
            {
                if (attemps > 0)
                    Thread.Sleep(500);

                var productos = driver.FindElement(By.Id("resultsCount")).Text;
                _ = int.TryParse(productos.Split(" ")[0].Trim(), out totalProducts);
                Console.WriteLine($"Se encontraron {totalProducts} productos para la categoria");

                if (totalProducts == 0)
                    Helper.WriteColor("[Reintentando...]", ConsoleColor.Red);

                attemps++;
            }

            return (GetProductsInfo(category, totalProducts));
        }

        private List<Product> GetProductsInfo(Category category, int productsCount)
        {
            List<Product> products = new List<Product>();
            int pageCount = (int)Math.Round(decimal.Divide(productsCount,12),0,MidpointRounding.ToPositiveInfinity);
            int actualPage = 0;
            while (actualPage < pageCount)
            {
                if (actualPage > 0)
                {
                    driver.Navigate().GoToUrl($"{category.url}?No={actualPage*12}");
                    Thread.Sleep(100);
                }
                Helper.PrintProgressBar($"Leyendo productos {actualPage+1}/{pageCount}", actualPage+1, pageCount);

                var productos = driver.FindElements(By.XPath(".//ul[@id='products']/li"));
                try
                {
                    foreach ( var item in productos ) 
                    {

                        string name = item.FindElement(By.XPath(".//span[@class='span_productName']/div")).Text;
                        string price = string.Empty;
                        try { price = item.FindElement(By.XPath(".//div/div/div/span/span[@class='atg_store_newPrice']")).Text; }
                        catch
                        {
                            try { price = item.FindElement(By.XPath(".//div[1]/div/div/div[3]/span")).Text; }
                            catch { price = item.FindElement(By.XPath(".//div[1]/div/div/div[2]/span")).Text; }
                        }
                        products.Add(new Product { superMarket = _superMarket, name = name, category = category.name, price = price });
                    }
                }
                catch (Exception ex) 
                {
                    Helper.WriteColor($"[{ex.Message}]",ConsoleColor.Black, ConsoleColor.Red);
                    continue; 
                }
                finally { actualPage++; }
            }
            Console.WriteLine();
            return products;
        }

        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
