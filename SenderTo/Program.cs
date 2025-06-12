using SenderTo;
using SenderTo.Application.Services;
using SenderTo.Application.Services.Telegram;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.GetServices();

builder.Services.AddHostedService<TelegramService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();