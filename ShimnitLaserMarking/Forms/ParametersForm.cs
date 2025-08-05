using ShimnitLaserMarking.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShimnitLaserMarking.Forms
{
    public partial class ParametersForm : Form
    {
        private LaserParameters _parameters;
        private LaserControllerType _controllerType;

        public LaserParameters Parameters => _parameters;

        public ParametersForm(LaserControllerType controllerType = LaserControllerType.RTC3)
        {
            _controllerType = controllerType;
            _parameters = controllerType == LaserControllerType.RTC4 
                ? LaserParameters.GetDefaultRTC4Parameters() 
                : LaserParameters.GetDefaultRTC3Parameters();
            
            InitializeComponent();
            LoadParametersToControls();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = $"Laser Parameters Configuration - {_controllerType}";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateParameterControls();
            CreateButtons();

            this.ResumeLayout(false);
        }

        private void CreateParameterControls()
        {
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(10, 10)
            };

            // Basic Parameters Tab
            var basicTab = new TabPage("Basic Parameters");
            CreateBasicParametersTab(basicTab);

            // Advanced Parameters Tab
            var advancedTab = new TabPage("Advanced Parameters");
            CreateAdvancedParametersTab(advancedTab);

            // Position & Movement Tab
            var positionTab = new TabPage("Position & Movement");
            CreatePositionParametersTab(positionTab);

            if (_controllerType == LaserControllerType.RTC4)
            {
                // RTC4 Specific Tab
                var rtc4Tab = new TabPage("RTC4 Features");
                CreateRTC4SpecificTab(rtc4Tab);
                tabControl.TabPages.Add(rtc4Tab);
            }

            tabControl.TabPages.AddRange(new[] { basicTab, advancedTab, positionTab });
            this.Controls.Add(tabControl);
        }

        private void CreateBasicParametersTab(TabPage tab)
        {
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(20)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Power
            tableLayout.Controls.Add(new Label { Text = "Power (%):", Anchor = AnchorStyles.Left }, 0, 0);
            numPower = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 1, Minimum = 0, Maximum = 100 };
            tableLayout.Controls.Add(numPower, 1, 0);

            // Speed
            tableLayout.Controls.Add(new Label { Text = "Speed (mm/min):", Anchor = AnchorStyles.Left }, 0, 1);
            numSpeed = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 10000 };
            tableLayout.Controls.Add(numSpeed, 1, 1);

            // Pulse Frequency
            tableLayout.Controls.Add(new Label { Text = "Pulse Freq (Hz):", Anchor = AnchorStyles.Left }, 0, 2);
            numPulseFrequency = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1000, Maximum = 100000 };
            tableLayout.Controls.Add(numPulseFrequency, 1, 2);

            // Passes
            tableLayout.Controls.Add(new Label { Text = "Passes:", Anchor = AnchorStyles.Left }, 0, 3);
            numPasses = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 20 };
            tableLayout.Controls.Add(numPasses, 1, 3);

            // Marking Depth
            tableLayout.Controls.Add(new Label { Text = "Marking Depth (mm):", Anchor = AnchorStyles.Left }, 0, 4);
            numMarkingDepth = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 3, Minimum = 0.001M, Maximum = 10 };
            tableLayout.Controls.Add(numMarkingDepth, 1, 4);

            tab.Controls.Add(tableLayout);
        }

        private void CreateAdvancedParametersTab(TabPage tab)
        {
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(20)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Scanner Speed
            tableLayout.Controls.Add(new Label { Text = "Scanner Speed:", Anchor = AnchorStyles.Left }, 0, 0);
            numScannerSpeed = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 1000 };
            tableLayout.Controls.Add(numScannerSpeed, 1, 0);

            // Scanner Acceleration
            tableLayout.Controls.Add(new Label { Text = "Scanner Accel:", Anchor = AnchorStyles.Left }, 0, 1);
            numScannerAcceleration = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 1000 };
            tableLayout.Controls.Add(numScannerAcceleration, 1, 1);

            // Jump Delay
            tableLayout.Controls.Add(new Label { Text = "Jump Delay (μs):", Anchor = AnchorStyles.Left }, 0, 2);
            numJumpDelay = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10000 };
            tableLayout.Controls.Add(numJumpDelay, 1, 2);

            // Mark Delay
            tableLayout.Controls.Add(new Label { Text = "Mark Delay (μs):", Anchor = AnchorStyles.Left }, 0, 3);
            numMarkDelay = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10000 };
            tableLayout.Controls.Add(numMarkDelay, 1, 3);

            // Polygon Delay
            tableLayout.Controls.Add(new Label { Text = "Polygon Delay (μs):", Anchor = AnchorStyles.Left }, 0, 4);
            numPolygonDelay = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10000 };
            tableLayout.Controls.Add(numPolygonDelay, 1, 4);

            // Quality Factor
            tableLayout.Controls.Add(new Label { Text = "Quality Factor:", Anchor = AnchorStyles.Left }, 0, 5);
            numQualityFactor = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 10 };
            tableLayout.Controls.Add(numQualityFactor, 1, 5);

            // Temperature Compensation
            tableLayout.Controls.Add(new Label { Text = "Temp Compensation:", Anchor = AnchorStyles.Left }, 0, 6);
            chkTemperatureCompensation = new CheckBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(chkTemperatureCompensation, 1, 6);

            tab.Controls.Add(tableLayout);
        }

        private void CreatePositionParametersTab(TabPage tab)
        {
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(20)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Start Position X
            tableLayout.Controls.Add(new Label { Text = "Start Position X:", Anchor = AnchorStyles.Left }, 0, 0);
            numStartPositionX = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 3, Minimum = -1000, Maximum = 1000 };
            tableLayout.Controls.Add(numStartPositionX, 1, 0);

            // Start Position Y
            tableLayout.Controls.Add(new Label { Text = "Start Position Y:", Anchor = AnchorStyles.Left }, 0, 1);
            numStartPositionY = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 3, Minimum = -1000, Maximum = 1000 };
            tableLayout.Controls.Add(numStartPositionY, 1, 1);

            // Focus Position
            tableLayout.Controls.Add(new Label { Text = "Focus Position:", Anchor = AnchorStyles.Left }, 0, 2);
            numFocusPosition = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 3, Minimum = -100, Maximum = 100 };
            tableLayout.Controls.Add(numFocusPosition, 1, 2);

            // Rotation
            tableLayout.Controls.Add(new Label { Text = "Rotation (°):", Anchor = AnchorStyles.Left }, 0, 3);
            numRotation = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Minimum = 0, Maximum = 360 };
            tableLayout.Controls.Add(numRotation, 1, 3);

            // Line Spacing
            tableLayout.Controls.Add(new Label { Text = "Line Spacing:", Anchor = AnchorStyles.Left }, 0, 4);
            numLineSpacing = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 3, Minimum = 0.001M, Maximum = 1 };
            tableLayout.Controls.Add(numLineSpacing, 1, 4);

            tab.Controls.Add(tableLayout);
        }

        private void CreateRTC4SpecificTab(TabPage tab)
        {
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(20)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // 3D Mode
            tableLayout.Controls.Add(new Label { Text = "3D Mode:", Anchor = AnchorStyles.Left }, 0, 0);
            chk3DMode = new CheckBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(chk3DMode, 1, 0);

            // Z-Axis Position
            tableLayout.Controls.Add(new Label { Text = "Z-Axis Position:", Anchor = AnchorStyles.Left }, 0, 1);
            numZAxisPosition = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 3, Minimum = -100, Maximum = 100 };
            tableLayout.Controls.Add(numZAxisPosition, 1, 1);

            // Optical Power Control
            tableLayout.Controls.Add(new Label { Text = "Optical Power Control:", Anchor = AnchorStyles.Left }, 0, 2);
            chkOpticalPowerControl = new CheckBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(chkOpticalPowerControl, 1, 2);

            // Pulse Width
            tableLayout.Controls.Add(new Label { Text = "Pulse Width (μs):", Anchor = AnchorStyles.Left }, 0, 3);
            numPulseWidth = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1000 };
            tableLayout.Controls.Add(numPulseWidth, 1, 3);

            // Duty Cycle
            tableLayout.Controls.Add(new Label { Text = "Duty Cycle (%):", Anchor = AnchorStyles.Left }, 0, 4);
            numDutyCycle = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 1, Minimum = 0, Maximum = 100 };
            tableLayout.Controls.Add(numDutyCycle, 1, 4);

            // Wobble Parameters
            var wobbleGroup = new GroupBox { Text = "Wobble Parameters", Height = 120 };
            var wobbleTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };

            wobbleTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            wobbleTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            wobbleTable.Controls.Add(new Label { Text = "Enable Wobble:", Anchor = AnchorStyles.Left }, 0, 0);
            chkWobbleEnabled = new CheckBox { Dock = DockStyle.Fill };
            wobbleTable.Controls.Add(chkWobbleEnabled, 1, 0);

            wobbleTable.Controls.Add(new Label { Text = "Amplitude:", Anchor = AnchorStyles.Left }, 0, 1);
            numWobbleAmplitude = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Minimum = 0, Maximum = 100 };
            wobbleTable.Controls.Add(numWobbleAmplitude, 1, 1);

            wobbleTable.Controls.Add(new Label { Text = "Frequency:", Anchor = AnchorStyles.Left }, 0, 2);
            numWobbleFrequency = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 1000 };
            wobbleTable.Controls.Add(numWobbleFrequency, 1, 2);

            wobbleGroup.Controls.Add(wobbleTable);
            tableLayout.Controls.Add(wobbleGroup, 0, 5);
            tableLayout.SetColumnSpan(wobbleGroup, 2);

            tab.Controls.Add(tableLayout);
        }

        private void CreateButtons()
        {
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom
            };

            var btnOK = new Button
            {
                Text = "OK",
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.Location = new Point(this.Width - 200, 10);
            btnOK.Click += BtnOK_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Location = new Point(this.Width - 110, 10);

            var btnDefaults = new Button
            {
                Text = "Reset to Defaults",
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnDefaults.Location = new Point(20, 10);
            btnDefaults.Click += BtnDefaults_Click;

            buttonPanel.Controls.AddRange(new[] { btnOK, btnCancel, btnDefaults });
            this.Controls.Add(buttonPanel);
        }

        // Control declarations
        private NumericUpDown numPower = null!;
        private NumericUpDown numSpeed = null!;
        private NumericUpDown numPulseFrequency = null!;
        private NumericUpDown numPasses = null!;
        private NumericUpDown numMarkingDepth = null!;
        private NumericUpDown numScannerSpeed = null!;
        private NumericUpDown numScannerAcceleration = null!;
        private NumericUpDown numJumpDelay = null!;
        private NumericUpDown numMarkDelay = null!;
        private NumericUpDown numPolygonDelay = null!;
        private NumericUpDown numQualityFactor = null!;
        private CheckBox chkTemperatureCompensation = null!;
        private NumericUpDown numStartPositionX = null!;
        private NumericUpDown numStartPositionY = null!;
        private NumericUpDown numFocusPosition = null!;
        private NumericUpDown numRotation = null!;
        private NumericUpDown numLineSpacing = null!;
        private CheckBox chk3DMode = null!;
        private NumericUpDown numZAxisPosition = null!;
        private CheckBox chkOpticalPowerControl = null!;
        private NumericUpDown numPulseWidth = null!;
        private NumericUpDown numDutyCycle = null!;
        private CheckBox chkWobbleEnabled = null!;
        private NumericUpDown numWobbleAmplitude = null!;
        private NumericUpDown numWobbleFrequency = null!;

        private void LoadParametersToControls()
        {
            numPower.Value = (decimal)_parameters.Power;
            numSpeed.Value = _parameters.Speed;
            numPulseFrequency.Value = _parameters.PulseFrequency;
            numPasses.Value = _parameters.Passes;
            numMarkingDepth.Value = (decimal)_parameters.MarkingDepth;
            numScannerSpeed.Value = _parameters.ScannerSpeed;
            numScannerAcceleration.Value = _parameters.ScannerAcceleration;
            numJumpDelay.Value = _parameters.JumpDelay;
            numMarkDelay.Value = _parameters.MarkDelay;
            numPolygonDelay.Value = _parameters.PolygonDelay;
            numQualityFactor.Value = _parameters.QualityFactor;
            chkTemperatureCompensation.Checked = _parameters.TemperatureCompensation;
            numStartPositionX.Value = (decimal)_parameters.StartPositionX;
            numStartPositionY.Value = (decimal)_parameters.StartPositionY;
            numFocusPosition.Value = (decimal)_parameters.FocusPosition;
            numRotation.Value = (decimal)_parameters.Rotation;
            numLineSpacing.Value = (decimal)_parameters.LineSpacing;

            if (_controllerType == LaserControllerType.RTC4)
            {
                chk3DMode.Checked = _parameters.Is3DMode;
                numZAxisPosition.Value = (decimal)_parameters.ZAxisPosition;
                chkOpticalPowerControl.Checked = _parameters.OpticalPowerControl;
                numPulseWidth.Value = _parameters.PulseWidth;
                numDutyCycle.Value = (decimal)_parameters.DutyCycle;
                chkWobbleEnabled.Checked = _parameters.WobbleEnabled;
                numWobbleAmplitude.Value = (decimal)_parameters.WobbleAmplitude;
                numWobbleFrequency.Value = _parameters.WobbleFrequency;
            }
        }

        private void SaveParametersFromControls()
        {
            _parameters.Power = (double)numPower.Value;
            _parameters.Speed = (int)numSpeed.Value;
            _parameters.PulseFrequency = (int)numPulseFrequency.Value;
            _parameters.Passes = (int)numPasses.Value;
            _parameters.MarkingDepth = (double)numMarkingDepth.Value;
            _parameters.ScannerSpeed = (int)numScannerSpeed.Value;
            _parameters.ScannerAcceleration = (int)numScannerAcceleration.Value;
            _parameters.JumpDelay = (int)numJumpDelay.Value;
            _parameters.MarkDelay = (int)numMarkDelay.Value;
            _parameters.PolygonDelay = (int)numPolygonDelay.Value;
            _parameters.QualityFactor = (int)numQualityFactor.Value;
            _parameters.TemperatureCompensation = chkTemperatureCompensation.Checked;
            _parameters.StartPositionX = (double)numStartPositionX.Value;
            _parameters.StartPositionY = (double)numStartPositionY.Value;
            _parameters.FocusPosition = (double)numFocusPosition.Value;
            _parameters.Rotation = (double)numRotation.Value;
            _parameters.LineSpacing = (double)numLineSpacing.Value;

            if (_controllerType == LaserControllerType.RTC4)
            {
                _parameters.Is3DMode = chk3DMode.Checked;
                _parameters.ZAxisPosition = (double)numZAxisPosition.Value;
                _parameters.OpticalPowerControl = chkOpticalPowerControl.Checked;
                _parameters.PulseWidth = (int)numPulseWidth.Value;
                _parameters.DutyCycle = (double)numDutyCycle.Value;
                _parameters.WobbleEnabled = chkWobbleEnabled.Checked;
                _parameters.WobbleAmplitude = (double)numWobbleAmplitude.Value;
                _parameters.WobbleFrequency = (int)numWobbleFrequency.Value;
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            try
            {
                SaveParametersFromControls();
                
                if (!_parameters.IsValid())
                {
                    MessageBox.Show("Invalid parameter values. Please check your settings.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving parameters: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDefaults_Click(object? sender, EventArgs e)
        {
            _parameters = _controllerType == LaserControllerType.RTC4 
                ? LaserParameters.GetDefaultRTC4Parameters() 
                : LaserParameters.GetDefaultRTC3Parameters();
            
            LoadParametersToControls();
        }
    }
}