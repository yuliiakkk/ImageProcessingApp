using BackendApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Додаємо сервіс DbContext з використанням SQL Server
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Додаємо контролери
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Налаштування CORS(обмежити до ендпоінтів доступ на сервері)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowALL", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Створюємо застосунок
var app = builder.Build();

// Зберігаємо доступ до сервісів для використання у фонових потоках
Program.Services = app.Services;

app.UseCors("AllowALL");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Додаємо клас Program зі статичною властивістю Services
public partial class Program
{
    public static IServiceProvider? Services { get; set; }
}
