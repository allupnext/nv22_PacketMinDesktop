using System;
using System.Drawing;
using System.Windows.Forms;

namespace NV22SpectralInteg.NumPad
{
    public partial class Numpad : Form
    {
        private TextBox targetTextBox;
        private string targetName;

        public Numpad(TextBox target, string targetName = "")
        {
            InitializeComponent();
            this.targetTextBox = target;
            this.targetName = targetName;

            CustomizeForm();
            BuildNumpadUI(targetName);
        }



        private void CustomizeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.Manual; 
            this.BackColor = ColorTranslator.FromHtml("#1e1e1e");
            this.Width = 360;
            this.Height = 480;
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

                btn.GotFocus += (s, e) => ((Button)s).Parent.Focus();

                // The code you asked for, but not the recommended approach
                if (key == "Back")
                {
                    btn.Text = "⬅️"; // Unicode for Left Arrow
                }
                else if (key == "Enter")
                {
                    btn.Text = "⏎"; // Unicode for Return Symbol
                }
                else
                {
                    btn.Text = key;
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
                    if (targetTextBox.Text.Length > 0)
                        targetTextBox.Text = ""; // or Substring(...) if needed
                    break;

                case "Enter":
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;

                default:
                    if (targetName == "OTPNumber")
                    {
                        targetTextBox.Text = key;
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