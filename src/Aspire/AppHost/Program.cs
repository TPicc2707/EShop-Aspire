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


// Projects
var catalog = builder
    .AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);


var basket = builder
    .AddProject<Projects.Basket_API>("basket-api")
    .WithReference(cache)
    .WithReference(catalog)
    .WaitFor(cache);


builder.Build().Run();
