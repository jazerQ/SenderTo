using QuoteService.Services;
using QuoteService.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<NeuroService>();
builder.Services.Configure<RabbitSettings>(
    builder.Configuration.GetSection("RabbitSettings"));
builder.Services.Configure<DeepSeekSettings>(
    builder.Configuration.GetSection("DeepSeekSettings"));
builder.Services.Configure<PublisherSettings>(
    builder.Configuration.GetSection("PublisherSettings"));
builder.Services.Configure<DiskSettings>(
builder.Configuration.GetSection("DiskSettings"));
builder.Services.AddHostedService<RabbitMqListener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();