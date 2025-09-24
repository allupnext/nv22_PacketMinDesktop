using NV22SpectralInteg.InactivityManager;
using NV22SpectralInteg.Login;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV22SpectralInteg.PrivacyPolicy
{
    public partial class PrivacyPolicyWindow : Form
    {
        private Button acceptButton;
        private Button declineButton;

        public PrivacyPolicyWindow()
        {
            Logger.Log("🛑 In PrivacyPolicyWindow Stopping any existing KioskIdleManager instance before starting a new one.");
            KioskIdleManager.Stop();

            Logger.Log("✨ In PrivacyPolicyWindow Starting KioskIdleManager with 10-second timeout for OTP screen.");
            KioskIdleManager.Start(10);
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // === WINDOW SETTINGS ===
            this.Text = "Terms & Conditions";
            this.Size = new Size(600, 700);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            int margin = 30;

            // === TITLE ===
            Label titleLabel = new Label
            {
                Text = "Privacy Policy",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(margin, margin)
            };
            this.Controls.Add(titleLabel);

            // === SCROLLABLE TERMS AREA ===
            Panel scrollPanel = new Panel
            {
                Location = new Point(margin, titleLabel.Bottom + 15),
                Size = new Size(this.ClientSize.Width - 2 * margin, 380),
                AutoScroll = true,
                BackColor = Color.FromArgb(28, 28, 28),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(scrollPanel);

            Label bodyLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(scrollPanel.Width - 25, 0),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gainsboro,
                Padding = new Padding(10),
                Text =
                "By using PocketMint, provided by PocketMint.AI, you agree to these terms...\r\n\r\n" +
                "Authorized Users:\r\n" +
                "At PocketMint, we are committed to protecting your privacy and ensuring the " +
                "security of your personal information...\r\n\r\n" +
                "Read more at: https://pocketmint.ai/privacy"
            };
            scrollPanel.Controls.Add(bodyLabel);

            // === ACCEPT BUTTON ===
            acceptButton = new Button
            {
                Text = "✓ I Accept",
                Size = new Size(160, 45),
                BackColor = ColorTranslator.FromHtml("#00C851"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Location = new Point((this.ClientSize.Width - 160) / 2, scrollPanel.Bottom + 25)
            };
            acceptButton.FlatAppearance.BorderSize = 0;
            acceptButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, acceptButton.Width, acceptButton.Height, 12, 12)
            );
            acceptButton.Cursor = Cursors.Hand;
            acceptButton.Click += AcceptButton_Click;
            this.Controls.Add(acceptButton);

            // === DECLINE BUTTON ===
            declineButton = new Button
            {
                Text = "✗ No, I Decline",
                Size = new Size(210, 45),
                BackColor = ColorTranslator.FromHtml("#ff4444"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Location = new Point((this.ClientSize.Width - 210) / 2, acceptButton.Bottom + 15)
            };
            declineButton.FlatAppearance.BorderSize = 0;
            declineButton.Region = Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, declineButton.Width, declineButton.Height, 12, 12)
            );
            declineButton.Cursor = Cursors.Hand;
            declineButton.Click += DeclineButton_Click;
            this.Controls.Add(declineButton);

            // === CLOSE BUTTON (TOP-RIGHT) ===
            Button closeButton = new Button
            {
                Text = "X",
                Size = new Size(35, 35),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(this.ClientSize.Width - 45, 10)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Cursor = Cursors.Hand;
            closeButton.Click += (s, e) => { this.Close(); };
            this.Controls.Add(closeButton);
            closeButton.BringToFront();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void DeclineButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
