using System.Security.Cryptography;
using Azure.Identity;
using Azure.Storage.Blobs;
using ApiFestaJulina.Repository;
using ApiFestaJulina.Services;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serviços
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Criptografia
builder.Services.AddScoped<HashAlgorithm>(_ => SHA512.Create());

// Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnectionFestaJunina"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

// Serviços personalizados
builder.Services.AddScoped<QRCodeServico>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["AzureBlobStorage:ConnectionString"];
    var serviceUri = configuration["AzureBlobStorage:ServiceUri"];

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return new BlobServiceClient(connectionString);
    }

    if (string.IsNullOrWhiteSpace(serviceUri))
    {
        throw new InvalidOperationException("Configure AzureBlobStorage:ServiceUri ou AzureBlobStorage:ConnectionString.");
    }

    return new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
});
builder.Services.AddScoped<AzureBlobStorageService>();

// JWT
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                "f8A9xK2#pL0zQw7@Rm5TnY3uVb6C!dE1"
            )
        )
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger / OpenAPI
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "../openapi/v1.json",
            "API Festa Junina 2026"
        );
    });
}

// Middlewares
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();