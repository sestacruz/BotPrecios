using System.Text.RegularExpressions;

namespace BotPrecios.Model
{
    public class Product
    {
        private string _price;
        public string superMarket {  get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string price 
        { 
            get => _price;
            set => _price = Regex.Replace(value, @"[^\d,]", "").Trim();
        }

        public override string ToString()
        {
            return $"{superMarket};{name};{category};{price}";
        }
    }
}
