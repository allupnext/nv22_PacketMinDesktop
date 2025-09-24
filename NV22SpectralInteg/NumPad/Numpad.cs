using System;
using System.Drawing;
using System.Windows.Forms;

namespace NV22SpectralInteg.NumPad
{
    public partial class Numpad : Form
    {
        private TextBox targetTextBox;
        private string targetName;
        private TextBox[] otpBoxes;
        private int currentIndex;

        public Numpad(TextBox target, string targetName = "", TextBox[] otpBoxes = null, int currentIndex = -1)
        {
            InitializeComponent();

            this.targetTextBox = target;

            this.targetName = targetName;
            this.otpBoxes = otpBoxes;
            this.currentIndex = currentIndex;

            CustomizeForm();
            BuildNumpadUI(targetName);
        }
        public void SetTarget(TextBox newTarget, string newName, TextBox[] newOtpBoxes = null, int newCurrentIndex = -1)
        {
            this.targetTextBox = newTarget;
            this.targetName = newName;
            this.otpBoxes = newOtpBoxes;
            this.currentIndex = newCurrentIndex;
        }

        private void CustomizeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = ColorTranslator.FromHtml("#1e1e1e");
            this.Width = 440;
            //this.Height = 480;
            this.Height = 280;
            this.ShowInTaskbar = false; 
        }

        private void BuildNumpadUI(string target)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                Padding = new Padding(5),
                BackColor = this.BackColor
            };

            for (int i = 0; i < 3; i++)
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            for (int i = 0; i < 4; i++)
                panel.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "Back", "0", "Enter" };

            foreach (var key in keys)
            {
                var btn = new Button
                {
                    Text = key,
                    Dock = DockStyle.Fill,
                    Font = new Font("Poppins", 20, FontStyle.Bold),
                    BackColor = ColorTranslator.FromHtml("#333333"),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Tag = key,
                    Margin = new Padding(5)
                };

                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseDownBackColor = ColorTranslator.FromHtml("#444444");
                btn.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#3a3a3a");

                // Check for special keys to use icons
                if (key == "Back")
                {
                    btn.Text = "⬅️";
                }
                else if (key == "Enter")
                {
                    btn.Text = "⏎";
                    btn.BackColor = ColorTranslator.FromHtml("#25c866"); // Make the Enter key green
                }

                btn.Click += (sender, e) => NumpadButton_Click(sender, e, target);
                panel.Controls.Add(btn);
            }

            this.Controls.Add(panel);
        }

        private void NumpadButton_Click(object sender, EventArgs e, string target)
        {
            var btn = sender as Button;
            var key = btn.Tag.ToString();

            switch (key)
            {
                case "Back":
                    // Special logic for OTP boxes
                    if (targetName == "OTPNumber")
                    {
                        // If the current box is not empty, clear it
                        if (targetTextBox.Text.Length > 0)
                        {
                            targetTextBox.Text = "";
                        }
                        // If the current box is empty and it's not the first one, move back
                        else if (currentIndex > 0)
                        {
                            otpBoxes[currentIndex - 1].Focus();
                        }
                    }
                    // General backspace logic for other text boxes
                    else if (targetTextBox.Text.Length > 0)
                    {
                        targetTextBox.Text = targetTextBox.Text.Substring(0, targetTextBox.Text.Length - 1);
                    }
                    break;

                case "Enter":
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;

                default:
                    if (targetName == "OTPNumber")
                    {
                        targetTextBox.Text = key;

                        // Move focus to the next box
                        if (currentIndex < otpBoxes.Length - 1)
                        {
                            otpBoxes[currentIndex + 1].Focus();
                        }
                    }
                    else
                    {
                        targetTextBox.Text += key;
                    }
                    break;
            }
        }

    }
}