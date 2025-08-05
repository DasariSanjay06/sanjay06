using Microsoft.Extensions.Configuration;
using Serilog;
using ShimnitLaserMarking.Forms;
using ShimnitLaserMarking.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace ShimnitLaserMarking
{
    internal static class Program
    {
        public static IConfiguration Configuration { get; private set; } = null!;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Initialize configuration
                InitializeConfiguration();

                // Initialize logging
                InitializeLogging();

                Log.Information("Starting Shimnit Laser Marking System");

                // Configure application
                ApplicationConfiguration.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Initialize database
                DatabaseService.InitializeDatabase();

                // Run main form
                Application.Run(new MainForm());

                Log.Information("Application shutdown completed");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                MessageBox.Show($"Fatal error occurred: {ex.Message}", "Fatal Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Production"}.json", optional: true);

            Configuration = builder.Build();
        }

        private static void InitializeLogging()
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "laser-marking-.log");
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.File(logPath, 
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                    retainedFileCountLimit: 30)
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}