using Blazored.LocalStorage;
using ComicsBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using SoloX.BlazorJsBlob;

namespace ComicsBlazor;

/// <summary>
/// Extension methods to setup the ComicsBlazor services.
/// </summary>
public static partial class ServiceCollectionExtensions
{

    /// <summary>
    /// Add ComicsBlazor services.
    /// </summary>
    /// <param name="services">The service collection to setup.</param>
    /// <param name="serviceLifetime">Service Lifetime to use to register the Services. (Default is Scoped)</param>
    /// <returns>The given service collection updated with the ComicsBlazor services.</returns>
    public static IServiceCollection AddComicsBlazor(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        return services.AddComicsBlazor(_ => { }, serviceLifetime);
    }

    /// <summary>
    /// Add ComicsBlazor services.
    /// </summary>
    /// <param name="services">The service collection to setup.</param>
    /// <param name="optionsBuilder">Options builder action delegate.</param>
    /// <param name="serviceLifetime">Service Lifetime to use to register the Services. (Default is Scoped)</param>
    /// <returns>The given service collection updated with the ComicsBlazor services.</returns>
    public static IServiceCollection AddComicsBlazor(this IServiceCollection services, Action<ComicsBlazorOptions> optionsBuilder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        services.AddJsBlob(options =>
        {
            options.EnableJsModuleLogs = false;
        }, serviceLifetime);
        // services.AddHttpClient();
        // services.AddSingleton<IBufferService, BufferService>();
        // services.AddScoped<EPubJsInterop>();

        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<BlobManagerService>();
                services.AddSingleton<ComicService>();
                services.AddSingleton<ComicsJsInterop>();
                services.AddBlazoredLocalStorageAsSingleton();
                // services.AddSingleton<EPubNavigationService>();
                // services.AddSingleton<EPubSettingsService>();
                // services.AddSingleton<EPubThemeService>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<BlobManagerService>();
                services.AddScoped<ComicService>();
                services.AddScoped<ComicsJsInterop>();
                services.AddBlazoredLocalStorage();

                // services.AddScoped<EPubNavigationService>();
                // services.AddScoped<EPubSettingsService>();
                // services.AddScoped<EPubThemeService>();
                break;
            case ServiceLifetime.Transient:
            default:
                services.AddTransient<BlobManagerService>();
                services.AddTransient<ComicService>();
                services.AddTransient<ComicsJsInterop>();
                services.AddBlazoredLocalStorage();

                // services.AddTransient<EPubNavigationService>();
                // services.AddTransient<EPubSettingsService>();
                // services.AddTransient<EPubThemeService>();
                break;
        }

        services.Configure(optionsBuilder);

        return services;
    }
}