using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using BotPrecios.Interfaces;
using System.Text.RegularExpressions;


namespace BotPrecios.Bots
{
    public class ChangoMas : IDisposable, IBot
    {
        private ChromeOptions _co;
        private IWebDriver driver;
        private const string _superMarket = Constants.ChangoMas;

        public ChangoMas() 
        {
            _co = new() { BrowserVersion = "123" };
            _co.AddArgument("--start-maximized");
            _co.AddArgument("--log-level=3");
            driver = new ChromeDriver(_co);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public List<Product> GetProductsData()
        {
            Helper.WriteColor("Comenzando la lectura de los productos de la CBA de [ChangoMas]", ConsoleColor.Yellow);
            Console.WriteLine("Leyendo categorias");
            List<Category> changoCategories = Helper.LoadJSONFile<Category>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\ChangoMas.json"));
            List<Product> products = new List<Product>();

            Console.WriteLine("Configurando Navegador");
            foreach (var category in changoCategories)
            {
                category.AddToDatabase("ChangoMas");
                products.AddRange(GetProducts(category));
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ChangoMas_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            Helper.WriteColor($"Fin de la carga de datos. El archivo se encuentra en [{filePath}]", ConsoleColor.DarkBlue);

            return products;
        }

        public List<Product> GetProducts(Category category)
        {
            driver.Navigate().GoToUrl(category.url);

            Console.WriteLine();
            Helper.WriteColor($"Buscando productos de la categoria [{category.name}]",ConsoleColor.White);
            var productos = driver.FindElement(By.ClassName("vtex-search-result-3-x-totalProducts--layout")).Text;
            _ = int.TryParse(productos.Split(" ")[0].Trim(), out int totalProducts);
            Console.WriteLine($"Se encontraron {totalProducts} productos para la categoria");
            Console.WriteLine();

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

                Helper.PrintProgressBar($"Leyendo pagina {actualPage}/{pageCount}", actualPage, pageCount);

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
                findedProducts.ForEach(x => x.AddToDataBase());
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
