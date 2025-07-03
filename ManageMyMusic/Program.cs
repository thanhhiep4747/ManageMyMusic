using ManageMyMusic.Core;
using ManageMyMusic.Core.Configuration;
using ManageMyMusic.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ManageMyMusic
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;// Support for displaying Vietnamese characters (can be removed if not needed)

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Get IMyConfiguration instance
                    var myConfig = services.GetRequiredService<IMusicConfiguration>();
                    Console.WriteLine("--- Direct Access via IMyConfiguration ---");
                    Console.WriteLine($"SourceFolder: {myConfig.AppSettings.SourceFolder}");
                    Console.WriteLine($"Default Connection: {myConfig.ConnectionStrings.DefaultConnection}");
                    Console.WriteLine("------------------------------------------");

                    var m_Actions = services.GetRequiredService<IActions>();
                    await m_Actions.DoActionsAsync();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables(); // For environment variables
                    if (args != null)
                    {
                        config.AddCommandLine(args); // For command-line arguments
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
                    services.Configure<ConnectionStrings>(hostContext.Configuration.GetSection("ConnectionStrings"));

                    services.AddSingleton<IMusicConfiguration, MusicConfiguration>();
                    services.AddScoped<IMusicDataExcute, MusicDataExcute>();

                    services.AddScoped<IActions, Actions>();
                });
    }
}


// Extract File: Extract all folder with regext [*]-[???].zip
// => merge them

// Merged with destination path
// 1. Get Information of music file
//    => Artist / Authors
// 2. Manage by Artist / Authors    Ca Sy / Nhac Sy