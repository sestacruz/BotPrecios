using BotPrecios.Bots;
using BotPrecios.Logging;
using BotPrecios.Model;
using Microsoft.Extensions.Configuration;

namespace BotPrecios.Services
{
    internal class BotService(IEnumerable<IBot> bots, ILogService logger, IPostsService postsHelper,
                      IStatisticsService statisticsHelper, IConfiguration config) : IBotService
    {
        private readonly IEnumerable<IBot> _bots = bots;
        private readonly ILogService _logger = logger;
        private readonly IPostsService _postsHelper = postsHelper;
        private readonly IStatisticsService _statisticsHelper = statisticsHelper;
        private readonly IConfiguration _config = config;

        public async Task RunAsync(string[] args)
        {
            string option = args.Length > 0 ? args[0] : string.Empty;
            string lastCategory = args.Length > 1 ? args[1] : string.Empty;

            var tasks = _bots.Select(bot => ProcessBotAsync(bot, option, lastCategory)).ToList();
            var products = await Task.WhenAll(tasks);

            _logger.ConsoleLog("Fin de la obtención de datos");

            // Código para generar estadísticas
            if (string.IsNullOrEmpty(option) || option == "statistics")
                await GenerateStatistics(products.SelectMany(p => p).ToList());
        }

        private async Task<List<Product>> ProcessBotAsync(IBot bot, string option, string lastCategory)
        {
            await _logger.ConsoleLog($"Iniciando bot {bot.GetType().Name}");
            var products = await bot.GetProductsData();
            bot.Dispose();
            return products;
        }

        private async Task GenerateStatistics(List<Product> products)
        {
            // Obtén las estadísticas CBA
            List<CBA> CBAs = _statisticsHelper.GetCBAStatistics(_logger);
            _logger.ConsoleLog(" ");

            // Obtén los productos y categorías más relevantes
            _statisticsHelper.GetTop5Products(_logger, out List<CBA> topPositiveProd, out List<CBA> topNegativeProd, out List<CBA> categoriesVariation);
            _logger.ConsoleLog(" ");

            _statisticsHelper.GetTop5Categories(_logger, categoriesVariation, out List<CBA> topPositiveCat, out List<CBA> topNegativeCat);
            _logger.ConsoleLog(" ");

            _statisticsHelper.GetMostsCBAs(_logger, CBAs, out string expensive, out string cheapest);

            bool debug = false;
#if DEBUG
            debug = true;
#endif


            // Publicar en X si no está en modo debug
            if (!debug)
            {
                _logger.ConsoleLog("Posteando en X");
                string apiUrl = _config["ApiURL"];
                _postsHelper.Initialize(apiUrl);

                _postsHelper.PublishMontlyCBA(CBAs);
                _postsHelper.PublishTop5CheapestCategory(topPositiveCat);
                _postsHelper.PublishTop5MostExpensiveCategory(topNegativeCat);
                _postsHelper.PublishTop5CheapestProduct(topPositiveProd);
                _postsHelper.PublishTop5MostExpensiveProduct(topNegativeProd);

                DateTime today = DateTime.Now;
                DateTime lastDayOfMonth = new(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                if (today.Date == lastDayOfMonth.Date)
                    _postsHelper.PublishTopMonthCBAs(expensive, cheapest);

                _logger.ConsoleLog("Posteos terminados");
            }
        }
    }
}