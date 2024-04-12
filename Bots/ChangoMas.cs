using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;
using OpenQA.Selenium.Support.UI;
using System.Xml.Linq;
using OpenQA.Selenium.Interactions;


namespace BotPrecios.Bots
{
    public class ChangoMas : IDisposable
    {
        private ChromeOptions _co;
        private IWebDriver driver;

        public ChangoMas(ChromeOptions co) 
        {
            _co = co;
            driver = new ChromeDriver(_co);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public void GetProductsData()
        {
            Helper.WriteColor("Comenzando la lectura de los productos de la CBA de [ChangoMas]", ConsoleColor.Yellow);
            Console.WriteLine("Leyendo categorias");
            List<Categories> changoCategories = Helper.LoadJSONFile<Categories>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\ChangoMas.json"));
            List<Product> products = new List<Product>();

            Console.WriteLine("Configurando Navegador");
            foreach (var category in changoCategories)
            {
                products.AddRange(GetProducts(category));
            }

            Dispose();

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ChangoMas_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            Helper.WriteColor($"Fin de la carga de datos. El archivo se encuentra en [{filePath}]", ConsoleColor.DarkBlue);
        }

        public List<Product> GetProducts(Categories category)
        {
            driver.Navigate().GoToUrl(category.url);

            Helper.WriteColor($"Buscando productos de la categoria [{category.name}]",ConsoleColor.White);
            var productos = driver.FindElement(By.ClassName("vtex-search-result-3-x-totalProducts--layout")).Text;
            _ = int.TryParse(productos.Split(" ")[0].Trim(), out int totalProducts);
            Console.WriteLine($"Se encontraron {totalProducts} productos para la categoria");

            return (GetProductsInfo(category.url, totalProducts));
        }

        private List<Product> GetProductsInfo(string url, int productsCount)
        {
            List<Product> products = new List<Product>();
            int pageCount = (int)Math.Round(decimal.Divide(productsCount,24),0,MidpointRounding.ToPositiveInfinity);
            int actualPage = 1;
            while (actualPage <= pageCount)
            {
                if (actualPage > 1)
                {
                    driver.Navigate().GoToUrl($"{url}?page={actualPage}");
                    Thread.Sleep(1000);
                }
                
                Console.WriteLine($"Leyendo productos");
                int cicles = 0;
                while (cicles < 4)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollBy(0, window.innerHeight)", "");
                    Thread.Sleep(750);
                    cicles++;
                }

                var productos = driver.FindElements(By.ClassName("vtex-search-result-3-x-galleryItem"));
                products.AddRange(productos.Select(x => new Product 
                                                        { 
                                                            name = x.FindElement(By.ClassName("vtex-product-summary-2-x-productBrand")).Text, 
                                                            price = x.FindElement(By.ClassName("valtech-gdn-dynamic-product-0-x-dynamicProductPrice")).Text 
                                                        }).ToList());
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
