using WatermarkService.Services;
using WatermarkService.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DiskSettings>(
    builder.Configuration.GetSection("DiskSettings"));
builder.Services.Configure<RabbitSettings>(
    builder.Configuration.GetSection("RabbitSettings"));

builder.Services.AddTransient<MarkService>();

builder.Services.AddHostedService<RabbitMqListener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
