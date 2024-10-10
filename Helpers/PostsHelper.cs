using BotPrecios.Bots;
using BotPrecios.Model;
using System.Net;
using System.Security.Policy;


namespace BotPrecios.Helpers
{
    public class PostsHelper
    {
        public PostsHelper(string url)
        {
            _url = url;
        }

        private string _message;
        private string _url;

        internal void PublishMontlyCBA(List<CBA> cbas)
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

                post = post.Replace($"[smIcon{i+1}]", icon);
                post = post.Replace($"[superMarket{i + 1}]", cbas[i].SuperMarket);
                post = post.Replace($"[smVar{i + 1}]", cbas[i].Variation.ToString("0.00") + "%");
                post = post.Replace($"[smDirectionIcon{i + 1}]", variationIcon);
            }
            _message = post;
            SendPostRequest();
        }

        internal void PublishTop5CheapestCategory(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5CheapestCategory.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int categoryLength = 40; //Se calculo en base al espacio disponible de la plantilla
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
            SendPostRequest();
        }

        internal void PublishTop5MostExpensiveCategory(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5MostExpensiveCategory.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int categoryLength = 40; //Se calculo en base al espacio disponible de la plantilla
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
            SendPostRequest();
        }

        internal void PublishTop5CheapestProduct(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5CheapestProducts.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int productLength = 35; //Se calculo en base al espacio disponible de la plantilla
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
            SendPostRequest();
        }

        internal void PublishTop5MostExpensiveProduct(List<CBA> cbas)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\Top5MostExpensiveProducts.txt"));

            for (int i = 0; i < cbas.Count; i++)
            {
                int productLength = 35; //Se calculo en base al espacio disponible de la plantilla
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
            SendPostRequest();
        }

        internal void PublisTopMonthCBAs(string cheap, string expensive)
        {
            string post = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Posts\\TopMonthCBAs.txt"));
            post = post.Replace("[cheapIcon]",SetSuperMarketIcon(cheap));
            post = post.Replace("[cheapSuperMarket]", cheap);
            post = post.Replace("[expensiveIcon]", SetSuperMarketIcon(expensive));
            post = post.Replace("[expensiveSuperMarket]", expensive);
            
            _message = post;
            SendPostRequest();
        }

        private string SetSuperMarketIcon(string superMarket)
        {
            string icon = string.Empty;
            switch (superMarket)
            {
                case Constants.Jumbo:
                    icon = "🟢";
                    break;
                case Constants.Coto:
                    icon = "🔴";
                    break;
                case Constants.Carrefour:
                    icon = "🔵";
                    break;
                case Constants.ChangoMas:
                    icon = "🟡";
                    break;
            }
            return icon;
        }

        private XResponse SendPostRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_url);
            var jsonRQ = System.Text.Json.JsonSerializer.Serialize(new { text = _message });
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonRQ);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = bytes.Length;
            webRequest.Method = "POST";

            Stream stream = webRequest.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)webRequest.GetResponse();
            Stream responseStream = response.GetResponseStream();
            string responseString = new StreamReader(responseStream).ReadToEnd();
            return System.Text.Json.JsonSerializer.Deserialize<XResponse>(responseString);
        }

    }
}
