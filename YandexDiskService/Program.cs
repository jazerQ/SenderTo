using YandexDiskService;
using YandexDiskService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

builder.Services.Configure<YandexSettings>(
    builder.Configuration.GetSection("YandexSettings"));

builder.Services.AddGrpc();

var app = builder.Build();


app.MapGrpcService<DiskImagerService>();

app.MapGet("/", () => "Hello World!");

app.Run();