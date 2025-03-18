var builder = DistributedApplication.CreateBuilder(args);

//Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithPgAdmin()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalogdb");

var cache = builder
      .AddRedis("cache")
      .WithRedisInsight()
      .WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder
      .AddRabbitMQ("rabbitmq")
      .WithManagementPlugin()
      .WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder
      .AddKeycloak("keycloak", 8080)
      .WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent);


// Projects
var catalog = builder
    .AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(rabbitmq)
    .WaitFor(catalogDb)
    .WaitFor(rabbitmq);


var basket = builder
    .AddProject<Projects.Basket_API>("basket-api")
    .WithReference(cache)
    .WithReference(catalog)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(cache)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);


builder.Build().Run();
