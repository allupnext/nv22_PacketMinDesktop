using Newtonsoft.Json;
using NV22SpectralInteg.InactivityManager;
using NV22SpectralInteg.Login;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV22SpectralInteg.Dashboard
{
    public partial class SuccessPopup : Form
    {
        private bool _isSucceed;
        private string _status;
        private readonly AppConfig config;

        // Constructor that accepts the name and amount to display
        public SuccessPopup(string recipientName, decimal amount, bool isSucceed, string message, string status)
        {
            this.Opacity = 0;
            this.Shown += SuccessPopup_Shown;
            InitializeCustomComponents(recipientName, amount, isSucceed, message,status);
            KioskIdleManager.Initialize(Logout);

            // Get the full path to the JSON file
            string configPath = Path.Combine(Application.StartupPath, "config.json");
            // Read the entire file into a string
            string jsonContent = File.ReadAllText(configPath);
            // Deserialize the JSON string into the class field
            this.config = JsonConvert.DeserializeObject<AppConfig>(jsonContent);


            int idleTime = config.ScreenTimeouts.ContainsKey("TransactionPopUp")
               ? config.ScreenTimeouts["TransactionPopUp"]
               : config.ScreenTimeouts["Default"];


            Logger.Log($"✨ SuccessPopup created. Starting {idleTime}-second inactivity timer.");
            KioskIdleManager.Start(idleTime, "Logout");

        }
        private void SuccessPopup_Shown(object sender, EventArgs e)
        {
            // Now that the form is fully rendered, make it visible in one go.
            this.Opacity = 1;
        }
        private void Logout()
        {
            KioskIdleManager.Stop();
            AppSession.Clear();

            Program.mainLoginForm.ResetToLogin();
            Program.mainLoginForm.Show();

            this.Hide();
            this.Close();
        }

        private void InitializeCustomComponents(string recipientName, decimal amount, bool isSucceed, string message, string status)
        {
            // Make sure popup does not show in Alt+Tab
            this.ShowInTaskbar = false;
            this._isSucceed = isSucceed;
            this._status = status;

            // ===== Form Styling =====
            this.FormBorderStyle = FormBorderStyle.None; // Removes the title bar and border
            this.StartPosition = FormStartPosition.CenterParent; // Centers the popup over the parent form
            this.BackColor = ColorTranslator.FromHtml("#11150f");
            this.ClientSize = (status == "pdf" ? new Size(500, 550) : new Size(500, 650));
            this.Padding = new Padding(20);

            // ===== Controls =====

            // Success Icon (Using a simple Label as a placeholder)
            Label successIcon = new Label
            {
                Text = $"{(isSucceed ? "✔️" : "❌")}",
                Font = new Font("Poppins", 25F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = isSucceed ? ColorTranslator.FromHtml("#25c866") : ColorTranslator.FromHtml("#FF0000"), // A nice green color
                Size = new Size(80, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point((this.ClientSize.Width - 80) / 2, 0)
            };
            // Make the icon circular
            GraphicsPath iconPath = new GraphicsPath();
            iconPath.AddEllipse(0, 0, successIcon.Width, successIcon.Height);
            successIcon.Region = new Region(iconPath);
            successIcon.Paint += SuccessIcon_Paint; 

            // Logo (Assuming you have a logo image in your project resources)
            PictureBox logo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(500, 200),
                BackColor = Color.White,
                Location = new Point((this.ClientSize.Width - 500) / 2, successIcon.Bottom + 10)
            };

            string imagePath = Path.Combine(Application.StartupPath, "Image", "-PocketMint.png");
            if (File.Exists(imagePath))
            {
                logo.Image = Image.FromFile(imagePath);
            }
            else
            {
                MessageBox.Show("Logo image not found at: " + imagePath, "Error");
            }

            // If you don't have an image, use a label as a placeholder:
            Label logoPlaceholder = new Label
            {
                Text = "PocketMint",
                Font = new Font("Poppins", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 188, 156),
                Size = new Size(300, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point((this.ClientSize.Width - 300) / 2, successIcon.Bottom + 10)
            };

            // Thank You Message
            Label thankYouLabel = new Label
            {
                Text = $"Thank You,",
                Font = new Font("Poppins", 22F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#11150f"),
                BackColor = Color.White,
                Size = new Size(this.ClientSize.Width - 40, 60),
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(20, logo.Bottom)
            };

            // Recipient Name Label
            Label nameLabel = new Label
            {
                Text = recipientName ?? "User", // The name goes here
                Font = new Font("Poppins", 22F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#11150f"),
                Size = new Size(this.ClientSize.Width - 40, 60), 
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.White,
                Location = new Point(20, thankYouLabel.Bottom - 5),
            };

            // Amount Display
            Label amountLabel = new Label
            {
                Text = amount.ToString("C", new System.Globalization.CultureInfo("en-US")), // Formats as $1,200.00
                Font = new Font("Poppins", 28F, FontStyle.Bold),
                ForeColor = isSucceed ? ColorTranslator.FromHtml("#25c866") : ColorTranslator.FromHtml("#FF0000"),
                Size = new Size(this.ClientSize.Width - 40, 50),
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 80,
                Location = new Point(20, nameLabel.Bottom - 10),
                Visible = !(status == "pdf") // Hide if status is "pdf"
            };

            // Receipt Info Message
            Label infoLabel = new Label
            {
                Text = message,
                Font = (status == "pdf" ? new Font("Poppins", 15F, FontStyle.Bold) : new Font("Poppins", 11F, FontStyle.Bold)),
                ForeColor = ColorTranslator.FromHtml("#7e8088"),
                BackColor = Color.White,
                AutoSize = false,
                Width = this.ClientSize.Width - 60, 
                Height = (status == "pdf" ? 45 : 90),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point((this.ClientSize.Width - (this.ClientSize.Width - 60)) / 2, (status == "pdf" ? nameLabel.Bottom : amountLabel.Bottom) + 10)
            };

            if (isSucceed && status == "pdf")
            {
                // Centered "Add New" button
                Point centerPosition = new Point((this.ClientSize.Width - 200) / 2, infoLabel.Bottom + 10);
                var addPanel = CreateButtonPanel("Ok", centerPosition, "addButton", ColorTranslator.FromHtml("#25c866"), DialogResult.OK);
                this.Controls.Add(addPanel);
            }
            else if (isSucceed && status == "bankadd")
            {
                // Add New button on left
                Point addPosition = new Point(38, infoLabel.Bottom + 3);
                var addPanel = CreateButtonPanel("Add New", addPosition, "addButton", ColorTranslator.FromHtml("#25c866"), DialogResult.OK);
                this.Controls.Add(addPanel);

                // Log Out button on right
                Point logoutPosition = new Point(248, infoLabel.Bottom + 3);
                var logoutPanel = CreateButtonPanel("Log Out", logoutPosition, "logoutButton", ColorTranslator.FromHtml("#25c866"), DialogResult.Cancel);
                this.Controls.Add(logoutPanel);
            }
            else
            {
                // Only centered "Log Out" button in red
                Point centerPosition = new Point((this.ClientSize.Width - 204) / 2, infoLabel.Bottom + 3);
                var logoutPanel = CreateButtonPanel("Log Out", centerPosition, "logoutButton", ColorTranslator.FromHtml("#FF0000"), DialogResult.Cancel);
                this.Controls.Add(logoutPanel);
            }

            // Add all controls to the form
            this.Controls.Add(successIcon);
            this.Controls.Add(logo); // or 'logo' if you have an image
            this.Controls.Add(thankYouLabel);
            this.Controls.Add(nameLabel);
            // Only add amountLabel if visible
            if (amountLabel.Visible)
                this.Controls.Add(amountLabel);
            this.Controls.Add(infoLabel);
        }

        private Panel CreateButtonPanel(string buttonText, Point panelLocation, string panelTag, Color buttonColor, DialogResult dialogResult, EventHandler clickEventHandler = null)
        {
            Panel buttonPanel = new Panel
            {
                Size = new Size(204, 66),
                Location = panelLocation,
                BackColor = Color.Transparent,
                Tag = panelTag
            };

            Button button = new Button
            {
                Text = buttonText,
                Font = new Font("Poppins", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = buttonColor,
                Size = new Size(200, 62),
                Location = new Point(2, 2),
                FlatStyle = FlatStyle.Flat
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.White;
            button.FlatAppearance.MouseDownBackColor = Color.White;
            button.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, button.Width, button.Height, 12, 12));

            // Default click behavior
            button.Click += (sender, e) =>
            {
                this.DialogResult = dialogResult;
                clickEventHandler?.Invoke(sender, e);
            };

            // Hover events
            button.MouseEnter += (sender, e) =>
            {
                button.ForeColor = buttonColor;
                buttonPanel.Invalidate();
            };

            button.MouseLeave += (sender, e) =>
            {
                button.ForeColor = Color.White;
                buttonPanel.Invalidate();
            };

            buttonPanel.Paint += ButtonPanel_Paint;
            buttonPanel.Controls.Add(button);

            return buttonPanel;
        }


        // Optional: Add code for rounded corners
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int topPadding = 40;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle cardRect = new Rectangle(0, topPadding, this.ClientSize.Width, this.ClientSize.Height - topPadding);

            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                int cornerRadius = 20;
                path.AddArc(new Rectangle(cardRect.Left, cardRect.Top, cornerRadius, cornerRadius), 180, 90);
                path.AddArc(new Rectangle(cardRect.Right - cornerRadius, cardRect.Top, cornerRadius, cornerRadius), 270, 90);
                path.AddArc(new Rectangle(cardRect.Right - cornerRadius, cardRect.Bottom - cornerRadius, cornerRadius, cornerRadius), 0, 90);
                path.AddArc(new Rectangle(cardRect.Left, cardRect.Bottom - cornerRadius, cornerRadius, cornerRadius), 90, 90);
                path.CloseFigure();

                // Fill the path with white color
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }
        }


        // NEW METHOD: Event handler for drawing the border around the successIcon
        private void SuccessIcon_Paint(object sender, PaintEventArgs e)
        {
            Label successIcon = sender as Label;
            if (successIcon == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Outer dark border (similar to the dark grey in the reference image)
            using (Pen darkBorderPen = new Pen(Color.White, 20)) // Dark grey, 4px thick
            {
                // Draw slightly larger than the icon to create the border
                e.Graphics.DrawEllipse(darkBorderPen, -2, -2, successIcon.Width + 4, successIcon.Height + 4);
            }
            // Inner white border
            using (Pen whiteBorderPen = new Pen(Color.White, 3)) // White, 3px thick
            {
                // Draw slightly larger than the icon to create the border
                e.Graphics.DrawEllipse(whiteBorderPen, -1, -1, successIcon.Width + 2, successIcon.Height + 2);
            }
        }

        // Helper method to determine if a button is hovered
        private bool IsButtonHovered(Panel panel)
        {
            if (panel.Tag == null) return false;

            Button button = panel.Controls.OfType<Button>().FirstOrDefault();
            if (button == null) return false;

            return button.ClientRectangle.Contains(button.PointToClient(Cursor.Position));
        }


        private void ButtonPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;

            // We only draw the border if the mouse is currently hovering over the button
            if (IsButtonHovered(panel))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                int cornerRadius = 12; // Same radius as your button

                // The rectangle for the border, slightly smaller than the panel itself
                // to give a 1px border. Adjust as needed.
                Rectangle borderRect = new Rectangle(1, 1, panel.Width - 3, panel.Height - 3);

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(new Rectangle(borderRect.Left, borderRect.Top, cornerRadius, cornerRadius), 180, 90);
                    path.AddArc(new Rectangle(borderRect.Right - cornerRadius, borderRect.Top, cornerRadius, cornerRadius), 270, 90);
                    path.AddArc(new Rectangle(borderRect.Right - cornerRadius, borderRect.Bottom - cornerRadius, cornerRadius, cornerRadius), 0, 90);
                    path.AddArc(new Rectangle(borderRect.Left, borderRect.Bottom - cornerRadius, cornerRadius, cornerRadius), 90, 90);
                    path.CloseFigure();

                    using (Pen borderPen = new Pen(_isSucceed ? ColorTranslator.FromHtml("#25c866") : ColorTranslator.FromHtml("#FF0000"), 3)) // Green border, 2px thick
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            }
        }
    }
}
