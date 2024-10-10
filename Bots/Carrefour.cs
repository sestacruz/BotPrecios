using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using BotPrecios.Interfaces;
using System.Text.RegularExpressions;
using BotPrecios.Helpers;


namespace BotPrecios.Bots
{
    public class Carrefour : IDisposable,IBot
    {
        private ChromeOptions _co;
        private IWebDriver driver;
        private bool cookiesAccepted = false;
        private const string _superMarket = Constants.Carrefour;
        private readonly ILogHelper _log;
        private string _lastCategory;

        internal Carrefour(ILogHelper log, string chromeVersion, string lastCategory = null)
        {
            _co = new() { BrowserVersion = chromeVersion };
            _co.AddArgument("--start-maximized");
            _co.AddArgument("--log-level=3");
            driver = new ChromeDriver(_co);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            _log = log;
            _lastCategory = lastCategory;
        }

        public async Task<List<Product>> GetProductsData()
        {
            if (string.IsNullOrEmpty(_lastCategory))
            {
                _log.ConsoleLog($"Eliminando productos de ({_superMarket}) para el día {DateTime.Now.ToString(Constants.dateFormat)}", foreColor: ConsoleColor.Blue);
                await Product.CleanProducts(_superMarket, DateTime.Now);
            }
            _log.ConsoleLog($"({_superMarket})Comenzando la lectura de los productos de la CBA de [Carrefour]", foreColor:ConsoleColor.Blue);
            _log.ConsoleLog($"({_superMarket})Leyendo categorias");
            List<Category> carrefourCategories = Utilities.LoadJSONFile<Category>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\Carrefour.json"));
            List<Product> products = [];

            _log.ConsoleLog($"({_superMarket})Configurando Navegador");
            foreach (var category in carrefourCategories)
            {
                if (!string.IsNullOrEmpty(_lastCategory) && category.name != _lastCategory)
                    continue;
                else
                {
                    _lastCategory = null;
                    category.AddToDatabase("Carrefour");
                    products.AddRange(await GetProducts(category));
                }
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"data-export\\{DateTime.Now:MMMM}");
            if (!Directory.Exists(filePath)) 
                Directory.CreateDirectory(filePath);
            filePath = Path.Combine(filePath, $"{_superMarket}_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            _log.ConsoleLog($"({_superMarket})Fin de la carga de datos. El archivo se encuentra en [{filePath}]", foreColor: ConsoleColor.DarkBlue);

            return products;
        }

        private Task<List<Product>> GetProducts(Category category)
        {
            driver.Navigate().GoToUrl(category.url);
            Thread.Sleep(500);
            if (!cookiesAccepted)
            {
                driver.FindElement(By.Id("onetrust-accept-btn-handler")).Click();
                cookiesAccepted = true;
                Thread.Sleep(500);
            }

            int totalProducts = 0;
            int attemps = 0;
            _log.ConsoleLog($"({_superMarket})Buscando productos de la categoria [{category.name}]", foreColor: ConsoleColor.White);
            while (totalProducts == 0 && attemps < 3)
            {
                if (attemps > 0)
                    Thread.Sleep(500);

                var productos = driver.FindElement(By.ClassName("valtech-carrefourar-search-result-2-x-totalProducts--layout")).Text;
                _ = int.TryParse(productos.Split(" ")[0].Trim(), out totalProducts);
                _log.ConsoleLog($"({_superMarket})Se encontraron [{totalProducts}] productos para la categoria", foreColor: totalProducts == 0 ? ConsoleColor.Red : ConsoleColor.White);

                if (totalProducts == 0)
                    _log.ConsoleLog($"({_superMarket})[Reintentando...]", foreColor: ConsoleColor.DarkYellow);

                attemps++;
            }

            return (GetProductsInfo(category, totalProducts));
        }

        private async Task<List<Product>> GetProductsInfo(Category category, int productsCount)
        {
            List<Product> products = new List<Product>();
            int pageCount = (int)Math.Round(decimal.Divide(productsCount,16),0,MidpointRounding.ToPositiveInfinity);
            int actualPage = 1;
            while (actualPage <= pageCount)
            {
                try
                {
                    if (actualPage > 1)
                    {
                        driver.Navigate().GoToUrl($"{category.url}?page={actualPage}");
                        Thread.Sleep(2000);
                    }
                    _log.ConsoleLog($"({_superMarket})Leyendo pagina {actualPage}/{pageCount}");

                    int cicles = 0;
                    while (cicles < 2)
                    {
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("window.scrollBy(0, window.innerHeight)", "");
                        Thread.Sleep(300);
                        cicles++;
                    }
                    Thread.Sleep(1500);
                    var productos = driver.FindElements(By.ClassName("valtech-carrefourar-search-result-0-x-galleryItem"));
                    try
                    {
                        var findedProducts = productos.Select(x => new Product
                        {
                            superMarket = _superMarket,
                            name = x.FindElement(By.ClassName("vtex-product-summary-2-x-productBrand")).Text,
                            category = category.name,
                            price = Convert.ToDecimal(Regex.Replace(x.FindElement(By.ClassName("valtech-carrefourar-product-price-0-x-currencyContainer")).Text, @"[^\d,]", ""))
                        }).ToList();
                        products.AddRange(findedProducts);
                        Product.AddAllToDataBase(findedProducts);
                    }
                    catch { continue; }
                    finally { actualPage++; }
                }
                catch { continue; }
                finally { actualPage++; }
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
