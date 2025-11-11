using BCSKioskServerCrypto;
using Newtonsoft.Json;
using NV22SpectralInteg.Classes;
using NV22SpectralInteg.Data;
using NV22SpectralInteg.InactivityManager;
using NV22SpectralInteg.Login;
using NV22SpectralInteg.Model;
using NV22SpectralInteg.Network;
using NV22SpectralInteg.Services;
using System.Data;
using System.Management;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;


namespace NV22SpectralInteg.Dashboard
{
    public partial class Dashboard : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        private bool Running = false;
        int pollTimer = 250;
        int reconnectionAttempts = 10, reconnectionInterval = 3;
        volatile bool Connected = false, ConnectionFail = false;
        CValidator Validator;
        bool FormSetup = false;
        System.Windows.Forms.Timer reconnectionTimer = new System.Windows.Forms.Timer();
        Thread ConnectionThread;
        private Label balanceLabel;
        private ComboBox comboBoxComPorts = new ComboBox();
        private PictureBox titleImage;

        private Panel headerPanel;
        private Panel contentPanel;
        private Label addAmountLabel;
        private Panel tableWrapper;
        private TableLayoutPanel notesTable;
        private Button confirmButton;
        private int currentGrandTotal = 0;
        private static bool IsApiEnabled;

        private readonly CValidator _validator;
        private readonly AppConfig config;


        public Dashboard(CValidator validator, AppConfig config)
        {
            Logger.Log("🖥️ Dashboard initialized");
            InitializeComponent();
            _validator = validator;
            _validator.ClearNoteEscrowCounts();
            this.config = config;

            IsApiEnabled = !config.IsDevelopment;

            InitializeDashboardUI();
            timer1.Interval = pollTimer;
            reconnectionTimer.Tick += new EventHandler(reconnectionTimer_Tick);
            
            this.Resize += new EventHandler(Dashboard_Resize);
        }

        private void Dashboard_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            var hasData = (_validator?.NoteEscrowCounts?.Any() ?? false);

            // Always center the main panels on resize
            CenterPanels();

            // Only center the internal content if it's currently visible
            if (hasData)
            {
                CenterContentPanelLayout();
            }
        }

        private void reconnectionTimer_Tick(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Timer t)
                t.Enabled = false;
        }

        public void UpdateBalanceDisplay()
        {
            if (balanceLabel != null)
            {
                decimal balance = AppSession.CustomerBALANCE ?? 0.00m;
                balanceLabel.Text = $"${balance.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}";
            }

            Logger.Log($"[BALANCE] Balance label updated to: {balanceLabel.Text}");
        }

        private void InitializeDashboardUI()
        {
            this.Text = "Dashboard";
            this.BackColor = ColorTranslator.FromHtml("#11150f");
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            headerPanel = new Panel
            {
                Size = new Size(1000, 500), 
                Anchor = AnchorStyles.None,
                BackColor = Color.Transparent,
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0)
            };
            this.Controls.Add(headerPanel);

            titleImage = new PictureBox
            {
                Name = "titlePictureBox",
                Size = new Size(320, 200),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            string imagePath = Path.Combine(Application.StartupPath, "Image", "PocketMint.png");
            if (File.Exists(imagePath))
                titleImage.Image = Image.FromFile(imagePath);



            Label welcomeTextLabel = new Label
            {
                Text = "Welcome,",
                ForeColor = Color.Silver,
                Font = new Font("Poppins", 24, FontStyle.Regular),
                AutoSize = false,
                Size = new Size(300, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
            };

            string customerName = AppSession.CustomerName;
            Font customerFont = new Font("Poppins", 40, FontStyle.Bold);

            Size nameSize = TextRenderer.MeasureText(customerName, customerFont);
            Label customerNameLabel = new Label
            {
                Text = customerName,
                ForeColor = Color.White,
                Font = customerFont,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = nameSize
            };




            // 4. Create the label using the correct text and size
            balanceLabel = new Label
            {
                Text = $"${AppSession.CustomerBALANCE}",
                ForeColor = Color.White,
                Font = new Font("Poppins", 40, FontStyle.Bold),
                AutoSize = true,
                //Size = balanceSize, // The size now correctly includes the '$'
                TextAlign = ContentAlignment.MiddleCenter,
            };
            Label balanceSubTextLabel = new Label
            {
                Text = "Balance",
                ForeColor = Color.DarkGray,
                Font = new Font("Poppins", 24, FontStyle.Regular),
                AutoSize = false,
                Size = new Size(360, 80),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            Button logoutButton = new Button
            {
                Text = "Logout",
                Font = new Font("Poppins", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(217, 83, 79), // A subtle red color
                Size = new Size(120, 40),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right // This is crucial for positioning
            };
            logoutButton.FlatAppearance.BorderSize = 0;
            logoutButton.Click += LogoutButton_Click; // Wire up the click event

            headerPanel.Controls.Add(titleImage);
            headerPanel.Controls.Add(welcomeTextLabel);
            headerPanel.Controls.Add(customerNameLabel);
            headerPanel.Controls.Add(balanceLabel);
            headerPanel.Controls.Add(balanceSubTextLabel);
            headerPanel.Controls.Add(logoutButton);


            headerPanel.Layout += (sender, e) =>
            {
                titleImage.Location = new Point((headerPanel.Width - titleImage.Width) / 2, 30);

                welcomeTextLabel.Location = new Point((headerPanel.Width - welcomeTextLabel.Width) / 2, titleImage.Bottom + 10);

                customerNameLabel.Location = new Point((headerPanel.Width - customerNameLabel.Width) / 2, welcomeTextLabel.Bottom - 10);

                balanceSubTextLabel.Location = new Point((headerPanel.Width - balanceSubTextLabel.Width) / 2, customerNameLabel.Bottom);

                balanceLabel.Location = new Point((headerPanel.Width - balanceLabel.Width) / 2, balanceSubTextLabel.Bottom - 25);

                logoutButton.Location = new Point(headerPanel.Width - logoutButton.Width - 20, 20);
            };



            contentPanel = new Panel
            {
                Size = new Size(800, 500),
                Anchor = AnchorStyles.None,
                BackColor = this.BackColor,
                Padding = new Padding(0, 20, 0, 0)
            };
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            addAmountLabel = new Label
            {
                Text = "Please Add your Amount",
                ForeColor = Color.White,
                Font = new Font("Poppins", 20, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            contentPanel.Controls.Add(addAmountLabel);

            confirmButton = new Button
            {
                Text = "Confirm",
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#00C853"),
                Font = new Font("Poppins", 18, FontStyle.Bold),
                Width = 360,
                Height = 60,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            confirmButton.FlatAppearance.BorderSize = 0;
            confirmButton.FlatAppearance.MouseOverBackColor = Color.White;
            confirmButton.FlatAppearance.MouseDownBackColor = Color.White;
            confirmButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, confirmButton.Width - 1, confirmButton.Height - 1, 12, 12)
            );


            confirmButton.Click += ConfirmButton_Click;
            confirmButton.MouseEnter += (sender, e) =>
            {
                confirmButton.ForeColor = ColorTranslator.FromHtml("#25c866");
            };

            confirmButton.MouseLeave += (sender, e) =>
            {
                confirmButton.ForeColor = Color.White;
            };

            tableWrapper = new Panel { Dock = DockStyle.None, Visible = false };
            contentPanel.Controls.Add(tableWrapper);



            CenterPanels();
        }

        private void CenterPanels()
        {
            headerPanel.Location = new Point(
                (this.ClientSize.Width - headerPanel.Width) / 2,
                20 // Some top margin
            );

            contentPanel.Location = new Point(
                (this.ClientSize.Width - contentPanel.Width) / 2,
                headerPanel.Bottom + 20 // Some spacing between header and content
            );
        }

        // In Dashboard.cs, add this new method anywhere inside the class
        //private async Task<(dynamic result, decimal totalAmount)> PerformTransactionInBackgroundAsync()
        //{
        //    using (var client = new HttpClient())
        //    {
        //        string apiUrl = "https://uat.pocketmint.ai/api/kiosks/user/transaction/persist";

        //        var amountDetails = _validator.NoteEscrowCounts
        //            .Select(kvp =>
        //            {
        //                string key = kvp.Key;
        //                int count = kvp.Value;
        //                var denominationMatch = System.Text.RegularExpressions.Regex.Match(key, @"\d+");
        //                int denomination = denominationMatch.Success && int.TryParse(denominationMatch.Value, out var d) ? d : 0;
        //                return new { denomination, count, total = denomination * count };
        //            })
        //            .ToList();

        //        decimal kioskTotalAmount = amountDetails.Sum(a => a.total);

        //        var requestBody = new
        //        {
        //            kioskId = AppSession.KioskId,
        //            kioskRegId = AppSession.KioskRegId,
        //            customerRegId = AppSession.CustomerRegId,
        //            kioskTotalAmount = kioskTotalAmount,
        //            amountDetails = amountDetails
        //        };

        //        //var requestBody = new
        //        //{
        //        //    kioskId = 3,
        //        //    kioskRegId = 63,
        //        //    customerRegId = 27,
        //        //    kioskTotalAmount = 1,
        //        //    amountDetails = new[]
        //        //    {
        //        //        new
        //        //        {
        //        //            denomination = 1,
        //        //            count = 1,
        //        //            total = 1
        //        //        }
        //        //    }
        //        //};

        //        Logger.Log("📤 Sending transaction request to API...");
        //        string jsonPayload = JsonConvert.SerializeObject(requestBody);
        //        Logger.Log($"📦 Payload: {jsonPayload}");

        //        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
        //        client.DefaultRequestHeaders.Remove("Authorization");
        //        client.DefaultRequestHeaders.Add("Authorization", $"{ApiService.AuthToken}");
        //        client.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=C4537CD8D22C7AF20A50A08992FD3EFF; Path=/; Secure; HttpOnly");

        //        // 1. Perform the network call
        //        HttpResponseMessage response = await client.PostAsync(apiUrl, content);
        //        string responseText = await response.Content.ReadAsStringAsync();
        //            Logger.Log($"📬 API Response: {responseText}");
        //        var result = JsonConvert.DeserializeObject<dynamic>(responseText);

        //        // 2. Perform the printing (still in the background)
        //        if (result != null)
        //        {
        //            var receiptData = new LocalRequestBean
        //            {
        //                operation = "bankadd",
        //                kioskTotalAmount = kioskTotalAmount,
        //                isSucceed = (bool)result.isSucceed,
        //                printmessage = (string)result.message,
        //                feeAmount = ((bool)result.isSucceed && result.data?.cryptoConversionFee != null) ? (decimal)result.data.cryptoConversionFee : 0.00m
        //            };
        //            var printer = new ReceiptPrinter(receiptData);
        //            printer.printReceipt(); // This no longer blocks the UI!
        //        }

        //        return (result, kioskTotalAmount);
        //    }
        //}

        private async void ConfirmButton_Click(object sender, EventArgs e)
        {
            var internet = await NetworkHelper.IsInternetAccessibleDetailedAsync();
            if (!internet.IsConnected)
            {
                MessageBox.Show("No internet connection detected.\nPlease try again once the internet connection is restored on this kiosk to complete your transaction.",
                 "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log($"Connected: {internet.IsConnected}, Reason: {internet.Reason}");
                AppSession.Clear();
                Program.mainLoginForm.ResetToLogin();
                Program.mainLoginForm.Show();
                return;
            }

            KioskIdleManager.Stop();
            stoprunning();

            Logger.Log("🟢 Preparing transaction...");

            //Safety check: Ensure there is actually data to send.
            //if (!_validator.NoteEscrowCounts.Any() && IsApiEnabled == true)
            //{
            //    Logger.Log("⚠️ Confirm button clicked, but no amount was detected. Resetting.");
            //    MessageBox.Show("No amount detected. Please insert notes to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    ResetForNewTransaction(); // Reset the screen and restart polling.
            //    return;
            //}

            var processingPopup = new ProcessingPopup();
            try
            {
                // 1. Show the popup. The UI thread is now free.
                processingPopup.Show(this);

                // 2. Run ALL slow operations in the background and wait for the result.
                // The UI thread remains responsive and the GIF animates smoothly.
                var result = await Task.Run(async () =>
                {
                    // Perform printing in the background after the API call
                    var apiResult = await ApiService.PersistTransactionAsync(_validator.NoteEscrowCounts);

                    // Perform printing in the background after the API call
                    // 1. Check for success directly on the ApiResult object
                    if (apiResult.Success)
                    {
                        // The data object is now strongly typed
                        var data = apiResult.Data;

                        var receiptData = new LocalRequestBean
                        {
                            operation = "bankadd",
                            kioskTotalAmount = currentGrandTotal, // Use the total from the dashboard
                            isSucceed = apiResult.Success,

                            // 2. Access message from the ApiResult object
                            printmessage = apiResult.Message,

                            // 3. Access data properties safely and cast them (using a null check for safety)
                            // NOTE: We assume 'cryptoConversionFee' is a property on TransactionPersistData.
                            // You MUST update TransactionPersistData to include this property.
                            feeAmount = (data?.cryptoConversionFee != null) ? data.cryptoConversionFee : 0.00m
                        };

                        var printer = new ReceiptPrinter(receiptData);
                        printer.printReceipt();
                    }

                    // Ensure the caller of this code also knows to handle ApiResult<T>
                    return apiResult;
                });

                // 3. Close the processing popup.
                processingPopup.Close();

                // 4. Check the result and show the final popup.
                if (result == null)
                {
                    Logger.Log("⚠️ API returned null response.");
                    ResetForNewTransaction();
                    return;
                }

                ShowResultAndPrintReceipt(result);
            }
            catch (Exception ex)
            {
                if (processingPopup != null && !processingPopup.IsDisposed)
                {
                    processingPopup.Close();
                }
                Logger.LogError("🚨 Exception in ConfirmButton_Click", ex);
                MessageBox.Show("A critical error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ShowResultAndPrintReceipt(ApiResult<TransactionPersistData> result)
        {

            var successPopup = new SuccessPopup(AppSession.CustomerName, currentGrandTotal, (bool)result.Success, (string)result.Message, "bankadd");
            successPopup.ShowDialog(this);

            // Handle the user's choice from the popup
            if ((bool)result.Success)
            {
                // Update balance and session info on success
                AppSession.StoreBalance = result.Data.storeBalance;

                if (successPopup.DialogResult == DialogResult.OK)
                {
                    Logger.Log("Add more button clicked for add transaction.");
                    UpdateBalanceDisplay();
                    ResetForNewTransaction();
                    MainLoop();
                }
                else
                {
                    Logger.Log("(Transaction completed) Manual logout initiated by user.");
                    FinalizeAndReturnToLogin();
                }
            }
            else // Transaction failed
            {
                if (successPopup.DialogResult == DialogResult.Cancel)
                {
                    Logger.Log("(Transaction failed) Manual logout initiated by user.");
                    FinalizeAndReturnToLogin();
                }
            }
        }

        internal void FinalizeAndReturnToLogin()
        {
            KioskIdleManager.Stop();
            stoprunning();
            AppSession.Clear();
            _validator.ClearNoteEscrowCounts();

            Program.mainLoginForm.ResetToLogin();
            Program.mainLoginForm.Show();

            //this.Hide();
            this.Close();
        }

        internal async void PerformTransaction()
        {
            ConfirmButton_Click(this, EventArgs.Empty);
        }

        private void ResetForNewTransaction()
        {
            Logger.Log("🔄 Resetting dashboard for a new transaction so clear ClearNoteEscrowCounts...");

            _validator.ClearNoteEscrowCounts();

            this.currentGrandTotal = 0;

            UpdateNotesDisplay();

            Logger.Log("▶️ Restarting MainLoop for new transaction...");
        }

        private Panel CreateNotesTable(out int grandTotal)
        {
            Logger.Log("CreateNotesTable called.");

            grandTotal = 0;
            var counts = _validator?.NoteEscrowCounts ?? new Dictionary<string, int>();
            Logger.Log($"Counts loaded. Total denominations: {counts.Count}");

            notesTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(1),
                Margin = new Padding(0),
                GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Dock = DockStyle.Top
            };

            notesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            notesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            notesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            AddStyledCell(notesTable, "Note", 0, 0, header: true, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: false);
            AddStyledCell(notesTable, "Count", 1, 0, header: true, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: false);
            AddStyledCell(notesTable, "Total", 2, 0, header: true, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: false);

            int row = 1;
            if (counts.Any())
            {
                Logger.Log("Beginning to populate table rows...");

                // ✅ CHANGE 2: Sort numerically by parsing the integer from the key.
                foreach (var kvp in counts.OrderBy(k => int.Parse(Regex.Match(k.Key, @"\d+").Value)))
                {
                    string denominationLabel = kvp.Key;
                    int count = kvp.Value;
                    var match = Regex.Match(denominationLabel, @"\d+");
                    decimal denomination = match.Success && decimal.TryParse(match.Value, out var d) ? d : 0;
                    decimal total = denomination * count;
                    grandTotal += (int)total;

                    Logger.Log($"Added row - Denomination: {denomination} USD, Count: {count}, Total: {total}");

                    AddStyledCell(notesTable, $"{denomination} USD", 0, row, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: false);
                    AddStyledCell(notesTable, count.ToString(), 1, row, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: false);
                    AddStyledCell(notesTable, total.ToString(), 2, row, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: false);
                    row++;
                }
            }
            else
            {
                Logger.Log("No note counts found. Table will not display any data rows.");
            }

            // Grand Total TableLayoutPanel - 1 row, 3 columns (or 2 + colSpan)
            var grandTotalTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Dock = DockStyle.Bottom,
                Padding = new Padding(1),
                Margin = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            grandTotalTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f)); // For label spanning 2 columns
            grandTotalTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // dummy, because colSpan 2
            grandTotalTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.5f));

            AddStyledCell(grandTotalTable, "Grand Total", 0, 0, header: true, colSpan: 2, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: true);
            AddStyledCell(grandTotalTable, grandTotal.ToString(), 2, 0, header: true, alignment: ContentAlignment.MiddleCenter, isGrandTotalRow: true);

            // Detect orientation based on current container size
            bool isHorizontal = this.Width > this.Height;
            int maxVisibleRows = isHorizontal ? 3 : 8; // 1 header + 2 or 7 data rows
            int rowHeight = 48; // approx height per row including padding/borders

            int maxTableHeight = maxVisibleRows * rowHeight;
            int tableContentHeight = notesTable.PreferredSize.Height;

            var scrollablePanel = new Panel
            {
                Width = 800,
                Height = Math.Min(notesTable.PreferredSize.Height, maxVisibleRows * rowHeight),
                AutoScroll = true,
                BackColor = Color.Transparent,
                Dock = DockStyle.Top,
                Padding = new Padding(0)
            };

            scrollablePanel.Controls.Add(notesTable);

            var wrapper = new Panel
            {
                Width = 800,
                Height = scrollablePanel.Height + grandTotalTable.PreferredSize.Height,
                BackColor = Color.Transparent,
                Dock = DockStyle.None
            };


            wrapper.Controls.Add(scrollablePanel);
            wrapper.Controls.Add(grandTotalTable);

            // Position grandTotalTable manually below scrollablePanel
            grandTotalTable.Top = scrollablePanel.Bottom + 5; // small gap
            grandTotalTable.Left = scrollablePanel.Left;

            // Handle resizing so dataTable width fits scrollablePanel width
            scrollablePanel.Layout += (s, e) =>
            {
                notesTable.Width = scrollablePanel.ClientSize.Width - (scrollablePanel.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0);
            };
            Logger.Log($"Notes table created with wrapper size: {wrapper.Width}x{wrapper.Height}");

            this.currentGrandTotal = grandTotal;
            return wrapper;
        }

        private void AddStyledCell(TableLayoutPanel table, string text, int col, int row, bool header = false, int colSpan = 1, ContentAlignment alignment = ContentAlignment.MiddleCenter, bool isGrandTotalRow = false)
        {
            Label label = new Label
            {
                Text = text,
                ForeColor = isGrandTotalRow ? ColorTranslator.FromHtml("#11150F") : header ? Color.White : ColorTranslator.FromHtml("#212529"),
                Font = new Font("Poppins", 14, header ? FontStyle.Bold : FontStyle.Regular),
                BackColor = isGrandTotalRow ? ColorTranslator.FromHtml("#D1E7DD") : header ? ColorTranslator.FromHtml("#212529") : ColorTranslator.FromHtml("#FFF"),
                Dock = DockStyle.Fill,
                MinimumSize = new Size(0, 40),
                Margin = new Padding(0), // Use Margin 0 for perfect borders
                Padding = new Padding(10, 0, 10, 0), // ✅ ADD PADDING for left/right alignment
                TextAlign = alignment // ✅ USE THE NEW ALIGNMENT PARAMETER
            };

            label.Paint += (sender, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, label.ClientRectangle,
                    Color.FromArgb(80, 80, 80), ButtonBorderStyle.Solid);
            };

            table.Controls.Add(label, col, row);

            if (colSpan > 1)
            {
                table.SetColumnSpan(label, colSpan);
            }
        }

        private void Validator_NoteEscrowUpdated(string key, int count)
        {
            Logger.Log("call Validator_NoteEscrowUpdated...");
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(UpdateNotesDisplay));
            else
                UpdateNotesDisplay();
        }

        public void UpdateNotesDisplay()
        {
            contentPanel.Controls.Remove(tableWrapper);
            tableWrapper.Dispose();

            Logger.Log("Updating Table...");
            tableWrapper = CreateNotesTable(out int grandTotal);
            contentPanel.Controls.Add(tableWrapper);

            ToggleDataView();

            CenterContentPanelLayout();
        }

        private void ToggleDataView()
        {
            var hasData = (_validator?.NoteEscrowCounts?.Any() ?? false);

            // Clear all controls from the content panel first
            contentPanel.Controls.Clear();

            // Set button visibility based on whether we have data
            confirmButton.Visible = hasData;

            if (hasData)
            {
                // 1. Create a new layout panel to hold the content centered vertically and horizontally.
                // We set its size to match contentPanel's size for the centering calculation to work correctly.
                var containerLayout = new FlowLayoutPanel
                {
                    // CRITICAL FIX: Change DockStyle.Fill to DockStyle.None so CenterContentPanelLayout can set the Location.
                    Dock = DockStyle.None,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.Transparent,
                    Padding = new Padding(20)
                };

                // 2. The button wrapper width should match the overall wrapper created in CreateNotesTable
                Panel buttonWrapper = new Panel
                {
                    // CRITICAL FIX: Use the wrapper's final width, not the arbitrary 800px width.
                    Width = tableWrapper.Width,
                    Height = confirmButton.Height,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 30, 0, 0)
                };
                // 3. Center the confirm button inside its wrapper
                confirmButton.Anchor = AnchorStyles.None; // Reset anchors
                confirmButton.Left = (buttonWrapper.Width - confirmButton.Width) / 2;
                confirmButton.Top = 0; // Top of the wrapper
                buttonWrapper.Controls.Add(confirmButton);

                // 4. Add the controls in flow order
                containerLayout.Controls.Add(tableWrapper);
                containerLayout.Controls.Add(buttonWrapper);

                // 5. Add the main content container to the content panel
                contentPanel.Controls.Add(containerLayout);
            }
            else
            {
                // No data: just show the "Please Add..." label, filling the space
                contentPanel.Controls.Add(addAmountLabel);
                addAmountLabel.Dock = DockStyle.Fill;
            }
        }

        private void CenterContentPanelLayout()
        {
            // Find the dynamically created FlowLayoutPanel inside the contentPanel
            // We are looking for the container that holds the table and button
            var containerLayout = contentPanel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();

            if (containerLayout == null) return;

            // Use containerLayout instead of centerPanel in the following lines
            containerLayout.Update();

            // 1. Calculate horizontal center within contentPanel (800px wide)
            int x = (contentPanel.Width - containerLayout.Width) / 2;

            // 2. Calculate vertical center within contentPanel, allowing a minimum top margin
            int y = Math.Max(10, (contentPanel.Height - containerLayout.Height) / 2);

            // 3. Apply the calculated location
            containerLayout.Location = new Point(x, y);

            Logger.Log($"Content panel layout centered at: {x}, {y} (Size: {containerLayout.Width}x{containerLayout.Height})");
        }

        // NEW: Function to find all available serial ports dynamically.
        public Dictionary<string, string> GetAllSerialPorts()
        {
            var ports = new Dictionary<string, string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%)'"))
                {
                    foreach (var device in searcher.Get())
                    {
                        string name = device["Name"]?.ToString();
                        string description = device["Description"]?.ToString();

                        if (name != null)
                        {
                            Match match = Regex.Match(name, @"(COM\d+)");
                            if (match.Success)
                            {
                                string comPort = match.Value;
                                if (!ports.ContainsKey(comPort))
                                {
                                    ports.Add(comPort, description);
                                }
                            }
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                MessageBox.Show("Could not query serial ports: " + ex.Message);
            }
            return ports;
        }
        private void Dashboard_Load(object sender, EventArgs e)
        {
            try
            {
                Logger.Log("⚡ Dashboard_Load triggered → Searching for USB Serial Device...");

                // Get all available serial ports
                Dictionary<string, string> availablePorts = GetAllSerialPorts();

                Logger.Log("📓 List of ports:");
                foreach (var port in availablePorts)
                {
                    Logger.Log($"  ->{port.Key}: {port.Value}");
                }

                // Find the first port that contains "USB Serial Device" in its description
                string targetComPort = null;
                foreach (var port in availablePorts)
                {
                    Logger.Log($"Dashboard_Load For loop Port Serach...");
                    Logger.Log($"Dashboard_Load Port - {port}");

                    // Make the check case-insensitive for more reliability
                    if (port.Value.IndexOf("USB Serial Device", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        targetComPort = port.Key; // Get the port name, e.g., "COM3"
                        Logger.Log($"🔌 USB Serial Device found: {targetComPort} ({port.Value})");
                        break; // Stop after finding the first one
                    }
                }

                // If a matching port was found, set it globally and connect
                //if (targetComPort != null)
                //{
                Logger.Log("✅ Device found. Proceeding with automatic connection.");

                // --- AUTOMATICALLY SET GLOBALS AND CONNECT ---
                //Global.ComPort = targetComPort;
                //Global.SSPAddress = Byte.Parse("0");

                Logger.Log("✅ Validator instance created successfully.");

                // 🔹 SUBSCRIBE TO THE VALIDATOR'S EVENT
                _validator.NoteEscrowUpdated += Validator_NoteEscrowUpdated;

                // Update UI to show what happened
                //comboBoxComPorts.Items.Clear();
                //comboBoxComPorts.Items.Add($"{targetComPort} - Auto-Connecting...");
                //comboBoxComPorts.SelectedIndex = 0;
                //comboBoxComPorts.Enabled = false;
                //}
                //else
                //{
                //    // --- FAIL IF NO DEVICE IS FOUND ---
                //    Logger.Log("❌ No 'USB Serial Device' found. Automatic connection failed.");
                //    //MessageBox.Show(
                //    //  "Required 'USB Serial Device' not found. Please connect the device and restart the application.",
                //    //  "Device Not Found",
                //    //  MessageBoxButtons.OK,
                //    //  MessageBoxIcon.Error
                //    //);

                //    // Update UI to reflect the failure state
                //    comboBoxComPorts.Items.Clear();
                //    comboBoxComPorts.Items.Add("Device Not Found");
                //    comboBoxComPorts.SelectedIndex = 0;
                //    comboBoxComPorts.Enabled = false;
                //}


                if (IsApiEnabled)
                {
                    _validator.UpdateCountsFromDb(AppSession.CustomerMobile!);
                    UpdateNotesDisplay();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("🚨 Error during Dashboard_Load", ex);
                MessageBox.Show(ex.ToString(), "EXCEPTION");
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }
        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Running)
            {
                Running = false;
                if (ConnectionThread != null && ConnectionThread.IsAlive)
                {
                    ConnectionThread.Join();
                }
            }
        }



        public bool IsComPortAvailable(string portName)
        {
            return !string.IsNullOrEmpty(portName) && System.IO.Ports.SerialPort.GetPortNames().Contains(portName);
        }


        private void SetupFormLayout()
        {
            // need validator class instance
            if (_validator == null)
            {
                MessageBox.Show("Validator class is null.", "ERROR");
                return;
            }
        }

        private bool ConnectToValidator()
        {
            Logger.Log("Begin ConnectToValidator sequence...");
            // setup the timer
            reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (int i = 0; i < reconnectionAttempts; i++)
            {
                Logger.Log($"Connection attempt #{i + 1}...");
                // reset timer
                reconnectionTimer.Enabled = true;

                // close com port in case it was open
                //Validator.SSPComms.CloseComPort();

                // turn encryption off for first stage
                _validator.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (_validator.OpenComPort(textBox1) && _validator.NegotiateKeys(textBox1))
                {
                    Logger.Log("Validator COM port opened and keys negotiated.");
                    _validator.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    byte maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        _validator.SetProtocolVersion(maxPVersion, textBox1);
                    }
                    else
                    {
                        MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
                        return false;
                    }
                    // get info from the validator and store useful vars
                    _validator.ValidatorSetupRequest(textBox1);
                    // Get Serial number
                    _validator.GetSerialNumber(textBox1);
                    // check this unit is supported by this program
                    if (!IsUnitTypeSupported(_validator.UnitType))
                    {
                        MessageBox.Show("Unsupported unit type, this SDK supports the BV series and the NV series (excluding the NV11)");
                        Application.Exit();
                        return false;
                    }
                    // inhibits, this sets which channels can receive notes
                    _validator.SetInhibits(textBox1);
                    // enable, this allows the validator to receive and act on commands
                    _validator.EnableValidator(textBox1);

                    Logger.Log("Validator setup completed successfully ✅");
                    return true;
                }
                while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
            }
            return false;
        }

        private void ConnectToValidatorThreaded()
        {
            // setup the timer
            reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (int i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                reconnectionTimer.Enabled = true;

                // close com port in case it was open
                _validator.SSPComms.CloseComPort();

                // turn encryption off for first stage
                _validator.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (_validator.OpenComPort() && _validator.NegotiateKeys())
                {
                    _validator.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    byte maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        _validator.SetProtocolVersion(maxPVersion);
                    }
                    else
                    {
                        MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
                        Connected = false;
                        return;
                    }
                    // get info from the validator and store useful vars
                    _validator.ValidatorSetupRequest();
                    // inhibits, this sets which channels can receive notes
                    _validator.SetInhibits();
                    // enable, this allows the validator to operate
                    _validator.EnableValidator();

                    Connected = true;
                    return;
                }
                while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
            }
            Connected = false;
            ConnectionFail = true;
        }

        // This function finds the maximum protocol version that a validator supports. To do this
        // it attempts to set a protocol version starting at 6 in this case, and then increments the
        // version until error 0xF8 is returned from the validator which indicates that it has failed
        // to set it. The function then returns the version number one less than the failed version.
        private byte FindMaxProtocolVersion()
        {
            // not dealing with protocol under level 6
            // attempt to set in validatorIsUnitTypeSupported
            byte b = 0x06;
            while (true)
            {
                _validator.SetProtocolVersion(b);
                if (_validator.CommandStructure.ResponseData[0] == CCommands.SSP_RESPONSE_FAIL)
                    return --b;
                b++;
                if (b > 20)
                    return 0x06; // return default if protocol 'runs away'
            }
        }

        // This function checks whether the type of validator is supported by this program. This program only
        // supports Note Validators so any other type should be rejected.
        private bool IsUnitTypeSupported(char type)
        {
            if (type == (char)0x00)
                return true;
            return false;
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            Logger.Log("Manual logout initiated by user.");
            FinalizeAndReturnToLogin();
        }


        private void btnHalt_Click(object sender, EventArgs e)
        {
            Logger.Log("▶️ Run button clicked → Starting MainLoop...");
            textBox1.AppendText("Poll loop stopped\r\n");
            Running = false;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            Logger.Log("⏹️ Halt button clicked → Stopping Poll loop...");
            MainLoop();
        }

        public void MainLoop()
        {
            Logger.Log("🔄 Entering MainLoop...");
            btnRun.Enabled = false;

            if (!IsComPortAvailable(Global.ComPort))
            {
                Logger.Log("❌ COM Port not available → Cannot start MainLoop.");
                MessageBox.Show("USB device not detected or COM port is invalid.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnRun.Enabled = true;
                return;
            }
            _validator.CommandStructure.ComPort = Global.ComPort;
            _validator.CommandStructure.SSPAddress = Global.SSPAddress;
            _validator.CommandStructure.Timeout = 3000;

            Logger.Log($"🔗 Attempting validator connection via port {Global.ComPort}...");

            // connect to the validator
            if (ConnectToValidator())
            {
                Logger.Log("✅ Validator connected successfully!");

                Running = true;
                textBox1.AppendText("\r\nPoll Loop\r\n*********************************\r\n");
                btnHalt.Enabled = true;
            }
            else
            {
                Logger.Log("❌ Validator connection failed.");
                return;
            }

            while (Running)
            {
                // if the poll fails, try to reconnect
                if (!_validator.DoPoll(textBox1))
                {
                    textBox1.AppendText("Poll failed, attempting to reconnect...\r\n");
                    Connected = false;
                    ConnectionThread = new Thread(ConnectToValidatorThreaded);
                    ConnectionThread.Start();
                    while (!Connected)
                    {
                        if (ConnectionFail)
                        {
                            textBox1.AppendText("Failed to reconnect to validator\r\n");
                            return;
                        }
                        Application.DoEvents();
                    }
                    textBox1.AppendText("Reconnected successfully\r\n");
                }

                timer1.Enabled = true;
                // update form
                //UpdateUI();
                // setup dynamic elements of win form once
                if (!FormSetup)
                {
                    SetupFormLayout();
                    FormSetup = true;
                }
                while (timer1.Enabled)
                {
                    Application.DoEvents();
                    Thread.Sleep(1); // Yield to free up CPU
                }
            }

            //close com port and threads
            _validator.SSPComms.CloseComPort();

            btnRun.Enabled = true;
            btnHalt.Enabled = false;
        }


        //private void SendButton_Click(object sender, EventArgs e)
        //{
        //  try
        //  {
        //      Logger.Log("📨 Send button clicked");

        //      if (!IsComPortAvailable(Global.ComPort))
        //      {
        //          Logger.Log("❌ Send aborted: USB device not found.");
        //          MessageBox.Show("Device not found! Please check USB connection.",
        //                          "Device Not Found",
        //                          MessageBoxButtons.OK,
        //                          MessageBoxIcon.Warning);
        //          return;
        //      }

        //      Logger.Log("✅ USB device validated → Launching SendWindow...");
        //      ReleaseCapture();

        //      var sendWindow = new SendWindow.SendWindow(this, Validator);
        //      sendWindow.Show();
        //      this.Hide();

        //      Logger.Log("📤 SendWindow launched successfully. Returning to MainLoop...");
        //      MainLoop();
        //  }
        //  catch (Exception ex)
        //  {
        //      Logger.LogError("🚨 SendButton_Click failed", ex);
        //      MessageBox.Show("An error occurred: " + ex.Message);
        //  }
        //}

        public void stoprunning()
        {
            try
            {
                Logger.Log("🛑 stoprunning() called → Cleaning up validator and threads...");
                Running = false;

                if (_validator != null)
                {
                    _validator.DisableValidator();
                    _validator.SSPComms.CloseComPort();
                    Logger.Log("✅ Validator disabled and COM port closed.");
                }

                if (ConnectionThread != null && ConnectionThread.IsAlive)
                {
                    Logger.Log("⚠️ Connection thread active → Aborting...");
                    ConnectionThread.Abort();
                }

                timer1.Enabled = false;
                Logger.Log("⏹️ Timer stopped. System is now safe.");
            }
            catch (Exception ex)
            {
                Logger.LogError("🚨 Error during stoprunning()", ex);
                MessageBox.Show("Error stopping validator: " + ex.Message);
            }
        }




    }
}
