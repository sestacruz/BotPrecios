using BotPrecios.Model;

internal interface IPostsService
{
    public void Initialize(string apiUrl);
    void PublishMontlyCBA(List<CBA> cbas);
    void PublishTop5CheapestCategory(List<CBA> cbas);
    void PublishTop5MostExpensiveCategory(List<CBA> cbas);
    void PublishTop5CheapestProduct(List<CBA> cbas);
    void PublishTop5MostExpensiveProduct(List<CBA> cbas);
    void PublishTopMonthCBAs(string cheap, string expensive);
}
