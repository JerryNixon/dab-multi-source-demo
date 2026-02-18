IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'RecommendationsDb')
    CREATE DATABASE RecommendationsDb;
GO

USE RecommendationsDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Recommendations')
CREATE TABLE dbo.Recommendations
(
    RecommendationId INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(50) NOT NULL,
    ProductId INT NOT NULL,
    Score DECIMAL(3,2) NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM dbo.Recommendations)
BEGIN
    INSERT INTO dbo.Recommendations (UserId, ProductId, Score) VALUES
        (N'jerry', 1, 0.95),
        (N'jerry', 3, 0.88),
        (N'jerry', 6, 0.92),
        (N'jerry', 4, 0.75),
        (N'jerry', 8, 0.70);
END
