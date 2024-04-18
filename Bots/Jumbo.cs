using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using BotPrecios.Interfaces;
using System.Text.RegularExpressions;
using BotPrecios.Helpers;


namespace BotPrecios.Bots
{
    public class Jumbo : IDisposable,IBot
    {
        private ChromeOptions _co;
        private IWebDriver driver;
        private const string _superMarket = Constants.Jumbo;

        public Jumbo() 
        {
            _co = new() { BrowserVersion = "123" };
            _co.AddArgument("--start-maximized");
            _co.AddArgument("--log-level=3");
            driver = new ChromeDriver(_co);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public List<Product> GetProductsData()
        {
            Utilities.WriteColor("Comenzando la lectura de los productos de la CBA de [Jumbo]",ConsoleColor.White, ConsoleColor.Green);
            Console.WriteLine("Leyendo categorias");
            List<Category> jumboCategories = Utilities.LoadJSONFile<Category>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\Jumbo.json"));
            List<Product> products = new List<Product>();

            Console.WriteLine("Configurando Navegador");
            foreach (var category in jumboCategories)
            {
                category.AddToDatabase("Jumbo");
                products.AddRange(GetProducts(category));
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Jumbo_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            Utilities.WriteColor($"Fin de la carga de datos. El archivo se encuentra en [{filePath}]", ConsoleColor.DarkBlue);

            return products;
        }

        public List<Product> GetProducts(Category category)
        {
            driver.Navigate().GoToUrl(category.url);

            Console.WriteLine();
            Utilities.WriteColor($"Buscando productos de la categoria [{category.name}]",ConsoleColor.White);
            Console.WriteLine();

            int totalPages = 0, attemps = 0, refreshes = 0;
            while (totalPages == 0 && attemps < 3 && refreshes < 2)
            {
                if (attemps > 0)
                    Thread.Sleep(500);

                totalPages = driver.FindElements(By.ClassName("discoargentina-search-result-custom-1-x-fetchMoreOptionItem")).Count;
                Console.WriteLine($"Se encontraron {totalPages} productos para la categoria");

                if (totalPages == 0)
                    Utilities.WriteColor("[Reintentando...]", ConsoleColor.Yellow);

                attemps++;
                if (attemps == 3)
                {
                    driver.Navigate().Refresh();
                    attemps = 0;
                    refreshes++;
                }
            }

            //driver.FindElements(By.ClassName("discoargentina-search-result-custom-1-x-fetchMoreOptionItem")).Count;

            return (GetPagesInfo(category, totalPages));
        }

        private List<Product> GetPagesInfo(Category category, int pageCount)
        {
            List<Product> products = new List<Product>();
            int actualPage = 1;
            while (actualPage <= pageCount)
            {
                Utilities.PrintProgressBar($"Leyendo pagina {actualPage}/{pageCount}", actualPage, pageCount);
                //Console.Write($"\rLeyendo pagina {actualPage}/{pageCount}");
                if (actualPage > 1)
                {
                    driver.Navigate().GoToUrl($"{category.url}?page={actualPage}");
                    Thread.Sleep(1000);
                }
                for (int i = 0; i < 4; i++) 
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollBy(0, window.innerHeight)", "");
                    Thread.Sleep(1500);
                }
                var productos = driver.FindElements(By.ClassName("vtex-search-result-3-x-galleryItem"));

                foreach (var product in productos)
                {
                    string price = Regex.Replace(product.FindElement(By.ClassName("jumboargentinaio-store-theme-1dCOMij_MzTzZOCohX1K7w")).Text, @"[^\d,]", "");
                    Product findedProduct = new Product
                    {
                        superMarket = _superMarket,
                        name = product.FindElement(By.ClassName("vtex-product-summary-2-x-productBrand")).Text,
                        category = category.name,
                        price = Convert.ToDecimal(price == string.Empty ? null : price)
                    };
                    products.Add(findedProduct);
                    findedProduct.AddToDataBase();
                }
                actualPage++;
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
