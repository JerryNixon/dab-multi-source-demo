IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'CatalogDb')
    CREATE DATABASE CatalogDb;
GO

USE CatalogDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
CREATE TABLE dbo.Products
(
    ProductId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (Name, Category, Price) VALUES
        (N'Wireless Mouse', N'Electronics', 29.99),
        (N'Mechanical Keyboard', N'Electronics', 89.99),
        (N'USB-C Hub', N'Electronics', 49.99),
        (N'Standing Desk', N'Furniture', 399.99),
        (N'Monitor Arm', N'Furniture', 79.99),
        (N'Noise-Cancelling Headphones', N'Audio', 199.99),
        (N'Webcam HD', N'Electronics', 59.99),
        (N'Desk Lamp', N'Furniture', 34.99);
END
