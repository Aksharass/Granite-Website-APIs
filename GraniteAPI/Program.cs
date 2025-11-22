using GraniteAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Force Development so Swagger auto-opens locally
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GraniteAPI", Version = "v1" });
});

// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable Swagger ALWAYS
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GraniteAPI v1");
    c.RoutePrefix = string.Empty;   // Swagger at root
});

// Disable HTTPS redirect for local testing
// app.UseHttpsRedirection();

app.UseCors("_myAllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();

app.Run();
