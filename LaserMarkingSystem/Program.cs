using LaserMarkingSystem.Forms;
using Serilog;
using System.Windows.Forms;

namespace LaserMarkingSystem
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configure logging
            ConfigureLogging();
            
            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                
                Log.Information("Starting Shimnit Laser Marking System");
                
                // Create and run the main form
                var mainForm = new MainForm();
                Application.Run(mainForm);
                
                Log.Information("Application closed successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application crashed");
                MessageBox.Show(
                    $"A critical error occurred: {ex.Message}\n\nPlease check the logs for more details.",
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureLogging()
        {
            var logPath = Path.Combine(Application.StartupPath, "Logs", "laser_marking_system.log");
            var logDirectory = Path.GetDirectoryName(logPath);
            
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory!);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }
    }
}