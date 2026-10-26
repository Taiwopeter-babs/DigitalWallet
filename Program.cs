using DigitalWallet.Controllers.HistoryController;
using DigitalWallet.Controllers.WalletController;
using DigitalWallet.Extensions;


var builder = WebApplication.CreateBuilder(args);

IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

builder.Services.AddOpenApi();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureKafkaOptions(builder.Configuration);
builder.Services.ConfigureServices();
builder.Services.ConfigureHostedServices();
builder.Services.ConfigureDatabaseContext(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureValidators();
builder.Services.ConfigureManagers();
builder.Services.AddAuthentication();


var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();


app.UseHttpsRedirection();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapWalletEndpoints();
app.MapHistoryEndpoints();

app.Run();