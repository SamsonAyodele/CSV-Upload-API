CREATE TYPE dbo.InventoryTableType AS TABLE
(
    Id INT,
    Name NVARCHAR(100),
    Category NVARCHAR(100),
    Price DECIMAL(18, 4),
    StockQuantity INT
)