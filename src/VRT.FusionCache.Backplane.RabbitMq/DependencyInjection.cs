// Based on https://github.com/ZiggyCreatures/FusionCache/blob/main/src/ZiggyCreatures.FusionCache.Backplane.StackExchangeRedis/StackExchangeRedisBackplaneExtensions.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq;
public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqBackplane(this IServiceCollection services, Action<RabbitMqBackplaneOptions>? setupOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        services.AddOptions();

        if (setupOptionsAction is not null)
        {
            services.Configure(setupOptionsAction);
        }
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
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        return builder
            .WithBackplane(sp =>
            {
                var options = sp.GetService<IOptionsMonitor<RabbitMqBackplaneOptions>>()?.Get(builder.CacheName);

                if (options is null)
                {
                    throw new InvalidOperationException($"Unable to find a valid {nameof(RabbitMqBackplaneOptions)} instance for the current cache name '{builder.CacheName}'.");
                }


                setupOptionsAction?.Invoke(options);

                var logger = sp.GetService<ILogger<RabbitMqBackplane>>();

                return new RabbitMqBackplane(options, logger);
            })
        ;
    }
}
