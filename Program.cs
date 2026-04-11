using System.Reflection;
using InventoryApi.Data;
using InventoryApi.Data.Interfaces;
using InventoryApi.Services;
using InventoryApi.Services.Interfaces;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IDbHelper, DbHelper>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Inventory API",
        Version = "v1",
        Description = "API for managing inventory items, including bulk upload and paginated retrieval with filtering capabilities."
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    // options.EnableAnnotations();
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v2");
                options.RoutePrefix = "swagger";
            });
}

app.UseSerilogRequestLogging();
// app.Use


app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// app.UseMiddleware<ErrorMiddleware>();

app.Run();


