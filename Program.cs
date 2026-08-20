using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using myMotionApi.Data;

// Load biến môi trường từ file .env
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL + EF Core) ──────────────────────────────────────────
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("DATABASE_URL không tìm thấy trong .env");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── App Config — đưa SENDER_ID / RECEIVER_ID vào IConfiguration ──────────────
builder.Configuration["AppSettings:SenderId"] =
    Environment.GetEnvironmentVariable("SENDER_ID") ?? string.Empty;
builder.Configuration["AppSettings:ReceiverId"] =
    Environment.GetEnvironmentVariable("RECEIVER_ID") ?? string.Empty;

// ── CORS (Cho phép FE gọi API) ────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Controllers & OpenAPI ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
