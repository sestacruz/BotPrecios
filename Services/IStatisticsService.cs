using BotPrecios.Logging;
using BotPrecios.Model;

internal interface IStatisticsService
{
    List<CBA> GetCBAStatistics(ILogService logger);
    void GetMostsCBAs(ILogService logger, List<CBA> cbas, out string mostExpensive, out string cheapest);
    void GetTop5Products(ILogService logger, out List<CBA> top5positive, out List<CBA> top5negative, out List<CBA> categoriesVariation);
    void GetTop5Categories(ILogService logger, List<CBA> categoriesVariation, out List<CBA> top5positive, out List<CBA> top5negative);
}
