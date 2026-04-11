CREATE OR ALTER PROCEDURE sp_bulk_upsert_inventory
    @InventoryTable dbo.InventoryTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        MERGE tbl_Inventory AS target
        USING @InventoryTable AS source
        ON target.Id = source.Id

        WHEN MATCHED THEN
            UPDATE SET
                target.Name = source.Name,
                target.Category = source.Category,
                target.Price = source.Price,
                target.StockQuantity = source.StockQuantity

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (Name, Category, Price, StockQuantity)
            VALUES (source.Name, source.Category, source.Price, source.StockQuantity);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @Error NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Error, 16, 1);
    END CATCH
END