using Newtonsoft.Json;
using NV22SpectralInteg.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
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
        private readonly bool isDevelopment = false;

        private Panel kioskPanel;
        private Panel loginPanel;
        private FlowLayoutPanel stackPanel;
        private TextBox phoneTextBox;
        private Label phonePrefix;
        private ComboBox countryDropdown;
        private List<CountryCode> countryCodes;

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
        private FlowLayoutPanel otpBoxesPanel;
        private Label infoLabel;
        private Button verifyButton;

        private CValidator validator;
        public LoginForm()
        {
            Logger.Log("LoginForm initialized 🚀");
            InitializeComponent();
            InitializeCountryCodes();
            InitializeKioskUI();
            InitializeLoginUI();

            this.validator = new CValidator();
            this.Shown += LoginForm_Shown;
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
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ColorTranslator.FromHtml("#0a0a0a");
            this.StartPosition = FormStartPosition.CenterScreen;

            if (isDevelopment)
            {
                AddCustomControlBox();
            }

            loginPanel = new Panel
            {
                Size = new Size(500, 550),
                BackColor = Color.Transparent
            };
            this.Controls.Add(loginPanel);

            this.Load += (s, e) =>
            {
                CenterLoginUI(loginPanel);
                if (isDevelopment) PositionControlBox();
            };

            this.Resize += (s, e) =>
            {
                CenterLoginUI(loginPanel);
                if (isDevelopment) PositionControlBox();
            };

            stackPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20, 10, 20, 20),
                BackColor = Color.Transparent,
                AutoScroll = true
            };
            loginPanel.Controls.Add(stackPanel);

            titleImage = new PictureBox
            {
                Name = "titlePictureBox",
                Size = new Size(500, 150),
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
                Width = 440,
                Margin = new Padding(0, 5, 0, 10),
            };

            phonePrefix = new Label
            {
                Text = "+91",
                Width = 50,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ColorTranslator.FromHtml("#454545"),
                ForeColor = Color.White,
                Font = new Font("Poppins", 12),
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
                Padding = new Padding(10, 8, 10, 8)
            };

            phoneTextBox = new TextBox
            {
                Text = "7283957717",
                BorderStyle = BorderStyle.None,
                BackColor = ColorTranslator.FromHtml("#222223"),
                ForeColor = Color.White,
                Font = new Font("Poppins", 12),
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            phoneTextContainer.Controls.Add(phoneTextBox);
            phonePanel.Controls.Add(phoneTextContainer);
            phonePanel.Controls.Add(phonePrefix);
            stackPanel.Controls.Add(phonePanel);

            loginButton = new Button
            {
                Name = "loginButton",
                Text = "Login",
                BackColor = ColorTranslator.FromHtml("#00C853"),
                ForeColor = Color.White,
                Font = new Font("Poppins", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Width = 460,
                Height = 55,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 20, 0, 10)
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, loginButton.Width, loginButton.Height, 12, 12)
            );
            loginButton.Click += LoginButton_Click;
            loginButton.MouseEnter += (s, e) => loginButton.BackColor = ColorTranslator.FromHtml("#00E676");
            loginButton.MouseLeave += (s, e) => loginButton.BackColor = ColorTranslator.FromHtml("#00C853");
            stackPanel.Controls.Add(loginButton);

            // ------------------------------------
            // OTP Verification Section (Initially Hidden)
            // ------------------------------------
            otpLabel = new Label
            {
                Text = "Enter OTP",
                ForeColor = Color.White,
                Font = new Font("Poppins", 14),
                Margin = new Padding(0, 15, 0, 20),
                AutoSize = false,
                Width = 460,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            stackPanel.Controls.Add(otpLabel);

            otpBoxesPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Width = 460,
                Height = 40,
                Padding = new Padding(35, 0, 0, 0),
                Visible = false
            };
            stackPanel.Controls.Add(otpBoxesPanel);

            otpTextBoxes = new TextBox[4];
            for (int i = 0; i < 4; i++)
            {
                var otpBoxContainer = new Panel
                {
                    Width = 60,
                    Height = 50,
                    Margin = new Padding(15, 0, 15, 0),
                    BackColor = Color.Gray,
                    Padding = new Padding(1)
                };

                var otpTextBox = new TextBox
                {
                    MaxLength = 1,
                    Font = new Font("Poppins", 18, FontStyle.Bold),
                    TextAlign = HorizontalAlignment.Center,
                    BackColor = ColorTranslator.FromHtml("#222223"),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    Dock = DockStyle.Fill,
                    Tag = i
                };

                otpTextBox.TextChanged += OtpTextBox_TextChanged;
                otpTextBox.KeyDown += OtpTextBox_KeyDown;
                otpBoxContainer.Controls.Add(otpTextBox);
                otpBoxesPanel.Controls.Add(otpBoxContainer);
                otpTextBoxes[i] = otpTextBox;
            }

            infoLabel = new Label
            {
                Text = "A code has been sent to your phone",
                ForeColor = Color.LightGray,
                Font = new Font("Poppins", 10),
                AutoSize = false,
                Width = 460,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 10, 0, 0),
                Visible = false
            };
            stackPanel.Controls.Add(infoLabel);

            var timerResendPanel = new Panel
            {
                Width = 460, // Match the width of other controls in your layout
                Height = 20, // Use the same height as your timer label
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 0) // Use the same margin as other controls
            };
            stackPanel.Controls.Add(timerResendPanel);

            var timerAndResendContainer = new Panel
            {
                Width = 460, // Match your stackPanel width
                Height = 20, // Height is same as timerLabel
                BackColor = Color.Transparent,
                Margin = new Padding(0, 5, 0, 5) // Use a consistent margin
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
                Height = 20,
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
                Height = 30,
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
                Height = 55,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 20, 0, 10),
                Visible = false
            };
            verifyButton.FlatAppearance.BorderSize = 0;
            verifyButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, verifyButton.Width, verifyButton.Height, 12, 12)
            );
            verifyButton.Click += VerifyButton_Click;
            verifyButton.MouseEnter += (s, e) => verifyButton.BackColor = ColorTranslator.FromHtml("#00E676");
            verifyButton.MouseLeave += (s, e) => verifyButton.BackColor = ColorTranslator.FromHtml("#00C853");
            stackPanel.Controls.Add(verifyButton);

            var footer = new Label
            {
                Text = "© PocketMint Wallet, 2025. You can visit our Privacy Policy and Terms Conditions.",
                ForeColor = Color.Gray,
                Font = new Font("Poppins", 8),
                Width = 440,
                AutoSize = false,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 5, 0, 0)
            };
            stackPanel.Controls.Add(footer);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            Logger.Log("Login button clicked (Send OTP) 🔑");

            if (string.IsNullOrEmpty(phoneTextBox.Text))
            {
                Logger.Log("Mobile number is empty ⚠️");
                MessageBox.Show("Please enter your mobile number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            fullMobileNumber = phonePrefix.Text.Trim() + phoneTextBox.Text.Trim();
            Logger.Log($"Attempting to send OTP to: {fullMobileNumber} 📲");

            bool otpSentSuccessfully;
            try
            {
                otpSentSuccessfully = await SendOtpAsync(fullMobileNumber);
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
            }
        }

        private async void VerifyButton_Click(object sender, EventArgs e)
        {
            Logger.Log("Verify OTP button clicked 🔐");

            string otp = otpTextBoxes[0].Text + otpTextBoxes[1].Text + otpTextBoxes[2].Text + otpTextBoxes[3].Text;

            //if (otp.Length != 4)
            //{
            //    MessageBox.Show("Please enter the complete 4-digit OTP.", "Invalid OTP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            bool isOtpValid;
            try
            {
                isOtpValid = await VerifyOtpAsync(fullMobileNumber, otp);
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
                this.Hide();

                var dashboard = new NV22SpectralInteg.Dashboard.Dashboard(this.validator);
                dashboard.Shown += (s, args) =>
                {
                    try
                    {
                        Logger.Log("Showing Privacy Policy popup 📜");
                        var terms = new NV22SpectralInteg.PrivacyPolicy.PrivacyPolicyWindow();
                        var result = terms.ShowDialog(dashboard);

                        if (result != DialogResult.OK)
                        {
                            Logger.Log("Privacy Policy not accepted ❌ Returning to login");
                            dashboard.Close();
                            this.ResetToLogin();
                            this.Show();
                        }
                        else
                        {
                            Logger.Log("Privacy Policy accepted ✅");
                            Global.ComPort = "COM7";
                            dashboard.MainLoop();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Error displaying Privacy Policy window ❌", ex);
                        MessageBox.Show($"Unexpected error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dashboard.Close();
                        this.ResetToLogin();
                        this.Show();
                    }
                };
                dashboard.Show();
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

            // Reset the main title
            subtitle.Text = "Login";

            // Hide all OTP-related controls
            otpLabel.Visible = false;
            otpBoxesPanel.Visible = false;
            infoLabel.Visible = false;
            verifyButton.Visible = false;
            timerLabel.Visible = false;
            resendButton.Visible = false;

            // Show the initial login button
            loginButton.Visible = true;

            // Clear any text from input fields
            foreach (var box in otpTextBoxes)
            {
                box.Clear();
            }

            // Stop the countdown timer if it's running
            countdownTimer.Stop();

            // Set the focus back to the phone number input
            phoneTextBox.Focus();

            // Recalculate the layout
            CenterLoginUI(loginPanel);
        }

        // Helper Methods that were missing from your snippet
        private void OtpTextBox_TextChanged(object sender, EventArgs e)
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

            int currentIndex = (int)currentTextBox.Tag;
            if (currentTextBox.Text.Length > 0 && currentIndex < otpTextBoxes.Length - 1)
            {
                otpTextBoxes[currentIndex + 1].Focus();
            }
        }

        private void OtpTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var currentTextBox = sender as TextBox;
            if (currentTextBox == null) return;

            int currentIndex = (int)currentTextBox.Tag;

            if (e.KeyCode == Keys.Back && currentTextBox.Text.Length == 0 && currentIndex > 0)
            {
                otpTextBoxes[currentIndex - 1].Focus();
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

        private Control CenterControl(Control ctrl)
        {
            var panel = new Panel
            {
                Width = 440,
                Height = ctrl.Height,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 5, 0, 5)
            };

            ctrl.Left = (panel.Width - ctrl.Width) / 2;
            panel.Controls.Add(ctrl);
            return panel;
        }

        private void AddCustomControlBox()
        {
            controlBoxPanel = new Panel
            {
                Width = 100,
                Height = 40,
                BackColor = Color.Transparent
            };

            var minimizeButton = new Label
            {
                Text = "—",
                Font = new Font("Poppins", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 40,
                Height = 40,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Left
            };
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            minimizeButton.MouseEnter += (s, e) => minimizeButton.ForeColor = Color.Gray;
            minimizeButton.MouseLeave += (s, e) => minimizeButton.ForeColor = Color.White;
            controlBoxPanel.Controls.Add(minimizeButton);

            var closeButton = new Label
            {
                Text = "✕",
                Font = new Font("Poppins", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 40,
                Height = 40,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right
            };
            closeButton.Click += (s, e) => this.Close();
            closeButton.MouseEnter += (s, e) => closeButton.ForeColor = Color.Red;
            closeButton.MouseLeave += (s, e) => closeButton.ForeColor = Color.White;
            controlBoxPanel.Controls.Add(closeButton);

            this.Controls.Add(controlBoxPanel);
        }

        private void PositionControlBox()
        {
            if (controlBoxPanel != null)
            {
                controlBoxPanel.Top = 10;
                controlBoxPanel.Left = this.ClientSize.Width - controlBoxPanel.Width - 10;
                controlBoxPanel.BringToFront();
            }
        }

        private async Task<(bool Success, string ErrorMessage)> ValidateAndSetKioskSessionAsync(string kioskId)
        {
            Logger.Log($"Validating Kiosk ID: {kioskId} 🖥️");
            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = "https://uat.pocketmint.ai/api/kiosks/get/kiosks/details";
                    Logger.Log($"Calling API: {apiUrl} 🌐");

                    var requestBody = new { kioskId = int.Parse(kioskId) };
                    string jsonPayload = JsonConvert.SerializeObject(requestBody);
                    Logger.Log($"Payload: {jsonPayload} 📦");

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.Add("Authorization", "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78");
                    client.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=C4537CD8D22C7AF20A50A08992FD3EFF; Path=/; Secure; HttpOnly");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    string responseText = await response.Content.ReadAsStringAsync();
                    Logger.Log($"Response received 📨: {responseText}");

                    if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("{"))
                    {
                        Logger.Log("Unexpected non-JSON response ⚠️");
                        return (false, "Unexpected (non-JSON) response received.");
                    }

                    var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                    if (result == null)
                    {
                        Logger.Log("API returned null ❌");
                        return (false, "API returned null result.");
                    }

                    if (result.isSucceed != true || result.data == null)
                    {
                        Logger.Log("Invalid Kiosk ID ❌");
                        return (false, "Invalid Kiosk ID. Try again.");
                    }

                    AppSession.KioskId = result.data.KIOSKID;
                    AppSession.KioskRegId = result.data.REGID;

                    Logger.Log($"Kiosk validated ✅ KioskID: {AppSession.KioskId}, RegID: {AppSession.KioskRegId}");
                    return (result.isSucceed == true, null);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error validating Kiosk ID ❌", ex);
                return (false, $"Error validating Kiosk ID:\n{ex.Message}");
            }
        }

        private async Task<bool> SendOtpAsync(string mobileNo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = "https://uat.pocketmint.ai/api/kiosks/send/user/mobileno/otp";

                    var payload = new
                    {
                        mobileNo = mobileNo,
                        kioskId = int.Parse(AppSession.KioskId)
                    };

                    string jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
                    client.DefaultRequestHeaders.Add("Authorization", "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    string responseText = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("{"))
                    {
                        return false;
                    }

                    var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                    if (result == null) return false;

                    if (result != null)
                    {
                        AppSession.smsId = result.smsId;
                    }
                    else
                    {
                        Logger.Log("No data found in OTP send API response ⚠️");
                    }

                    return result.isSucceed == true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in SendOtpAsync", ex);
                return false;
            }
        }

        private async Task<bool> VerifyOtpAsync(string mobileNo, string otp)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = "https://uat.pocketmint.ai/api/kiosks/validate/user/mobileno/otp";
                    Logger.Log($"Calling OTP Verification API: {apiUrl} 🌐");

                    var payload = new
                    {
                        mobileNo = mobileNo,
                        kioskId = int.Parse(AppSession.KioskId),
                        otp = otp,
                        smsId = AppSession.smsId,
                    };

                    string jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    Logger.Log($"Payload: {jsonPayload} 📦");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
                    client.DefaultRequestHeaders.Add("Authorization", "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    string responseText = await response.Content.ReadAsStringAsync();
                    Logger.Log($"Response received 📨: {responseText}");

                    if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("{"))
                    {
                        return false;
                    }

                    var result = JsonConvert.DeserializeObject<dynamic>(responseText);
                    if (result == null) return false;

                    if (result.data != null)
                    {
                        AppSession.CustomerRegId = result.data.REGID;
                        AppSession.CustomerName = result.data.NAME;
                        AppSession.CustomerBALANCE = result.data.BALANCE;
                    }
                    else
                    {
                        Logger.Log("No data found in OTP send API response ⚠️");
                    }

                    return result.isSucceed == true;
                    //return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in VerifyOtpAsync", ex);
                return false;
            }
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

        private void CountdownTimer_Tick(object sender, EventArgs e)
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

        private async void ResendButton_Click(object sender, EventArgs e)
        {
            Logger.Log("Resend OTP clicked. yeniden");

            bool otpResent = await SendOtpAsync(fullMobileNumber);

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

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AppSession.KioskId))
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
                Padding = new Padding(10, 6, 10, 6)
            };

            var kioskTextBox = new TextBox
            {
                Text = "1",
                Font = new Font("Poppins", 11),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#222223"),
                ForeColor = Color.White,
                Margin = new Padding(0)
            };

            kioskTextContainer.Controls.Add(kioskTextBox);
            kioskPanelContainer.Controls.Add(kioskTextContainer);

            var submitBtn = new Button
            {
                Text = "Submit",
                Font = new Font("Poppins", 11, FontStyle.Bold),
                Height = 40,
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#00C853"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            submitBtn.FlatAppearance.BorderSize = 0;

            layout.Controls.Add(kioskLabel, 0, 0);
            layout.Controls.Add(kioskPanelContainer, 0, 1);
            layout.Controls.Add(submitBtn, 0, 2);

            submitBtn.Click += async (s, e) =>
            {
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
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                loadingLabel.Left = (this.ClientSize.Width - loadingLabel.Width) / 2;
                loadingLabel.Top = kioskPanel.Bottom + 20;
                this.Controls.Add(loadingLabel);
                loadingLabel.BringToFront();
                loadingLabel.Refresh();

                var (isValid, errorMessage) = await ValidateAndSetKioskSessionAsync(kioskId);

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
                    MessageBox.Show(errorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            kioskPanel.Visible = false;

            this.Load += (s, e) => CenterKioskPanel();
            this.Resize += (s, e) => CenterKioskPanel();
        }
    }

    //public partial class LoginForm : Form
    //{
    //    private PictureBox titleImage;
    //    private Label subtitle;
    //    private Panel controlBoxPanel;
    //    private readonly bool isDevelopment = false;

    //    private Panel kioskPanel;
    //    private Panel loginPanel;
    //    private System.Windows.Forms.TextBox phoneTextBox;
    //    private Label phonePrefix;
    //    private System.Windows.Forms.ComboBox countryDropdown;
    //    private List<CountryCode> countryCodes;

    //    private Panel otpPanel;
    //    private System.Windows.Forms.TextBox[] otpTextBoxes;
    //    private string fullMobileNumber;

    //    // NEW: Controls for timer and resend functionality
    //    private Label timerLabel;
    //    private System.Windows.Forms.Button resendButton;
    //    private System.Windows.Forms.Timer countdownTimer;
    //    private int timeRemaining;

    //    private Label phoneLabel;
    //    private Panel phonePanel;
    //    private Button loginButton;
    //    private Label otpLabel;
    //    private FlowLayoutPanel otpBoxesPanel;
    //    private Label infoLabel;
    //    private Button verifyButton;

    //    //bool Running = false; // Indicates the status of the main poll loop
    //    //int pollTimer = 250; // Timer in ms between polls
    //    //int reconnectionAttempts = 10, reconnectionInterval = 3; // Connection info to deal with retrying connection to validator
    //    //volatile bool Connected = false, ConnectionFail = false; // Threading bools to indicate status of connection with validator
    //    CValidator Validator; // The main validator class - used to send commands to the unit
    //    //bool FormSetup = false; // Boolean so the form will only be setup once
    //    //System.Windows.Forms.Timer reconnectionTimer = new System.Windows.Forms.Timer(); // Timer used to give a delay between reconnect attempts
    //    //Thread ConnectionThread; // Thread used to connect to the validator

    //    public LoginForm()
    //    {
    //        Logger.Log("LoginForm initialized 🚀");
    //        InitializeComponent();
    //        InitializeCountryCodes();
    //        InitializeKioskUI();
    //        InitializeLoginUI();
    //        InitializeOtpUI();
    //        //timer1.Interval = pollTimer;
    //        //reconnectionTimer.Tick += new EventHandler(reconnectionTimer_Tick);

    //        this.Validator = new CValidator();
    //        this.Shown += LoginForm_Shown;
    //    }

    //    private void reconnectionTimer_Tick(object sender, EventArgs e)
    //    {
    //        if (sender is System.Windows.Forms.Timer)
    //        {
    //            System.Windows.Forms.Timer t = sender as System.Windows.Forms.Timer;
    //            t.Enabled = false;
    //        }
    //    }

    //    //private void timer1_Tick(object sender, EventArgs e)
    //    //{
    //    //    timer1.Enabled = false;
    //    //}

    //    //// The main program loop, this is to control the validator, it polls at
    //    //// a value set in this class (pollTimer).
    //    //void MainLoop()
    //    //{
    //    //    //btnRun.Enabled = false;
    //    //    Validator.CommandStructure.ComPort = Global.ComPort;
    //    //    Validator.CommandStructure.SSPAddress = Global.SSPAddress;
    //    //    Validator.CommandStructure.Timeout = 3000;

    //    //    // connect to the validator
    //    //    if (ConnectToValidator())
    //    //    {
    //    //        Running = true;
    //    //        Logger.Log("\r\nPoll Loop\r\n*********************************\r\n");
    //    //        //btnHalt.Enabled = true;
    //    //    }

    //    //    while (Running)
    //    //    {
    //    //        // if the poll fails, try to reconnect
    //    //        if (!Validator.DoPoll(textBox1))
    //    //        {
    //    //            Logger.Log("Poll failed, attempting to reconnect...\r\n");
    //    //            Connected = false;
    //    //            ConnectionThread = new Thread(ConnectToValidatorThreaded);
    //    //            ConnectionThread.Start();
    //    //            while (!Connected)
    //    //            {
    //    //                if (ConnectionFail)
    //    //                {
    //    //                    Logger.Log("Failed to reconnect to validator\r\n");
    //    //                    return;
    //    //                }
    //    //                Application.DoEvents();
    //    //            }
    //    //            Logger.Log("Reconnected successfully\r\n");
    //    //        }

    //    //        timer1.Enabled = true;
    //    //        // update form
    //    //        //UpdateUI();
    //    //        // setup dynamic elements of win form once
    //    //        if (!FormSetup)
    //    //        {
    //    //            SetupFormLayout();
    //    //            FormSetup = true;
    //    //        }
    //    //        while (timer1.Enabled)
    //    //        {
    //    //            Application.DoEvents();
    //    //            Thread.Sleep(1); // Yield to free up CPU
    //    //        }
    //    //    }

    //    //    //close com port and threads
    //    //    Validator.SSPComms.CloseComPort();

    //    //    //btnRun.Enabled = true;
    //    //    //btnHalt.Enabled = false;
    //    //}

    //    //// This is a one off function that is called the first time the MainLoop()
    //    //// function runs, it just sets up a few of the UI elements that only need
    //    //// updating once.
    //    //private void SetupFormLayout()
    //    //{
    //    //    // need validator class instance
    //    //    if (Validator == null)
    //    //    {
    //    //        MessageBox.Show("Validator class is null.", "ERROR");
    //    //        return;
    //    //    }
    //    //}

    //    //// This function opens the com port and attempts to connect with the validator. It then negotiates
    //    //// the keys for encryption and performs some other setup commands.
    //    //private bool ConnectToValidator()
    //    //{
    //    //    // setup the timer
    //    //    reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

    //    //    // run for number of attempts specified
    //    //    for (int i = 0; i < reconnectionAttempts; i++)
    //    //    {
    //    //        // reset timer
    //    //        reconnectionTimer.Enabled = true;

    //    //        // close com port in case it was open
    //    //        Validator.SSPComms.CloseComPort();

    //    //        // turn encryption off for first stage
    //    //        Validator.CommandStructure.EncryptionStatus = false;

    //    //        // open com port and negotiate keys
    //    //        if (Validator.OpenComPort(textBox1) && Validator.NegotiateKeys(textBox1))
    //    //        {
    //    //            Validator.CommandStructure.EncryptionStatus = true; // now encrypting
    //    //            // find the max protocol version this validator supports
    //    //            byte maxPVersion = FindMaxProtocolVersion();
    //    //            if (maxPVersion > 6)
    //    //            {
    //    //                Validator.SetProtocolVersion(maxPVersion, textBox1);
    //    //            }
    //    //            else
    //    //            {
    //    //                MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
    //    //                return false;
    //    //            }
    //    //            // get info from the validator and store useful vars
    //    //            Validator.ValidatorSetupRequest(textBox1);
    //    //            // Get Serial number
    //    //            Validator.GetSerialNumber(textBox1);
    //    //            // check this unit is supported by this program
    //    //            if (!IsUnitTypeSupported(Validator.UnitType))
    //    //            {
    //    //                MessageBox.Show("Unsupported unit type, this SDK supports the BV series and the NV series (excluding the NV11)");
    //    //                Application.Exit();
    //    //                return false;
    //    //            }
    //    //            // inhibits, this sets which channels can receive notes
    //    //            Validator.SetInhibits(textBox1);
    //    //            // enable, this allows the validator to receive and act on commands
    //    //            Validator.EnableValidator(textBox1);

    //    //            return true;
    //    //        }
    //    //        while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
    //    //    }
    //    //    return false;
    //    //}

    //    //// This is the same as the above function but set up differently for threading.
    //    //private void ConnectToValidatorThreaded()
    //    //{
    //    //    // setup the timer
    //    //    reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

    //    //    // run for number of attempts specified
    //    //    for (int i = 0; i < reconnectionAttempts; i++)
    //    //    {
    //    //        // reset timer
    //    //        reconnectionTimer.Enabled = true;

    //    //        // close com port in case it was open
    //    //        Validator.SSPComms.CloseComPort();

    //    //        // turn encryption off for first stage
    //    //        Validator.CommandStructure.EncryptionStatus = false;

    //    //        // open com port and negotiate keys
    //    //        if (Validator.OpenComPort() && Validator.NegotiateKeys())
    //    //        {
    //    //            Validator.CommandStructure.EncryptionStatus = true; // now encrypting
    //    //            // find the max protocol version this validator supports
    //    //            byte maxPVersion = FindMaxProtocolVersion();
    //    //            if (maxPVersion > 6)
    //    //            {
    //    //                Validator.SetProtocolVersion(maxPVersion);
    //    //            }
    //    //            else
    //    //            {
    //    //                MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
    //    //                Connected = false;
    //    //                return;
    //    //            }
    //    //            // get info from the validator and store useful vars
    //    //            Validator.ValidatorSetupRequest();
    //    //            // inhibits, this sets which channels can receive notes
    //    //            Validator.SetInhibits();
    //    //            // enable, this allows the validator to operate
    //    //            Validator.EnableValidator();

    //    //            Connected = true;
    //    //            return;
    //    //        }
    //    //        while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
    //    //    }
    //    //    Connected = false;
    //    //    ConnectionFail = true;
    //    //}
    //    //// ... (InitializeCountryCodes and InitializeLoginUI are unchanged) ...

    //    //// This function finds the maximum protocol version that a validator supports. To do this
    //    //// it attempts to set a protocol version starting at 6 in this case, and then increments the
    //    //// version until error 0xF8 is returned from the validator which indicates that it has failed
    //    //// to set it. The function then returns the version number one less than the failed version.
    //    //private byte FindMaxProtocolVersion()
    //    //{
    //    //    // not dealing with protocol under level 6
    //    //    // attempt to set in validator
    //    //    byte b = 0x06;
    //    //    while (true)
    //    //    {
    //    //        Validator.SetProtocolVersion(b);
    //    //        if (Validator.CommandStructure.ResponseData[0] == CCommands.SSP_RESPONSE_FAIL)
    //    //            return --b;
    //    //        b++;
    //    //        if (b > 20)
    //    //            return 0x06; // return default if protocol 'runs away'
    //    //    }
    //    //}

    //    //// This function checks whether the type of validator is supported by this program. This program only
    //    //// supports Note Validators so any other type should be rejected.
    //    //private bool IsUnitTypeSupported(char type)
    //    //{
    //    //    if (type == (char)0x00)
    //    //        return true;
    //    //    return false;
    //    //}

    //    private void InitializeCountryCodes()
    //    {
    //        countryCodes = new List<CountryCode>
    //        {
    //            new CountryCode { Name = "United States", DialCode = "+1", DisplayName = "United States (+1)" },
    //            new CountryCode { Name = "India", DialCode = "+91", DisplayName = "India (+91)" },
    //            new CountryCode { Name = "United Kingdom", DialCode = "+44", DisplayName = "United Kingdom (+44)" },
    //            new CountryCode { Name = "Canada", DialCode = "+1", DisplayName = "Canada (+1)" },
    //            new CountryCode { Name = "Australia", DialCode = "+61", DisplayName = "Australia (+61)" }
    //        };
    //        Logger.Log("Country codes initialized 📱");
    //    }


    //    internal void InitializeLoginUI()
    //    {
    //        this.Text = "Login";
    //        this.WindowState = FormWindowState.Maximized;
    //        this.FormBorderStyle = FormBorderStyle.None;
    //        this.BackColor = ColorTranslator.FromHtml("#0a0a0a");
    //        this.StartPosition = FormStartPosition.CenterScreen;

    //        if (isDevelopment)
    //        {
    //            AddCustomControlBox();
    //        }

    //        loginPanel = new Panel
    //        {
    //            Size = new Size(500, 550),
    //            BackColor = Color.Transparent
    //        };
    //        this.Controls.Add(loginPanel);

    //        this.Load += (s, e) =>
    //        {
    //            CenterLoginUI(loginPanel);
    //            if (isDevelopment) PositionControlBox();
    //        };

    //        this.Resize += (s, e) =>
    //        {
    //            CenterLoginUI(loginPanel);
    //            if (isDevelopment) PositionControlBox();
    //        };

    //        var stackPanel = new FlowLayoutPanel
    //        {
    //            Dock = DockStyle.Fill,
    //            FlowDirection = FlowDirection.TopDown,
    //            WrapContents = false,
    //            Padding = new Padding(20),
    //            BackColor = Color.Transparent,
    //            AutoScroll = true
    //        };
    //        loginPanel.Controls.Add(stackPanel);

    //        titleImage = new PictureBox
    //        {
    //            Name = "titlePictureBox",
    //            Size = new Size(500, 150),
    //            SizeMode = PictureBoxSizeMode.Zoom,
    //            BackColor = Color.Transparent,
    //        };

    //        string imagePath = Path.Combine(Application.StartupPath, "Image", "PocketMint.png");
    //        if (File.Exists(imagePath))
    //        {
    //            titleImage.Image = Image.FromFile(imagePath);
    //        }
    //        else
    //        {
    //            MessageBox.Show("Logo image not found at: " + imagePath, "Error");
    //        }

    //        this.Controls.Add(titleImage);

    //        subtitle = new Label
    //        {
    //            Name = "subtitleLabel",
    //            Text = "Login",
    //            Font = new Font("Poppins", 24, FontStyle.Bold),
    //            ForeColor = Color.White,
    //            AutoSize = true
    //        };
    //        this.Controls.Add(subtitle);

    //        var phoneLabel = new Label
    //        {
    //            Text = "Mobile No.",
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 14),
    //            Margin = new Padding(0, 5, 0, 2),
    //            AutoSize = true
    //        };
    //        stackPanel.Controls.Add(phoneLabel);

    //        var phonePanel = new Panel
    //        {
    //            BackColor = ColorTranslator.FromHtml("#1a1a1a"),
    //            Height = 40,
    //            Width = 440,
    //            Margin = new Padding(0, 5, 0, 10),
    //        };

    //        phonePrefix = new Label
    //        {
    //            Text = "+91",
    //            Width = 50,
    //            Height = 40,
    //            TextAlign = ContentAlignment.MiddleCenter,
    //            BackColor = ColorTranslator.FromHtml("#454545"),
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 12),
    //            Dock = DockStyle.Left,
    //            Cursor = Cursors.Hand
    //        };

    //        var dropdownWrapper = new Panel
    //        {
    //            Width = 300,
    //            Height = 40,
    //            BackColor = Color.White,
    //            BorderStyle = BorderStyle.None,
    //            Padding = Padding.Empty,
    //            Visible = false,
    //        };

    //        countryDropdown = new System.Windows.Forms.ComboBox
    //        {
    //            DropDownStyle = ComboBoxStyle.DropDownList,
    //            DataSource = countryCodes,
    //            DisplayMember = "DisplayName",
    //            ValueMember = "DialCode",
    //            Width = 298,
    //            Font = new Font("Poppins", 10),
    //            FlatStyle = FlatStyle.Flat,
    //            BackColor = Color.White,
    //            ForeColor = Color.Black,
    //            Margin = new Padding(0),
    //        };

    //        countryDropdown.Region = System.Drawing.Region.FromHrgn(
    //            NativeMethods.CreateRoundRectRgn(0, 0, countryDropdown.Width, countryDropdown.Height, 8, 8)
    //        );

    //        dropdownWrapper.Controls.Add(countryDropdown);
    //        this.Controls.Add(dropdownWrapper);

    //        phonePrefix.Click += (s, e) =>
    //        {
    //            var screenLocation = phonePrefix.PointToScreen(new Point(0, phonePrefix.Height));
    //            var clientLocation = this.PointToClient(screenLocation);
    //            dropdownWrapper.Location = clientLocation;
    //            dropdownWrapper.BringToFront();
    //            dropdownWrapper.Visible = true;
    //            countryDropdown.DroppedDown = true;
    //        };

    //        countryDropdown.SelectedIndexChanged += (s, e) =>
    //        {
    //            if (countryDropdown.SelectedItem is CountryCode selectedCode)
    //            {
    //                phonePrefix.Text = selectedCode.DialCode;
    //            }
    //            dropdownWrapper.Visible = false;
    //        };

    //        var phoneTextContainer = new Panel
    //        {
    //            BackColor = ColorTranslator.FromHtml("#222223"),
    //            Dock = DockStyle.Fill,
    //            Padding = new Padding(10, 8, 10, 8)
    //        };

    //        phoneTextBox = new System.Windows.Forms.TextBox
    //        {
    //            Text = "7283957717",
    //            BorderStyle = BorderStyle.None,
    //            BackColor = ColorTranslator.FromHtml("#222223"),
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 12),
    //            Dock = DockStyle.Fill,
    //            Margin = new Padding(0)
    //        };

    //        phoneTextContainer.Controls.Add(phoneTextBox);
    //        phonePanel.Controls.Add(phoneTextContainer);
    //        phonePanel.Controls.Add(phonePrefix);
    //        stackPanel.Controls.Add(phonePanel);

    //        var loginButton = new System.Windows.Forms.Button
    //        {
    //            Text = "Login",
    //            BackColor = ColorTranslator.FromHtml("#00C853"),
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 18, FontStyle.Bold),
    //            FlatStyle = FlatStyle.Flat,
    //            Width = 460,
    //            Height = 55,
    //            Cursor = Cursors.Hand,
    //            Margin = new Padding(0, 20, 0, 10)
    //        };

    //        loginButton.FlatAppearance.BorderSize = 0;
    //        loginButton.Region = Region.FromHrgn(
    //            NativeMethods.CreateRoundRectRgn(0, 0, loginButton.Width, loginButton.Height, 12, 12)
    //        );

    //        loginButton.Click += LoginButton_Click;
    //        loginButton.MouseEnter += (s, e) => loginButton.BackColor = ColorTranslator.FromHtml("#00E676");
    //        loginButton.MouseLeave += (s, e) => loginButton.BackColor = ColorTranslator.FromHtml("#00C853");
    //        stackPanel.Controls.Add(loginButton);

    //        var footer = new Label
    //        {
    //            Text = "© PocketMint Wallet, 2025. You can visit our Privacy Policy and Terms Conditions.",
    //            ForeColor = Color.Gray,
    //            Font = new Font("Poppins", 8),
    //            Width = 440,
    //            AutoSize = false,
    //            Height = 40,
    //            TextAlign = ContentAlignment.MiddleCenter,
    //            Margin = new Padding(0, 30, 0, 0)
    //        };
    //        stackPanel.Controls.Add(footer);
    //    }

    //    // MODIFIED: This method is now much larger to include the new timer and resend controls.
    //    private void InitializeOtpUI()
    //    {
    //        otpPanel = new Panel
    //        {
    //            Size = new Size(500, 380),
    //            BackColor = Color.Transparent,
    //            Visible = false
    //        };
    //        this.Controls.Add(otpPanel);

    //        var stackPanel = new FlowLayoutPanel
    //        {
    //            Dock = DockStyle.Fill,
    //            FlowDirection = FlowDirection.TopDown,
    //            WrapContents = false,
    //            Padding = new Padding(20),
    //            BackColor = Color.Transparent,
    //            AutoScroll = true
    //        };
    //        otpPanel.Controls.Add(stackPanel);

    //        var otpLabel = new Label
    //        {
    //            Text = "Enter OTP",
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 14),
    //            Margin = new Padding(0, 5, 0, 10),
    //            AutoSize = false,
    //            Width = 460,
    //            TextAlign = ContentAlignment.MiddleCenter
    //        };
    //        stackPanel.Controls.Add(otpLabel);

    //        var otpBoxesPanel = new FlowLayoutPanel
    //        {
    //            FlowDirection = FlowDirection.LeftToRight,
    //            WrapContents = false,
    //            Width = 460,
    //            Height = 60,
    //            Padding = new Padding(35, 0, 0, 0)
    //        };
    //        stackPanel.Controls.Add(otpBoxesPanel);

    //        otpTextBoxes = new System.Windows.Forms.TextBox[4];
    //        for (int i = 0; i < 4; i++)
    //        {
    //            var otpBoxContainer = new Panel
    //            {
    //                Width = 60,
    //                Height = 50,
    //                Margin = new Padding(15, 0, 15, 0),
    //                BackColor = Color.Gray,
    //                Padding = new Padding(1)
    //            };

    //            var otpTextBox = new System.Windows.Forms.TextBox
    //            {
    //                MaxLength = 1,
    //                Font = new Font("Poppins", 18, FontStyle.Bold),
    //                TextAlign = HorizontalAlignment.Center,
    //                BackColor = ColorTranslator.FromHtml("#222223"),
    //                ForeColor = Color.White,
    //                BorderStyle = BorderStyle.None,
    //                Dock = DockStyle.Fill,
    //                Tag = i
    //            };

    //            otpTextBox.TextChanged += OtpTextBox_TextChanged;
    //            otpTextBox.KeyDown += OtpTextBox_KeyDown;
    //            otpBoxContainer.Controls.Add(otpTextBox);
    //            otpBoxesPanel.Controls.Add(otpBoxContainer);
    //            otpTextBoxes[i] = otpTextBox;
    //        }

    //        // NEW: Adding the info label, timer, and resend button
    //        var infoLabel = new Label
    //        {
    //            Text = "A code has been sent to your phone",
    //            ForeColor = Color.LightGray,
    //            Font = new Font("Poppins", 10),
    //            AutoSize = false,
    //            Width = 460,
    //            TextAlign = ContentAlignment.MiddleCenter,
    //            Margin = new Padding(0, 10, 0, 0)
    //        };
    //        stackPanel.Controls.Add(infoLabel);

    //        timerLabel = new Label
    //        {
    //            Text = "00:59",
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 11, FontStyle.Bold),
    //            AutoSize = false,
    //            Width = 460,
    //            Height = 30,
    //            TextAlign = ContentAlignment.MiddleCenter,
    //            Visible = false // Initially hidden, shown when timer starts
    //        };
    //        stackPanel.Controls.Add(timerLabel);

    //        resendButton = new System.Windows.Forms.Button
    //        {
    //            Text = "Resend OTP",
    //            ForeColor = Color.DodgerBlue,
    //            BackColor = Color.Transparent,
    //            Font = new Font("Poppins", 10, FontStyle.Bold | FontStyle.Underline),
    //            FlatStyle = FlatStyle.Flat,
    //            FlatAppearance = { BorderSize = 0, MouseDownBackColor = Color.Transparent, MouseOverBackColor = Color.Transparent },
    //            AutoSize = true,
    //            Cursor = Cursors.Hand,
    //            Visible = false // Initially hidden
    //        };
    //        resendButton.Click += ResendButton_Click;
    //        stackPanel.Controls.Add(CenterControl(resendButton)); // Use helper to center it

    //        // NEW: Initialize the countdown timer
    //        countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
    //        countdownTimer.Tick += CountdownTimer_Tick;


    //        var verifyButton = new System.Windows.Forms.Button
    //        {
    //            Text = "Verify OTP",
    //            BackColor = ColorTranslator.FromHtml("#00C853"),
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 18, FontStyle.Bold),
    //            FlatStyle = FlatStyle.Flat,
    //            Width = 460,
    //            Height = 55,
    //            Cursor = Cursors.Hand,
    //            Margin = new Padding(0, 20, 0, 10)
    //        };
    //        verifyButton.FlatAppearance.BorderSize = 0;
    //        verifyButton.Region = Region.FromHrgn(
    //            NativeMethods.CreateRoundRectRgn(0, 0, verifyButton.Width, verifyButton.Height, 12, 12)
    //        );
    //        verifyButton.Click += VerifyButton_Click;
    //        verifyButton.MouseEnter += (s, e) => verifyButton.BackColor = ColorTranslator.FromHtml("#00E676");
    //        verifyButton.MouseLeave += (s, e) => verifyButton.BackColor = ColorTranslator.FromHtml("#00C853");
    //        stackPanel.Controls.Add(verifyButton);
    //    }

    //    private void OtpTextBox_TextChanged(object sender, EventArgs e)
    //    {
    //        var currentTextBox = sender as System.Windows.Forms.TextBox;
    //        if (currentTextBox == null) return;

    //        var containerPanel = currentTextBox.Parent as Panel;
    //        if (containerPanel == null) return;

    //        if (currentTextBox.Text.Length > 0)
    //        {
    //            containerPanel.Padding = new Padding(1, 1, 1, 0);
    //        }
    //        else
    //        {
    //            containerPanel.Padding = new Padding(1);
    //        }

    //        int currentIndex = (int)currentTextBox.Tag;
    //        if (currentTextBox.Text.Length > 0 && currentIndex < otpTextBoxes.Length - 1)
    //        {
    //            otpTextBoxes[currentIndex + 1].Focus();
    //        }
    //    }

    //    // ... (OtpTextBox_KeyDown, AddCustomControlBox, PositionControlBox, etc. are unchanged) ...
    //    private void OtpTextBox_KeyDown(object sender, KeyEventArgs e)
    //    {
    //        var currentTextBox = sender as System.Windows.Forms.TextBox;
    //        if (currentTextBox == null) return;

    //        int currentIndex = (int)currentTextBox.Tag;

    //        if (e.KeyCode == Keys.Back && currentTextBox.Text.Length == 0 && currentIndex > 0)
    //        {
    //            otpTextBoxes[currentIndex - 1].Focus();
    //        }
    //    }

    //    private void AddCustomControlBox()
    //    {
    //        controlBoxPanel = new Panel
    //        {
    //            Width = 100,
    //            Height = 40,
    //            BackColor = Color.Transparent
    //        };

    //        var minimizeButton = new Label
    //        {
    //            Text = "—",
    //            Font = new Font("Poppins", 14, FontStyle.Bold),
    //            ForeColor = Color.White,
    //            TextAlign = ContentAlignment.MiddleCenter,
    //            Width = 40,
    //            Height = 40,
    //            Cursor = Cursors.Hand,
    //            Dock = DockStyle.Left
    //        };
    //        minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
    //        minimizeButton.MouseEnter += (s, e) => minimizeButton.ForeColor = Color.Gray;
    //        minimizeButton.MouseLeave += (s, e) => minimizeButton.ForeColor = Color.White;
    //        controlBoxPanel.Controls.Add(minimizeButton);

    //        var closeButton = new Label
    //        {
    //            Text = "✕",
    //            Font = new Font("Poppins", 14, FontStyle.Bold),
    //            ForeColor = Color.White,
    //            TextAlign = ContentAlignment.MiddleCenter,
    //            Width = 40,
    //            Height = 40,
    //            Cursor = Cursors.Hand,
    //            Dock = DockStyle.Right
    //        };
    //        closeButton.Click += (s, e) => this.Close();
    //        closeButton.MouseEnter += (s, e) => closeButton.ForeColor = Color.Red;
    //        closeButton.MouseLeave += (s, e) => closeButton.ForeColor = Color.White;
    //        controlBoxPanel.Controls.Add(closeButton);

    //        this.Controls.Add(controlBoxPanel);
    //    }

    //    private void PositionControlBox()
    //    {
    //        if (controlBoxPanel != null)
    //        {
    //            controlBoxPanel.Top = 10;
    //            controlBoxPanel.Left = this.ClientSize.Width - controlBoxPanel.Width - 10;
    //            controlBoxPanel.BringToFront();
    //        }
    //    }

    //    private void CenterLoginUI(Control contentPanel)
    //    {
    //        if (titleImage == null || subtitle == null || contentPanel == null) return;

    //        int topOffset = 50;
    //        int totalHeight = titleImage.Height + 5 + subtitle.Height + 30 + contentPanel.Height;
    //        int topStart = Math.Max(0, (this.ClientSize.Height - totalHeight) / 2 + topOffset);

    //        titleImage.Left = (this.ClientSize.Width - titleImage.Width) / 2;
    //        titleImage.Top = topStart;

    //        subtitle.Left = (this.ClientSize.Width - subtitle.Width) / 2;
    //        subtitle.Top = titleImage.Bottom + 5;

    //        var activePanel = loginPanel.Visible ? loginPanel : (otpPanel.Visible ? otpPanel : contentPanel);
    //        activePanel.Left = (this.ClientSize.Width - activePanel.Width) / 2;
    //        activePanel.Top = subtitle.Bottom + 30;
    //    }

    //    private void CenterKioskPanel()
    //    {
    //        if (kioskPanel != null && titleImage != null)
    //        {
    //            int gap = 0;
    //            int totalHeight = titleImage.Height + gap + kioskPanel.Height;
    //            int topStart = (this.ClientSize.Height - totalHeight) / 2;

    //            titleImage.Left = (this.ClientSize.Width - titleImage.Width) / 2;
    //            titleImage.Top = Math.Max(20, topStart);

    //            kioskPanel.Left = (this.ClientSize.Width - kioskPanel.Width) / 2;
    //            kioskPanel.Top = titleImage.Bottom + gap;
    //        }
    //    }

    //    private Control CenterControl(Control ctrl)
    //    {
    //        var panel = new Panel
    //        {
    //            Width = 440,
    //            Height = ctrl.Height,
    //            BackColor = Color.Transparent,
    //            Margin = new Padding(0, 5, 0, 5)
    //        };

    //        ctrl.Left = (panel.Width - ctrl.Width) / 2;
    //        panel.Controls.Add(ctrl);
    //        return panel;
    //    }

    //    // MODIFIED: Starts the countdown timer on successful OTP send.
    //    private async void LoginButton_Click(object sender, EventArgs e)
    //    {
    //        Logger.Log("Login button clicked (Send OTP) 🔑");

    //        if (string.IsNullOrEmpty(phoneTextBox.Text))
    //        {
    //            Logger.Log("Mobile number is empty ⚠️");
    //            MessageBox.Show("Please enter your mobile number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //            return;
    //        }

    //        fullMobileNumber = phonePrefix.Text.Trim() + phoneTextBox.Text.Trim();
    //        Logger.Log($"Attempting to send OTP to: {fullMobileNumber} 📲");

    //        bool otpSentSuccessfully;
    //        try
    //        {
    //            otpSentSuccessfully = await SendOtpAsync(fullMobileNumber);
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.LogError("API Error during OTP send ❌", ex);
    //            MessageBox.Show($"Error sending OTP:\n{ex.Message}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //            return;
    //        }

    //        if (otpSentSuccessfully)
    //        {
    //            Logger.Log("OTP sent successfully. Switching to OTP verification view. ✅");
    //            subtitle.Text = "Verify OTP";
    //            loginPanel.Visible = false;
    //            otpPanel.Visible = true;
    //            CenterLoginUI(otpPanel);
    //            otpTextBoxes[0].Focus();

    //            // NEW: Start the timer
    //            StartTimer();
    //        }
    //        else
    //        {
    //            string errMsg = $"Failed to send OTP to: {fullMobileNumber} ❌";
    //            Logger.LogError(errMsg, new Exception("OTP sending API returned false."));
    //            MessageBox.Show("Invalid mobile number or failed to send OTP. Please try again.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //        }
    //    }

    //    // ... (VerifyButton_Click, LoginForm_Shown, InitializeKioskUI, ValidateAndSetKioskSessionAsync, SendOtpAsync, and VerifyOtpAsync are unchanged) ...

    //    private async void VerifyButton_Click(object sender, EventArgs e)
    //    {
    //        Logger.Log("Verify OTP button clicked 🔐");

    //        string otp = otpTextBoxes[0].Text + otpTextBoxes[1].Text + otpTextBoxes[2].Text + otpTextBoxes[3].Text;

    //        if (otp.Length != 4)
    //        {
    //            MessageBox.Show("Please enter the complete 4-digit OTP.", "Invalid OTP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //            return;
    //        }

    //        bool isOtpValid;
    //        try
    //        {
    //            isOtpValid = await VerifyOtpAsync(fullMobileNumber, otp);
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.LogError("API Error during OTP verification ❌", ex);
    //            MessageBox.Show($"Error verifying OTP:\n{ex.Message}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //            return;
    //        }

    //        if (isOtpValid)
    //        {
    //            countdownTimer.Stop(); // Stop the timer on success
    //            Logger.Log($"Login successful ✅ User: {AppSession.CustomerName}, Balance: {AppSession.CustomerBALANCE}💰");
    //            Logger.LogNewUserStart($"{AppSession.CustomerName}");

    //            this.Hide();

    //            var dashboard = new NV22SpectralInteg.Dashboard.Dashboard(this.Validator);

    //            dashboard.Shown += (s, args) =>
    //            {
    //                try
    //                {
    //                    Logger.Log("Showing Privacy Policy popup 📜");
    //                    var terms = new NV22SpectralInteg.PrivacyPolicy.PrivacyPolicyWindow();
    //                    var result = terms.ShowDialog(dashboard);

    //                    if (result != DialogResult.OK)
    //                    {
    //                        Logger.Log("Privacy Policy not accepted ❌ Returning to login");
    //                        dashboard.Close();
    //                        this.ResetToLogin();
    //                        this.Show();
    //                    }
    //                    else
    //                    {
    //                        Logger.Log("Privacy Policy accepted ✅");
    //                        Global.ComPort = "COM5";
    //                        //Validator = new CValidator();
    //                        dashboard.MainLoop();
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    Logger.LogError("Error displaying Privacy Policy window ❌", ex);
    //                    MessageBox.Show($"Unexpected error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                    dashboard.Close();
    //                    this.ResetToLogin();
    //                    this.Show();
    //                }
    //            };
    //            dashboard.Show();
    //        }
    //        else
    //        {
    //            Logger.LogError("Invalid OTP entered.", new Exception("OTP verification API returned false."));
    //            MessageBox.Show("The OTP you entered is incorrect. Please try again.", "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

    //            foreach (var box in otpTextBoxes) box.Clear();
    //            otpTextBoxes[0].Focus();
    //        }
    //    }

    //    public void ResetToLogin()
    //    {
    //        Logger.Log("Resetting login form to initial state 🔄");

    //        // Reset the main title
    //        subtitle.Text = "Login";

    //        // Hide all OTP-related controls
    //        otpLabel.Visible = false;
    //        otpBoxesPanel.Visible = false;
    //        infoLabel.Visible = false;
    //        verifyButton.Visible = false;
    //        timerLabel.Visible = false;
    //        resendButton.Visible = false;

    //        // Show the initial login button
    //        loginButton.Visible = true;

    //        // Clear any text from input fields
    //        foreach (var box in otpTextBoxes)
    //        {
    //            box.Clear();
    //        }

    //        // Stop the countdown timer if it's running
    //        countdownTimer.Stop();

    //        // Set the focus back to the phone number input
    //        phoneTextBox.Focus();

    //        // Recalculate the layout
    //        CenterLoginUI(loginPanel);
    //    }

    //    private void LoginForm_Shown(object sender, EventArgs e)
    //    {
    //        if (string.IsNullOrEmpty(AppSession.KioskId))
    //        {
    //            kioskPanel.Visible = true;
    //            loginPanel.Visible = false;
    //            if (subtitle != null)
    //            {
    //                subtitle.Visible = false;
    //            }
    //        }
    //        else
    //        {
    //            kioskPanel.Visible = false;
    //            loginPanel.Visible = true;
    //            if (subtitle != null)
    //            {
    //                subtitle.Visible = true;
    //            }
    //        }
    //    }

    //    private void InitializeKioskUI()
    //    {
    //        kioskPanel = new Panel
    //        {
    //            Size = new Size(380, 160),
    //            BackColor = ColorTranslator.FromHtml("#1e1e1e"),
    //            BorderStyle = BorderStyle.FixedSingle
    //        };
    //        this.Controls.Add(kioskPanel);

    //        var layout = new TableLayoutPanel
    //        {
    //            Dock = DockStyle.Fill,
    //            ColumnCount = 1,
    //            RowCount = 3,
    //            Padding = new Padding(20),
    //            BackColor = Color.Transparent
    //        };

    //        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
    //        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
    //        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
    //        kioskPanel.Controls.Add(layout);

    //        var kioskLabel = new Label
    //        {
    //            Text = "Enter Kiosk ID",
    //            ForeColor = Color.White,
    //            Font = new Font("Poppins", 11, FontStyle.Bold),
    //            Dock = DockStyle.Fill,
    //            TextAlign = ContentAlignment.MiddleLeft
    //        };

    //        var kioskPanelContainer = new Panel
    //        {
    //            BackColor = ColorTranslator.FromHtml("#1a1a1a"),
    //            Height = 40,
    //            Width = 340,
    //            Margin = new Padding(0, 5, 0, 10),
    //            Dock = DockStyle.Fill,
    //        };

    //        var kioskTextContainer = new Panel
    //        {
    //            BackColor = ColorTranslator.FromHtml("#222223"),
    //            Dock = DockStyle.Fill,
    //            Padding = new Padding(10, 6, 10, 6)
    //        };

    //        var kioskTextBox = new System.Windows.Forms.TextBox
    //        {
    //            Text = "1",
    //            Font = new Font("Poppins", 11),
    //            BorderStyle = BorderStyle.None,
    //            Dock = DockStyle.Fill,
    //            BackColor = ColorTranslator.FromHtml("#222223"),
    //            ForeColor = Color.White,
    //            Margin = new Padding(0)
    //        };

    //        kioskTextContainer.Controls.Add(kioskTextBox);
    //        kioskPanelContainer.Controls.Add(kioskTextContainer);

    //        var submitBtn = new System.Windows.Forms.Button
    //        {
    //            Text = "Submit",
    //            Font = new Font("Poppins", 11, FontStyle.Bold),
    //            Height = 40,
    //            Dock = DockStyle.Fill,
    //            BackColor = ColorTranslator.FromHtml("#00C853"),
    //            ForeColor = Color.White,
    //            FlatStyle = FlatStyle.Flat
    //        };
    //        submitBtn.FlatAppearance.BorderSize = 0;

    //        layout.Controls.Add(kioskLabel, 0, 0);
    //        layout.Controls.Add(kioskPanelContainer, 0, 1);
    //        layout.Controls.Add(submitBtn, 0, 2);

    //        submitBtn.Click += async (s, e) =>
    //        {
    //            string kioskId = kioskTextBox.Text.Trim();
    //            if (string.IsNullOrWhiteSpace(kioskId))
    //            {
    //                MessageBox.Show("Kiosk ID is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                return;
    //            }

    //            var loadingLabel = new Label
    //            {
    //                Text = "Please wait...",
    //                ForeColor = Color.White,
    //                Font = new Font("Poppins", 12, FontStyle.Italic),
    //                AutoSize = true,
    //                BackColor = Color.Transparent
    //            };

    //            loadingLabel.Left = (this.ClientSize.Width - loadingLabel.Width) / 2;
    //            loadingLabel.Top = kioskPanel.Bottom + 20;
    //            this.Controls.Add(loadingLabel);
    //            loadingLabel.BringToFront();
    //            loadingLabel.Refresh();

    //            var (isValid, errorMessage) = await ValidateAndSetKioskSessionAsync(kioskId);

    //            this.Controls.Remove(loadingLabel);
    //            loadingLabel.Dispose();

    //            if (isValid)
    //            {
    //                kioskPanel.Visible = false;
    //                loginPanel.Visible = true;
    //                subtitle.Visible = true;
    //                CenterLoginUI(loginPanel);
    //                this.Controls.Remove(kioskPanel);
    //                kioskPanel.Dispose();
    //            }
    //            else
    //            {
    //                MessageBox.Show(errorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //            }
    //        };
    //        kioskPanel.Visible = false;

    //        this.Load += (s, e) => CenterKioskPanel();
    //        this.Resize += (s, e) => CenterKioskPanel();
    //    }

    //    private async Task<(bool Success, string ErrorMessage)> ValidateAndSetKioskSessionAsync(string kioskId)
    //    {
    //        Logger.Log($"Validating Kiosk ID: {kioskId} 🖥️");
    //        try
    //        {
    //            using (var client = new HttpClient())
    //            {
    //                string apiUrl = "https://uat.pocketmint.ai/api/kiosks/get/kiosks/details";
    //                Logger.Log($"Calling API: {apiUrl} 🌐");

    //                var requestBody = new { kioskId = int.Parse(kioskId) };
    //                string jsonPayload = JsonConvert.SerializeObject(requestBody);
    //                Logger.Log($"Payload: {jsonPayload} 📦");

    //                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    //                client.DefaultRequestHeaders.Accept.Clear();
    //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //                client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
    //                client.DefaultRequestHeaders.Remove("Authorization");
    //                client.DefaultRequestHeaders.Add("Authorization", "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78");
    //                client.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=C4537CD8D22C7AF20A50A08992FD3EFF; Path=/; Secure; HttpOnly");

    //                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
    //                string responseText = await response.Content.ReadAsStringAsync();
    //                Logger.Log($"Response received 📨: {responseText}");

    //                if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("{"))
    //                {
    //                    Logger.Log("Unexpected non-JSON response ⚠️");
    //                    return (false, "Unexpected (non-JSON) response received.");
    //                }

    //                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
    //                if (result == null)
    //                {
    //                    Logger.Log("API returned null ❌");
    //                    return (false, "API returned null result.");
    //                }

    //                if (result.isSucceed != true || result.data == null)
    //                {
    //                    Logger.Log("Invalid Kiosk ID ❌");
    //                    return (false, "Invalid Kiosk ID. Try again.");
    //                }

    //                AppSession.KioskId = result.data.KIOSKID;
    //                AppSession.KioskRegId = result.data.REGID;

    //                Logger.Log($"Kiosk validated ✅ KioskID: {AppSession.KioskId}, RegID: {AppSession.KioskRegId}");
    //                return (result.isSucceed == true, null);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.LogError("Error validating Kiosk ID ❌", ex);
    //            return (false, $"Error validating Kiosk ID:\n{ex.Message}");
    //        }
    //    }

    //    private async Task<bool> SendOtpAsync(string mobileNo)
    //    {
    //        try
    //        {
    //            using (var client = new HttpClient())
    //            {
    //                string apiUrl = "https://uat.pocketmint.ai/api/kiosks/send/user/mobileno/otp";

    //                var payload = new
    //                {
    //                    mobileNo = mobileNo,
    //                    kioskId = int.Parse(AppSession.KioskId)
    //                };

    //                string jsonPayload = JsonConvert.SerializeObject(payload);
    //                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    //                client.DefaultRequestHeaders.Accept.Clear();
    //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //                client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
    //                client.DefaultRequestHeaders.Add("Authorization", "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78");

    //                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
    //                string responseText = await response.Content.ReadAsStringAsync();

    //                if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("{"))
    //                {
    //                    return false;
    //                }

    //                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
    //                if (result == null) return false;


    //                // IMPORTANT: We store user data here, assuming the send OTP API returns it.
    //                if (result != null)
    //                {
    //                    AppSession.smsId = result.smsId;
    //                }
    //                else
    //                {
    //                    Logger.Log("No data found in OTP send API response ⚠️");
    //                }

    //                return result.isSucceed == true;
    //            }
    //            //return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.LogError("Exception in SendOtpAsync", ex);
    //            return false;
    //        }
    //    }

    //    // NEW: Function to verify the entered OTP via an API call
    //    private async Task<bool> VerifyOtpAsync(string mobileNo, string otp)
    //    {
    //        try
    //        {
    //            using (var client = new HttpClient())
    //            {
    //                // !!! IMPORTANT: Please confirm this is the correct API endpoint for OTP verification !!!
    //                string apiUrl = "https://uat.pocketmint.ai/api/kiosks/validate/user/mobileno/otp";
    //                Logger.Log($"Calling OTP Verification API: {apiUrl} 🌐");

    //                var payload = new
    //                {
    //                    mobileNo = mobileNo,
    //                    kioskId = int.Parse(AppSession.KioskId),
    //                    otp = otp,
    //                    smsId = AppSession.smsId,
    //                };

    //                string jsonPayload = JsonConvert.SerializeObject(payload);
    //                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    //                Logger.Log($"Payload: {jsonPayload} 📦");

    //                client.DefaultRequestHeaders.Accept.Clear();
    //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //                client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.35.0");
    //                client.DefaultRequestHeaders.Add("Authorization", "a55cf4p6-e57a-3w20-8ag4-33s55d27ev78");

    //                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
    //                string responseText = await response.Content.ReadAsStringAsync();
    //                Logger.Log($"Response received 📨: {responseText}");

    //                if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("{"))
    //                {
    //                    return false;
    //                }

    //                var result = JsonConvert.DeserializeObject<dynamic>(responseText);
    //                if (result == null) return false;

    //                if (result.data != null)
    //                {
    //                    AppSession.CustomerRegId = result.data.REGID;
    //                    AppSession.CustomerName = result.data.NAME;
    //                    AppSession.CustomerBALANCE = result.data.BALANCE;
    //                }
    //                else
    //                {
    //                    Logger.Log("No data found in OTP send API response ⚠️");
    //                }

    //                // Return true only if the API call indicates success
    //                return result.isSucceed == true;
    //            }

    //            //return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.LogError("Exception in VerifyOtpAsync", ex);
    //            return false;
    //        }
    //    }


    //    // NEW: All methods related to the timer and resend functionality

    //    /// <summary>
    //    /// Starts or restarts the countdown timer.
    //    /// </summary>
    //    private void StartTimer()
    //    {
    //        timeRemaining = 3;
    //        timerLabel.Text = $"00:{timeRemaining}";

    //        timerLabel.Visible = true;
    //        resendButton.Visible = false;

    //        countdownTimer.Start();
    //    }

    //    /// <summary>
    //    /// Handles the timer tick event every second.
    //    /// </summary>
    //    private void CountdownTimer_Tick(object sender, EventArgs e)
    //    {
    //        timeRemaining--;
    //        timerLabel.Text = $"00:{timeRemaining:00}";

    //        if (timeRemaining <= 0)
    //        {
    //            countdownTimer.Stop();
    //            timerLabel.Visible = false;
    //            resendButton.Visible = true;
    //        }
    //    }

    //    /// <summary>
    //    /// Handles the click event for the "Resend OTP" button.
    //    /// </summary>
    //    private async void ResendButton_Click(object sender, EventArgs e)
    //    {
    //        Logger.Log("Resend OTP clicked. yeniden");

    //        // Call the same method used to send the initial OTP
    //        bool otpResent = await SendOtpAsync(fullMobileNumber);

    //        if (otpResent)
    //        {
    //            MessageBox.Show("A new OTP has been sent to your mobile number.", "OTP Resent", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //            // Restart the timer
    //            StartTimer();
    //        }
    //        else
    //        {
    //            MessageBox.Show("Failed to resend OTP. Please try again in a moment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        }
    //    }
    //}

    public class CountryCode
    {
        public string Name { get; set; }
        public string DialCode { get; set; }
        public string DisplayName { get; set; }
    }

    internal class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }
}

