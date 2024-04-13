using BotPrecios.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text;


namespace BotPrecios.Bots
{
    public class Jumbo : IDisposable
    {
        private ChromeOptions _co;
        private IWebDriver driver;

        public Jumbo(ChromeOptions co) 
        {
            _co = co;
            driver = new ChromeDriver(_co);
        }

        public void GetProductsData()
        {
            Helper.WriteColor("Comenzando la lectura de los productos de la CBA de [Jumbo]", ConsoleColor.Green);
            Console.WriteLine("Leyendo categorias");
            List<Category> jumboCategories = Helper.LoadJSONFile<Category>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories\\Jumbo.json"));
            List<Product> products = new List<Product>();

            Console.WriteLine("Configurando Navegador");
            foreach (var category in jumboCategories)
            {
                products.AddRange(GetProducts(category));
            }

            Dispose();

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Jumbo_{DateTime.Now:yyyyMMdd}.csv");
            File.WriteAllLines(filePath, products.Select(x => x.ToString()), Encoding.UTF8);
            Helper.WriteColor($"Fin de la carga de datos. El archivo se encuentra en [{filePath}]", ConsoleColor.DarkBlue);
        }

        public List<Product> GetProducts(Category category)
        {
            driver.Navigate().GoToUrl(category.url);
            Thread.Sleep(2000);

            Helper.WriteColor($"Buscando productos de la categoria [{category.name}]",ConsoleColor.White);
            int totalPages = driver.FindElements(By.ClassName("discoargentina-search-result-custom-1-x-fetchMoreOptionItem")).Count;

            return(GetPagesInfo(category, totalPages));
        }

        private List<Product> GetPagesInfo(Category category, int pageCount)
        {
            List<Product> products = new List<Product>();
            Console.WriteLine($"Leyendo pagina 1");
            for (int i = 2; i <= pageCount+1; i++)
            {
                int cicles = 0;
                while (cicles < 4)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollBy(0, window.innerHeight)","");
                    Thread.Sleep(1000);
                    cicles++;
                }
                var productos = driver.FindElements(By.ClassName("vtex-search-result-3-x-galleryItem"));
                products.AddRange(productos.Select(x => new Product 
                                                        { 
                                                            name = x.FindElement(By.ClassName("vtex-product-summary-2-x-productBrand")).Text, 
                                                            category = category.name,
                                                            price = x.FindElement(By.ClassName("jumboargentinaio-store-theme-1dCOMij_MzTzZOCohX1K7w")).Text 
                                                        }).ToList());
                driver.Navigate().GoToUrl($"{category.url}?page={i}");
                Console.WriteLine($"Leyendo pagina {i}");
                Thread.Sleep(1000);
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
