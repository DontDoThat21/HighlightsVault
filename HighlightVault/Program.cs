using Microsoft.EntityFrameworkCore;

namespace HighlightsVault
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();

            // Add logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            string connStr = "";
#if DEBUG
            connStr = builder.Configuration.GetConnectionString("SqlServerConnectionDev");
#else
            connStr = builder.Configuration.GetConnectionString("SqlServerConnection");
#endif
            try
            {
                //builder.Services.AddDbContext<ApplicationDbContext>(options =>
                //         options.UseSqlServer(connStr));

                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        connStr,
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 2, // Number of retry attempts
                                maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
                                errorNumbersToAdd: null); // Additional SQL error codes to consider as transient
                        }).EnableSensitiveDataLogging()
                          .LogTo(Console.WriteLine, LogLevel.Information));

            }
            catch (Exception ex)
            {
                var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during application startup.");
                throw;
            }            
            try
            {
                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthorization();
                app.UseSession();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.Run();
            }
            catch (Exception ex)
            {
                var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during application startup.");
                throw;

            }
        }
    }
}
