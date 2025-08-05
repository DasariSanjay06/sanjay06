using ShimnitLaserMarking.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShimnitLaserMarking.Forms
{
    public partial class SystemLogsForm : Form
    {
        private DataGridView dataGridView = null!;
        private ComboBox cmbLevelFilter = null!;
        private DateTimePicker dtpFromDate = null!;
        private DateTimePicker dtpToDate = null!;
        private Button btnSearch = null!;
        private Button btnClear = null!;

        public SystemLogsForm()
        {
            InitializeComponent();
            LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "System Logs";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            CreateFilterPanel();
            CreateDataGrid();

            this.ResumeLayout(false);
        }

        private void CreateFilterPanel()
        {
            var filterPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            var lblLevel = new Label
            {
                Text = "Level:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            cmbLevelFilter = new ComboBox
            {
                Location = new Point(60, 17),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLevelFilter.Items.AddRange(new[] { "All", "Information", "Warning", "Error", "Fatal" });
            cmbLevelFilter.SelectedIndex = 0;

            var lblFromDate = new Label
            {
                Text = "From:",
                Location = new Point(200, 20),
                AutoSize = true
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(240, 17),
                Width = 120,
                Value = DateTime.Today.AddDays(-7)
            };

            var lblToDate = new Label
            {
                Text = "To:",
                Location = new Point(380, 20),
                AutoSize = true
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(410, 17),
                Width = 120,
                Value = DateTime.Today
            };

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(550, 15),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSearch.Click += BtnSearch_Click;

            btnClear = new Button
            {
                Text = "Clear Old",
                Location = new Point(640, 15),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClear.Click += BtnClear_Click;

            var btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(730, 15),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadDataAsync();

            filterPanel.Controls.AddRange(new Control[] 
            { 
                lblLevel, cmbLevelFilter, lblFromDate, dtpFromDate, 
                lblToDate, dtpToDate, btnSearch, btnClear, btnRefresh 
            });

            this.Controls.Add(filterPanel);
        }

        private void CreateDataGrid()
        {
            dataGridView = new DataGridView
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
            dataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Timestamp", HeaderText = "Timestamp", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm:ss" } },
                new DataGridViewTextBoxColumn { Name = "Level", HeaderText = "Level", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "Source", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "Message", Width = 400, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "UserName", HeaderText = "User", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "MachineName", HeaderText = "Machine", Width = 100 }
            });

            // Color code log levels
            dataGridView.CellFormatting += DataGridView_CellFormatting;

            this.Controls.Add(dataGridView);
        }

        private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "Level" && e.Value != null)
            {
                var level = e.Value.ToString();
                switch (level)
                {
                    case "Error":
                    case "Fatal":
                        e.CellStyle.BackColor = Color.LightCoral;
                        e.CellStyle.ForeColor = Color.DarkRed;
                        break;
                    case "Warning":
                        e.CellStyle.BackColor = Color.LightYellow;
                        e.CellStyle.ForeColor = Color.DarkOrange;
                        break;
                    case "Information":
                        e.CellStyle.BackColor = Color.LightBlue;
                        e.CellStyle.ForeColor = Color.DarkBlue;
                        break;
                }
            }
        }

        private async void LoadDataAsync()
        {
            try
            {
                var logs = await DatabaseService.GetSystemLogsAsync(limit: 1000);
                dataGridView.DataSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading system logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                var level = cmbLevelFilter.SelectedIndex > 0 ? cmbLevelFilter.Text : null;
                var logs = await DatabaseService.GetSystemLogsAsync(level, 1000);

                // Apply date filter
                logs = logs.Where(l => l.Timestamp.Date >= dtpFromDate.Value.Date && 
                                      l.Timestamp.Date <= dtpToDate.Value.Date).ToList();

                dataGridView.DataSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnClear_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show("This will delete logs older than 30 days. Continue?", 
                    "Clear Old Logs", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    await DatabaseService.CleanupOldLogsAsync(30);
                    MessageBox.Show("Old logs cleared successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}