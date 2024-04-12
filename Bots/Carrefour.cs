using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using OpenQA.Selenium.Support.UI;
using System.Xml.Linq;
using OpenQA.Selenium.Interactions;


namespace BotPrecios.Bots
{
    public class Carrefour : IDisposable
    {
        private ChromeOptions _co;
        private IWebDriver driver;
        private bool cookiesAccepted = false;

        public Carrefour(ChromeOptions co) 
        {
            _co = co;
            driver = new ChromeDriver(_co);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public void GetProductsData()
        {
            Helper.WriteColor("Comenzando la lectura de los productos de la CBA de [Carrefour]", ConsoleColor.Blue);
            Console.WriteLine("Leyendo categorias");
            List<Categories> carrefourCategories = Helper.LoadJSONFile<Categories>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\Carrefour.json"));
            List<Product> products = new List<Product>();

            Console.WriteLine("Configurando Navegador");
            foreach (var category in carrefourCategories)
            {
                products.AddRange(GetProducts(category));
            }

            Dispose();

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Carrefour_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            Helper.WriteColor($"Fin de la carga de datos. El archivo se encuentra en [{filePath}]", ConsoleColor.DarkBlue);
        }

        public List<Product> GetProducts(Categories category)
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
            Helper.WriteColor($"Buscando productos de la categoria [{category.name}]", ConsoleColor.White);
            while (totalProducts == 0 && attemps < 3)
            {
                if (attemps > 0)
                    Thread.Sleep(500);

                var productos = driver.FindElement(By.ClassName("valtech-carrefourar-search-result-0-x-totalProducts--layout")).Text;
                _ = int.TryParse(productos.Split(" ")[0].Trim(), out totalProducts);
                Console.WriteLine($"Se encontraron {totalProducts} productos para la categoria");

                if (totalProducts == 0)
                    Helper.WriteColor("[Reintentando...]", ConsoleColor.Red);

                attemps++;
            }

            return (GetProductsInfo(category.url, totalProducts));
        }

        private List<Product> GetProductsInfo(string url, int productsCount)
        {
            List<Product> products = new List<Product>();
            int pageCount = (int)Math.Round(decimal.Divide(productsCount,16),0,MidpointRounding.ToPositiveInfinity);
            int actualPage = 1;
            while (actualPage <= pageCount)
            {
                if (actualPage > 1)
                {
                    driver.Navigate().GoToUrl($"{url}?page={actualPage}");
                    Thread.Sleep(2000);
                }
                
                Console.WriteLine($"Leyendo productos");
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
                    products.AddRange(productos.Select(x => new Product
                    {
                        name = x.FindElement(By.ClassName("vtex-product-summary-2-x-productBrand")).Text,
                        price = x.FindElement(By.ClassName("valtech-carrefourar-product-price-0-x-currencyContainer")).Text
                    }).ToList());
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
