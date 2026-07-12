using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLink.Abstractions;
using SQLink.Data;
using SQLink.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Register SQLite Database
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlite("Data Source=transactions.db"));

// Register Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis")["ConnectionString"] ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(configuration);
});

// Register services
builder.Services.AddSingleton<ITransactionStore, TransactionStore>();
builder.Services.AddSingleton<IRedisMessageBroker, RedisMessageBroker>();
builder.Services.AddSingleton<ISignalRPublisher, SignalRPublisher>();
builder.Services.AddSingleton<IRealtimePublisher>(sp =>
    (IRealtimePublisher)sp.GetRequiredService<ISignalRPublisher>());
builder.Services.AddScoped<ITransactionRepository>(sp => sp.GetRequiredService<TransactionDbContext>());
builder.Services.AddScoped<ITransactionService, TransactionService>();

// Register Redis Pub/Sub subscription service
builder.Services.AddHostedService<RedisSubscriptionService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending EF Core migrations
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}

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

public partial class Program { }

namespace SQLink { }
