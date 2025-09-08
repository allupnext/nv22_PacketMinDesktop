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
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // === WINDOW SETTINGS ===
            this.Text = "Terms & Conditions";
            this.Size = new Size(450, 550);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(17, 17, 17);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            int margin = 20;

            // === TITLE ===
            Label titleLabel = new Label
            {
                Text = "Privacy",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(margin, margin)
            };
            this.Controls.Add(titleLabel);

            // === SCROLLABLE TERMS AREA ===
            Panel scrollPanel = new Panel
            {
                Location = new Point(margin, titleLabel.Bottom + 10),
                Size = new Size(this.ClientSize.Width - 2 * margin, 320),
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            this.Controls.Add(scrollPanel);


            Label bodyLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(scrollPanel.Width - 20, 0),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.LightGray,
                Text =
                "By using PocketMint, provided by PocketMint.AI, you agree to these terms...\r\n\r\n" +
                "Authorized Users:\r\n" +
                "At PocketMint, we are committed to protecting your privacy and ensuring the " +
                "security of your personal information...\r\n\r\n" +
                "Read more at: https://pocketmint.ai/privacy"
            };

            scrollPanel.Controls.Add(bodyLabel);


            // Below code in open link in browser

            //LinkLabel linkLabel = new LinkLabel
            //{
            //    AutoSize = true,
            //    MaximumSize = new Size(scrollPanel.Width - 20, 0),
            //    Font = new Font("Segoe UI", 11),
            //    ForeColor = Color.LightGray,
            //    LinkColor = Color.LightBlue,
            //    ActiveLinkColor = Color.DeepSkyBlue,
            //    Text =
            //    "By using PocketMint, provided by PocketMint.AI, you agree to these terms...\r\n\r\n" +
            //    "Authorized Users:\r\n" +
            //    "At PocketMint, we are committed to protecting your privacy and ensuring the " +
            //    "security of your personal information...\r\n\r\n" +
            //    "Read more at: https://pocketmint.ai/privacy"
            //};
            //linkLabel.Links.Add(linkLabel.Text.IndexOf("https://pocketmint.ai/privacy"), "https://pocketmint.ai/privacy".Length, "https://pocketmint.ai/privacy");
            //linkLabel.LinkClicked += (s, e) =>
            //{
            //    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            //    {
            //        FileName = e.Link.LinkData.ToString(),
            //        UseShellExecute = true
            //    });
            //};

            //scrollPanel.Controls.Add(linkLabel);


            // ACCEPT BUTTON
            acceptButton = new Button
            {
                Text = "I Accept",
                Size = new Size(120, 35), // Bigger
                BackColor = ColorTranslator.FromHtml("#00C851"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold), // Larger font
                FlatStyle = FlatStyle.Flat,
                Location = new Point((this.ClientSize.Width - 120) / 2, scrollPanel.Bottom + 20)
            };
            acceptButton.Click += AcceptButton_Click;
            this.Controls.Add(acceptButton);

            // DECLINE BUTTON
            declineButton = new Button
            {
                Text = "No, I Decline",
                Size = new Size(160, 35),
                BackColor = ColorTranslator.FromHtml("#ff4444"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Location = new Point((this.ClientSize.Width - 160) / 2, acceptButton.Bottom + 10)
            };
            declineButton.Click += DeclineButton_Click;
            this.Controls.Add(declineButton);
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
