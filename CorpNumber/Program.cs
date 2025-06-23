using CorpNumber.Models; // добавь пространство имён, где находится твой DbContext
using Microsoft.EntityFrameworkCore;

namespace CorpNumber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Настроим Kestrel слушать на IP 10.82.1.185 и портах 5299 (http) и 7091 (https)
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(System.Net.IPAddress.Parse("10.82.1.185"), 5299); // HTTP
                options.Listen(System.Net.IPAddress.Parse("10.82.1.185"), 7091, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            // Остальное без изменений
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<CorpNumberDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Phones}/{action=Index}/{id?}");

            app.Run();

        }
    }
}
