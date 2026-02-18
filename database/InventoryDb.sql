IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'InventoryDb')
    CREATE DATABASE InventoryDb;
GO

USE InventoryDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory')
CREATE TABLE dbo.Inventory
(
    InventoryId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    StockCount INT NOT NULL,
    Warehouse NVARCHAR(50) NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM dbo.Inventory)
BEGIN
    INSERT INTO dbo.Inventory (ProductId, StockCount, Warehouse) VALUES
        (1, 150, N'Seattle'),
        (2, 75, N'Seattle'),
        (3, 200, N'Portland'),
        (4, 30, N'Seattle'),
        (5, 90, N'Portland'),
        (6, 45, N'Seattle'),
        (7, 120, N'Portland'),
        (8, 60, N'Seattle');
END
