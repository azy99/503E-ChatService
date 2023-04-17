using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ChatService.Web.Configuration;
using ChatService.Web.Storage;
using ChatService.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Configuration
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<BlobSettings>(builder.Configuration.GetSection("Blob"));

// Add Services
builder.Services.AddSingleton<IProfileStore, CosmosProfileStore>();
builder.Services.AddSingleton<IConversationStore, CosmosConversationStore>();
builder.Services.AddSingleton<IMessageStore, CosmosMessageStore>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddSingleton(sp =>
{
    var cosmosOptions = sp.GetRequiredService<IOptions<CosmosSettings>>();
    return new CosmosClient(cosmosOptions.Value.ConnectionString);
});
builder.Services.AddSingleton<IFileStore, BlobStore>();
builder.Services.AddSingleton(sp =>
{
    var blobOptions = sp.GetRequiredService<IOptions<BlobSettings>>();
    return new BlobContainerClient(blobOptions.Value.ConnectionString, blobOptions.Value.ContainerName);

});

var blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=yuneschibani;AccountKey=8te+CN7tG9282JbJ4uidYA07reDXep5Lu7btF5xheAr+MLVcuPCGA8S7NfVDT7xnmAdofmFfxO8m+AStQWkR6Q==;EndpointSuffix=core.windows.net");

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

public partial class Program { }