using BCSKioskServerCrypto;
using Newtonsoft.Json;
using NV22SpectralInteg.Classes;
using NV22SpectralInteg.Dashboard;
using NV22SpectralInteg.Data;
using NV22SpectralInteg.InactivityManager;
using NV22SpectralInteg.Model;
using NV22SpectralInteg.NumPad;
using NV22SpectralInteg.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV22SpectralInteg.Login
{
    public partial class LoginForm : Form
    {
        private PictureBox titleImage;
        private Label subtitle;
        private Panel controlBoxPanel;
        private readonly AppConfig config;
        private readonly bool isDevelopment;

        private Panel kioskPanel;
        private Panel loginPanel;
        private FlowLayoutPanel stackPanel;
        private TextBox phoneTextBox;
        private Label phonePrefix;
        private ComboBox countryDropdown;
        private List<CountryCode> countryCodes;
             
        private Panel[] otpBoxContainers; 
        private TextBox[] otpTextBoxes;
        private string fullMobileNumber;

        private Label timerLabel;
        private Button resendButton;
        private System.Windows.Forms.Timer countdownTimer;
        private int timeRemaining;

        private Label phoneLabel;
        private Panel phonePanel;
        private Button loginButton;
        private Label otpLabel;
        private TableLayoutPanel otpBoxesPanel;
        private Label infoLabel;
        private Button verifyButton;
        private Numpad currentNumpad;

        private CValidator validator;
        public LoginForm()
        {
            this.Opacity = 0;
            this.Shown += LoginForm_Shown;
            try
            {
                // Get the full path to the JSON file
                string configPath = Path.Combine(Application.StartupPath, "config.json");
                // Read the entire file into a string
                string jsonContent = File.ReadAllText(configPath);
                // Deserialize the JSON string into the class field
                this.config = JsonConvert.DeserializeObject<AppConfig>(jsonContent);
                // Initialize the isDevelopment field from the config
                this.isDevelopment = this.config.IsDevelopment;
            }
            catch (Exception ex)
            {
                // Handle errors if the config file is missing or corrupt
                MessageBox.Show($"Error loading configuration: {ex.Message}");
                // Set a default value if loading fails
                this.isDevelopment = true;  
            }


            Logger.Log("🛑 In LoginForm Stopping any existing KioskIdleManager instance before starting a new one.");
            KioskIdleManager.Stop();
            
            Logger.Log("LoginForm initialized 🚀");
            InitializeComponent();
            InitializeCountryCodes();
            InitializeKioskUI();
            InitializeLoginUI();
            //this.FormBorderStyle = FormBorderStyle.Sizable; // Removes the title bar and border

            this.validator = new CValidator();

            

            currentNumpad = new Numpad(new TextBox(), "")
            {
                Owner = this // Set the owner for proper management
            };
            currentNumpad.Hide();
            currentNumpad.Owner = this; // Set the owner for proper management
            currentNumpad.Hide();
        }

        private void InitializeCountryCodes()
        {
            countryCodes = new List<CountryCode>
            {
                new CountryCode { Name = "United States", DialCode = "+1", DisplayName = "United States (+1)" },
                new CountryCode { Name = "India", DialCode = "+91", DisplayName = "India (+91)" },
                new CountryCode { Name = "United Kingdom", DialCode = "+44", DisplayName = "United Kingdom (+44)" },
                new CountryCode { Name = "Canada", DialCode = "+1", DisplayName = "Canada (+1)" },
                new CountryCode { Name = "Australia", DialCode = "+61", DisplayName = "Australia (+61)" }
            };
            Logger.Log("Country codes initialized 📱");
        }

        internal void InitializeLoginUI()
        {
            this.Text = "Login";
            this.BackColor = ColorTranslator.FromHtml("#11150f");
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;


            loginPanel = new Panel
            {
                Size = new Size(500, 700),
                BackColor = Color.Transparent
            };
            this.Controls.Add(loginPanel);

            this.Load += (s, e) =>
            {
                CenterLoginUI(loginPanel);
            };

            this.Resize += (s, e) =>
            {
                CenterLoginUI(loginPanel);
            };

            stackPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20, 10, 20, 20),
                BackColor = Color.Transparent,
                AutoScroll = true,
                
            };
            loginPanel.Controls.Add(stackPanel);

            titleImage = new PictureBox
            {
                Name = "titlePictureBox",
                Size = new Size(500, 250),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
            };

            string imagePath = Path.Combine(Application.StartupPath, "Image", "PocketMint.png");
            if (File.Exists(imagePath))
            {
                titleImage.Image = Image.FromFile(imagePath);
            }
            else
            {
                MessageBox.Show("Logo image not found at: " + imagePath, "Error");
            }

            this.Controls.Add(titleImage);

            subtitle = new Label
            {
                Name = "subtitleLabel",
                Text = "Login",
                Font = new Font("Poppins", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            this.Controls.Add(subtitle);

            // ------------------------------------
            // Mobile Number Input Section
            // ------------------------------------
            phoneLabel = new Label
            {
                Text = "Mobile No.",
                ForeColor = Color.White,
                Font = new Font("Poppins", 14),
                Margin = new Padding(0, 0, 0, 2),
                AutoSize = true
            };
            stackPanel.Controls.Add(phoneLabel);

            phonePanel = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#1a1a1a"),
                Height = 40,
                // --- FIX: Changed width from 440 to 460 to match the login button ---
                Width = 460,
                Margin = new Padding(0, 5, 0, 10),
            };

            phonePrefix = new Label
            {
                // --- NOTE: Your code correctly sets "+91". The screenshot might be from an older version. ---
                Text = isDevelopment ? "+91" : "+1",
                Width = 80,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ColorTranslator.FromHtml("#454545"),
                ForeColor = Color.White,
                Font = new Font("Poppins", 16),
                Dock = DockStyle.Left,
                Cursor = Cursors.Hand
            };

            var dropdownWrapper = new Panel
            {
                Width = 300,
                Height = 40,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = Padding.Empty,
                Visible = false,
            };

            countryDropdown = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = countryCodes,
                DisplayMember = "DisplayName",
                ValueMember = "DialCode",
                Width = 298,
                Font = new Font("Poppins", 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Margin = new Padding(0),
            };

            countryDropdown.Region = System.Drawing.Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, countryDropdown.Width, countryDropdown.Height, 8, 8)
            );

            dropdownWrapper.Controls.Add(countryDropdown);
            this.Controls.Add(dropdownWrapper);

            phonePrefix.Click += (s, e) =>
            {
                var screenLocation = phonePrefix.PointToScreen(new Point(0, phonePrefix.Height));
                var clientLocation = this.PointToClient(screenLocation);
                dropdownWrapper.Location = clientLocation;
                dropdownWrapper.BringToFront();
                dropdownWrapper.Visible = true;
                countryDropdown.DroppedDown = true;
            };

            countryDropdown.SelectedIndexChanged += (s, e) =>
            {
                if (countryDropdown.SelectedItem is CountryCode selectedCode)
                {
                    phonePrefix.Text = selectedCode.DialCode;
                }
                dropdownWrapper.Visible = false;
            };

            var phoneTextContainer = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#222223"),
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 0)
            };

            phoneTextBox = new TextBox
            {
                Text = isDevelopment ? "7283957717" : "",
                BorderStyle = BorderStyle.None,
                BackColor = ColorTranslator.FromHtml("#222223"),
                ForeColor = Color.White,
                Font = new Font("Poppins", 16),
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                ReadOnly = true
            };

            phoneTextBox.Click += (s, e) =>
            {
                ShowNumpad(phoneTextBox, "MobileNumber");
            };

            phoneTextContainer.Controls.Add(phoneTextBox);
            phonePanel.Controls.Add(phoneTextContainer);
            phonePanel.Controls.Add(phonePrefix);
            stackPanel.Controls.Add(phonePanel);

            // --- Dynamically calculate and set vertical padding ---

            // 1. Measure the height of the text using the textbox's font
            int textHeight = TextRenderer.MeasureText("0", phoneTextBox.Font).Height;

            // 2. Get the available height inside the container
            int containerHeight = phoneTextContainer.ClientSize.Height;

            // 3. Calculate the required top padding to center the text
            int topPadding = (containerHeight - textHeight) / 2;

            // 4. Apply the new, calculated padding
            if (topPadding > 0)
            {
                phoneTextContainer.Padding = new Padding(10, topPadding, 10, 0);
            }

            loginButton = new Button
            {
                Name = "loginButton",
                Text = "Login",
                BackColor = ColorTranslator.FromHtml("#25c866"),
                ForeColor = Color.White,
                Font = new Font("Poppins", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Width = 460,
                Height = 60,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 20, 0, 0)
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.FlatAppearance.MouseOverBackColor = Color.White;
            loginButton.FlatAppearance.MouseDownBackColor = Color.White;
            loginButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, loginButton.Width, loginButton.Height, 12, 12)
            );
            loginButton.Click += LoginButton_Click;
            stackPanel.Controls.Add(loginButton);

            // AddButton hover events
            loginButton.MouseEnter += (sender, e) =>
            {
                loginButton.ForeColor = ColorTranslator.FromHtml("#25c866");
                loginButton.Invalidate(); // Trigger panel's Paint event to draw border
            };
            loginButton.MouseLeave += (sender, e) =>
            {
                loginButton.ForeColor = Color.White;
                loginButton.Invalidate(); // Trigger panel's Paint event to remove border
            };


            // ------------------------------------
            // OTP Verification Section (Initially Hidden)
            // ------------------------------------
            otpLabel = new Label
            {
                Text = "Enter OTP",
                ForeColor = Color.White,
                Font = new Font("Poppins", 14),
                Margin = new Padding(0, 15, 0, 10),
                AutoSize = false,
                Width = 460,
                Height= 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            stackPanel.Controls.Add(otpLabel);

            // Inside your form's constructor or load method:
            // Use a TableLayoutPanel instead of FlowLayoutPanel for better control over cell sizes
            otpBoxesPanel = new TableLayoutPanel
            {
                Width = 460, // Match the width of other main controls
                Height = 80, // Set a fixed height for the panel
                RowCount = 1,
                ColumnCount = 4,
                Visible = false,
                BackColor = Color.Transparent, // Ensure the panel is transparent
                Padding = new Padding(0)
            };
            stackPanel.Controls.Add(otpBoxesPanel);

            // Set column styles to make them all equal width
            for (int i = 0; i < 4; i++)
            {
                otpBoxesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            otpTextBoxes = new TextBox[4];

            for (int i = 0; i < 4; i++)
            {
                // A panel to act as a container and manage the border
                var containerPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(10, 10, 10, 10),
                    Padding = new Padding(1), // Use padding to create the border effect
                    BackColor = ColorTranslator.FromHtml("#363636") // Border color
                };

                var otpTextBox = new TextBox
                {
                    MaxLength = 1,
                    Font = new Font("Poppins", 20, FontStyle.Bold),
                    TextAlign = HorizontalAlignment.Center,
                    BackColor = ColorTranslator.FromHtml("#1C1C1C"), // A dark charcoal gray
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.None, // No default border
                    Dock = DockStyle.Fill,
                    Tag = i,
                    ReadOnly = true
                };

                // Event handlers for functionality
                otpTextBox.Enter += OtpTextBox_Enter;
                otpTextBox.Leave += OtpTextBox_Leave;
                otpTextBox.TextChanged += OtpTextBox_TextChanged;
                otpTextBox.KeyDown += OtpTextBox_KeyDown;

                // Attach the new click handler
                otpTextBox.Click += ShowNumpad_Click;

                // Add TextBox to the container panel
                containerPanel.Controls.Add(otpTextBox);
                // Add the container panel to the TableLayoutPanel
                otpBoxesPanel.Controls.Add(containerPanel, i, 0);
                otpTextBoxes[i] = otpTextBox;
            }

            infoLabel = new Label
            {
                Text = "A code has been sent to your phone",
                ForeColor = Color.LightGray,
                Font = new Font("Poppins", 12),
                AutoSize = false,
                Width = 460,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10),
                Visible = false,
            };
            stackPanel.Controls.Add(infoLabel);

            var timerAndResendContainer = new Panel
            {
                Width = 460, // Match your stackPanel width
                Height = 30, // Height is same as timerLabel
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10) // Use a consistent margin
            };
            stackPanel.Controls.Add(timerAndResendContainer);

            // Create the timer label and add it to the container
            timerLabel = new Label
            {
                Text = "00:59",
                ForeColor = Color.White,
                Font = new Font("Poppins", 11, FontStyle.Bold),
                AutoSize = false,
                Width = 460,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                Location = new Point(0, 0) // Position at the top-left of the container
            };
            timerAndResendContainer.Controls.Add(timerLabel);

            // Create the resend button and add it to the container
            resendButton = new Button
            {
                Text = "Resend OTP",
                ForeColor = Color.DodgerBlue,
                BackColor = Color.Transparent,
                Font = new Font("Poppins", 10, FontStyle.Bold | FontStyle.Underline),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseDownBackColor = Color.Transparent, MouseOverBackColor = Color.Transparent },
                AutoSize = true,
                Width = 460,
                Height = 40,
                Cursor = Cursors.Hand,
                Visible = false, // Initially hidden
                Location = new Point(0, 0)
            };
            resendButton.Click += ResendButton_Click;
            // Position the button centered within the container
            resendButton.Location = new Point(
                (timerAndResendContainer.Width - resendButton.Width) / 2,
                (timerAndResendContainer.Height - resendButton.Height) / 2
            );
            timerAndResendContainer.Controls.Add(resendButton);

            countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            countdownTimer.Tick += CountdownTimer_Tick;

            verifyButton = new Button
            {
                Name = "verifyButton",
                Text = "Verify OTP",
                BackColor = ColorTranslator.FromHtml("#00C853"),
                ForeColor = Color.White,
                Font = new Font("Poppins", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Width = 460,
                Height = 60,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 20, 0, 10),
                Visible = false
            };
            verifyButton.FlatAppearance.BorderSize = 0;
            verifyButton.FlatAppearance.MouseOverBackColor = Color.White;
            verifyButton.FlatAppearance.MouseDownBackColor = Color.White;
            verifyButton.Region = Region.FromHrgn(
               NativeMethods.CreateRoundRectRgn(0, 0, verifyButton.Width, verifyButton.Height, 12, 12)
            );

            verifyButton.Click += VerifyButton_Click;
            // AddButton hover events
            verifyButton.MouseEnter += (sender, e) =>
            {
                verifyButton.ForeColor = ColorTranslator.FromHtml("#25c866");
            };

            verifyButton.MouseLeave += (sender, e) =>
            {
                verifyButton.ForeColor = Color.White;
            };
            stackPanel.Controls.Add(verifyButton);

            var footer = new Label
            {
                Text = "© PocketMint Wallet, 2025. You can visit our Privacy Policy and Terms Conditions.",
                ForeColor = Color.Gray,
                Font = new Font("Poppins", 10),
                Width = 460,
                AutoSize = false,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 0),
            };   
            stackPanel.Controls.Add(footer);
        }

        private async void LoginButton_Click(object? sender, EventArgs e)
        {
            Logger.Log("Login button clicked (Send OTP) 🔑");
            currentNumpad.Close();

            if (string.IsNullOrEmpty(phoneTextBox.Text))
            {
                Logger.Log("Mobile number is empty ⚠️");
                MessageBox.Show("Please enter your mobile number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (phoneTextBox.Text.Trim().Length == 6)
            {
                Logger.Log($"Attempting to validate settlementCode is : {phoneTextBox.Text} 🔐");

                var processingPopup = new ProcessingPopup();
                try
                {
                    // Show the popup early
                    processingPopup.Show(this);
                    await Task.Delay(100); // small delay helps ensure popup renders smoothly

                    ApiResult<SettlementReportData> apiResult = await ApiService.SubmitSettlementReportAsync(phoneTextBox.Text.Trim());

                    // Use the strongly-typed 'Success' property
                    if (apiResult.Success)
                    {
                        // Access the data payload safely
                        var settlementData = apiResult.Data;

                        // Access the message from the ApiResult object
                        Logger.Log("Settlement code is valid.");
                        Logger.Log($"✅ Settlement response received. Success: {apiResult.Success}, Message: {apiResult.Message}");

                        // Access the specific data property safely
                        Logger.Log($"🔍 Receipt URL: {settlementData?.RECEIPTURL}");

                        // Check the specific data property directly
                        if (settlementData?.RECEIPTURL != null)
                        {
                            string pdfUrl = settlementData.RECEIPTURL;

                            var printer = new PdfPrintService.PdfPrintService();
                            await printer.PreviewAndPrintPdfFromUrl(pdfUrl);

                            processingPopup.Close();

                            // NOTE: Update ShowResultAndPrintReceipt to accept the type-safe result object
                            ShowResultAndPrintReceipt(apiResult);
                            return;
                        }
                        else
                        {
                            processingPopup.Close();
                            MessageBox.Show("Receipt URL is missing or response is invalid.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else // Handle failure case
                    {
                        Logger.Log($"❌ Settlement failed: {apiResult.Message}");
                        processingPopup.Close();
                        MessageBox.Show($"Settlement failed: {apiResult.Message}", "Settlement Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    processingPopup.Close();
                    Logger.LogError("🚨 Exception in ValidSettlementCode", ex);
                    MessageBox.Show("A critical error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    if (processingPopup != null && !processingPopup.IsDisposed)
                    {
                        processingPopup.Close();
                    }
                }
            }
            
            fullMobileNumber = phonePrefix.Text.Trim() + phoneTextBox.Text.Trim();
            Logger.Log($"Attempting to send OTP to: {fullMobileNumber} 📲");

            bool otpSentSuccessfully;
            try
            {
                otpSentSuccessfully = await ApiService.SendOtpAsync(fullMobileNumber);
            }
            catch (Exception ex)
            {
                Logger.LogError("API Error during OTP send ❌", ex);
                MessageBox.Show($"Error sending OTP:\n{ex.Message}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (otpSentSuccessfully)
            {
                Logger.Log("OTP sent successfully. Showing OTP verification controls. ✅");

                Logger.Log($"✨ In Login OTP sent so Starting KioskIdleManager with {30}-second timeout for OTP screen.");
                KioskIdleManager.Start(30, "Logout");

                // Hide ONLY the login button
                loginButton.Visible = false;

                // Change the subtitle and show the OTP controls
                subtitle.Text = "Verify OTP";
                otpLabel.Visible = true;
                otpBoxesPanel.Visible = true;
                infoLabel.Visible = true;
                verifyButton.Visible = true;

                // Start the timer
                StartTimer();

                otpTextBoxes[0].Focus();

                // Recenter the UI to account for the size change
                CenterLoginUI(loginPanel);
            }
            else
            {
                string errMsg = $"Failed to send OTP to: {fullMobileNumber} ❌";
                Logger.LogError(errMsg, new Exception("OTP sending API returned false."));
                MessageBox.Show("Invalid mobile number or failed to send OTP. Please try after some time.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Optional: If sending the OTP fails, you might want to restart the timer
                // with the original short timeout.

                Logger.Log("✨ In Login OTP send failed so Starting KioskIdleManager with 10-second timeout to return to login screen.");
                ResetToLogin();
            }
        }

        private void ShowResultAndPrintReceipt(ApiResult<SettlementReportData> result)
        {

            var successPopup = new SuccessPopup(AppSession.StoreName!, 0, result.Success != null ? (bool)result.Success : true, (string)result.Message ?? "ok", "pdf");
            successPopup.ShowDialog(this);

            // Handle the user's choice from the popup
            if (result.Success != null ? (bool)result.Success : true)
            {
                if (successPopup.DialogResult == DialogResult.OK)
                {
                    ResetToLogin();
                }
                else   
                {
                    Logger.Log("(Kiosk Report) Manual logout initiated by user.");
                }
            }
        }


        private async void VerifyButton_Click(object? sender, EventArgs e)
        {
            Logger.Log("Verify OTP button clicked 🔐");
            currentNumpad.Close();
            string otp = otpTextBoxes[0].Text + otpTextBoxes[1].Text + otpTextBoxes[2].Text + otpTextBoxes[3].Text;

            //if (otp.Length != 4)
            //{
            //    MessageBox.Show("Please enter the complete 4-digit OTP.", "Invalid OTP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            bool isOtpValid;
            try
            {
                isOtpValid = await ApiService.VerifyOtpAsync(fullMobileNumber, otp);
            }
            catch (Exception ex)
            {
                Logger.LogError("API Error during OTP verification ❌", ex);
                MessageBox.Show($"Error verifying OTP:\n{ex.Message}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isOtpValid)
            {
                countdownTimer.Stop();
                Logger.Log($"Login successful ✅ User: {AppSession.CustomerName}, Balance: {AppSession.CustomerBALANCE}💰");
                Logger.LogNewUserStart($"{AppSession.CustomerName}");

                // Stop the timer for the OTP screen before proceeding.
                KioskIdleManager.Stop();

                var dashboard = new NV22SpectralInteg.Dashboard.Dashboard(this.validator, this.config);

                if (config != null)
                {
                    Global.ComPort = config.ComPort;
                    Logger.Log($"ComPort set from config file: {Global.ComPort}");
                }

                dashboard.Show();
                this.Hide();

                KioskIdleManager.Initialize(Program.PerformLogout);

                Logger.Log($"✨ Starting {30}-second timer for Privacy Policy.");
                KioskIdleManager.Start(30, "Logout");


                var terms = new NV22SpectralInteg.PrivacyPolicy.PrivacyPolicyWindow(dashboard);
                var result = terms.ShowDialog(dashboard); // This makes the policy appear over the dashboard.

                KioskIdleManager.Stop();
                Logger.Log("🛑 Privacy Policy closed. Timer stopped.");

                if (result == DialogResult.OK)
                {
                    Logger.Log("Privacy Policy accepted ✅. Starting dashboard main loop.");
                    KioskIdleManager.Initialize(dashboard.PerformTransaction);
                   
                    Logger.Log($"✨ Starting {60}-second timer for Dashboard.");
                    KioskIdleManager.Start(60, "Logout");

                    dashboard.MainLoop();
                }
                else
                {
                    Logger.Log("Privacy Policy not accepted ❌ Returning to login");
                    if (!dashboard.IsDisposed)
                    {
                        dashboard.Close();
                    }
                    Logger.Log("✨ In Login Privacy Policy not accepted so Starting KioskIdleManager with 10-second timeout to return to login screen.");
                    this.ResetToLogin();
                    this.Show();
                }
            }
            else
            {
                Logger.LogError("Invalid OTP entered.", new Exception("OTP verification API returned false."));
                MessageBox.Show("The OTP you entered is incorrect. Please try again.", "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                foreach (var box in otpTextBoxes) box.Clear();
                otpTextBoxes[0].Focus();
            }
        }

        public void ResetToLogin()
        {
            Logger.Log("Resetting login form to initial state 🔄");
            Logger.Log("✨ In ResetToLogin Stopping KioskIdleManager");
            KioskIdleManager.Stop();

            Logger.LogSeparator("USER Logout");

            // Reset UI first
            subtitle.Text = "Login";
            phoneTextBox.Text = "";

            // Hide OTP-related controls
            otpLabel.Visible = false;
            otpBoxesPanel.Visible = false;
            infoLabel.Visible = false;
            verifyButton.Visible = false;
            timerLabel.Visible = false;
            resendButton.Visible = false;
            currentNumpad.Hide();

            // Show the initial login button
            loginButton.Visible = true;

            // Clear OTP boxes
            foreach (var box in otpTextBoxes)
                box.Clear();

            // Stop countdown timer
            countdownTimer.Stop();

            // Recalculate layout before focus
            CenterLoginUI(loginPanel);

            // Set focus
            phoneTextBox.Focus();

            // Finally re-initialize idle manager
            KioskIdleManager.Initialize(Program.PerformLogout);
        }


        private void ShowNumpad(TextBox target, string targetName, TextBox[]? otpBoxes = null, int currentIndex = -1)
        {
            // Check if the numpadInstance is null or has been disposed of.
            // If so, create a new instance.
            if (currentNumpad == null || currentNumpad.IsDisposed)
            {
                // Pass the target into the constructor and store the new instance
                currentNumpad = new Numpad(target, targetName, otpBoxes ?? Array.Empty<TextBox>(), currentIndex);
                currentNumpad.Owner = this; // Set the owner for proper management.
            }
            else
            {
                // If the numpad is already open, simply update its target.
                // The SetTarget method must be implemented in your Numpad class.
                currentNumpad.SetTarget(target, targetName, otpBoxes ?? Array.Empty<TextBox>(), currentIndex);
            }

            Point textBoxScreenLocation;

            // Get the screen location of the target TextBox
            if (targetName == "OTPNumber" && otpBoxes != null && otpBoxes.Length > 0)
            {
                // Fix the location to the first OTP box to prevent the numpad from moving.
                textBoxScreenLocation = otpBoxes[0].PointToScreen(Point.Empty);
            }
            else
            {
                // For all other text boxes (like phone number or kiosk ID), use the current target's location.
                textBoxScreenLocation = target.PointToScreen(Point.Empty);
            }
            // Calculate the new numpad location
            int numpadX = textBoxScreenLocation.X;
            int numpadY = textBoxScreenLocation.Y + target.Height + 5;

            // Set the location of the numpad
            currentNumpad.Location = new Point(numpadX, numpadY);

            // Show the numpad
            currentNumpad.Show();
        }



        // Add these two new event handler methods to your class
        private void ShowNumpad_Click(object? sender, EventArgs e)
        {
            if (sender is TextBox currentTextBox && currentTextBox.Tag is int currentIndex)
            {
                // Call the shared method for the OTP fields
                ShowNumpad(currentTextBox, "OTPNumber", otpTextBoxes, currentIndex);
            }
        }

        private void OtpTextBox_Enter(object? sender, EventArgs e)
        {
            if (sender is TextBox currentTextBox && currentTextBox.Parent is Panel parentPanel)
            {
                ShowNumpad_Click(sender, e);
            }
        }

        private void OtpTextBox_Leave(object? sender, EventArgs e)
        {
            if (sender is TextBox currentTextBox && currentTextBox.Parent is Panel parentPanel)
            {
                // Change the border color back to gray when the box is inactive
                parentPanel.BackColor = Color.DimGray;
            }
        }

        // Helper Methods that were missing from your snippet
        private void OtpTextBox_TextChanged(object? sender, EventArgs e)
        {
            var currentTextBox = sender as TextBox;
            if (currentTextBox == null) return;

            var containerPanel = currentTextBox.Parent as Panel;
            if (containerPanel == null) return;

            if (currentTextBox.Text.Length > 0)
            {
                containerPanel.Padding = new Padding(1, 1, 1, 0);
            }
            else
            {
                containerPanel.Padding = new Padding(1);
            }

        }

        private void OtpTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            var currentTextBox = sender as TextBox;
            if (currentTextBox == null || currentTextBox.Tag == null) return;

            if (currentTextBox.Tag is int currentIndex)
            {
                if (e.KeyCode == Keys.Back && currentTextBox.Text.Length == 0 && currentIndex > 0)
                {
                    otpTextBoxes[currentIndex - 1].Focus();
                }
            }
        }

        private void CenterLoginUI(Control contentPanel)
        {
            if (titleImage == null || subtitle == null || contentPanel == null) return;

            int topOffset = 50;
            int totalHeight = titleImage.Height + 5 + subtitle.Height + 30 + contentPanel.Height;
            int topStart = Math.Max(0, (this.ClientSize.Height - totalHeight) / 2 + topOffset);

            titleImage.Left = (this.ClientSize.Width - titleImage.Width) / 2;
            titleImage.Top = topStart;

            subtitle.Left = (this.ClientSize.Width - subtitle.Width) / 2;
            subtitle.Top = titleImage.Bottom + 5;

            loginPanel.Left = (this.ClientSize.Width - loginPanel.Width) / 2;
            loginPanel.Top = subtitle.Bottom + 30;
        }

       

        private void StartTimer()
        {
            timeRemaining = 59;
            timerLabel.Text = $"00:{timeRemaining:00}";

            // Make timer visible, hide button
            timerLabel.Visible = true;
            resendButton.Visible = false;

            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            timeRemaining--;
            timerLabel.Text = $"00:{timeRemaining:00}";

            if (timeRemaining <= 0)
            {
                countdownTimer.Stop();
                // Make button visible, hide timer
                timerLabel.Visible = false;
                resendButton.Visible = true;
            }
        }

        private async void ResendButton_Click(object? sender, EventArgs e)
        {
            Logger.Log("Resend OTP clicked. yeniden");

            bool otpResent = await ApiService.SendOtpAsync(fullMobileNumber);

            if (otpResent)
            {
                //MessageBox.Show("A new OTP has been sent to your mobile number.", "OTP Resent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StartTimer();
            }
            else
            {
                MessageBox.Show("Failed to resend OTP. Please try again in a moment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoginForm_Shown(object? sender, EventArgs e)
        {
            this.Opacity = 1;
            var kioskInfo = TransactionRepository.GetKioskInfo();

            if (string.IsNullOrEmpty(kioskInfo.kioskId) || config.UpdateKioskId)
            {
                kioskPanel.Visible = true;
                loginPanel.Visible = false;
                if (subtitle != null)
                {
                    subtitle.Visible = false;
                }
            }
            else
            {
                AppSession.KioskId = kioskInfo.kioskId;
                AppSession.KioskRegId = kioskInfo.kioskRegId;
                AppSession.StoreName = kioskInfo.storeName;
                AppSession.StoreAddress = kioskInfo.storeAddress;

                kioskPanel.Visible = false;
                loginPanel.Visible = true;
                if (subtitle != null)
                {
                    subtitle.Visible = true;
                }
            }
        }

        private void InitializeKioskUI()
        {
            kioskPanel = new Panel
            {
                Size = new Size(380, 160),
                BackColor = ColorTranslator.FromHtml("#1e1e1e"),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(kioskPanel);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            kioskPanel.Controls.Add(layout);

            var kioskLabel = new Label
            {
                Text = "Enter Kiosk ID",
                ForeColor = Color.White,
                Font = new Font("Poppins", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var kioskPanelContainer = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#1a1a1a"),
                Height = 40,
                Width = 340,
                Margin = new Padding(0, 5, 0, 10),
                Dock = DockStyle.Fill,
            };

            var kioskTextContainer = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#222223"),
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 0)
            };

            var kioskTextBox = new TextBox
            {
                Text = isDevelopment ? "1" : "",
                Font = new Font("Poppins", 11),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#222223"),
                ForeColor = Color.White,
                Margin = new Padding(0),
                ReadOnly = true
            };

            kioskTextBox.Click += (s, e) =>
            {
                ShowNumpad(kioskTextBox, "KioskNumber");
            };

            kioskTextContainer.Controls.Add(kioskTextBox);
            kioskPanelContainer.Controls.Add(kioskTextContainer);

            var submitBtn = new Button
            {
                Text = "Submit",
                Font = new Font("Poppins", 11, FontStyle.Bold),
                Width = 340,
                Height = 65,
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#25c866"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            submitBtn.FlatAppearance.BorderSize = 0;
            submitBtn.FlatAppearance.MouseOverBackColor = Color.White;
            submitBtn.FlatAppearance.MouseDownBackColor = Color.White;
            submitBtn.Region = Region.FromHrgn(
               NativeMethods.CreateRoundRectRgn(0, 0, submitBtn.Width, submitBtn.Height, 12, 12)
            );
            // AddButton hover events
            submitBtn.MouseEnter += (sender, e) =>
            {
                submitBtn.ForeColor = ColorTranslator.FromHtml("#25c866");
            };

            submitBtn.MouseLeave += (sender, e) =>
            {
                submitBtn.ForeColor = Color.White;
            };

            layout.Controls.Add(kioskLabel, 0, 0);
            layout.Controls.Add(kioskPanelContainer, 0, 1);
            layout.Controls.Add(submitBtn, 0, 2);

            submitBtn.Click += async (s, e) =>
            {
                currentNumpad.Close();
                string kioskId = kioskTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(kioskId))
                {
                    MessageBox.Show("Kiosk ID is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var loadingLabel = new Label
                {
                    Text = "Please wait...",
                    ForeColor = Color.White,
                    Font = new Font("Poppins", 12, FontStyle.Italic),
                    AutoSize = true, // Set back to true to get the correct width automatically
                    BackColor = Color.Transparent
                };

                this.Controls.Add(loadingLabel); // Add it to the form first

                loadingLabel.Left = (this.ClientSize.Width - loadingLabel.Width) / 2;
                loadingLabel.Top = kioskPanel.Bottom + 20; // Position it below the input box

                loadingLabel.BringToFront();

                var (isValid, Message) = await ApiService.ValidateAndSetKioskSessionAsync(kioskId);

                this.Controls.Remove(loadingLabel);
                loadingLabel.Dispose();

                if (isValid)
                {
                    kioskPanel.Visible = false;
                    loginPanel.Visible = true;
                    subtitle.Visible = true;
                    CenterLoginUI(loginPanel);
                    this.Controls.Remove(kioskPanel);
                    kioskPanel.Dispose();
                }
                else
                {
                    MessageBox.Show(Message, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            kioskPanel.Visible = false;

            this.Load += (s, e) => CenterKioskPanel();
            this.Resize += (s, e) => CenterKioskPanel();
        }

        private void CenterKioskPanel()
        {
            if (kioskPanel != null && titleImage != null)
            {
                int gap = 0;
                int totalHeight = titleImage.Height + gap + kioskPanel.Height;

                double verticalBias = 0.4;
                int topStart = (int)((this.ClientSize.Height - totalHeight) * verticalBias);

                titleImage.Left = (this.ClientSize.Width - titleImage.Width) / 2;
                titleImage.Top = Math.Max(20, topStart);

                kioskPanel.Left = (this.ClientSize.Width - kioskPanel.Width) / 2;
                kioskPanel.Top = titleImage.Bottom + gap;
            }
        }

    }

    public class CountryCode
    {
        public string? Name { get; set; }
        public string? DialCode { get; set; }
        public string? DisplayName { get; set; }
    }

    internal class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool HideCaret(IntPtr hWnd);
    }
}

