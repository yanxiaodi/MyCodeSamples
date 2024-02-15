using Microsoft.Extensions.DependencyInjection;

namespace WebApiClientDemo.Client;
public static class WebApiClientExtensions
{
    private const string BaseUrlNullOrEmptyErrorMessage = "Base Url can't be null or empty.";
    public static IServiceCollection AddWebApiClients(this IServiceCollection services, WebApiClientOptions options)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (options == null || string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new ArgumentNullException(nameof(options), BaseUrlNullOrEmptyErrorMessage);
        }

        services.AddHttpClient<IWeatherForecastClient, WeatherForecastClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddHttpClient<IProductsClient, ProductsClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}