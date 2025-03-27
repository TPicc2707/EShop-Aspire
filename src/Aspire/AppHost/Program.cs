var builder = DistributedApplication.CreateBuilder(args);

//Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithPgAdmin()
        //.WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalogdb");

var cache = builder
      .AddRedis("cache")
      .WithRedisInsight()
      //.WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder
      .AddRabbitMQ("rabbitmq")
      .WithManagementPlugin()
      //.WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder
      .AddKeycloak("keycloak", 8080)
      //.WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsRunMode)
{
    postgres.WithDataVolume();
    cache.WithDataVolume();
    rabbitmq.WithDataVolume();
    keycloak.WithDataVolume();
}

var ollama = builder
      .AddOllama("ollama", 11434)
      .WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent)
      .WithOpenWebUI();

var llama = ollama.AddModel("llama3.2");

var embedding = ollama.AddModel("all-minilm");


// Projects
var catalog = builder
    .AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(rabbitmq)
    .WithReference(llama)
    .WithReference(embedding)
    .WaitFor(catalogDb)
    .WaitFor(rabbitmq)
    .WaitFor(llama)
    .WaitFor(embedding);


var basket = builder
    .AddProject<Projects.Basket_API>("basket-api")
    .WithReference(cache)
    .WithReference(catalog)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(cache)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);


var webapp = builder
    .AddProject<Projects.WebApp>("webapp")
    .WithExternalHttpEndpoints()
    .WithReference(catalog)
    .WithReference(cache)
    .WithReference(basket)
    .WaitFor(catalog)
    .WaitFor(basket);


builder.Build().Run();
