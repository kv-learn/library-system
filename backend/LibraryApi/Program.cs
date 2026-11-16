using LibraryApi.Services;
using LibraryApi.Data;
using LibraryApi.Data.Seeding;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<LibraryDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);

            // Sample data for local development only
             if (builder.Environment.IsDevelopment())
             {
                  options.UseSeeding((context, _) =>
                        DevDataSeeder.Seed((LibraryDbContext)context));
                  options.UseAsyncSeeding((context, _, ct) =>
                        DevDataSeeder.SeedAsync((LibraryDbContext)context, ct));
            }

            }, ServiceLifetime.Scoped);
            builder.Services.AddScoped<BookService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
