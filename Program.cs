using DigitalWallet.Extensions;
using DigitalWallet.Services.KafkaService;
using DigitalWallet.Storage;

var builder = WebApplication.CreateBuilder(args);

IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureKafkaOptions(builder.Configuration);
builder.Services.ConfigureHostedServices();

builder.Services.ConfigureDatabaseContext(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureOptions(builder.Configuration);
builder.Services.ConfigureValidators();
builder.Services.ConfigureServices();
builder.Services.ConfigureManagers();
builder.Services.AddAuthentication();


var app = builder.Build();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();