using Newtonsoft.Json;
using NV22SpectralInteg.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DashboardForm = NV22SpectralInteg.Dashboard.Dashboard;
using CValidator = NV22SpectralInteg.Classes.CValidator;

namespace NV22SpectralInteg.SendWindow
{
    public partial class SendWindow : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private readonly DashboardForm _dashboard;
        private readonly CValidator _validator;
        private TableLayoutPanel _table;
        private Label _totalAmountLabel;
        private Panel _tableWrapper;


        public SendWindow(DashboardForm dashboard, CValidator validator)
        {
            InitializeComponent();

            _dashboard = dashboard;
            _validator = validator;
            //_noteEscrowCounts = _dashboard.testCounts; // for testing
            _validator.NoteEscrowUpdated += Validator_NoteEscrowUpdated;

            // Basic form setup
            this.Text = "Send Transaction";
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeSendWindowUI();
        }

        private void InitializeSendWindowUI()
        {
            // Title Panel
            Panel titlePanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            Label titleLabel = new Label
            {
                Text = "Send Transaction",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            titlePanel.Controls.Add(titleLabel);
            this.Controls.Add(titlePanel);

            // Content Panel
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = this.BackColor,
                Padding = new Padding(20),
            };
            this.Controls.Add(contentPanel);

            // Heading
            Label heading = new Label
            {
                Text = "Dollar Information",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = false,
                Height = 120,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
            };
            contentPanel.Controls.Add(heading);

            // Create styled table (wrapped in scroll panel)
            _tableWrapper = CreateStyledTable(out int grandTotal);
            contentPanel.Controls.Add(_tableWrapper);

            // Center table wrapper
            _tableWrapper.Location = new Point(
                (contentPanel.ClientSize.Width - _tableWrapper.Width) / 2,
                (contentPanel.ClientSize.Height - _tableWrapper.Height) / 2
            );
            contentPanel.Layout += (s, e) =>
            {
                _tableWrapper.Location = new Point(
                    (contentPanel.ClientSize.Width - _tableWrapper.Width) / 2,
                    (contentPanel.ClientSize.Height - _tableWrapper.Height) / 2
                );
            };

            // Bottom panel
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 160,
                Padding = new Padding(0, 10, 0, 20),
                BackColor = Color.Transparent,
            };

            _totalAmountLabel = new Label
            {
                Text = $"Total: ${grandTotal}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = false,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 10),
            };

            bottomPanel.Controls.Add(_totalAmountLabel);

            // Complete button
            RoundedButton completeButton = new RoundedButton
            {
                Size = new Size(240, 60),
                Text = "Complete",
                BackColor = Color.FromArgb(40, 180, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.None,
            };

            completeButton.FlatAppearance.BorderSize = 0;
            completeButton.Click += completeButton_Click;

            bottomPanel.Controls.Add(completeButton);

            bottomPanel.Layout += (s, e) =>
            {
                completeButton.Location = new Point(
                    (bottomPanel.ClientSize.Width - completeButton.Width) / 2,
                    _totalAmountLabel.Bottom + 10
                );
            };

            contentPanel.Controls.Add(bottomPanel);
            bottomPanel.BringToFront();
        }

        private void Validator_NoteEscrowUpdated(string key, int count)
        {
            Logger.Log($"💵 Escrow updated → {key} = {count}");
            if (InvokeRequired)
            {
                Invoke(new Action(() => Validator_NoteEscrowUpdated(key, count)));
                return;
            }

            // 🔹 Rebuild or update the table
            UpdateStyledTable();
        }

        private void UpdateStyledTable()
        {
            Logger.Log("📊 Updating escrow table UI...");

            int grandTotal;
            var newTableWrapper = CreateStyledTable(out grandTotal);

            var contentPanel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Fill);
            if (contentPanel == null) return;

            if (_tableWrapper != null)
            {
                Logger.Log("♻️ Replacing old table with new one.");

                contentPanel.Controls.Remove(_tableWrapper);
                _tableWrapper.Dispose();
            }

            _tableWrapper = newTableWrapper;
            contentPanel.Controls.Add(_tableWrapper);
            _tableWrapper.BringToFront();

            // 🔹 Recenter the table after replacing it
            _tableWrapper.Location = new Point(
                (contentPanel.ClientSize.Width - _tableWrapper.Width) / 2,
                (contentPanel.ClientSize.Height - _tableWrapper.Height) / 2
            );

            if (_totalAmountLabel != null)
                _totalAmountLabel.Text = $"Total: ${grandTotal}";
            Logger.Log($"💰 Updated grand total: ${grandTotal}");
        }




        // Build table with styling
        private Panel CreateStyledTable(out int grandTotal)
        {
            _table = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                Dock = DockStyle.Top,
                GrowStyle = TableLayoutPanelGrowStyle.AddRows
            };

            // Equal column widths
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));

            int row = 0;
            grandTotal = 0;

            var counts = _validator.NoteEscrowCounts; // 🔹 always live data
            if (counts == null || counts.Count == 0)
            {

                _table.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

                Label emptyLabel = new Label
                {
                    Text = "No records available",
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(8),
                    AutoSize = false,
                    Margin = new Padding(0),
                    MinimumSize = new Size(0, 50)
                };

                _table.Controls.Add(emptyLabel, 0, row);
                _table.SetColumnSpan(emptyLabel, 3);
            }
            else
            {
                _table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                AddStyledCell(_table, "Money", 0, row, header: true);
                AddStyledCell(_table, "Count", 1, row, header: true);
                AddStyledCell(_table, "Total", 2, row, header: true);
                row++;

                foreach (var kvp in counts)
                {
                    string denominationLabel = kvp.Key;   // e.g. "20.00 USD"
                    int count = kvp.Value;

                    var match = System.Text.RegularExpressions.Regex.Match(denominationLabel, @"\d+(\.\d+)?");
                    decimal denomination = match.Success && decimal.TryParse(match.Value, out var d) ? d : 0;

                    decimal total = denomination * count;
                    grandTotal += (int)total;   // keep grandTotal as int

                    Color rowBack = Color.FromArgb(24, 24, 24);

                    AddStyledCell(_table, denominationLabel, 0, row, backColor: rowBack);
                    AddStyledCell(_table, count.ToString(), 1, backColor: rowBack, row: row);
                    AddStyledCell(_table, $"${total}", 2, backColor: rowBack, row: row);
                    row++;
                }

            }

            Panel wrapper = new Panel
            {
                AutoScroll = true,
                BackColor = Color.Transparent,
                Width = 600,
                Height = Math.Min(_table.PreferredSize.Height, 9 * 45),
                Dock = DockStyle.None
            };

            wrapper.Controls.Add(_table);
            _table.Dock = DockStyle.Top;

            return wrapper;
        }


        // Helper to add cells with styling
        private void AddStyledCell(TableLayoutPanel table, string text, int col, int row,
                           bool bold = false, bool header = false,
                           int span = 1, Color? backColor = null,
                           Color? foreColor = null, bool large = false, bool center = true)
        {
            Label label = new Label
            {
                Text = text,
                ForeColor = foreColor ?? (header ? Color.White : Color.Gainsboro),
                Font = new Font("Segoe UI", large ? 14 : (header ? 12 : 11),
                                (bold || header) ? FontStyle.Bold : FontStyle.Regular),
                BackColor = backColor ?? (header ? Color.FromArgb(45, 45, 45) : Color.Transparent),
                TextAlign = ContentAlignment.MiddleCenter,  // 🔹 Always center
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                AutoSize = false,
                Margin = new Padding(1),
                MinimumSize = new Size(0, 40) // consistent row height
            };

            table.Controls.Add(label, col, row);

            if (span > 1)
                table.SetColumnSpan(label, span);
        }

        private async void completeButton_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Log("🟢 Complete button clicked → Preparing transaction...");

                using (var client = new HttpClient())
                {
                    string apiUrl = "https://uat.pocketmint.ai/kiosks/api/user/transaction/persist";

                    // build amountDetails
                    var amountDetails = _validator.NoteEscrowCounts
                        .Select(kvp =>
                        {
                            string key = kvp.Key;   // e.g. "20.00 USD"
                            int count = kvp.Value;

                            var denominationMatch = System.Text.RegularExpressions.Regex.Match(key, @"\d+");
                            int denomination = denominationMatch.Success && int.TryParse(denominationMatch.Value, out var d) ? d : 0;

                            return new
                            {
                                denomination = denomination,
                                count = count,
                                total = denomination * count
                            };
                        })
                        .ToList();

                    // 🔹 calculate total amount for kiosk
                    int kioskTotalAmount = amountDetails.Sum(a => a.total);

                    var requestBody = new
                    {
                        kioskId = AppSession.KioskId,
                        kioskRegId = AppSession.KioskRegId,
                        customerRegId = AppSession.CustomerRegId,
                        kioskTotalAmount = kioskTotalAmount,   // ✅ must include
                        amountDetails = amountDetails
                    };

                    Logger.Log("📤 Sending transaction request to API...");
                    string jsonPayload = JsonConvert.SerializeObject(requestBody);
                    Logger.Log($"📦 Payload: {jsonPayload}");

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.Add("Authorization", "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78");
                    client.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=C4537CD8D22C7AF20A50A08992FD3EFF; Path=/; Secure; HttpOnly");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    string responseText = await response.Content.ReadAsStringAsync();
                    Logger.Log($"📬 API Response: {responseText}");

                    var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                    if (result == null)
                    {
                        Logger.Log("⚠️ API returned null response.");
                        return;
                    }

                    if (!(bool)result.isSucceed)
                    {
                        Logger.Log($"❌ Transaction failed → {result.message}");
                        ReleaseCapture();
                        MessageBox.Show($"{result.message}",
                                     "",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Warning);
                        _dashboard.stoprunning();
                        _dashboard.Show();
                        this.Hide();
                        _validator.ClearNoteEscrowCounts();
                    }
                    else
                    {
                        Logger.Log("✅ Transaction succeeded!");
                        Logger.Log($"💳 New balance: {result.balance}");

                        ReleaseCapture();

                        MessageBox.Show($"{result.message}",
                                     "",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Information);

                        _dashboard.stoprunning();
                        try
                        {
                            string rawBalance = result.balance?.ToString() ?? "null";
                            if (decimal.TryParse(rawBalance, out decimal newBalance))
                            {
                                Logger.Log($"💰 Balance parsed successfully: {newBalance}");
                                AppSession.CustomerBALANCE = newBalance;
                            }
                            else
                            {
                                Logger.Log($"⚠️ Failed to parse balance. Raw value: {rawBalance}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("🚨 Error parsing balance", ex);
                        }

                        _dashboard.Show();
                        _dashboard.UpdateBalanceDisplay();
                        this.Hide();

                        Logger.Log("🧹 Clearing escrow counts after transaction.");
                        _validator.ClearNoteEscrowCounts();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("🚨 Exception in completeButton_Click", ex);
                MessageBox.Show("An error occurred while completing the transaction:\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
