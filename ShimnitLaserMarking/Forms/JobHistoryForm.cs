using ShimnitLaserMarking.Models;
using ShimnitLaserMarking.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShimnitLaserMarking.Forms
{
    public partial class JobHistoryForm : Form
    {
        private DataGridView dataGridView = null!;
        private ComboBox cmbStatusFilter = null!;
        private DateTimePicker dtpFromDate = null!;
        private DateTimePicker dtpToDate = null!;
        private Button btnSearch = null!;

        public JobHistoryForm()
        {
            InitializeComponent();
            LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Job History";
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

            var lblStatus = new Label
            {
                Text = "Status:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(60, 17),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatusFilter.Items.AddRange(new[] { "All", "Created", "Queued", "InProgress", "Completed", "Failed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;

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
                Value = DateTime.Today.AddDays(-30)
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

            var btnExport = new Button
            {
                Text = "Export",
                Location = new Point(640, 15),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.Click += BtnExport_Click;

            filterPanel.Controls.AddRange(new Control[] 
            { 
                lblStatus, cmbStatusFilter, lblFromDate, dtpFromDate, 
                lblToDate, dtpToDate, btnSearch, btnExport 
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
                new DataGridViewTextBoxColumn { Name = "JobNumber", HeaderText = "Job Number", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "VehicleNumberPlate", HeaderText = "Number Plate", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "VehicleType", HeaderText = "Vehicle Type", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "OwnerName", HeaderText = "Owner", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "MarkingText", HeaderText = "Marking Text", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "ControllerType", HeaderText = "Controller", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CreatedDate", HeaderText = "Created", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } },
                new DataGridViewTextBoxColumn { Name = "CompletedDate", HeaderText = "Completed", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } },
                new DataGridViewTextBoxColumn { Name = "QualityRating", HeaderText = "Quality", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "OperatorName", HeaderText = "Operator", Width = 100 }
            });

            // Color code status
            dataGridView.CellFormatting += DataGridView_CellFormatting;

            this.Controls.Add(dataGridView);
        }

        private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                var status = e.Value.ToString();
                switch (status)
                {
                    case "Completed":
                        e.CellStyle.BackColor = Color.LightGreen;
                        break;
                    case "Failed":
                        e.CellStyle.BackColor = Color.LightCoral;
                        break;
                    case "InProgress":
                        e.CellStyle.BackColor = Color.LightBlue;
                        break;
                    case "Cancelled":
                        e.CellStyle.BackColor = Color.LightGray;
                        break;
                }
            }
        }

        private async void LoadDataAsync()
        {
            try
            {
                var jobs = await DatabaseService.GetMarkingJobsAsync();
                dataGridView.DataSource = jobs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading job history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                var jobs = await DatabaseService.GetMarkingJobsAsync();

                // Apply filters
                if (cmbStatusFilter.SelectedIndex > 0)
                {
                    var status = (MarkingJobStatus)Enum.Parse(typeof(MarkingJobStatus), cmbStatusFilter.Text);
                    jobs = jobs.Where(j => j.Status == status).ToList();
                }

                jobs = jobs.Where(j => j.CreatedDate.Date >= dtpFromDate.Value.Date && 
                                      j.CreatedDate.Date <= dtpToDate.Value.Date).ToList();

                dataGridView.DataSource = jobs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering jobs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"JobHistory_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCsv(saveDialog.FileName);
                    MessageBox.Show("Data exported successfully!", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string fileName)
        {
            using var writer = new System.IO.StreamWriter(fileName);
            
            // Write headers
            var headers = string.Join(",", dataGridView.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText));
            writer.WriteLine(headers);

            // Write data
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    var values = string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(c => $"\"{c.Value}\""));
                    writer.WriteLine(values);
                }
            }
        }
    }
}