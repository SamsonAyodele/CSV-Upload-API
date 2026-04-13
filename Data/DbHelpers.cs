using Microsoft.Data.SqlClient;
using System.Data;
using InventoryApi.Models;
using InventoryApi.Data.Interfaces;
using InventoryApi.Models.Common;

namespace InventoryApi.Data;

public class DbHelper : IDbHelper
{
    private readonly string _connection;

    public DbHelper(IConfiguration config)
    {
        _connection = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task BulkUpsertAsync(List<Models.Inventory> items)
    {
        using var con = new SqlConnection(_connection);
        using var cmd = new SqlCommand("sp_bulk_upsert_inventory", con);

        cmd.CommandType = CommandType.StoredProcedure;

        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Category", typeof(string));
        table.Columns.Add("Price", typeof(decimal));
        table.Columns.Add("StockQuantity", typeof(int));

        foreach (var item in items)
        {
            table.Rows.Add(item.Id, item.Name, item.Category, item.Price, item.StockQuantity);
        }

        var param = cmd.Parameters.AddWithValue("@tvp_InventoryTable", table);
        param.SqlDbType = SqlDbType.Structured;
        param.TypeName = "dbo.InventoryTableType";

        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }


    // public async Task UpsertAsync(Models.Inventory item)
    // {
    //     using var con = new SqlConnection(_connection);
    //     using var cmd = new SqlCommand("sp_upsert_inventory", con);

    //     cmd.CommandType = CommandType.StoredProcedure;

    //     cmd.Parameters.AddWithValue("@Id", item.Id);
    //     cmd.Parameters.AddWithValue("@Name", item.Name);
    //     cmd.Parameters.AddWithValue("@Category", item.Category);
    //     cmd.Parameters.AddWithValue("@Price", item.Price);
    //     cmd.Parameters.AddWithValue("@StockQuantity", item.StockQuantity);

    //     await con.OpenAsync();
    //     await cmd.ExecuteNonQueryAsync();
    // }



    public async Task<PagedResponse<Inventory>> GetInventoryAsync(int page, int size, InventoryFilter? filter)
    {
        var response = new PagedResponse<Inventory>()
        {
            Page = page,
            Size = size,
        };
        var list = new List<Inventory>();

        using var con = new SqlConnection(_connection);
        using var cmd = new SqlCommand("sp_get_inventories", con);

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Page", page);
        cmd.Parameters.AddWithValue("@Size", size);
        cmd.Parameters.AddWithValue("@Name", filter?.Name ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Category", filter?.Category ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MinPrice", filter?.MinPrice ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MaxPrice", filter?.MaxPrice ?? (object)DBNull.Value);

        await con.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        int totalCount = 0;
        if (await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(reader.GetOrdinal("TotalRecords"));
            response.TotalRecords = totalCount;
        }

        await reader.NextResultAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Inventory
            {
                Id = (int)reader["Id"],
                Name = reader["Name"]?.ToString() ?? "",
                Category = reader["Category"]?.ToString() ?? "",
                Price = (decimal)reader["Price"],
                StockQuantity = (int)reader["StockQuantity"]
            });
        }
        response.Items = list;
        return response;
    }

    public async Task<Inventory> GetSingleInventoryAsync(int id)
    {
        using var con = new SqlConnection(_connection);
        using var cmd = new SqlCommand("sp_get_inventory_by_id", con);

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        await con.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Inventory
            {
                Id = (int)reader["Id"],
                Name = reader["Name"]?.ToString() ?? "",
                Category = reader["Category"]?.ToString() ?? "",
                Price = (decimal)reader["Price"],
                StockQuantity = (int)reader["StockQuantity"]
            };
        }

        throw new KeyNotFoundException($"Inventory with ID {id} not found.");

    }
}