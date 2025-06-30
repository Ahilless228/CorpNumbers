using CorpNumber.Models; // твой DbContext
using Microsoft.EntityFrameworkCore;

namespace CorpNumber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Слушать по IP и портам
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(System.Net.IPAddress.Parse("10.82.1.185"), 5299); // HTTP
                options.Listen(System.Net.IPAddress.Parse("10.82.1.185"), 7091, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            // Подключаем строку подключения к БД
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<CorpNumberDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Подключаем контроллеры с представлениями
            builder.Services.AddControllersWithViews();

            // Подключаем сессии
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();

            // Используем сессии ДО авторизации
            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Phones}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
