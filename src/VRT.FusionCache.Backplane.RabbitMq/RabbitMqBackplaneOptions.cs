using Microsoft.Extensions.Options;

namespace VRT.FusionCache.Backplane.RabbitMq;

/// <summary>
/// RabbitMQ backplane options.
/// </summary>
public sealed class RabbitMqBackplaneOptions : IOptions<RabbitMqBackplaneOptions>
{
    /// <summary>
    /// RabbitMQ fanout exchange name.
    /// </summary>
    public string ExchangeName { get; set; } = "vrt.fusioncache.events";

    /// <summary>
    /// The configuration used to connect to RabbitMq.
    /// </summary>
    public RabbitMqOptions RabbitMq { get; set; } = new();

    RabbitMqBackplaneOptions IOptions<RabbitMqBackplaneOptions>.Value => this;

    public sealed class RabbitMqOptions
    {
        /// <summary>
        /// RabbitMQ host name or IP address.
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// RabbitMQ port number.
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// RabbitMQ user name.
        /// </summary>
        public string UserName { get; set; } = "guest";
        /// <summary>
        /// RabbitMQ password.
        /// </summary>
        public string Password { get; set; } = "guest";
        /// <summary>
        /// Gets a value indicating whether automatic recovery is enabled.
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; } = true;
    }
}
