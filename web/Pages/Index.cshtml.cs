using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public List<Product> Products { get; set; } = [];
    public List<InventoryItem> Inventories { get; set; } = [];
    public List<Recommendation> UserRecommendations { get; set; } = [];
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("dab");

            // Single GraphQL query hitting all three databases through DAB multi-source
            var query = new
            {
                query = """
                {
                    products {
                        items {
                            ProductId
                            Name
                            Category
                            Price
                        }
                    }
                    inventories {
                        items {
                            InventoryId
                            ProductId
                            StockCount
                            Warehouse
                        }
                    }
                    recommendations {
                        items {
                            RecommendationId
                            UserId
                            ProductId
                            Score
                        }
                    }
                }
                """
            };

            var json = JsonSerializer.Serialize(query);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/graphql", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"{(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<GraphQLResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Products = (result?.Data?.Products?.Items ?? [])
                .OrderBy(p => p.ProductId)
                .ToList();

            Inventories = (result?.Data?.Inventories?.Items ?? [])
                .OrderBy(i => i.ProductId)
                .ToList();

            UserRecommendations = (result?.Data?.Recommendations?.Items ?? [])
                .Where(r => string.Equals(r.UserId, "jerry", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Score)
                .ToList();
        }
        catch (Exception ex)
        {
            Error = $"Failed to load data from DAB: {ex.Message}";
        }
    }
}

// --- GraphQL response models ---

public class GraphQLResponse
{
    public GraphQLData? Data { get; set; }
}

public class GraphQLData
{
    public ProductsResult? Products { get; set; }
    public InventoriesResult? Inventories { get; set; }
    public RecommendationsResult? Recommendations { get; set; }
}

public class ProductsResult { public List<Product> Items { get; set; } = []; }
public class InventoriesResult { public List<InventoryItem> Items { get; set; } = []; }
public class RecommendationsResult { public List<Recommendation> Items { get; set; } = []; }

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
}

public class InventoryItem
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int StockCount { get; set; }
    public string Warehouse { get; set; } = "";
}

public class Recommendation
{
    public int RecommendationId { get; set; }
    public string UserId { get; set; } = "";
    public int ProductId { get; set; }
    public decimal Score { get; set; }
}
