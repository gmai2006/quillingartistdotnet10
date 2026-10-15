using System.Text;
using System.Text.Json.Serialization;
using ApiService.Configuration;
using ApiService.Dao;
using ApiService.Data;
using ApiService.Endpoints;
using ApiService.Security;
using ApiService.Services;
using ApiService.Services.Communications;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.Section));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.Section));
builder.Services.Configure<SmsOptions>(builder.Configuration.GetSection(SmsOptions.Section));
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Section))
    .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= 32, "Jwt:SigningKey must be set and at least 32 bytes.")
    .Validate(o => o.Issuer.Length > 0 && o.Audience.Length > 0, "Jwt:Issuer and Jwt:Audience must be set.")
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
    if (builder.Configuration.GetValue<bool>("Database:ShowSql"))
    {
        options.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
    }
});

builder.Services.AddScoped<AddressesDao>();
builder.Services.AddScoped<CustomersDao>();
builder.Services.AddScoped<OrderItemsDao>();
builder.Services.AddScoped<OrdersDao>();
builder.Services.AddScoped<ProductsDao>();
builder.Services.AddScoped<UsersDao>();

builder.Services.AddScoped<AddressesService>();
builder.Services.AddScoped<CustomersService>();
builder.Services.AddScoped<OrderItemsService>();
builder.Services.AddScoped<OrdersService>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<StripeCheckoutService>();

builder.Services.AddHttpClient<EmailDeliveryService>(c => c.BaseAddress = new Uri(BrevoJson.BaseUrl));
builder.Services.AddHttpClient<SmsDeliveryService>(c => c.BaseAddress = new Uri(BrevoJson.BaseUrl));

builder.Services.AddScoped<AuthContext>();

builder.Services.AddHostedService<NightlyJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<JwtAuthMiddleware>();

var api = app.MapGroup("/api");
api.MapAddressesEndpoints();
api.MapCustomersEndpoints();
api.MapOrderItemsEndpoints();
api.MapOrdersEndpoints();
api.MapProductsEndpoints();
api.MapUsersEndpoints();
api.MapCheckoutEndpoints();

app.Run();
