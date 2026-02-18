var builder = DistributedApplication.CreateBuilder(args);

var saPassword = builder.AddParameter("sa-password", secret: true);

var (databaseRoot, apiRoot) = GetPaths(builder);

var sqlServer = builder
    .AddSqlServer("sql-server", password: saPassword)
    .WithHostPort(5007)
    .WithDataVolume("sql-data")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDbFile = Path.Combine(databaseRoot.FullName, "CatalogDb.sql");
var catalogDbScript = File.ReadAllText(catalogDbFile);
var catalogDb = sqlServer
    .AddDatabase("CatalogDb")
    .WithCreationScript(catalogDbScript);

var inventoryDbFile = Path.Combine(databaseRoot.FullName, "InventoryDb.sql");
var inventoryDbScript = File.ReadAllText(inventoryDbFile);
var inventoryDb = sqlServer
    .AddDatabase("InventoryDb")
    .WithCreationScript(inventoryDbScript);

var recommendationsDbFile = Path.Combine(databaseRoot.FullName, "RecommendationsDb.sql");
var recommendationsDbScript = File.ReadAllText(recommendationsDbFile);
var recommendationsDb = sqlServer
    .AddDatabase("RecommendationsDb")
    .WithCreationScript(recommendationsDbScript);

var dabServer = builder
    .AddContainer("data-api", image: "azure-databases/data-api-builder", tag: "1.7.83-rc")
    .WithImageRegistry("mcr.microsoft.com")
    .WithBindMount(source: Path.Combine(apiRoot.FullName, "dab-config.json"), target: "/App/dab-config.json", isReadOnly: true)
    .WithBindMount(source: Path.Combine(apiRoot.FullName, "dab-config-catalog.json"), target: "/App/dab-config-catalog.json", isReadOnly: true)
    .WithBindMount(source: Path.Combine(apiRoot.FullName, "dab-config-inventory.json"), target: "/App/dab-config-inventory.json", isReadOnly: true)
    .WithBindMount(source: Path.Combine(apiRoot.FullName, "dab-config-recommendations.json"), target: "/App/dab-config-recommendations.json", isReadOnly: true)
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

(DirectoryInfo databaseRoot, DirectoryInfo apiRoot) GetPaths(IDistributedApplicationBuilder builder)
{
    var appHostRoot = builder.Environment.ContentRootPath;
    var repoRoot = Path.GetFullPath(Path.Combine(appHostRoot, ".."));
    var databaseRoot = new DirectoryInfo(Path.Combine(repoRoot, "database"));
    var apiRoot = new DirectoryInfo(Path.Combine(repoRoot, "api"));
    
    if (!databaseRoot.Exists || !apiRoot.Exists)
    {
        throw new InvalidOperationException(
            $"Invalid repository layout. Expected sibling folders 'database' and 'api' next to 'Aspire.AppHost'. Repo root resolved to: '{repoRoot}'.");
    }

    return (databaseRoot, apiRoot);
}
