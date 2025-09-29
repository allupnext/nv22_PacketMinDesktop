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
        // Constructor that accepts the name and amount to display
        public SuccessPopup(string recipientName, decimal amount, bool isSucceed, string message)
        {
            InitializeCustomComponents(recipientName, amount, isSucceed, message);
            KioskIdleManager.Initialize(Logout);
            Logger.Log("✨ SuccessPopup created. Starting 7-second inactivity timer.");
            KioskIdleManager.Start(7);
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

        private void InitializeCustomComponents(string recipientName, decimal amount, bool isSucceed, string message)
        {
            // Make sure popup does not show in Alt+Tab
            this.ShowInTaskbar = false;

            // ===== Form Styling =====
            this.FormBorderStyle = FormBorderStyle.None; // Removes the title bar and border
            this.StartPosition = FormStartPosition.CenterParent; // Centers the popup over the parent form
            this.BackColor = ColorTranslator.FromHtml("#11150f");
            this.ClientSize = new Size(500, 650);
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
                Text = recipientName, // The name goes here
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
                ForeColor = ColorTranslator.FromHtml("#25c866"),
                Size = new Size(this.ClientSize.Width - 40, 50),
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 80,
                Location = new Point(20, nameLabel.Bottom - 10),
            };

            // Receipt Info Message
            Label infoLabel = new Label
            {
                Text = message,
                Font = new Font("Poppins", 11F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#7e8088"),
                BackColor = Color.White,
                AutoSize = false,
                Width = this.ClientSize.Width - 60, 
                Height = 90,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point((this.ClientSize.Width - (this.ClientSize.Width - 60)) / 2, amountLabel.Bottom + 10)
            };

            // --- Log Out Button and its container Panel ---
            Panel logoutButtonPanel = new Panel
            {
                Size = new Size(204, 66), // Slightly larger than the button for the border
                Location = new Point(248, infoLabel.Bottom + 3), // Adjust position to center button
                BackColor = Color.Transparent,
                Tag = "logoutButton"
            };

            Button logoutButton = new Button
            {
                Text = "Log Out",
                Font = new Font("Poppins", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#25c866"),
                Size = new Size(200, 62),
                Location = new Point(2, 2), // Position button inside panel
                FlatStyle = FlatStyle.Flat
            };
            logoutButton.FlatAppearance.BorderSize = 0;
            logoutButton.FlatAppearance.MouseOverBackColor = Color.White;
            logoutButton.FlatAppearance.MouseDownBackColor = Color.White;

            // Create a rounded region for the button itself (the green part)
            logoutButton.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, logoutButton.Width, logoutButton.Height, 12, 12));
            logoutButton.Click += (sender, e) => { this.DialogResult = DialogResult.Cancel; };

            logoutButtonPanel.Paint += ButtonPanel_Paint;
            logoutButtonPanel.Controls.Add(logoutButton); // Add button to panel
            this.Controls.Add(logoutButtonPanel); // Add panel to form

            // LogoutButton hover events
            logoutButton.MouseEnter += (sender, e) =>
            {
                logoutButton.ForeColor = ColorTranslator.FromHtml("#25c866");
                logoutButtonPanel.Invalidate(); // Trigger panel's Paint event to draw border
            };
            logoutButton.MouseLeave += (sender, e) =>
            {
                logoutButton.ForeColor = Color.White;
                logoutButtonPanel.Invalidate(); // Trigger panel's Paint event to remove border
            };

            if (isSucceed)
            {
                // 'Add New' Button
                // --- Add New Button and its container Panel ---
                Panel addButtonPanel = new Panel
                {
                    Size = new Size(204, 66), // Slightly larger than the button for the border
                    Location = new Point(38, infoLabel.Bottom + 3), // Adjust position to center button
                    BackColor = Color.Transparent, // Ensure panel background is transparent
                    Tag = "addButton" // Give it a tag to identify later
                };

                Button addButton = new Button
                {
                    Text = "Add New",
                    Font = new Font("Poppins", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = ColorTranslator.FromHtml("#25c866"),
                    Size = new Size(200, 62),
                    Location = new Point(2, 2), // Position button inside panel
                    FlatStyle = FlatStyle.Flat
                };
                addButton.FlatAppearance.BorderSize = 0;
                addButton.FlatAppearance.MouseOverBackColor = Color.White;
                addButton.FlatAppearance.MouseDownBackColor = Color.White;
                // Create a rounded region for the button itself (the green part)
                addButton.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, addButton.Width, addButton.Height, 12, 12));
                addButton.Click += (sender, e) => { this.DialogResult = DialogResult.OK; };

                addButtonPanel.Paint += ButtonPanel_Paint;
                addButtonPanel.Controls.Add(addButton); // Add button to panel
                this.Controls.Add(addButtonPanel); // Add panel to form

                // Handle hover color change and border

                // AddButton hover events
                addButton.MouseEnter += (sender, e) =>
                {
                    addButton.ForeColor = ColorTranslator.FromHtml("#25c866");
                    addButtonPanel.Invalidate(); // Trigger panel's Paint event to draw border
                };
                addButton.MouseLeave += (sender, e) =>
                {
                    addButton.ForeColor = Color.White;
                    addButtonPanel.Invalidate(); // Trigger panel's Paint event to remove border
                };
            }
            else
            {
                // Position the Log Out button for when it's the only one
                logoutButtonPanel.Location = new Point((this.ClientSize.Width - 204) / 2, infoLabel.Bottom + 3);
            }

            // Add all controls to the form
            this.Controls.Add(successIcon);
            this.Controls.Add(logo); // or 'logo' if you have an image
            this.Controls.Add(thankYouLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(amountLabel);
            this.Controls.Add(infoLabel);
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

                    using (Pen borderPen = new Pen(ColorTranslator.FromHtml("#25c866"), 3)) // Green border, 2px thick
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            }
        }
    }
}
