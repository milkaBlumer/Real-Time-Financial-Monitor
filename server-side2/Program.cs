using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLink.Abstractions;
using SQLink.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Register Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis")["ConnectionString"] ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(configuration);
});

// Register services
builder.Services.AddSingleton<SQLink.Abstractions.ITransactionStore, SQLink.Services.TransactionStore>();
builder.Services.AddSingleton<SQLink.ConnectedClientTracker>();
builder.Services.AddSingleton<SQLink.Abstractions.IRealtimePublisher, SQLink.Services.SignalRRealtimePublisher>();
builder.Services.AddSingleton<SQLink.Services.TransactionService>();

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionStore, TransactionStore>();
builder.Services.AddScoped<IRealtimePublisher, SignalRRealtimePublisher>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", realtime = "signalr" }));
app.MapControllers();
app.MapHub<SQLink.TransactionHub>("/hub/transactions");

app.Run();

namespace SQLink { }
