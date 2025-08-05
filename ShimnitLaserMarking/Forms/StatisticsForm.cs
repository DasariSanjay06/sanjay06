using ShimnitLaserMarking.Models;
using ShimnitLaserMarking.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShimnitLaserMarking.Forms
{
    public partial class StatisticsForm : Form
    {
        public StatisticsForm()
        {
            InitializeComponent();
            LoadStatisticsAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "System Statistics";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            CreateStatisticsLayout();

            this.ResumeLayout(false);
        }

        private void CreateStatisticsLayout()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20)
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            // Job Statistics
            var jobStatsGroup = new GroupBox
            {
                Text = "Job Statistics",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var jobStatsLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Text = "Loading..."
            };
            jobStatsGroup.Controls.Add(jobStatsLabel);

            // Performance Statistics
            var perfStatsGroup = new GroupBox
            {
                Text = "Performance Statistics",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var perfStatsLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Text = "Loading..."
            };
            perfStatsGroup.Controls.Add(perfStatsLabel);

            // Quality Statistics
            var qualityStatsGroup = new GroupBox
            {
                Text = "Quality Statistics",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var qualityStatsLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Text = "Loading..."
            };
            qualityStatsGroup.Controls.Add(qualityStatsLabel);

            // Controller Statistics
            var controllerStatsGroup = new GroupBox
            {
                Text = "Controller Statistics",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var controllerStatsLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Text = "Loading..."
            };
            controllerStatsGroup.Controls.Add(controllerStatsLabel);

            // System Information
            var systemInfoGroup = new GroupBox
            {
                Text = "System Information",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var systemInfoLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Text = GetSystemInfo()
            };
            systemInfoGroup.Controls.Add(systemInfoLabel);

            // Recent Activity
            var recentActivityGroup = new GroupBox
            {
                Text = "Recent Activity",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var recentActivityLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Text = "Loading..."
            };
            recentActivityGroup.Controls.Add(recentActivityLabel);

            mainPanel.Controls.Add(jobStatsGroup, 0, 0);
            mainPanel.Controls.Add(perfStatsGroup, 1, 0);
            mainPanel.Controls.Add(qualityStatsGroup, 0, 1);
            mainPanel.Controls.Add(controllerStatsGroup, 1, 1);
            mainPanel.Controls.Add(systemInfoGroup, 0, 2);
            mainPanel.Controls.Add(recentActivityGroup, 1, 2);

            this.Controls.Add(mainPanel);

            // Store references for updating
            this.jobStatsLabel = jobStatsLabel;
            this.perfStatsLabel = perfStatsLabel;
            this.qualityStatsLabel = qualityStatsLabel;
            this.controllerStatsLabel = controllerStatsLabel;
            this.recentActivityLabel = recentActivityLabel;
        }

        private Label jobStatsLabel = null!;
        private Label perfStatsLabel = null!;
        private Label qualityStatsLabel = null!;
        private Label controllerStatsLabel = null!;
        private Label recentActivityLabel = null!;

        private async void LoadStatisticsAsync()
        {
            try
            {
                var jobs = await DatabaseService.GetMarkingJobsAsync();
                var logs = await DatabaseService.GetSystemLogsAsync(limit: 100);

                // Job Statistics
                var totalJobs = jobs.Count;
                var completedJobs = jobs.Count(j => j.Status == MarkingJobStatus.Completed);
                var failedJobs = jobs.Count(j => j.Status == MarkingJobStatus.Failed);
                var successRate = totalJobs > 0 ? (completedJobs * 100.0 / totalJobs) : 0;

                var todayJobs = jobs.Count(j => j.CreatedDate.Date == DateTime.Today);
                var thisWeekJobs = jobs.Count(j => j.CreatedDate >= DateTime.Today.AddDays(-7));

                jobStatsLabel.Text = $"Total Jobs: {totalJobs:N0}\n" +
                                   $"Completed: {completedJobs:N0}\n" +
                                   $"Failed: {failedJobs:N0}\n" +
                                   $"Success Rate: {successRate:F1}%\n\n" +
                                   $"Today: {todayJobs:N0}\n" +
                                   $"This Week: {thisWeekJobs:N0}";

                // Performance Statistics
                var avgMarkingTime = completedJobs > 0 ? 
                    jobs.Where(j => j.Status == MarkingJobStatus.Completed && j.StartedDate.HasValue && j.CompletedDate.HasValue)
                        .Average(j => (j.CompletedDate!.Value - j.StartedDate!.Value).TotalMinutes) : 0;

                perfStatsLabel.Text = $"Avg Marking Time: {avgMarkingTime:F1} min\n" +
                                    $"System Uptime: {GetUptime()}\n" +
                                    $"Memory Usage: {GetMemoryUsage()}\n" +
                                    $"CPU Usage: {GetCpuUsage()}";

                // Quality Statistics
                var qualityJobs = jobs.Where(j => j.QualityRating > 0).ToList();
                var avgQuality = qualityJobs.Any() ? qualityJobs.Average(j => j.QualityRating) : 0;
                var highQualityJobs = qualityJobs.Count(j => j.QualityRating >= 8);
                var lowQualityJobs = qualityJobs.Count(j => j.QualityRating < 6);

                qualityStatsLabel.Text = $"Avg Quality Rating: {avgQuality:F1}/10\n" +
                                       $"High Quality (8+): {highQualityJobs:N0}\n" +
                                       $"Low Quality (<6): {lowQualityJobs:N0}\n" +
                                       $"Rated Jobs: {qualityJobs.Count:N0}";

                // Controller Statistics
                var rtc3Jobs = jobs.Count(j => j.ControllerType == LaserControllerType.RTC3);
                var rtc4Jobs = jobs.Count(j => j.ControllerType == LaserControllerType.RTC4);

                controllerStatsLabel.Text = $"RTC3 Jobs: {rtc3Jobs:N0}\n" +
                                          $"RTC4 Jobs: {rtc4Jobs:N0}\n" +
                                          $"Most Used: {(rtc3Jobs > rtc4Jobs ? "RTC3" : "RTC4")}\n" +
                                          $"Controllers Available: 2";

                // Recent Activity
                var recentJobs = jobs.OrderByDescending(j => j.CreatedDate).Take(5).ToList();
                var recentErrors = logs.Where(l => l.Level == "Error").OrderByDescending(l => l.Timestamp).Take(3).ToList();

                var recentActivity = "Recent Jobs:\n";
                foreach (var job in recentJobs)
                {
                    recentActivity += $"• {job.VehicleNumberPlate} - {job.Status}\n";
                }

                if (recentErrors.Any())
                {
                    recentActivity += "\nRecent Errors:\n";
                    foreach (var error in recentErrors)
                    {
                        recentActivity += $"• {error.Timestamp:HH:mm} - {error.Message.Substring(0, Math.Min(30, error.Message.Length))}...\n";
                    }
                }

                recentActivityLabel.Text = recentActivity;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSystemInfo()
        {
            return $"Application: Shimnit Laser Marking v1.0\n" +
                   $"Operating System: {Environment.OSVersion}\n" +
                   $"Machine Name: {Environment.MachineName}\n" +
                   $"User: {Environment.UserName}\n" +
                   $"Started: {DateTime.Now.AddHours(-1):dd/MM/yyyy HH:mm}";
        }

        private string GetUptime()
        {
            var uptime = DateTime.Now - DateTime.Today;
            return $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }

        private string GetMemoryUsage()
        {
            var workingSet = Environment.WorkingSet / (1024 * 1024);
            return $"{workingSet:N0} MB";
        }

        private string GetCpuUsage()
        {
            return "5.2%"; // Simulated for demonstration
        }
    }
}