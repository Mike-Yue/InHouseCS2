
using Azure.Storage.Blobs;
using InHouseCS2.Core.Clients;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Managers;
using InHouseCS2.Core.Managers.Contracts;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using InHouseCS2.Core.EntityStores.Models;
using InHouseCS2.Core.EntityStores;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2Service;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddHostedService<MatchParsingWorkPoller>();

builder.Services.AddDbContext<AzureSqlDbContext>(options => options.UseSqlServer(
    Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING")
    ));

BlobContainerClient blobContainerClient = new BlobContainerClient(
connectionString: Environment.GetEnvironmentVariable("BlOBCONTAINER_CONNECTION_STRING"),
blobContainerName: "demos");

builder.Services.AddScoped<IMediaStorageClient>(serviceProvider =>
{
    return new AzureBlobStorageClient(
        blobContainerClient,
        serviceProvider.GetRequiredService<ILogger<AzureBlobStorageClient>>());
});

builder.Services.AddScoped(typeof(IEntityStore<MatchUploadEntity>), typeof(AzureSqlEntityStore<MatchUploadEntity>));
builder.Services.AddScoped(typeof(IEntityStore<ParseMatchTaskEntity>), typeof(AzureSqlEntityStore<ParseMatchTaskEntity>));
builder.Services.AddScoped<IUploadsManager>(serviceProvider =>
{
    return new UploadsManager(
        serviceProvider.GetRequiredService<IMediaStorageClient>(),
        serviceProvider.GetRequiredService<IEntityStore<MatchUploadEntity>>(),
        serviceProvider.GetRequiredService<IEntityStore<ParseMatchTaskEntity>>(),
        serviceProvider.GetRequiredService<ILogger<UploadsManager>>());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
