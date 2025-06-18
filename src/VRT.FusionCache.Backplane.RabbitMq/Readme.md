# FusionCache Backplane for RabbitMQ

## FusionCache 
![FusionCache logo](https://raw.githubusercontent.com/ZiggyCreatures/FusionCache/main/docs/logo-256x256.png)


FusionCache is an easy to use, fast and robust hybrid cache with advanced resiliency features.
Find out [more](https://github.com/ZiggyCreatures/FusionCache).

## 📦 This package

This package is a backplane implementation on [RabbitMq](http://rabbitmq.com/) 
based on the [RabbitMQ.Client](https://github.com/rabbitmq/rabbitmq-dotnet-client) library.

## 🛠️ Configuration
To use this backplane, you need to register it in your `IServiceCollection`

### Binding options from configuration

Example of binding options from configuration:

```csharp
services.AddRabbitMqBackplane(configuration.GetSection("RabbitMqBackplane").Bind)
```

### Manual configuration

```csharp
result.AddRabbitMqBackplane(options =>
{
    options.ExchangeName = "FusionCacheBackplane.events";
    options.RabbitMq.HostName = "localhost";
    options.RabbitMq.UserName = "admin";
    options.RabbitMq.Password = "some_secret_password";
});
```

### Fusion configuration

After registering the backplane, you can use it with FusionCache like this:

```csharp
result
    .AddFusionCache()
    .WithRegisteredDistributedCache()
    .WithRegisteredBackplane()
    .WithSystemTextJsonSerializer();
```

:bulb: Be aware that the backplane will not be used if you do not register distributed cache.

## 📝 License
This project is licensed under the [MIT License](https://github.com/rutkowskit/VRT.FusionCache.Backplane.RabbitMq/blob/master/LICENSE.txt)


## 📜 Version History

### v1.0.1 - Initial Version
