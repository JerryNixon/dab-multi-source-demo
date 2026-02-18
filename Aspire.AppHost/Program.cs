var builder = DistributedApplication.CreateBuilder(args);

var saPassword = builder.AddParameter("sa-password", secret: true);

var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));.
var databaseRoot = Path.Combine(repoRoot, "database");
var apiRoot = Path.Combine(repoRoot, "api");

var sqlServer = builder
    .AddSqlServer("sql-server", password: saPassword)
    .WithHostPort(5007)
    .WithDataVolume("sql-data")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDbFile = Path.Combine(databaseRoot, "CatalogDb.sql");
var catalogDbScript = File.ReadAllText(catalogDbFile);
var catalogDb = sqlServer
    .AddDatabase("CatalogDb")
    .WithCreationScript(catalogDbScript);

var inventoryDbFile = Path.Combine(databaseRoot, "InventoryDb.sql");
var inventoryDbScript = File.ReadAllText(inventoryDbFile);
var inventoryDb = sqlServer
    .AddDatabase("InventoryDb")
    .WithCreationScript(inventoryDbScript);

var recommendationsDbFile = Path.Combine(databaseRoot, "RecommendationsDb.sql");
var recommendationsDbScript = File.ReadAllText(recommendationsDbFile);
var recommendationsDb = sqlServer
    .AddDatabase("RecommendationsDb")
    .WithCreationScript(recommendationsDbScript);

var dabServer = builder
    .AddContainer("data-api", image: "azure-databases/data-api-builder", tag: "1.7.83-rc")
    .WithImageRegistry("mcr.microsoft.com")
    .WithBindMount(source: Path.Combine(apiRoot, "dab-config.json"), target: "/App/dab-config.json", isReadOnly: true)
    .WithBindMount(source: Path.Combine(apiRoot, "dab-config-catalog.json"), target: "/App/dab-config-catalog.json", isReadOnly: true)
    .WithBindMount(source: Path.Combine(apiRoot, "dab-config-inventory.json"), target: "/App/dab-config-inventory.json", isReadOnly: true)
    .WithBindMount(source: Path.Combine(apiRoot, "dab-config-recommendations.json"), target: "/App/dab-config-recommendations.json", isReadOnly: true)
    .WithHttpEndpoint(name: "http", port: 5005, targetPort: 5000, isProxied: false)
    .WithEnvironment("CATALOG_CONNECTION_STRING", catalogDb)
    .WithEnvironment("INVENTORY_CONNECTION_STRING", inventoryDb)
    .WithEnvironment("RECOMMENDATIONS_CONNECTION_STRING", recommendationsDb)
    .WithUrls(context =>
    {
        context.Urls.Clear();
        context.Urls.Add(new() { Url = "/graphql", DisplayText = "GraphQL", Endpoint = context.GetEndpoint("http") });
        context.Urls.Add(new() { Url = "/swagger", DisplayText = "Swagger", Endpoint = context.GetEndpoint("http") });
        context.Urls.Add(new() { Url = "/health", DisplayText = "Health", Endpoint = context.GetEndpoint("http") });
    })
    .WithOtlpExporter()
    .WithParentRelationship(catalogDb)
    .WithHttpHealthCheck("/health")
    .WaitFor(catalogDb)
    .WaitFor(inventoryDb)
    .WaitFor(recommendationsDb);

var webui = builder
    .AddProject<Projects.WebUI>("webui", launchProfileName: "http")
    .WithEndpoint("http", e =>
    {
        e.Port = 5006;
        e.TargetPort = 5006;
        e.IsProxied = false;
    })
    .WithEnvironment("DAB_URL", dabServer.GetEndpoint("http"))
    .WaitFor(dabServer);

await builder.Build().RunAsync();
