# FusionCache Backplane for RabbitMQ project

## Overview

This project is a `FusionCache` `RabbitMQ` backplane implementation.

## Versioning

This project follows [Semantic Versioning](https://semver.org/). The version number is in the format `MAJOR.MINOR.PATCH`.
The version number is updated automatically during the build process using the `MinVer` library.

## Releasing new version

To release a new version, follow these steps:

1. Add new tag to the repository with the new version number, e.g. `git tag v1.0.1`.
1. Push the tag to the remote repository: `git push --tags`.
1. The build pipeline will automatically build the project on GitHub with the new version number and update the NuGet package.
1. The new version will be available on NuGet.org shortly after the build is completed.


## Integration Tests

### Running Integration Tests
To run integration tests, you need to have RabbitMQ server running locally.
You can use Docker to run RabbitMQ. More information can be found in the [RabbitMQ documentation](https://hub.docker.com/_/rabbitmq).


To run the integration tests:
1. Open a terminal and navigate to the project directory (`tests\VRT.FusionCache.Backplane.RabbitMq.Tests.Integration`)
1. Execute the following command in the terminal:
	```bash
	dotnet test
	```

or just run the tests from your IDE (Visual Studio, Visual Studio code etc.).


### Configuring Integration Tests

You can configure the RabbitMQ connection settings for the integration tests by modifying the `appsettings.json` file in the `tests\VRT.FusionCache.Backplane.RabbitMq.Tests.Integration` directory. The default configuration is as follows:
```json
{
  "RabbitMqBackplane": {
    "ExchangeName": "test.fusioncache.exchange",
    "RabbitMq": {
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest",
      "AutomaticRecoveryEnabled": true
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
If you want to change logging level, to get more execution details, you can modify the `LogLevel` section.



