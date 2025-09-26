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

var builder = WebApplication.CreateBuilder(args);


// Controllers + JSON (optional: guard against reference loops if any entity slips through)
builder.Services.AddControllers();

// Swagger/OpenAPI
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

// Database
var keyVaultUri = builder.Configuration["KeyVaultUri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

var cosmos = builder.Configuration.GetRequiredSection("Cosmos");
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseCosmos(
        cosmos["Endpoint"]!,
        cosmos["Key"]!,
        cosmos["Database"]!
    ));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // add this
}

app.UseSwagger();
app.UseSwaggerUI(); // optional: c => { c.RoutePrefix = string.Empty; }



app.UseExceptionMapping();    // our custom exception -> HTTP mapping
app.UseHttpsRedirection();
app.UseCors("dev");           // remove or change if not needed
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();