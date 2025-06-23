using CorpNumber.Models; // добавь пространство имён, где находится твой DbContext
using Microsoft.EntityFrameworkCore;

namespace CorpNumber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ⬇️ Добавим строку подключения из appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // ⬇️ Регистрируем контекст базы данных с этой строкой подключения
            builder.Services.AddDbContext<CorpNumberDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
