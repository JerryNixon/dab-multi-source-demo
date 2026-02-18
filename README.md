# Multi-Source DAB Demo

Demonstrates Data API Builder's **multi-source config** — one master config with three child configs, each pointing to a separate database.

## Services

Aspire orchestrates all services. One SQL Server hosts three databases. One DAB instance serves all three via multi-source config files.

```mermaid
flowchart LR
  WebUI["Web UI"] --> DAB
  DAB["API Builder"] --> CatalogDb[(CatalogDb)]
  DAB --> InventoryDb[(InventoryDb)]
  DAB --> RecommendationsDb[(RecommendationsDb)]
  subgraph SQL Server
    CatalogDb
    InventoryDb
    RecommendationsDb
  end
```

| Service | Purpose |
|---------|---------|
| SQL Server | Hosts all three databases |
| API Builder (DAB) | GraphQL + REST API via multi-source config |
| Web UI | ASP.NET Core Razor Pages, queries DAB GraphQL |
| Aspire Dashboard | Monitoring at `:15888` |

## Data model across the three databases

```mermaid
erDiagram
    CatalogDb_PRODUCTS {
        int ProductId PK
        nvarchar Name
        nvarchar Category
        decimal Price
    }

    InventoryDb_INVENTORY {
        int InventoryId PK
        int ProductId
        int StockCount
        nvarchar Warehouse
    }

    RecommendationsDb_RECOMMENDATIONS {
        int RecommendationId PK
        nvarchar UserId
        int ProductId
        decimal Score
    }

    CatalogDb_PRODUCTS ||--o{ InventoryDb_INVENTORY : "ProductId"
    CatalogDb_PRODUCTS ||--o{ RecommendationsDb_RECOMMENDATIONS : "ProductId, UserId"
```


