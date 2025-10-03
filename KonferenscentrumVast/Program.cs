using KonferenscentrumVast.Data;
using KonferenscentrumVast.Repository.Implementations;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using KonferenscentrumVast.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);


// Controllers + JSON (optional: guard against reference loops if any entity slips through)
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Konferenscentrum Väst API", Version = "v1" });

    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingContractRepository, BookingContractRepository>();

// Application services
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<FacilityService>();
builder.Services.AddScoped<BookingContractService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SendGridService>();

// Database
if (builder.Environment.IsDevelopment())
{
    var kvUri = builder.Configuration["KeyVaultUri"];
    if (!string.IsNullOrWhiteSpace(kvUri))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
    }
}

var cosmosEndpoint = builder.Configuration["Cosmos:Endpoint"];
var cosmosKey = builder.Configuration["Cosmos:Key"];
var cosmosDatabase = builder.Configuration["Cosmos:Database"];
if (string.IsNullOrWhiteSpace(cosmosEndpoint) || string.IsNullOrWhiteSpace(cosmosKey) || string.IsNullOrWhiteSpace(cosmosDatabase))
{
    throw new InvalidOperationException("One or more required Cosmos DB configuration values are missing (Endpoint, Key, or Database).");
}

// Register DbContext with Cosmos
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseCosmos(cosmosEndpoint, cosmosKey, cosmosDatabase));

var blobConnection = builder.Configuration["BlobStorage--Connection"];
var containerName = builder.Configuration["BlobStorage--ContainerName"];

if (string.IsNullOrWhiteSpace(blobConnection))
{
    throw new InvalidOperationException("Connection-string missing in 'BlobStorage:ConnectionString'.");
}

if (string.IsNullOrWhiteSpace(containerName))
{
    throw new InvalidOperationException("Container name missing in 'BlobStorage:ContainerName'.");
}

var blobServiceClient = new BlobServiceClient(blobConnection);

builder.Services.AddSingleton(blobServiceClient);
builder.Services.AddScoped<BlobService>(provider =>
    new BlobService(provider.GetRequiredService<BlobServiceClient>(),
    containerName!,
    provider.GetRequiredService<ILogger<BlobService>>()
));

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("dev", policy =>
    {
        policy.WithOrigins("http://localhost:3000",
        "http://localhost:5173",
        "http://localhost:7166",
        "https://konferens-bokning.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // add this
}


app.UseSwagger();
app.UseSwaggerUI(); // optional: c => { c.RoutePrefix = string.Empty; }
app.UseExceptionMapping();    // our custom exception -> HTTP mapping
app.UseCors("dev");           // remove or change if not needed
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();