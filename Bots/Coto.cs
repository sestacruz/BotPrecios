using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using BotPrecios.Interfaces;
using System.Text.RegularExpressions;
using BotPrecios.Helpers;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.BiDi.Modules.Log;


namespace BotPrecios.Bots
{
    public class Coto : IDisposable, IBot
    {
        private ChromeOptions _co;
        private IWebDriver driver;
        private const string _superMarket = Constants.Coto;
        private readonly ILogHelper _log;
        private string _lastCategory;

        internal Coto(ILogHelper log, string chromeVersion,string lastCategory = null)
        {
            _co = new() { BrowserVersion = chromeVersion };
            _co.AddArgument("--start-maximized");
            _co.AddArgument("--log-level=3");
            driver = new ChromeDriver(_co);
            _log = log;
            _lastCategory = lastCategory;
        }

        public async Task<List<Product>> GetProductsData()
        {
            if (string.IsNullOrEmpty(_lastCategory))
            {
                _log.ConsoleLog($"Eliminando productos de ({_superMarket}) para el día {DateTime.Now.ToString(Constants.dateFormat)}", foreColor: ConsoleColor.White, backColor: ConsoleColor.Red);
                await Product.CleanProducts(_superMarket, DateTime.Now);
            }
            _log.ConsoleLog($"({_superMarket})Comenzando la lectura de los productos de la CBA de [COTO]", foreColor: ConsoleColor.White, backColor: ConsoleColor.Red);
            _log.ConsoleLog($"({_superMarket})Leyendo categorias");
            List<Category> cotoCategories = Utilities.LoadJSONFile<Category>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\Coto.json"));
            List<Product> products = [];

            _log.ConsoleLog($"({_superMarket})Configurando Navegador");
            foreach (var category in cotoCategories)
            {
                if (!string.IsNullOrEmpty(_lastCategory) && category.name != _lastCategory)
                    continue;
                else
                {
                    _lastCategory = null;
                    category.AddToDatabase("Coto");
                    products.AddRange(await GetProducts(category));
                }
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"data-export\\{DateTime.Now:MMMM}");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            filePath = Path.Combine(filePath, $"{_superMarket}_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            _log.ConsoleLog($"({_superMarket}) Fin de la carga de datos. El archivo se encuentra en [{filePath}]", foreColor: ConsoleColor.DarkBlue);

            return products;
        }

        public Task<List<Product>> GetProducts(Category category)
        {
            driver.Navigate().GoToUrl(category.url);
            Thread.Sleep(500);

            int totalProducts = 0;
            int attemps = 0;
            _log.ConsoleLog($"Buscando productos de la categoria [{category.name}]", foreColor: ConsoleColor.White);
            while (totalProducts == 0 && attemps < 3)
            {
                if (attemps > 0)
                    Thread.Sleep(500);

                var productos = driver.FindElement(By.XPath(".//header/strong[contains(text(), 'productos encontrados')]")).Text;
                _ = int.TryParse(productos.Split(" ")[0].Trim(), out totalProducts);
                _log.ConsoleLog($"({_superMarket})Se encontraron [{totalProducts}] productos para la categoria", foreColor: totalProducts == 0 ? ConsoleColor.Red : ConsoleColor.White);

                if (totalProducts == 0)
                    _log.ConsoleLog($"({_superMarket})[Reintentando...]", foreColor:ConsoleColor.DarkYellow);

                attemps++;
            }

            return (GetProductsInfo(category, totalProducts));
        }

        private async Task<List<Product>> GetProductsInfo(Category category, int productsCount)
        {
            List<Product> products = [];
            int pageCount = (int)Math.Round(decimal.Divide(productsCount,12),0,MidpointRounding.ToPositiveInfinity);
            int actualPage = 1;
            while (actualPage < pageCount)
            {
                _log.ConsoleLog($"({_superMarket})Leyendo productos {actualPage}/{pageCount}");
                _log.ConsoleLog("Busca los elementos de productos", "DEBUG");
                var productos = driver.FindElements(By.TagName("catalogue-product"));
                try
                {
                    _log.ConsoleLog("Itera los productos", "DEBUG");
                    foreach ( var item in productos ) 
                    {
                        _log.ConsoleLog("|-Busca nombre producto", "DEBUG");
                        string name = item.FindElement(By.ClassName("nombre-producto")).Text;
                        _log.ConsoleLog($"|-Nombre: {name}", "DEBUG");
                        _log.ConsoleLog("|-Busca precio producto", "DEBUG");
                        string price = item.FindElement(By.ClassName("card-title")).Text;
                        _log.ConsoleLog($"|-Precio: {price}", "DEBUG");
                        //try { price = item.FindElement(By.ClassName("card-title")).Text; }
                        //catch
                        //{
                        //    try { price = item.FindElement(By.XPath(".//div[1]/div/div/div[3]/span")).Text; }
                        //    catch 
                        //    {
                        //        try 
                        //        { 
                        //            price = item.FindElement(By.XPath(".//div[1]/div/div/div[2]/span")).Text;
                        //            if (price == "OFERTA")
                        //                price = item.FindElement(By.XPath(".//span[@class='price_discount']")).Text;
                        //        }
                        //        catch { } //Producto no disponible
                        //    }
                        //}
                        price = Regex.Replace(price, @"[^\d.,]", "");
                        price = price.Replace(".", "").Replace(',', '.');

                        Product actual = new Product { superMarket = _superMarket, name = name, category = category.name, price = Convert.ToDecimal(price) };
                        _log.ConsoleLog("|-Guarda en base de datos", "DEBUG");
                        actual.AddToDataBase();
                        _log.ConsoleLog("|-Agrega en lista", "DEBUG");
                        products.Add(actual);
                    }
                }
                catch (Exception ex) 
                {
                    _log.ConsoleLog($"({_superMarket}) {ex.Message}",Constants.ErrorLevel.Error);
                    continue; 
                }
                finally 
                {
                    try
                    {
                        IWebElement element = driver.FindElement(By.XPath(".//a[text()='Siguiente']"));
                        _log.ConsoleLog("Cambia pagina", "DEBUG");
                        Actions actions = new(driver);
                        actions.Click(element).Perform();
                        Thread.Sleep(500);
                        actualPage++;
                    }
                    catch
                    {
                        _log.ConsoleLog($"({_superMarket}) No se encontró el botón Siguiente", Constants.ErrorLevel.Warning);
                        actualPage = pageCount;                       
                    }
                }
            }
            return products;
        }

        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
