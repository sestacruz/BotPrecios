using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using BotPrecios.Interfaces;
using System.Text.RegularExpressions;
using BotPrecios.Helpers;


namespace BotPrecios.Bots
{
    internal class ChangoMas : IDisposable, IBot
    {
        private ChromeOptions _co;
        private IWebDriver driver;
        private const string _superMarket = Constants.ChangoMas;
        private readonly ILogHelper _log;

        public ChangoMas(ILogHelper log)
        {
            _co = new() { BrowserVersion = "123" };
            _co.AddArgument("--start-maximized");
            _co.AddArgument("--log-level=3");
            driver = new ChromeDriver(_co);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            _log = log;
        }

        public List<Product> GetProductsData()
        {
            _log.ConsoleLog($"({_superMarket})Comenzando la lectura de los productos de la CBA de [ChangoMas]",foreColor: ConsoleColor.Yellow);
            _log.ConsoleLog($"({_superMarket})Leyendo categorias");
            List<Category> changoCategories = Utilities.LoadJSONFile<Category>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\ChangoMas.json"));
            List<Product> products = new List<Product>();

            _log.ConsoleLog($"({_superMarket})({_superMarket})Configurando Navegador");
            foreach (var category in changoCategories)
            {
                category.AddToDatabase("ChangoMas");
                products.AddRange(GetProducts(category));
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ChangoMas_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            _log.ConsoleLog($"({_superMarket})Fin de la carga de datos. El archivo se encuentra en [{filePath}]", foreColor: ConsoleColor.DarkBlue);

            return products;
        }

        public List<Product> GetProducts(Category category)
        {
            driver.Navigate().GoToUrl(category.url);

            _log.ConsoleLog($"({_superMarket})Buscando productos de la categoria [{category.name}]", foreColor: ConsoleColor.White);

            int attemps = 0;
            int totalProducts = 0;
            
            while (totalProducts == 0 && attemps < 3)
            {
                Thread.Sleep(1000*attemps);

                var productos = driver.FindElement(By.ClassName("vtex-search-result-3-x-totalProducts--layout")).Text;
                _ = int.TryParse(productos.Split(" ")[0].Trim(), out totalProducts);
                _log.ConsoleLog($"({_superMarket})Se encontraron [{totalProducts}] productos para la categoria",foreColor:totalProducts == 0? ConsoleColor.Red : ConsoleColor.White);

                if (totalProducts == 0)
                    _log.ConsoleLog($"({_superMarket})[Reintentando...]", foreColor: ConsoleColor.DarkYellow);

                attemps++;
            }

            return (GetProductsInfo(category, totalProducts));
        }

        private List<Product> GetProductsInfo(Category category, int productsCount)
        {
            List<Product> products = new List<Product>();
            int pageCount = (int)Math.Round(decimal.Divide(productsCount,24),0,MidpointRounding.ToPositiveInfinity);
            int actualPage = 1;
            while (actualPage <= pageCount)
            {
                if (actualPage > 1)
                {
                    driver.Navigate().GoToUrl($"{category.url}?page={actualPage}");
                    Thread.Sleep(1000);
                }

                _log.ConsoleLog($"({_superMarket})Leyendo pagina {actualPage}/{pageCount}");

                int cicles = 0;
                while (cicles < 4)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollBy(0, window.innerHeight)", "");
                    Thread.Sleep(750);
                    cicles++;
                }

                var productos = driver.FindElements(By.ClassName("vtex-search-result-3-x-galleryItem"));
                var findedProducts = productos.Select(x => new Product
                {
                    superMarket = _superMarket,
                    name = x.FindElement(By.ClassName("vtex-product-summary-2-x-productBrand")).Text,
                    category = category.name,
                    price = Convert.ToDecimal(Regex.Replace(x.FindElement(By.ClassName("valtech-gdn-dynamic-product-0-x-dynamicProductPrice")).Text, @"[^\d,]", ""))
                }).ToList();
                products.AddRange(findedProducts);
                Product.AddAllToDataBase(products);
                actualPage++;
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
