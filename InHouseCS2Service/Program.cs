
using Azure.Storage.Blobs;
using InHouseCS2.Core.Clients;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Managers;
using InHouseCS2.Core.Managers.Contracts;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using InHouseCS2.Core.EntityStores;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2Service;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Common.Contracts;
using InHouseCS2.Core.Common;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.Configure<LoggerFilterOptions>(options =>
{
    var defaultRule = options.Rules.FirstOrDefault(rule =>
        rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

    if (defaultRule is not null)
    {
        options.Rules.Remove(defaultRule);
    }

    options.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>(
        "", LogLevel.Information);
});
builder.Logging.AddApplicationInsights();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddHttpClient("MatchParserHttpClient", (httpClient) =>
{
    httpClient.BaseAddress = new Uri("https://andrew-server.com/");
});

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<MatchParsingWorkPoller>();

builder.Services.AddDbContext<AzureSqlDbContext>(options => options.UseSqlServer(
    Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING")
    ));

builder.Services.AddScoped<IMatchParserServiceClient, MatchParserServiceClient>();

BlobContainerClient blobContainerClient = new BlobContainerClient(
connectionString: Environment.GetEnvironmentVariable("BlOBCONTAINER_CONNECTION_STRING"),
blobContainerName: "demos");

builder.Services.AddScoped<IMediaStorageClient>(serviceProvider =>
{
    return new AzureBlobStorageClient(
        blobContainerClient,
        serviceProvider.GetRequiredService<ILogger<AzureBlobStorageClient>>());
});

builder.Services.AddScoped(typeof(ITransactionOperation), typeof(AzureSqlTransactionOperation));
builder.Services.AddScoped(typeof(IEntityStore<MatchUploadEntity, int>), typeof(AzureSqlEntityStore<MatchUploadEntity, int>));
builder.Services.AddScoped(typeof(IEntityStore<SeasonEntity, int>), typeof(AzureSqlEntityStore<SeasonEntity, int>));
builder.Services.AddScoped(typeof(IEntityStore<MatchEntity, string>), typeof(AzureSqlEntityStore<MatchEntity, string>));
builder.Services.AddScoped(typeof(IEntityStore<PlayerEntity, long>), typeof(AzureSqlEntityStore<PlayerEntity, long>));
builder.Services.AddScoped(typeof(IEntityStore<KillEventEntity, int>), typeof(AzureSqlEntityStore<KillEventEntity, int>));
builder.Services.AddScoped(typeof(IEntityStore<PlayerMatchStatEntity, int>), typeof(AzureSqlEntityStore<PlayerMatchStatEntity, int>));

builder.Services.AddScoped<IUploadsManager>(serviceProvider =>
{
    return new UploadsManager(
        serviceProvider.GetRequiredService<IMediaStorageClient>(),
        serviceProvider.GetRequiredService<ITransactionOperation>(),
        serviceProvider.GetRequiredService<IMatchParserServiceClient>(),
        serviceProvider.GetRequiredService<IBackgroundTaskQueue>(),
        serviceProvider.GetRequiredService<ILogger<UploadsManager>>());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AzureSqlDbContext>();
    context.Database.EnsureCreated();
    // DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
