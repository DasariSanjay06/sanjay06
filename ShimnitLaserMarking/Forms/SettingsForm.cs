using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShimnitLaserMarking.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Application Settings";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var tabControl = new TabControl { Dock = DockStyle.Fill };

            // General Settings Tab
            var generalTab = new TabPage("General");
            CreateGeneralTab(generalTab);

            // Controller Settings Tab
            var controllerTab = new TabPage("Controllers");
            CreateControllerTab(controllerTab);

            // Database Settings Tab
            var databaseTab = new TabPage("Database");
            CreateDatabaseTab(databaseTab);

            tabControl.TabPages.AddRange(new[] { generalTab, controllerTab, databaseTab });
            this.Controls.Add(tabControl);

            // Buttons
            var buttonPanel = new Panel { Height = 50, Dock = DockStyle.Bottom };
            var btnOK = new Button
            {
                Text = "OK",
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnOK.Location = new Point(this.Width - 180, 10);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White
            };
            btnCancel.Location = new Point(this.Width - 90, 10);

            buttonPanel.Controls.AddRange(new[] { btnOK, btnCancel });
            this.Controls.Add(buttonPanel);

            this.ResumeLayout(false);
        }

        private void CreateGeneralTab(TabPage tab)
        {
            var groupBox = new GroupBox
            {
                Text = "General Settings",
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var lblInfo = new Label
            {
                Text = "Configure general application settings here.\n\nCompany: Shimnit Company India\nVersion: 1.0.0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft
            };

            groupBox.Controls.Add(lblInfo);
            tab.Controls.Add(groupBox);
        }

        private void CreateControllerTab(TabPage tab)
        {
            var groupBox = new GroupBox
            {
                Text = "Laser Controller Settings",
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            tableLayout.Controls.Add(new Label { Text = "RTC3 Port:", Anchor = AnchorStyles.Left }, 0, 0);
            var txtRTC3Port = new TextBox { Text = "COM1", Dock = DockStyle.Fill };
            tableLayout.Controls.Add(txtRTC3Port, 1, 0);

            tableLayout.Controls.Add(new Label { Text = "RTC4 Port:", Anchor = AnchorStyles.Left }, 0, 1);
            var txtRTC4Port = new TextBox { Text = "COM2", Dock = DockStyle.Fill };
            tableLayout.Controls.Add(txtRTC4Port, 1, 1);

            tableLayout.Controls.Add(new Label { Text = "Baud Rate:", Anchor = AnchorStyles.Left }, 0, 2);
            var numBaudRate = new NumericUpDown { Value = 115200, Dock = DockStyle.Fill, Minimum = 9600, Maximum = 921600 };
            tableLayout.Controls.Add(numBaudRate, 1, 2);

            tableLayout.Controls.Add(new Label { Text = "Timeout (ms):", Anchor = AnchorStyles.Left }, 0, 3);
            var numTimeout = new NumericUpDown { Value = 5000, Dock = DockStyle.Fill, Minimum = 1000, Maximum = 30000 };
            tableLayout.Controls.Add(numTimeout, 1, 3);

            groupBox.Controls.Add(tableLayout);
            tab.Controls.Add(groupBox);
        }

        private void CreateDatabaseTab(TabPage tab)
        {
            var groupBox = new GroupBox
            {
                Text = "Database Settings",
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var lblInfo = new Label
            {
                Text = "Database: SQL Server LocalDB\nConnection: Automatic\nBackup: Enabled (24h interval)\n\nAll marking jobs and system logs are automatically stored in the database.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft
            };

            groupBox.Controls.Add(lblInfo);
            tab.Controls.Add(groupBox);
        }
    }
}