using SenderTo;
using SenderTo.Application.Services.PublisherService;
using SenderTo.Application.Services.Telegram;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.GetServices();

builder.Services.AddHostedService<TelegramService>();
builder.Services.AddGrpc();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<PublisherService>();
app.MapControllers();
app.Run();