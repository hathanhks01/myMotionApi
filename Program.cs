using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using myMotionApi.Data;

// Chỉ load file .env nếu file có tồn tại (tránh lỗi khi chạy trên Docker/Render)
if (File.Exists(".env"))
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL + EF Core) ──────────────────────────────────────────
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL không tìm thấy trong biến môi trường hoặc .env");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── App Config — đưa SENDER_ID / RECEIVER_ID vào IConfiguration ──────────────
builder.Configuration["AppSettings:SenderId"] =
    Environment.GetEnvironmentVariable("SENDER_ID") ?? string.Empty;
builder.Configuration["AppSettings:ReceiverId"] =
    Environment.GetEnvironmentVariable("RECEIVER_ID") ?? string.Empty;

// ── CORS (Cho phép FE gọi API từ mọi domain như Vercel / localhost) ───────────
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
    app.UseHttpsRedirection();
}

// Health check endpoint
app.MapGet("/", () => Results.Ok(new { status = "healthy", message = "myMotionApi is running! 💌" }));

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
