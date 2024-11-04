using BotPrecios.Model;
using System.Net;

namespace BotPrecios.Services
{
    internal class PostsService : IPostsService
    {
        private string _message;
        private string _apiUrl;

        public void Initialize(string apiUrl)
        {
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new ArgumentException("La URL de la API no puede ser nula o vacía.", nameof(apiUrl));
            }

            _apiUrl = apiUrl;
        }

        public async void PublishMontlyCBA(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\CBAMonthCost.txt"));
            post = post.Replace("[month]", DateTime.Now.ToString("MMMM"));

            for (int i = 0; i < cbas.Count; i++)
            {
                string icon = SetSuperMarketIcon(cbas[i].SuperMarket);

                string variationIcon = "🟰";
                if (cbas[i].Variation > 0)
                    variationIcon = "🔺";
                else if (cbas[i].Variation < 0)
                    variationIcon = "🔻";

                post = post.Replace($"[smIcon{i + 1}]", icon);
                post = post.Replace($"[superMarket{i + 1}]", cbas[i].SuperMarket);
                post = post.Replace($"[smVar{i + 1}]", cbas[i].Variation.ToString("0.00") + "%");
                post = post.Replace($"[smDirectionIcon{i + 1}]", variationIcon);
            }
            _message = post;
            await SendPostRequest();
        }

        public async void PublishTop5CheapestCategory(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5CheapestCategory.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int categoryLength = 40;
                string icon = SetSuperMarketIcon(cbas[i].SuperMarket);
                post = post.Replace($"[smIcon{i + 1}]", icon);
                categoryLength -= icon.Length;
                post = post.Replace($"[varCategory{i + 1}]", cbas[i].Variation.ToString("0.00") + "%");
                categoryLength -= cbas[i].Variation.ToString("0.00").Length;
                post = post.Replace($"[superMarket{i + 1}]", cbas[i].SuperMarket);
                categoryLength -= cbas[i].SuperMarket.Length;
                string categoryName = cbas[i].Category.Length > categoryLength ? cbas[i].Category.Substring(0, categoryLength - 3) + "..." : cbas[i].Category;
                post = post.Replace($"[category{i + 1}]", categoryName);
            }
            _message = post;
            await SendPostRequest();
        }

        public async void PublishTop5MostExpensiveCategory(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5MostExpensiveCategory.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int categoryLength = 40;
                string icon = SetSuperMarketIcon(cbas[i].SuperMarket);
                post = post.Replace($"[smIcon{i + 1}]", icon);
                categoryLength -= icon.Length;
                post = post.Replace($"[varCategory{i + 1}]", cbas[i].Variation.ToString("0.00") + "%");
                categoryLength -= cbas[i].Variation.ToString("0.00").Length;
                post = post.Replace($"[superMarket{i + 1}]", cbas[i].SuperMarket);
                categoryLength -= cbas[i].SuperMarket.Length;
                string categoryName = cbas[i].Category.Length > categoryLength ? cbas[i].Category.Substring(0, categoryLength - 3) + "..." : cbas[i].Category;
                post = post.Replace($"[category{i + 1}]", categoryName);
            }
            _message = post;
            await SendPostRequest();
        }

        public async void PublishTop5CheapestProduct(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5CheapestProducts.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int productLength = 35;
                string icon = SetSuperMarketIcon(cbas[i].SuperMarket);
                post = post.Replace($"[smIcon{i + 1}]", icon);
                productLength -= icon.Length;
                post = post.Replace($"[varProduct{i + 1}]", cbas[i].Variation.ToString("0.00") + "%");
                productLength -= cbas[i].Variation.ToString("0.00").Length;
                post = post.Replace($"[superMarket{i + 1}]", cbas[i].SuperMarket);
                productLength -= cbas[i].SuperMarket.Length;
                string productName = cbas[i].Product.Length > productLength ? cbas[i].Product.Substring(0, productLength - 3) + "..." : cbas[i].Product;
                post = post.Replace($"[product{i + 1}]", productName);
            }
            _message = post;
            await SendPostRequest();
        }

        public async void PublishTop5MostExpensiveProduct(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5MostExpensiveProducts.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int productLength = 35;
                string icon = SetSuperMarketIcon(cbas[i].SuperMarket);
                post = post.Replace($"[smIcon{i + 1}]", icon);
                productLength -= icon.Length;
                post = post.Replace($"[varProduct{i + 1}]", cbas[i].Variation.ToString("0.00") + "%");
                productLength -= cbas[i].Variation.ToString("0.00").Length;
                post = post.Replace($"[superMarket{i + 1}]", cbas[i].SuperMarket);
                productLength -= cbas[i].SuperMarket.Length;
                string productName = cbas[i].Product.Length > productLength ? cbas[i].Product.Substring(0, productLength - 3) + "..." : cbas[i].Product;
                post = post.Replace($"[product{i + 1}]", productName);
            }
            _message = post;
            await SendPostRequest();
        }

        public async void PublishTopMonthCBAs(string cheap, string expensive)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\TopMonthCBAs.txt"));
            post = post.Replace("[cheapIcon]", SetSuperMarketIcon(cheap));
            post = post.Replace("[cheapSuperMarket]", cheap);
            post = post.Replace("[expensiveIcon]", SetSuperMarketIcon(expensive));
            post = post.Replace("[expensiveSuperMarket]", expensive);

            _message = post;
            await SendPostRequest();
        }

        private static string SetSuperMarketIcon(string superMarket)
        {
            return superMarket switch
            {
                Constants.Jumbo => "🟢",
                Constants.Coto => "🔴",
                Constants.Carrefour => "🔵",
                Constants.ChangoMas => "🟡",
                _ => string.Empty,
            };
        }

        private async Task<XResponse> SendPostRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_apiUrl);
            var jsonRQ = System.Text.Json.JsonSerializer.Serialize(new { text = _message });
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonRQ);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = bytes.Length;
            webRequest.Method = "POST";

            using Stream stream = await webRequest.GetRequestStreamAsync();
            stream.Write(bytes, 0, bytes.Length);

            using HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            using Stream responseStream = response.GetResponseStream();
            string responseString = new StreamReader(responseStream).ReadToEnd();
            return System.Text.Json.JsonSerializer.Deserialize<XResponse>(responseString)!;
        }
    }
}
