using Serilog;
using ShimnitLaserMarking.Forms;
using ShimnitLaserMarking.Models;
using ShimnitLaserMarking.Services;
using ShimnitLaserMarking.Services.LaserControllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShimnitLaserMarking.Forms
{
    public partial class MainForm : Form
    {
        private readonly LaserControllerService _controllerService;
        private readonly Timer _statusTimer;
        private MarkingJob? _currentJob;

        public MainForm()
        {
            InitializeComponent();
            _controllerService = new LaserControllerService();
            _statusTimer = new Timer { Interval = 2000 };
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();
            
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await LoadVehicleTypesAsync();
                await LoadControllersAsync();
                await RefreshJobListAsync();
                UpdateControlStates();
                
                Log.Information("Main form initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing main form");
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main Form
            this.Text = "Shimnit Laser Marking System - Vehicle Number Plates";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Create main layout
            CreateMenuStrip();
            CreateToolStrip();
            CreateStatusStrip();
            CreateMainLayout();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateMenuStrip()
        {
            var menuStrip = new MenuStrip();
            this.MainMenuStrip = menuStrip;

            // File Menu
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&New Job", null, NewJob_Click);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("&Settings", null, Settings_Click);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit", null, Exit_Click);

            // Controller Menu
            var controllerMenu = new ToolStripMenuItem("&Controller");
            controllerMenu.DropDownItems.Add("&Connect", null, Connect_Click);
            controllerMenu.DropDownItems.Add("&Disconnect", null, Disconnect_Click);
            controllerMenu.DropDownItems.Add(new ToolStripSeparator());
            controllerMenu.DropDownItems.Add("&Test Laser", null, TestLaser_Click);
            controllerMenu.DropDownItems.Add("&Calibrate", null, Calibrate_Click);

            // View Menu
            var viewMenu = new ToolStripMenuItem("&View");
            viewMenu.DropDownItems.Add("&Job History", null, JobHistory_Click);
            viewMenu.DropDownItems.Add("&System Logs", null, SystemLogs_Click);
            viewMenu.DropDownItems.Add("&Statistics", null, Statistics_Click);

            // Help Menu
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&About", null, About_Click);

            menuStrip.Items.AddRange(new[] { fileMenu, controllerMenu, viewMenu, helpMenu });
            this.Controls.Add(menuStrip);
        }

        private void CreateToolStrip()
        {
            var toolStrip = new ToolStrip();
            toolStrip.ImageScalingSize = new Size(32, 32);
            
            toolStrip.Items.Add(new ToolStripButton("New Job", null, NewJob_Click) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Connect", null, Connect_Click) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripButton("Disconnect", null, Disconnect_Click) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Start Marking", null, StartMarking_Click) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            toolStrip.Items.Add(new ToolStripButton("Stop Marking", null, StopMarking_Click) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });

            this.Controls.Add(toolStrip);
        }

        private void CreateStatusStrip()
        {
            var statusStrip = new StatusStrip();
            
            statusLabelController = new ToolStripStatusLabel("Controller: Disconnected");
            statusLabelStatus = new ToolStripStatusLabel("Status: Ready");
            statusLabelTime = new ToolStripStatusLabel($"Time: {DateTime.Now:HH:mm:ss}");
            
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabelController, statusLabelStatus, statusLabelTime });
            this.Controls.Add(statusStrip);
        }

        private void CreateMainLayout()
        {
            // Main split container
            var mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // Top panel split container
            var topSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 350
            };

            // Left panel - Job Creation
            CreateJobCreationPanel(topSplitContainer.Panel1);
            
            // Right panel - Controller Status
            CreateControllerStatusPanel(topSplitContainer.Panel2);

            // Bottom panel - Job List and Progress
            CreateJobListPanel(mainSplitContainer.Panel2);

            mainSplitContainer.Panel1.Controls.Add(topSplitContainer);
            this.Controls.Add(mainSplitContainer);
        }

        private void CreateJobCreationPanel(Control parent)
        {
            var groupBox = new GroupBox
            {
                Text = "Create Marking Job",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(5)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Vehicle Number Plate
            tableLayout.Controls.Add(new Label { Text = "Number Plate:", Anchor = AnchorStyles.Left }, 0, 0);
            txtVehicleNumber = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F) };
            tableLayout.Controls.Add(txtVehicleNumber, 1, 0);

            // Vehicle Type
            tableLayout.Controls.Add(new Label { Text = "Vehicle Type:", Anchor = AnchorStyles.Left }, 0, 1);
            cmbVehicleType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            tableLayout.Controls.Add(cmbVehicleType, 1, 1);

            // Owner Name
            tableLayout.Controls.Add(new Label { Text = "Owner Name:", Anchor = AnchorStyles.Left }, 0, 2);
            txtOwnerName = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(txtOwnerName, 1, 2);

            // Marking Text
            tableLayout.Controls.Add(new Label { Text = "Marking Text:", Anchor = AnchorStyles.Left }, 0, 3);
            txtMarkingText = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(txtMarkingText, 1, 3);

            // Controller Type
            tableLayout.Controls.Add(new Label { Text = "Controller:", Anchor = AnchorStyles.Left }, 0, 4);
            cmbControllerType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            tableLayout.Controls.Add(cmbControllerType, 1, 4);

            // Position
            tableLayout.Controls.Add(new Label { Text = "Position (X,Y):", Anchor = AnchorStyles.Left }, 0, 5);
            var positionPanel = new Panel { Dock = DockStyle.Fill };
            numPositionX = new NumericUpDown { Width = 80, DecimalPlaces = 2, Minimum = -1000, Maximum = 1000 };
            numPositionY = new NumericUpDown { Width = 80, DecimalPlaces = 2, Minimum = -1000, Maximum = 1000, Left = 90 };
            positionPanel.Controls.AddRange(new Control[] { numPositionX, numPositionY });
            tableLayout.Controls.Add(positionPanel, 1, 5);

            // Buttons
            var buttonPanel = new Panel { Dock = DockStyle.Fill };
            btnCreateJob = new Button { Text = "Create Job", Size = new Size(80, 30), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White };
            btnParametersConfig = new Button { Text = "Parameters", Size = new Size(80, 30), Left = 90, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White };
            btnCreateJob.Click += BtnCreateJob_Click;
            btnParametersConfig.Click += BtnParametersConfig_Click;
            buttonPanel.Controls.AddRange(new Control[] { btnCreateJob, btnParametersConfig });
            tableLayout.Controls.Add(buttonPanel, 1, 6);

            groupBox.Controls.Add(tableLayout);
            parent.Controls.Add(groupBox);
        }

        private void CreateControllerStatusPanel(Control parent)
        {
            var groupBox = new GroupBox
            {
                Text = "Controller Status",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(5)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Status indicators
            tableLayout.Controls.Add(new Label { Text = "Connection:", Anchor = AnchorStyles.Left }, 0, 0);
            lblConnectionStatus = new Label { Text = "Disconnected", ForeColor = Color.Red, Dock = DockStyle.Fill };
            tableLayout.Controls.Add(lblConnectionStatus, 1, 0);

            tableLayout.Controls.Add(new Label { Text = "Laser Status:", Anchor = AnchorStyles.Left }, 0, 1);
            lblLaserStatus = new Label { Text = "Not Ready", ForeColor = Color.Orange, Dock = DockStyle.Fill };
            tableLayout.Controls.Add(lblLaserStatus, 1, 1);

            tableLayout.Controls.Add(new Label { Text = "Temperature:", Anchor = AnchorStyles.Left }, 0, 2);
            lblTemperature = new Label { Text = "N/A", Dock = DockStyle.Fill };
            tableLayout.Controls.Add(lblTemperature, 1, 2);

            tableLayout.Controls.Add(new Label { Text = "Power Level:", Anchor = AnchorStyles.Left }, 0, 3);
            lblPowerLevel = new Label { Text = "N/A", Dock = DockStyle.Fill };
            tableLayout.Controls.Add(lblPowerLevel, 1, 3);

            tableLayout.Controls.Add(new Label { Text = "Position:", Anchor = AnchorStyles.Left }, 0, 4);
            lblPosition = new Label { Text = "N/A", Dock = DockStyle.Fill };
            tableLayout.Controls.Add(lblPosition, 1, 4);

            // Progress bar
            tableLayout.Controls.Add(new Label { Text = "Progress:", Anchor = AnchorStyles.Left }, 0, 5);
            progressBarMarking = new ProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Continuous };
            tableLayout.Controls.Add(progressBarMarking, 1, 5);

            // Control buttons
            var buttonPanel = new Panel { Dock = DockStyle.Fill };
            btnConnect = new Button { Text = "Connect", Size = new Size(70, 25), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White };
            btnDisconnect = new Button { Text = "Disconnect", Size = new Size(80, 25), Left = 80, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White };
            btnStartMarking = new Button { Text = "Start", Size = new Size(60, 25), Left = 170, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White };
            btnStopMarking = new Button { Text = "Stop", Size = new Size(60, 25), Left = 240, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White };
            
            btnConnect.Click += Connect_Click;
            btnDisconnect.Click += Disconnect_Click;
            btnStartMarking.Click += StartMarking_Click;
            btnStopMarking.Click += StopMarking_Click;
            
            buttonPanel.Controls.AddRange(new Control[] { btnConnect, btnDisconnect, btnStartMarking, btnStopMarking });
            tableLayout.Controls.Add(buttonPanel, 1, 6);

            groupBox.Controls.Add(tableLayout);
            parent.Controls.Add(groupBox);
        }

        private void CreateJobListPanel(Control parent)
        {
            var groupBox = new GroupBox
            {
                Text = "Marking Jobs",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            dataGridViewJobs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray
            };

            // Configure columns
            dataGridViewJobs.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "JobNumber", HeaderText = "Job Number", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "VehicleNumberPlate", HeaderText = "Number Plate", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "VehicleType", HeaderText = "Vehicle Type", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "OwnerName", HeaderText = "Owner", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "MarkingText", HeaderText = "Marking Text", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "ControllerType", HeaderText = "Controller", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CreatedDate", HeaderText = "Created", Width = 130 }
            });

            dataGridViewJobs.SelectionChanged += DataGridViewJobs_SelectionChanged;

            groupBox.Controls.Add(dataGridViewJobs);
            parent.Controls.Add(groupBox);
        }

        // Control declarations
        private TextBox txtVehicleNumber = null!;
        private ComboBox cmbVehicleType = null!;
        private TextBox txtOwnerName = null!;
        private TextBox txtMarkingText = null!;
        private ComboBox cmbControllerType = null!;
        private NumericUpDown numPositionX = null!;
        private NumericUpDown numPositionY = null!;
        private Button btnCreateJob = null!;
        private Button btnParametersConfig = null!;
        private Label lblConnectionStatus = null!;
        private Label lblLaserStatus = null!;
        private Label lblTemperature = null!;
        private Label lblPowerLevel = null!;
        private Label lblPosition = null!;
        private ProgressBar progressBarMarking = null!;
        private Button btnConnect = null!;
        private Button btnDisconnect = null!;
        private Button btnStartMarking = null!;
        private Button btnStopMarking = null!;
        private DataGridView dataGridViewJobs = null!;
        private ToolStripStatusLabel statusLabelController = null!;
        private ToolStripStatusLabel statusLabelStatus = null!;
        private ToolStripStatusLabel statusLabelTime = null!;

        // Event handlers and methods will continue in the next part...
        private async void BtnCreateJob_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtVehicleNumber.Text))
                {
                    MessageBox.Show("Please enter a vehicle number plate.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var job = new MarkingJob
                {
                    VehicleNumberPlate = txtVehicleNumber.Text.Trim().ToUpper(),
                    VehicleType = cmbVehicleType.Text,
                    OwnerName = txtOwnerName.Text.Trim(),
                    MarkingText = string.IsNullOrWhiteSpace(txtMarkingText.Text) ? txtVehicleNumber.Text.Trim().ToUpper() : txtMarkingText.Text.Trim(),
                    ControllerType = (LaserControllerType)Enum.Parse(typeof(LaserControllerType), cmbControllerType.Text),
                    PositionX = (double)numPositionX.Value,
                    PositionY = (double)numPositionY.Value,
                    CreatedBy = Environment.UserName,
                    LaserPower = 50.0, // Default values
                    LaserSpeed = 1000,
                    PulseFrequency = 20000,
                    Passes = 1
                };

                await DatabaseService.CreateMarkingJobAsync(job);
                await RefreshJobListAsync();
                ClearJobInputs();

                MessageBox.Show("Job created successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating marking job");
                MessageBox.Show($"Error creating job: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnParametersConfig_Click(object? sender, EventArgs e)
        {
            var paramForm = new ParametersForm();
            paramForm.ShowDialog(this);
        }

        private async void Connect_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbControllerType.SelectedItem == null)
                {
                    MessageBox.Show("Please select a controller type first.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var controllerType = (LaserControllerType)Enum.Parse(typeof(LaserControllerType), cmbControllerType.Text);
                var success = await _controllerService.ConnectAsync(controllerType);
                
                if (success)
                {
                    MessageBox.Show("Controller connected successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to connect to controller.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                UpdateControlStates();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error connecting to controller");
                MessageBox.Show($"Connection error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Disconnect_Click(object? sender, EventArgs e)
        {
            try
            {
                await _controllerService.DisconnectAsync();
                UpdateControlStates();
                MessageBox.Show("Controller disconnected.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error disconnecting controller");
                MessageBox.Show($"Disconnect error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StartMarking_Click(object? sender, EventArgs e)
        {
            if (_currentJob == null)
            {
                MessageBox.Show("Please select a job to mark.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var success = await _controllerService.StartMarkingAsync(_currentJob);
                if (!success)
                {
                    MessageBox.Show("Failed to start marking operation.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error starting marking");
                MessageBox.Show($"Marking error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StopMarking_Click(object? sender, EventArgs e)
        {
            try
            {
                await _controllerService.StopMarkingAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping marking");
                MessageBox.Show($"Stop error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TestLaser_Click(object? sender, EventArgs e)
        {
            try
            {
                var success = await _controllerService.TestLaserAsync();
                MessageBox.Show(success ? "Laser test completed successfully." : "Laser test failed.", 
                    "Test Result", MessageBoxButtons.OK, 
                    success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error testing laser");
                MessageBox.Show($"Test error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Calibrate_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show("This will calibrate the laser controller. Continue?", 
                    "Calibration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    var success = await _controllerService.CalibrateAsync();
                    MessageBox.Show(success ? "Calibration completed successfully." : "Calibration failed.", 
                        "Calibration Result", MessageBoxButtons.OK, 
                        success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during calibration");
                MessageBox.Show($"Calibration error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NewJob_Click(object? sender, EventArgs e)
        {
            ClearJobInputs();
            txtVehicleNumber.Focus();
        }

        private void Settings_Click(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog(this);
        }

        private void JobHistory_Click(object? sender, EventArgs e)
        {
            var historyForm = new JobHistoryForm();
            historyForm.ShowDialog(this);
        }

        private void SystemLogs_Click(object? sender, EventArgs e)
        {
            var logsForm = new SystemLogsForm();
            logsForm.ShowDialog(this);
        }

        private void Statistics_Click(object? sender, EventArgs e)
        {
            var statsForm = new StatisticsForm();
            statsForm.ShowDialog(this);
        }

        private void About_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Shimnit Laser Marking System v1.0\nDeveloped for Shimnit Company India\n\nFor vehicle number plate laser marking using RTC3/RTC4 controllers.", 
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private async void DataGridViewJobs_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridViewJobs.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewJobs.SelectedRows[0];
                if (selectedRow.DataBoundItem is MarkingJob job)
                {
                    _currentJob = job;
                    UpdateControlStates();
                }
            }
        }

        private async void StatusTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                await UpdateStatusDisplayAsync();
                statusLabelTime.Text = $"Time: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating status display");
            }
        }

        private async Task LoadVehicleTypesAsync()
        {
            var vehicleTypes = await DatabaseService.GetVehicleTypesAsync();
            cmbVehicleType.DataSource = vehicleTypes;
            cmbVehicleType.DisplayMember = "Name";
            cmbVehicleType.ValueMember = "Id";
        }

        private async Task LoadControllersAsync()
        {
            cmbControllerType.Items.Clear();
            cmbControllerType.Items.AddRange(new[] { "RTC3", "RTC4" });
            cmbControllerType.SelectedIndex = 0;
        }

        private async Task RefreshJobListAsync()
        {
            var jobs = await DatabaseService.GetMarkingJobsAsync(limit: 50);
            dataGridViewJobs.DataSource = jobs;
        }

        private async Task UpdateStatusDisplayAsync()
        {
            var status = await _controllerService.GetStatusAsync();
            
            if (status != null)
            {
                lblConnectionStatus.Text = status.Status.ToString();
                lblConnectionStatus.ForeColor = status.Status == LaserStatus.Connected || status.Status == LaserStatus.Ready ? Color.Green : Color.Red;
                
                lblLaserStatus.Text = status.Status.ToString();
                lblTemperature.Text = $"{status.Temperature:F1}°C";
                lblPowerLevel.Text = $"{status.PowerLevel:F1}%";
                lblPosition.Text = "Ready";
                
                statusLabelController.Text = $"Controller: {status.Status}";
                statusLabelStatus.Text = $"Status: {status.Status}";
            }
        }

        private void UpdateControlStates()
        {
            var isConnected = _controllerService.IsConnected;
            var isReady = _controllerService.IsReady;
            var hasSelectedJob = _currentJob != null;

            btnConnect.Enabled = !isConnected;
            btnDisconnect.Enabled = isConnected;
            btnStartMarking.Enabled = isReady && hasSelectedJob && _currentJob?.Status == MarkingJobStatus.Created;
            btnStopMarking.Enabled = isConnected;
            btnCreateJob.Enabled = true;
        }

        private void ClearJobInputs()
        {
            txtVehicleNumber.Clear();
            txtOwnerName.Clear();
            txtMarkingText.Clear();
            numPositionX.Value = 0;
            numPositionY.Value = 0;
            cmbVehicleType.SelectedIndex = -1;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _statusTimer?.Stop();
                _statusTimer?.Dispose();
                _controllerService?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during form closing");
            }
            
            base.OnFormClosing(e);
        }
    }
}