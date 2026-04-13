using Microsoft.AspNetCore.Mvc;
using InventoryApi.Services.Interfaces;
using InventoryApi.Models.Common;
using InventoryApi.Models;
// using Swashbuckle.AspNetCore.Annotations;
// using Swashbuckle.AspNetCore.Filters;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }
    /// <summary>
    /// Uploads inventory items from a CSV file. The CSV should have the following columns: Name, Category, Price, Quantity. 
    /// The API will process the file and return the total number of records processed along with any errors encountered during processing.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<BulkUploadResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<BulkUploadResponse>), 400)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            if (file == null)
            {
                return BadRequest(new ApiResponse<ResultDto>
                {
                    Errors = new List<string> { "No file uploaded" },
                    Message = "File upload failed",
                    StatusCode = 400,
                });
            }


            var (totalProcessed, errors) = await _service.UploadCsvAsync(file.OpenReadStream());
            if (errors.Count > 0)
            {
                return BadRequest(new ApiResponse<ResultDto>
                {
                    Errors = errors,
                    Message = "File processed with errors",
                    StatusCode = 400,

                });
            }

            return Ok(new ApiResponse<BulkUploadResponse>
            {
                Data = new BulkUploadResponse
                {
                    TotalProcessed = totalProcessed
                },
                Message = $"{totalProcessed} records processed successfully",
                IsSuccessful = true,
                StatusCode = 200,

            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {
                Message = "An error occurred while processing the request" + ex.Message,
                StatusCode = 500,
            });
        }
    }
    /// <summary>
    /// Retrieves a paginated list of inventory items with optional filters
    /// </summary>
    /// <param name="page">Page number (default is 1)</param>
    /// <param name="size">Number of records per page (default is 5)</param>
    /// <param name="filter">Filter criteria (Name, Category, Price range)</param>
    /// <returns>Paginated inventory response</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Inventory>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    public async Task<IActionResult> Get(
        int page = 1,
        int size = 5, [FromQuery] InventoryFilter? filter = null)
    {
        try
        {
            if (page <= 0 || size <= 0)
                return BadRequest(new ApiResponse<string>
                {

                    Message = "Request failed",
                    StatusCode = 400,
                });
            var data = await _service.GetInventoryAsync(page, size, filter);

            var response = new ApiResponse<PagedResponse<Inventory>>
            {
                Message = "Inventory retrieved successfully",
                IsSuccessful = true,
                StatusCode = 200,
                Data = data
            };

            return Ok(response);

        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {

                Message = "An error occurred while processing the request" + ex.Message,
                StatusCode = 500,
            });
        }
    }

    /// <summary>
    /// Retrieves a single inventory item by its ID.  
    /// it returns a 400 Bad Request response. This endpoint allows clients to fetch specific inventory items based on their unique identifier.    
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Inventory>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    public async Task<IActionResult> GetSingleInventory(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<string>
                {

                    Message = "Request failed",
                    StatusCode = 400,
                });
            var data = await _service.GetSingleInventoryAsync(id);

            if (data == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Message = "Inventory item not found",
                    StatusCode = 404,
                });
            }

            var response = new ApiResponse<Inventory>
            {
                Message = "Inventory item retrieved successfully",
                IsSuccessful = true,
                StatusCode = 200,
                Data = data
            };

            return Ok(response);

        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {

                Message = "An error occurred while processing the request" + ex.Message,
                StatusCode = 500,
            });
        }
    }
}
