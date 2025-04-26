
using Azure.Storage.Blobs;
using InHouseCS2.Core.Clients;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Managers;
using InHouseCS2.Core.Managers.Contracts;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

BlobContainerClient blobContainerClient = new BlobContainerClient(
    connectionString: Environment.GetEnvironmentVariable("BlOBCONTAINER_CONNECTION_STRING"),
    blobContainerName: "demos");

builder.Services.AddScoped<IMediaStorageClient>(serviceProvider =>
{
    return new AzureBlobStorageClient(
        blobContainerClient,
        serviceProvider.GetRequiredService<ILogger<AzureBlobStorageClient>>());
});
builder.Services.AddScoped<IUploadsManager>(serviceProvider =>
{
    return new UploadsManager(
        serviceProvider.GetRequiredService<IMediaStorageClient>(),
        serviceProvider.GetRequiredService<ILogger<UploadsManager>>());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
