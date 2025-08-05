using LaserMarkingSystem.Models;
using LaserMarkingSystem.Services;
using LaserMarkingSystem.Database;
using Serilog;

namespace LaserMarkingSystem.Forms
{
    public partial class MainForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly LaserControllerService _laserService;
        private readonly ILogger _logger;
        private LaserController? _currentController;
        private List<MarkingJob> _jobs = new();
        private List<LaserParameters> _parameters = new();
        private bool _isMarking = false;

        public MainForm()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _laserService = new LaserControllerService();
            _logger = Log.ForContext<MainForm>();
            
            InitializeForm();
            LoadData();
        }

        private void InitializeForm()
        {
            // Form setup
            this.Text = "Shimnit Laser Marking System v1.0";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = new Icon(Application.StartupPath + "\\Resources\\laser_icon.ico");

            // Initialize controls
            InitializeMenuStrip();
            InitializeStatusStrip();
            InitializeTabControl();
            InitializeEventHandlers();
        }

        private void InitializeMenuStrip()
        {
            var menuStrip = new MenuStrip();
            
            // File Menu
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&New Job", null, OnNewJob);
            fileMenu.DropDownItems.Add("&Open Job", null, OnOpenJob);
            fileMenu.DropDownItems.Add("&Save Job", null, OnSaveJob);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("&Export Report", null, OnExportReport);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit", null, OnExit);
            
            // Tools Menu
            var toolsMenu = new ToolStripMenuItem("&Tools");
            toolsMenu.DropDownItems.Add("&Controller Settings", null, OnControllerSettings);
            toolsMenu.DropDownItems.Add("&Parameter Library", null, OnParameterLibrary);
            toolsMenu.DropDownItems.Add("&Pattern Editor", null, OnPatternEditor);
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());
            toolsMenu.DropDownItems.Add("&System Settings", null, OnSystemSettings);
            
            // View Menu
            var viewMenu = new ToolStripMenuItem("&View");
            viewMenu.DropDownItems.Add("&Job Queue", null, OnViewJobQueue);
            viewMenu.DropDownItems.Add("&History", null, OnViewHistory);
            viewMenu.DropDownItems.Add("&Statistics", null, OnViewStatistics);
            
            // Help Menu
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&User Manual", null, OnUserManual);
            helpMenu.DropDownItems.Add("&About", null, OnAbout);
            
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, viewMenu, helpMenu });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void InitializeStatusStrip()
        {
            var statusStrip = new StatusStrip();
            
            var lblStatus = new ToolStripStatusLabel("Ready");
            var lblController = new ToolStripStatusLabel("Controller: Disconnected");
            var lblJobs = new ToolStripStatusLabel("Jobs: 0");
            var lblProgress = new ToolStripProgressBar();
            
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, new ToolStripSeparator(), lblController, new ToolStripSeparator(), lblJobs, new ToolStripSeparator(), lblProgress });
            
            this.Controls.Add(statusStrip);
        }

        private void InitializeTabControl()
        {
            var tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            
            // Job Creation Tab
            var jobTab = CreateJobCreationTab();
            tabControl.TabPages.Add(jobTab);
            
            // Job Queue Tab
            var queueTab = CreateJobQueueTab();
            tabControl.TabPages.Add(queueTab);
            
            // Controller Status Tab
            var statusTab = CreateControllerStatusTab();
            tabControl.TabPages.Add(statusTab);
            
            // Settings Tab
            var settingsTab = CreateSettingsTab();
            tabControl.TabPages.Add(settingsTab);
            
            this.Controls.Add(tabControl);
        }

        private TabPage CreateJobCreationTab()
        {
            var tab = new TabPage("Job Creation");
            
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            
            // Left Panel - Job Details
            var jobPanel = CreateJobDetailsPanel();
            panel.Controls.Add(jobPanel, 0, 0);
            
            // Right Panel - Laser Parameters
            var paramPanel = CreateLaserParametersPanel();
            panel.Controls.Add(paramPanel, 1, 0);
            
            tab.Controls.Add(panel);
            return tab;
        }

        private Panel CreateJobDetailsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(10)
            };
            
            // Job Name
            layout.Controls.Add(new Label { Text = "Job Name:", Anchor = AnchorStyles.Left }, 0, 0);
            var txtJobName = new TextBox { Dock = DockStyle.Fill, Name = "txtJobName" };
            layout.Controls.Add(txtJobName, 1, 0);
            
            // Number Plate
            layout.Controls.Add(new Label { Text = "Number Plate:", Anchor = AnchorStyles.Left }, 0, 1);
            var txtNumberPlate = new TextBox { Dock = DockStyle.Fill, Name = "txtNumberPlate" };
            layout.Controls.Add(txtNumberPlate, 1, 1);
            
            // Vehicle Type
            layout.Controls.Add(new Label { Text = "Vehicle Type:", Anchor = AnchorStyles.Left }, 0, 2);
            var cboVehicleType = new ComboBox { Dock = DockStyle.Fill, Name = "cboVehicleType" };
            cboVehicleType.Items.AddRange(new object[] { "Car", "Motorcycle", "Truck", "Bus", "Other" });
            layout.Controls.Add(cboVehicleType, 1, 2);
            
            // Customer Name
            layout.Controls.Add(new Label { Text = "Customer Name:", Anchor = AnchorStyles.Left }, 0, 3);
            var txtCustomerName = new TextBox { Dock = DockStyle.Fill, Name = "txtCustomerName" };
            layout.Controls.Add(txtCustomerName, 1, 3);
            
            // Marking Text
            layout.Controls.Add(new Label { Text = "Marking Text:", Anchor = AnchorStyles.Left }, 0, 4);
            var txtMarkingText = new TextBox { Dock = DockStyle.Fill, Name = "txtMarkingText", Multiline = true, Height = 60 };
            layout.Controls.Add(txtMarkingText, 1, 4);
            
            // Priority
            layout.Controls.Add(new Label { Text = "Priority:", Anchor = AnchorStyles.Left }, 0, 5);
            var numPriority = new NumericUpDown { Dock = DockStyle.Fill, Name = "numPriority", Minimum = 1, Maximum = 10, Value = 5 };
            layout.Controls.Add(numPriority, 1, 5);
            
            // Operator
            layout.Controls.Add(new Label { Text = "Operator:", Anchor = AnchorStyles.Left }, 0, 6);
            var txtOperator = new TextBox { Dock = DockStyle.Fill, Name = "txtOperator" };
            layout.Controls.Add(txtOperator, 1, 6);
            
            // Buttons
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40 };
            var btnCreateJob = new Button { Text = "Create Job", Width = 100, Name = "btnCreateJob" };
            var btnPreview = new Button { Text = "Preview", Width = 100, Name = "btnPreview" };
            buttonPanel.Controls.AddRange(new Control[] { btnCreateJob, btnPreview });
            layout.Controls.Add(buttonPanel, 0, 7);
            layout.SetColumnSpan(buttonPanel, 2);
            
            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreateLaserParametersPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(10)
            };
            
            // Parameter Name
            layout.Controls.Add(new Label { Text = "Parameter Set:", Anchor = AnchorStyles.Left }, 0, 0);
            var cboParameterSet = new ComboBox { Dock = DockStyle.Fill, Name = "cboParameterSet" };
            layout.Controls.Add(cboParameterSet, 1, 0);
            
            // Power
            layout.Controls.Add(new Label { Text = "Power (W):", Anchor = AnchorStyles.Left }, 0, 1);
            var numPower = new NumericUpDown { Dock = DockStyle.Fill, Name = "numPower", DecimalPlaces = 1, Increment = 0.5m, Maximum = 1000, Value = 50 };
            layout.Controls.Add(numPower, 1, 1);
            
            // Speed
            layout.Controls.Add(new Label { Text = "Speed (mm/s):", Anchor = AnchorStyles.Left }, 0, 2);
            var numSpeed = new NumericUpDown { Dock = DockStyle.Fill, Name = "numSpeed", DecimalPlaces = 1, Increment = 10m, Maximum = 10000, Value = 1000 };
            layout.Controls.Add(numSpeed, 1, 2);
            
            // Frequency
            layout.Controls.Add(new Label { Text = "Frequency (Hz):", Anchor = AnchorStyles.Left }, 0, 3);
            var numFrequency = new NumericUpDown { Dock = DockStyle.Fill, Name = "numFrequency", DecimalPlaces = 0, Increment = 1000m, Maximum = 100000, Value = 20000 };
            layout.Controls.Add(numFrequency, 1, 3);
            
            // Pulse Width
            layout.Controls.Add(new Label { Text = "Pulse Width (μs):", Anchor = AnchorStyles.Left }, 0, 4);
            var numPulseWidth = new NumericUpDown { Dock = DockStyle.Fill, Name = "numPulseWidth", DecimalPlaces = 1, Increment = 0.1m, Maximum = 1000, Value = 10 };
            layout.Controls.Add(numPulseWidth, 1, 4);
            
            // Focus Position
            layout.Controls.Add(new Label { Text = "Focus Position (mm):", Anchor = AnchorStyles.Left }, 0, 5);
            var numFocusPosition = new NumericUpDown { Dock = DockStyle.Fill, Name = "numFocusPosition", DecimalPlaces = 2, Increment = 0.01m, Maximum = 100, Value = 0 };
            layout.Controls.Add(numFocusPosition, 1, 5);
            
            // Marking Depth
            layout.Controls.Add(new Label { Text = "Marking Depth (mm):", Anchor = AnchorStyles.Left }, 0, 6);
            var numMarkingDepth = new NumericUpDown { Dock = DockStyle.Fill, Name = "numMarkingDepth", DecimalPlaces = 2, Increment = 0.01m, Maximum = 10, Value = 0.1m };
            layout.Controls.Add(numMarkingDepth, 1, 6);
            
            // Repetitions
            layout.Controls.Add(new Label { Text = "Repetitions:", Anchor = AnchorStyles.Left }, 0, 7);
            var numRepetitions = new NumericUpDown { Dock = DockStyle.Fill, Name = "numRepetitions", DecimalPlaces = 0, Increment = 1m, Maximum = 100, Value = 1 };
            layout.Controls.Add(numRepetitions, 1, 7);
            
            // Spot Size
            layout.Controls.Add(new Label { Text = "Spot Size (mm):", Anchor = AnchorStyles.Left }, 0, 8);
            var numSpotSize = new NumericUpDown { Dock = DockStyle.Fill, Name = "numSpotSize", DecimalPlaces = 2, Increment = 0.01m, Maximum = 10, Value = 0.1m };
            layout.Controls.Add(numSpotSize, 1, 8);
            
            // Enable Galvo
            layout.Controls.Add(new Label { Text = "Enable Galvo:", Anchor = AnchorStyles.Left }, 0, 9);
            var chkEnableGalvo = new CheckBox { Dock = DockStyle.Fill, Name = "chkEnableGalvo", Text = "Enable" };
            layout.Controls.Add(chkEnableGalvo, 1, 9);
            
            // Galvo Delay
            layout.Controls.Add(new Label { Text = "Galvo Delay (ms):", Anchor = AnchorStyles.Left }, 0, 10);
            var numGalvoDelay = new NumericUpDown { Dock = DockStyle.Fill, Name = "numGalvoDelay", DecimalPlaces = 1, Increment = 0.1m, Maximum = 1000, Value = 0.1m };
            layout.Controls.Add(numGalvoDelay, 1, 10);
            
            // Material
            layout.Controls.Add(new Label { Text = "Material:", Anchor = AnchorStyles.Left }, 0, 11);
            var cboMaterial = new ComboBox { Dock = DockStyle.Fill, Name = "cboMaterial" };
            cboMaterial.Items.AddRange(new object[] { "Aluminum", "Steel", "Plastic", "Glass", "Wood", "Other" });
            layout.Controls.Add(cboMaterial, 1, 11);
            
            panel.Controls.Add(layout);
            return panel;
        }

        private TabPage CreateJobQueueTab()
        {
            var tab = new TabPage("Job Queue");
            
            var dgvJobs = new DataGridView
            {
                Dock = DockStyle.Fill,
                Name = "dgvJobs",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            
            dgvJobs.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "JobName", HeaderText = "Job Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "NumberPlate", HeaderText = "Number Plate", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "Customer", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Priority", HeaderText = "Priority", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "ScheduledTime", HeaderText = "Scheduled", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "OperatorName", HeaderText = "Operator", Width = 100 }
            });
            
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40 };
            var btnStartJob = new Button { Text = "Start Job", Width = 100, Name = "btnStartJob" };
            var btnStopJob = new Button { Text = "Stop Job", Width = 100, Name = "btnStopJob" };
            var btnDeleteJob = new Button { Text = "Delete Job", Width = 100, Name = "btnDeleteJob" };
            var btnRefresh = new Button { Text = "Refresh", Width = 100, Name = "btnRefresh" };
            
            buttonPanel.Controls.AddRange(new Control[] { btnStartJob, btnStopJob, btnDeleteJob, btnRefresh });
            
            tab.Controls.Add(dgvJobs);
            tab.Controls.Add(buttonPanel);
            
            return tab;
        }

        private TabPage CreateControllerStatusTab()
        {
            var tab = new TabPage("Controller Status");
            
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            
            // Left Panel - Connection Status
            var connectionPanel = new Panel { Dock = DockStyle.Fill };
            var connectionLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(10)
            };
            
            connectionLayout.Controls.Add(new Label { Text = "Controller:", Anchor = AnchorStyles.Left }, 0, 0);
            var cboController = new ComboBox { Dock = DockStyle.Fill, Name = "cboController" };
            connectionLayout.Controls.Add(cboController, 1, 0);
            
            connectionLayout.Controls.Add(new Label { Text = "Status:", Anchor = AnchorStyles.Left }, 0, 1);
            var lblStatus = new Label { Dock = DockStyle.Fill, Name = "lblStatus", Text = "Disconnected", ForeColor = Color.Red };
            connectionLayout.Controls.Add(lblStatus, 1, 1);
            
            connectionLayout.Controls.Add(new Label { Text = "IP Address:", Anchor = AnchorStyles.Left }, 0, 2);
            var lblIpAddress = new Label { Dock = DockStyle.Fill, Name = "lblIpAddress", Text = "N/A" };
            connectionLayout.Controls.Add(lblIpAddress, 1, 2);
            
            connectionLayout.Controls.Add(new Label { Text = "Serial Number:", Anchor = AnchorStyles.Left }, 0, 3);
            var lblSerialNumber = new Label { Dock = DockStyle.Fill, Name = "lblSerialNumber", Text = "N/A" };
            connectionLayout.Controls.Add(lblSerialNumber, 1, 3);
            
            connectionLayout.Controls.Add(new Label { Text = "Firmware:", Anchor = AnchorStyles.Left }, 0, 4);
            var lblFirmware = new Label { Dock = DockStyle.Fill, Name = "lblFirmware", Text = "N/A" };
            connectionLayout.Controls.Add(lblFirmware, 1, 4);
            
            connectionLayout.Controls.Add(new Label { Text = "Temperature:", Anchor = AnchorStyles.Left }, 0, 5);
            var lblTemperature = new Label { Dock = DockStyle.Fill, Name = "lblTemperature", Text = "N/A" };
            connectionLayout.Controls.Add(lblTemperature, 1, 5);
            
            connectionLayout.Controls.Add(new Label { Text = "Voltage:", Anchor = AnchorStyles.Left }, 0, 6);
            var lblVoltage = new Label { Dock = DockStyle.Fill, Name = "lblVoltage", Text = "N/A" };
            connectionLayout.Controls.Add(lblVoltage, 1, 6);
            
            var btnConnect = new Button { Text = "Connect", Width = 100, Name = "btnConnect" };
            var btnDisconnect = new Button { Text = "Disconnect", Width = 100, Name = "btnDisconnect" };
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40 };
            buttonPanel.Controls.AddRange(new Control[] { btnConnect, btnDisconnect });
            connectionLayout.Controls.Add(buttonPanel, 0, 7);
            connectionLayout.SetColumnSpan(buttonPanel, 2);
            
            connectionPanel.Controls.Add(connectionLayout);
            layout.Controls.Add(connectionPanel, 0, 0);
            
            // Right Panel - System Info
            var infoPanel = new Panel { Dock = DockStyle.Fill };
            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                Padding = new Padding(10)
            };
            
            var txtSystemInfo = new TextBox { Dock = DockStyle.Fill, Name = "txtSystemInfo", Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            infoLayout.Controls.Add(txtSystemInfo, 0, 0);
            
            infoPanel.Controls.Add(infoLayout);
            layout.Controls.Add(infoPanel, 1, 0);
            
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateSettingsTab()
        {
            var tab = new TabPage("Settings");
            
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            
            // Left Panel - Settings Tree
            var treeSettings = new TreeView { Dock = DockStyle.Fill, Name = "treeSettings" };
            layout.Controls.Add(treeSettings, 0, 0);
            
            // Right Panel - Settings Details
            var settingsPanel = new Panel { Dock = DockStyle.Fill };
            var settingsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(10)
            };
            
            // Add some sample settings
            settingsLayout.Controls.Add(new Label { Text = "Default Controller:", Anchor = AnchorStyles.Left }, 0, 0);
            var cboDefaultController = new ComboBox { Dock = DockStyle.Fill, Name = "cboDefaultController" };
            cboDefaultController.Items.AddRange(new object[] { "RTC3", "RTC4" });
            settingsLayout.Controls.Add(cboDefaultController, 1, 0);
            
            settingsLayout.Controls.Add(new Label { Text = "Default Power (W):", Anchor = AnchorStyles.Left }, 0, 1);
            var numDefaultPower = new NumericUpDown { Dock = DockStyle.Fill, Name = "numDefaultPower", DecimalPlaces = 1, Increment = 0.5m, Maximum = 1000, Value = 50 };
            settingsLayout.Controls.Add(numDefaultPower, 1, 1);
            
            settingsLayout.Controls.Add(new Label { Text = "Default Speed (mm/s):", Anchor = AnchorStyles.Left }, 0, 2);
            var numDefaultSpeed = new NumericUpDown { Dock = DockStyle.Fill, Name = "numDefaultSpeed", DecimalPlaces = 1, Increment = 10m, Maximum = 10000, Value = 1000 };
            settingsLayout.Controls.Add(numDefaultSpeed, 1, 2);
            
            settingsLayout.Controls.Add(new Label { Text = "Company Name:", Anchor = AnchorStyles.Left }, 0, 3);
            var txtCompanyName = new TextBox { Dock = DockStyle.Fill, Name = "txtCompanyName", Text = "Shimnit India" };
            settingsLayout.Controls.Add(txtCompanyName, 1, 3);
            
            settingsLayout.Controls.Add(new Label { Text = "Log Level:", Anchor = AnchorStyles.Left }, 0, 4);
            var cboLogLevel = new ComboBox { Dock = DockStyle.Fill, Name = "cboLogLevel" };
            cboLogLevel.Items.AddRange(new object[] { "Debug", "Information", "Warning", "Error" });
            cboLogLevel.SelectedIndex = 1;
            settingsLayout.Controls.Add(cboLogLevel, 1, 4);
            
            var btnSaveSettings = new Button { Text = "Save Settings", Width = 100, Name = "btnSaveSettings" };
            var btnResetSettings = new Button { Text = "Reset to Default", Width = 100, Name = "btnResetSettings" };
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40 };
            buttonPanel.Controls.AddRange(new Control[] { btnSaveSettings, btnResetSettings });
            settingsLayout.Controls.Add(buttonPanel, 0, 5);
            settingsLayout.SetColumnSpan(buttonPanel, 2);
            
            settingsPanel.Controls.Add(settingsLayout);
            layout.Controls.Add(settingsPanel, 1, 0);
            
            tab.Controls.Add(layout);
            return tab;
        }

        private void InitializeEventHandlers()
        {
            // Menu event handlers
            var btnCreateJob = this.FindControl<Button>("btnCreateJob");
            if (btnCreateJob != null) btnCreateJob.Click += OnCreateJob;
            
            var btnPreview = this.FindControl<Button>("btnPreview");
            if (btnPreview != null) btnPreview.Click += OnPreviewJob;
            
            var btnConnect = this.FindControl<Button>("btnConnect");
            if (btnConnect != null) btnConnect.Click += OnConnectController;
            
            var btnDisconnect = this.FindControl<Button>("btnDisconnect");
            if (btnDisconnect != null) btnDisconnect.Click += OnDisconnectController;
            
            var btnStartJob = this.FindControl<Button>("btnStartJob");
            if (btnStartJob != null) btnStartJob.Click += OnStartJob;
            
            var btnStopJob = this.FindControl<Button>("btnStopJob");
            if (btnStopJob != null) btnStopJob.Click += OnStopJob;
            
            var btnDeleteJob = this.FindControl<Button>("btnDeleteJob");
            if (btnDeleteJob != null) btnDeleteJob.Click += OnDeleteJob;
            
            var btnRefresh = this.FindControl<Button>("btnRefresh");
            if (btnRefresh != null) btnRefresh.Click += OnRefreshJobs;
            
            var btnSaveSettings = this.FindControl<Button>("btnSaveSettings");
            if (btnSaveSettings != null) btnSaveSettings.Click += OnSaveSettings;
            
            var btnResetSettings = this.FindControl<Button>("btnResetSettings");
            if (btnResetSettings != null) btnResetSettings.Click += OnResetSettings;
            
            // Form closing event
            this.FormClosing += OnFormClosing;
        }

        private void LoadData()
        {
            try
            {
                // Load controllers
                var controllers = _databaseService.GetLaserControllers();
                var cboController = this.FindControl<ComboBox>("cboController");
                if (cboController != null)
                {
                    cboController.Items.Clear();
                    cboController.Items.AddRange(controllers.ToArray());
                    if (controllers.Any())
                        cboController.SelectedIndex = 0;
                }
                
                // Load parameters
                _parameters = _databaseService.GetLaserParameters();
                var cboParameterSet = this.FindControl<ComboBox>("cboParameterSet");
                if (cboParameterSet != null)
                {
                    cboParameterSet.Items.Clear();
                    cboParameterSet.Items.AddRange(_parameters.ToArray());
                    if (_parameters.Any())
                        cboParameterSet.SelectedIndex = 0;
                }
                
                // Load jobs
                RefreshJobQueue();
                
                _logger.Information("Data loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load data");
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshJobQueue()
        {
            try
            {
                _jobs = _databaseService.GetMarkingJobs();
                var dgvJobs = this.FindControl<DataGridView>("dgvJobs");
                if (dgvJobs != null)
                {
                    dgvJobs.Rows.Clear();
                    foreach (var job in _jobs)
                    {
                        dgvJobs.Rows.Add(
                            job.Id,
                            job.JobName,
                            job.NumberPlate,
                            job.CustomerName,
                            job.Status,
                            job.Priority,
                            job.ScheduledTime.ToString("yyyy-MM-dd HH:mm"),
                            job.OperatorName
                        );
                    }
                }
                
                // Update job count in status bar
                var statusStrip = this.Controls.OfType<StatusStrip>().FirstOrDefault();
                if (statusStrip != null)
                {
                    var lblJobs = statusStrip.Items.OfType<ToolStripStatusLabel>().FirstOrDefault(x => x.Text.StartsWith("Jobs:"));
                    if (lblJobs != null)
                        lblJobs.Text = $"Jobs: {_jobs.Count}";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to refresh job queue");
            }
        }

        // Event Handlers
        private void OnNewJob(object? sender, EventArgs e)
        {
            // Clear job creation form
            var txtJobName = this.FindControl<TextBox>("txtJobName");
            if (txtJobName != null) txtJobName.Clear();
            
            var txtNumberPlate = this.FindControl<TextBox>("txtNumberPlate");
            if (txtNumberPlate != null) txtNumberPlate.Clear();
            
            var cboVehicleType = this.FindControl<ComboBox>("cboVehicleType");
            if (cboVehicleType != null) cboVehicleType.SelectedIndex = -1;
            
            var txtCustomerName = this.FindControl<TextBox>("txtCustomerName");
            if (txtCustomerName != null) txtCustomerName.Clear();
            
            var txtMarkingText = this.FindControl<TextBox>("txtMarkingText");
            if (txtMarkingText != null) txtMarkingText.Clear();
            
            var numPriority = this.FindControl<NumericUpDown>("numPriority");
            if (numPriority != null) numPriority.Value = 5;
            
            var txtOperator = this.FindControl<TextBox>("txtOperator");
            if (txtOperator != null) txtOperator.Clear();
        }

        private void OnOpenJob(object? sender, EventArgs e)
        {
            // Implementation for opening existing job
            MessageBox.Show("Open Job functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSaveJob(object? sender, EventArgs e)
        {
            // Implementation for saving job
            MessageBox.Show("Save Job functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnExportReport(object? sender, EventArgs e)
        {
            // Implementation for exporting reports
            MessageBox.Show("Export Report functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnExit(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnControllerSettings(object? sender, EventArgs e)
        {
            // Implementation for controller settings
            MessageBox.Show("Controller Settings functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnParameterLibrary(object? sender, EventArgs e)
        {
            // Implementation for parameter library
            MessageBox.Show("Parameter Library functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnPatternEditor(object? sender, EventArgs e)
        {
            // Implementation for pattern editor
            MessageBox.Show("Pattern Editor functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSystemSettings(object? sender, EventArgs e)
        {
            // Implementation for system settings
            MessageBox.Show("System Settings functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnViewJobQueue(object? sender, EventArgs e)
        {
            // Switch to job queue tab
            var tabControl = this.Controls.OfType<TabControl>().FirstOrDefault();
            if (tabControl != null)
                tabControl.SelectedIndex = 1;
        }

        private void OnViewHistory(object? sender, EventArgs e)
        {
            // Implementation for viewing history
            MessageBox.Show("History functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnViewStatistics(object? sender, EventArgs e)
        {
            // Implementation for viewing statistics
            MessageBox.Show("Statistics functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnUserManual(object? sender, EventArgs e)
        {
            // Implementation for user manual
            MessageBox.Show("User Manual functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnAbout(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Shimnit Laser Marking System v1.0\n\n" +
                "Automated Laser Code Marking System for Vehicle Number Plates\n\n" +
                "© 2024 Shimnit India. All rights reserved.",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private async void OnConnectController(object? sender, EventArgs e)
        {
            try
            {
                var cboController = this.FindControl<ComboBox>("cboController");
                if (cboController?.SelectedItem is LaserController controller)
                {
                    var success = await _laserService.ConnectAsync(controller);
                    if (success)
                    {
                        _currentController = controller;
                        UpdateControllerStatus(controller);
                        _logger.Information("Connected to controller: {ControllerName}", controller.Name);
                    }
                    else
                    {
                        MessageBox.Show("Failed to connect to controller.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to connect to controller");
                MessageBox.Show($"Failed to connect to controller: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDisconnectController(object? sender, EventArgs e)
        {
            try
            {
                _laserService.Disconnect();
                _currentController = null;
                UpdateControllerStatus(null);
                _logger.Information("Disconnected from controller");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to disconnect from controller");
            }
        }

        private void UpdateControllerStatus(LaserController? controller)
        {
            var lblStatus = this.FindControl<Label>("lblStatus");
            var lblIpAddress = this.FindControl<Label>("lblIpAddress");
            var lblSerialNumber = this.FindControl<Label>("lblSerialNumber");
            var lblFirmware = this.FindControl<Label>("lblFirmware");
            
            if (controller != null)
            {
                lblStatus?.SetProperty("Text", controller.Status.ToString());
                lblStatus?.SetProperty("ForeColor", controller.IsConnected ? Color.Green : Color.Red);
                lblIpAddress?.SetProperty("Text", controller.IpAddress);
                lblSerialNumber?.SetProperty("Text", controller.SerialNumber);
                lblFirmware?.SetProperty("Text", controller.FirmwareVersion);
            }
            else
            {
                lblStatus?.SetProperty("Text", "Disconnected");
                lblStatus?.SetProperty("ForeColor", Color.Red);
                lblIpAddress?.SetProperty("Text", "N/A");
                lblSerialNumber?.SetProperty("Text", "N/A");
                lblFirmware?.SetProperty("Text", "N/A");
            }
        }

        private async void OnCreateJob(object? sender, EventArgs e)
        {
            try
            {
                var job = new MarkingJob
                {
                    JobName = this.FindControl<TextBox>("txtJobName")?.Text ?? "",
                    NumberPlate = this.FindControl<TextBox>("txtNumberPlate")?.Text ?? "",
                    VehicleType = this.FindControl<ComboBox>("cboVehicleType")?.Text ?? "",
                    CustomerName = this.FindControl<TextBox>("txtCustomerName")?.Text ?? "",
                    MarkingText = this.FindControl<TextBox>("txtMarkingText")?.Text ?? "",
                    Priority = (int)(this.FindControl<NumericUpDown>("numPriority")?.Value ?? 5),
                    OperatorName = this.FindControl<TextBox>("txtOperator")?.Text ?? "",
                    Status = JobStatus.Pending,
                    ScheduledTime = DateTime.Now
                };
                
                // Get laser parameters
                var parameters = new LaserParameters
                {
                    Name = "Job Parameters",
                    Power = (double)(this.FindControl<NumericUpDown>("numPower")?.Value ?? 50m),
                    Speed = (double)(this.FindControl<NumericUpDown>("numSpeed")?.Value ?? 1000m),
                    Frequency = (double)(this.FindControl<NumericUpDown>("numFrequency")?.Value ?? 20000m),
                    PulseWidth = (double)(this.FindControl<NumericUpDown>("numPulseWidth")?.Value ?? 10m),
                    FocusPosition = (double)(this.FindControl<NumericUpDown>("numFocusPosition")?.Value ?? 0m),
                    MarkingDepth = (double)(this.FindControl<NumericUpDown>("numMarkingDepth")?.Value ?? 0.1m),
                    Repetitions = (int)(this.FindControl<NumericUpDown>("numRepetitions")?.Value ?? 1m),
                    SpotSize = (double)(this.FindControl<NumericUpDown>("numSpotSize")?.Value ?? 0.1m),
                    EnableGalvo = this.FindControl<CheckBox>("chkEnableGalvo")?.Checked ?? false,
                    GalvoDelay = (double)(this.FindControl<NumericUpDown>("numGalvoDelay")?.Value ?? 0.1m),
                    Material = this.FindControl<ComboBox>("cboMaterial")?.Text ?? "Aluminum",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };
                
                job.Parameters = parameters;
                
                _databaseService.SaveMarkingJob(job);
                RefreshJobQueue();
                
                MessageBox.Show("Job created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnNewJob(sender, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create job");
                MessageBox.Show($"Failed to create job: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnPreviewJob(object? sender, EventArgs e)
        {
            // Implementation for job preview
            MessageBox.Show("Job preview functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void OnStartJob(object? sender, EventArgs e)
        {
            try
            {
                var dgvJobs = this.FindControl<DataGridView>("dgvJobs");
                if (dgvJobs?.CurrentRow != null)
                {
                    var jobId = (int)dgvJobs.CurrentRow.Cells["Id"].Value;
                    var job = _jobs.FirstOrDefault(j => j.Id == jobId);
                    
                    if (job != null && _currentController != null)
                    {
                        // Set laser parameters
                        var success = await _laserService.SetLaserParametersAsync(job.Parameters, _currentController.Type);
                        if (success)
                        {
                            // Start marking
                            success = await _laserService.StartMarkingAsync(job.MarkingText, _currentController.Type);
                            if (success)
                            {
                                job.Status = JobStatus.InProgress;
                                _databaseService.SaveMarkingJob(job);
                                RefreshJobQueue();
                                _isMarking = true;
                                
                                                                 // Update status in status bar
                                 var statusStrip = this.Controls.OfType<StatusStrip>().FirstOrDefault();
                                 if (statusStrip != null)
                                 {
                                     var lblStatus = statusStrip.Items.OfType<ToolStripStatusLabel>().FirstOrDefault();
                                     if (lblStatus != null)
                                         lblStatus.Text = $"Marking job: {job.JobName}";
                                 }
                                
                                MessageBox.Show("Job started successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Failed to start marking.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to set laser parameters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a job and ensure controller is connected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start job");
                MessageBox.Show($"Failed to start job: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnStopJob(object? sender, EventArgs e)
        {
            try
            {
                if (_currentController != null && _isMarking)
                {
                    var success = await _laserService.StopMarkingAsync(_currentController.Type);
                    if (success)
                    {
                        _isMarking = false;
                        
                                                 // Update status in status bar
                         var statusStrip = this.Controls.OfType<StatusStrip>().FirstOrDefault();
                         if (statusStrip != null)
                         {
                             var lblStatus = statusStrip.Items.OfType<ToolStripStatusLabel>().FirstOrDefault();
                             if (lblStatus != null)
                                 lblStatus.Text = "Ready";
                         }
                        
                        MessageBox.Show("Job stopped successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to stop marking.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop job");
                MessageBox.Show($"Failed to stop job: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDeleteJob(object? sender, EventArgs e)
        {
            // Implementation for deleting job
            MessageBox.Show("Delete job functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnRefreshJobs(object? sender, EventArgs e)
        {
            RefreshJobQueue();
        }

        private void OnSaveSettings(object? sender, EventArgs e)
        {
            try
            {
                var defaultController = this.FindControl<ComboBox>("cboDefaultController")?.Text ?? "RTC4";
                var defaultPower = this.FindControl<NumericUpDown>("numDefaultPower")?.Value.ToString() ?? "50";
                var defaultSpeed = this.FindControl<NumericUpDown>("numDefaultSpeed")?.Value.ToString() ?? "1000";
                var companyName = this.FindControl<TextBox>("txtCompanyName")?.Text ?? "Shimnit India";
                var logLevel = this.FindControl<ComboBox>("cboLogLevel")?.Text ?? "Information";
                
                _databaseService.SetSetting("DefaultController", defaultController);
                _databaseService.SetSetting("DefaultPower", defaultPower);
                _databaseService.SetSetting("DefaultSpeed", defaultSpeed);
                _databaseService.SetSetting("CompanyName", companyName);
                _databaseService.SetSetting("LogLevel", logLevel);
                
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save settings");
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnResetSettings(object? sender, EventArgs e)
        {
            // Implementation for resetting settings
            MessageBox.Show("Reset settings functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnFormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                _laserService.Disconnect();
                _logger.Information("Application closing");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during application shutdown");
            }
        }
    }

    // Extension methods for easier control access
    public static class ControlExtensions
    {
        public static T? FindControl<T>(this Control parent, string name) where T : Control
        {
            return parent.Controls.OfType<T>().FirstOrDefault(c => c.Name == name);
        }

        public static void SetProperty(this Control control, string propertyName, object value)
        {
            var property = control.GetType().GetProperty(propertyName);
            property?.SetValue(control, value);
        }
    }
}