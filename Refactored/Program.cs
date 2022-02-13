using Microsoft.EntityFrameworkCore;
using Refactored.Domain.V1;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

string X_Correlation_ID = string.Empty;

var builder = WebApplication.CreateBuilder(args);

#region Logger Configuration
string logFilePath = typeof(Program).Assembly.GetName().Name;
const string outTemplate = "{Timestamp:HH:mm:ss.fff} [{Level}] <{SourceContext}> {Message} {NewLine} {Exception}";
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Debug)  // <<== verbosity+, use: Serilog.Events.LogEventLevel.Debug
                       .Enrich.FromLogContext()
#if DEBUG
                       .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                       .Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached)   // ... Used to filter out potentially bad data due debugging.
                       .WriteTo.Console()       // .WriteTo.Conditional(l => l.).Console()
#endif
                       .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                       .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                       .Enrich.WithProperty("X-Correlation-ID", X_Correlation_ID)
                       .WriteTo.Logger(config => config.WriteTo.File(path: logFilePath,
                                                                     retainedFileCountLimit: 31,
                                                                     outputTemplate: outTemplate, shared: true));
});
#endregion

#region Add services to the container.
builder.Services.AddEndpointsApiExplorer();         // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddDbContext<RefactoredDB>(opt => opt.UseInMemoryDatabase("Refactored"));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddTransient<IProduct, Product>();
#endregion

var app = builder.Build();
app.Logger.LogDebug("■ Current Log File Name.....: {logFilePath}", logFilePath);

// ... Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


#region Products API actions
// ... NOTICE: this resolves thwo cases: '/products' and also 'products?name=xxx' 
app.MapGet("/products", async (HttpContext httpContext, RefactoredDB db) =>
{
    string name = httpContext.Request.Query["name"];            // ... ?name= parameter in querystring
    if (string.IsNullOrEmpty(name))
        return await db.Products.OrderBy(p => p.Name)
                                .ToListAsync();
    else
        return await db.Products.Where(p => p.Name == name)
                                .OrderBy(p => p.Name)
                                .ToListAsync();
});


app.MapGet("/products/{id}", async (Guid id, RefactoredDB db) =>
{
    X_Correlation_ID = null;
    return await db.Products.FindAsync(id)
        is Product product
            ? Results.Ok(product)
            : Results.NotFound();
});


app.MapPost("/products", async (Product product, RefactoredDB db, HttpContext httpContext) =>
{
    X_Correlation_ID = httpContext.Request.Headers["X-Correlation-Id"].ToString();

    db.Products.Add(product);
    await db.SaveChangesAsync();

    app.Logger.LogInformation("[POST] {@product}", product);

    return Results.Created($"/products/{product.Id}", product);
});


app.MapPut("/products/{id}", async (Guid id, IProduct inputProduct, RefactoredDB db, HttpContext httpContext) =>
{
    X_Correlation_ID = httpContext.Request.Headers["X-Correlation-Id"].ToString();

    var product = await db.Products.FindAsync(id);
    if (product is null)
        return Results.NotFound();

    product = (Product)inputProduct.Clone();

    await db.SaveChangesAsync();

    app.Logger.LogInformation("[PUT] {@product}", product);

    return Results.NoContent();
});


app.MapDelete("/products/{id}", async (Guid id, RefactoredDB db, HttpContext httpContext) =>
{
    X_Correlation_ID = httpContext.Request.Headers["X-Correlation-Id"].ToString();

    if (await db.Products.FindAsync(id) is Product product)
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        app.Logger.LogInformation("[DELETE] {@product}", product);
        return Results.Ok(product);
    }

    app.Logger.LogError("[DELETE] {id} NOT FOUND", id);
    return Results.NotFound();
});
#endregion


#region ProductsOptions API actions
app.MapGet("/products/{id}/options", async (Guid id, RefactoredDB db) =>
{
    X_Correlation_ID = null;
    var result = await db.Products.Include(p => p.ProductOptions)
                                  .ToListAsync();
    return result is null
            ? Results.NotFound()
            : Results.Ok(result);
});


app.MapPost("/products/{id}/options", async (Guid id, ProductOption productOption, RefactoredDB db, HttpContext httpContext) =>
{
    if (id != productOption.ProductId)
    {
        app.Logger.LogError("");
        return Results.BadRequest($"The Product ID:{id} doesn't match the ProductOption ProductId:{productOption.ProductId}");
    }

    X_Correlation_ID = httpContext.Request.Headers["X-Correlation-Id"].ToString();

    db.ProductOptions.Add(productOption);
    await db.SaveChangesAsync();

    app.Logger.LogInformation("[POST] /products/{id}/options {@productOption}", id, productOption);

    return Results.Created($"ProductOption:", JsonSerializer.Serialize(productOption));
});

// ... TODO boilerplate code for the other methods

#endregion

app.Logger.LogInformation("Luanched  {version}", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

app.UseAuthentication();
app.UseAuthorization();

app.Run();
