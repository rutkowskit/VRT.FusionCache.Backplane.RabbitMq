// Based on https://github.com/ZiggyCreatures/FusionCache/blob/main/src/ZiggyCreatures.FusionCache.Backplane.StackExchangeRedis/StackExchangeRedisBackplaneExtensions.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VRT.FusionCache.Backplane.RabbitMq.BusService;
using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq;
public static class DependencyInjection
{
    /// <summary>
    /// Adds a RabbitMq backplane to the service collection
    /// </summary>
    /// <param name="services">service collection</param>
    /// <param name="setupOptionsAction">Setup options action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddRabbitMqBackplane(this IServiceCollection services,
        Action<RabbitMqBackplaneOptions>? setupOptionsAction = null)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        services.AddOptions();

        if (setupOptionsAction is not null)
        {
            services.Configure(setupOptionsAction);
        }
        services.AddRabbitMqBusService();
        services.TryAddTransient<RabbitMqBackplane>();
        services.TryAddTransient<IFusionCacheBackplane, RabbitMqBackplane>();
        return services;
    }

    /// <summary>
	/// Adds a RabbitMq based implementation of a backplane to the <see cref="IFusionCacheBuilder" />.
	/// </summary>
	/// <param name="builder">The <see cref="IFusionCacheBuilder" /> to add the backplane to.</param>
	/// <param name="setupOptionsAction">The <see cref="Action{RabbitMqBackplaneOptions}"/> to configure the provided <see cref="RabbitMqBackplaneOptions"/>.</param>
	/// <returns>The <see cref="IFusionCacheBuilder"/> so that additional calls can be chained.</returns>
	public static IFusionCacheBuilder WithRabbitMqBackplane(this IFusionCacheBuilder builder, Action<RabbitMqBackplaneOptions>? setupOptionsAction = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder
            .WithBackplane(sp =>
            {
                var options = sp.GetService<IOptionsMonitor<RabbitMqBackplaneOptions>>()?.Get(builder.CacheName)
                    ?? throw new InvalidOperationException($"Unable to find a valid {nameof(RabbitMqBackplaneOptions)} instance for the current cache name '{builder.CacheName}'.");

                setupOptionsAction?.Invoke(options);

                var logger = sp.GetService<ILogger<RabbitMqBackplane>>();
                var busService = sp.GetRequiredService<IBusService>();

                return new RabbitMqBackplane(options, busService, logger);
            });
    }

    private static IServiceCollection AddRabbitMqBusService(this IServiceCollection services, Action<RabbitMqBackplaneOptions.RabbitMqOptions>? setupOptionsAction = null)
    {
        services.TryAddSingleton(CreateConnectionFactory);
        services.TryAddSingleton<IBusService, RabbitMqBackplaneMessageBusService>();
        services.TryAddSingleton<IBusSubscriberService>(p => p.GetRequiredService<IBusService>());
        services.TryAddSingleton<IBusPublisherService>(p => p.GetRequiredService<IBusService>());
        services.TryAddSingleton<IBusEventSubscriberService>(p => p.GetRequiredService<IBusService>());
        return services;
    }

    private static IConnectionFactory CreateConnectionFactory(IServiceProvider provider)
    {
        var options = provider.GetService<IOptions<RabbitMqBackplaneOptions>>()?.Value.RabbitMq
            ?? new();

        return new ConnectionFactory()
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            Port = options.Port,
            AutomaticRecoveryEnabled = options.AutomaticRecoveryEnabled,
            TopologyRecoveryEnabled = true
        };
    }
}
