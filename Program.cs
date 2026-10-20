// namespace DigitalWallet;
//
// public static class Program
// {
//     public static void Main(string[] args)
//     {
//         var builder = WebApplication.CreateBuilder(args);
//         
//         builder.Configuration
//             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//             .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
//
//         // Add services to the container.
//         builder.Services.AddAuthorization();
//
//         // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//         builder.Services.AddOpenApi();
//
//         var app = builder.Build();
//
//         // Configure the HTTP request pipeline.
//         if (app.Environment.IsDevelopment())
//         {
//             app.MapOpenApi();
//         }
//
//         app.UseHttpsRedirection();
//
//         app.UseAuthorization();
//
//         var summaries = new[]
//         {
//             "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//         };
//
//         app.MapGet("/weatherforecast", (HttpContext httpContext) =>
//             {
//                 var forecast = Enumerable.Range(1, 5).Select(index =>
//                         new WeatherForecast
//                         {
//                             Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//                             TemperatureC = Random.Shared.Next(-20, 55),
//                             Summary = summaries[Random.Shared.Next(summaries.Length)]
//                         })
//                     .ToArray();
//                 return forecast;
//             })
//             .WithName("GetWeatherForecast");
//
//         app.Run();
//     }
// }


using DigitalWallet.Extensions;

var builder = WebApplication.CreateBuilder(args);

IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.ConfigureApiClients();
builder.Services.ConfigureDatabaseContext(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureOptions(builder.Configuration);
builder.Services.ConfigureApiClients();
builder.Services.ConfigureValidators();
builder.Services.ConfigureServices();
builder.Services.ConfigureManagers();
builder.Services.AddAuthentication();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();