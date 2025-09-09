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
        public SuccessPopup(string recipientName, decimal amount)
        {
            InitializeComponent(); // This is important, don't comment it out
            InitializeCustomComponents(recipientName, amount);
        }

        private void InitializeCustomComponents(string recipientName, decimal amount)
        {
            // ===== Form Styling =====
            this.FormBorderStyle = FormBorderStyle.None; // Removes the title bar and border
            this.StartPosition = FormStartPosition.CenterParent; // Centers the popup over the parent form
            this.BackColor = Color.White;
            this.ClientSize = new Size(500, 680); // Set a fixed size for the popup card
            this.Padding = new Padding(20);

            // ===== Controls =====

            // Success Icon (Using a simple Label as a placeholder)
            Label successIcon = new Label
            {
                Text = "✓",
                Font = new Font("Poppins", 24F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(46, 204, 113), // A nice green color
                Size = new Size(75, 75),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point((this.ClientSize.Width - 75) / 2, 20)
            };
            // Make the icon circular
            GraphicsPath iconPath = new GraphicsPath();
            iconPath.AddEllipse(0, 0, successIcon.Width, successIcon.Height);
            successIcon.Region = new Region(iconPath);

            // Logo (Assuming you have a logo image in your project resources)
            PictureBox logo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(450, 150),
                Location = new Point((this.ClientSize.Width - 450) / 2, successIcon.Bottom + 10)
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
                ForeColor = Color.FromArgb(64, 64, 64),
                Size = new Size(this.ClientSize.Width - 40, 60),
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(20, logo.Bottom + 30)
            };

            // Recipient Name Label
            Label nameLabel = new Label
            {
                Text = recipientName, // The name goes here
                Font = new Font("Poppins", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Size = new Size(this.ClientSize.Width - 40, 60), 
                TextAlign = ContentAlignment.TopCenter, 
                Location = new Point(20, thankYouLabel.Bottom - 10),
            };

            // Amount Display
            Label amountLabel = new Label
            {
                Text = amount.ToString("C", new System.Globalization.CultureInfo("en-US")), // Formats as $1,200.00
                Font = new Font("Poppins", 28F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Size = new Size(this.ClientSize.Width - 40, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 80,
                Location = new Point(20, nameLabel.Bottom + 40),
            };

            // Receipt Info Message
            Label infoLabel = new Label
            {
                Text = "Transaction completed. Receipt has been sent to your registered Mobile number.",
                Font = new Font("Poppins", 9F, FontStyle.Regular),
                ForeColor = Color.Gray,
                Size = new Size(this.ClientSize.Width - 60, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(30, amountLabel.Bottom + 5)
            };

            // 'Add New' Button
            Button addButton = new Button
            {
                Text = "Add New",
                Font = new Font("Poppins", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(26, 188, 156),
                Size = new Size(140, 50),
                Location = new Point(90, infoLabel.Bottom + 20),
                FlatStyle = FlatStyle.Flat
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, addButton.Width, addButton.Height, 12, 12)
            );
            addButton.Click += (sender, e) => { this.DialogResult = DialogResult.OK; }; // Set result and close

            // 'Log Out' Button
            Button logoutButton = new Button
            {
                Text = "Log Out",
                Font = new Font("Poppins", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                BackColor = Color.FromArgb(236, 240, 241),
                Size = new Size(140, 50),
                Location = new Point(250, infoLabel.Bottom + 20),
                FlatStyle = FlatStyle.Flat
            };
            logoutButton.FlatAppearance.BorderSize = 0;
            logoutButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, logoutButton.Width, logoutButton.Height, 12, 12)
            );
            logoutButton.Click += (sender, e) => { this.DialogResult = DialogResult.Cancel; }; // Set result and close

            // Add all controls to the form
            this.Controls.Add(successIcon);
            this.Controls.Add(logo); // or 'logo' if you have an image
            this.Controls.Add(thankYouLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(amountLabel);
            this.Controls.Add(infoLabel);
            this.Controls.Add(addButton);
            this.Controls.Add(logoutButton);
        }

        // Optional: Add code for rounded corners
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (GraphicsPath path = new GraphicsPath())
            {
                int cornerRadius = 20;
                path.AddArc(new Rectangle(0, 0, cornerRadius, cornerRadius), 180, 90);
                path.AddArc(new Rectangle(this.Width - cornerRadius, 0, cornerRadius, cornerRadius), 270, 90);
                path.AddArc(new Rectangle(this.Width - cornerRadius, this.Height - cornerRadius, cornerRadius, cornerRadius), 0, 90);
                path.AddArc(new Rectangle(0, this.Height - cornerRadius, cornerRadius, cornerRadius), 90, 90);
                path.CloseFigure();
                this.Region = new Region(path);
            }
        }
    }
}
