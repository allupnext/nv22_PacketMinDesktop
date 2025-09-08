using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV22SpectralInteg
{
    public partial class WelcomeForm : Form
    {
        private PictureBox pictureBox; // Make it accessible to event handlers

        public WelcomeForm()
        {
            InitializeComponent();

            // Make the form transparent and borderless
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Lime;  // Arbitrary color for transparency key
            this.TransparencyKey = Color.Lime;

            // Set size and position
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            // Prevent resizing
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create a panel with green background
            Panel backgroundPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#25C866")
            };
            this.Controls.Add(backgroundPanel);

            // Load image
            string imagePath = Path.Combine(Application.StartupPath, "Image", "Logo.png");

            if (!File.Exists(imagePath))
            {
                MessageBox.Show("Image not found:\n" + imagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Initialize picture box
            pictureBox = new PictureBox
            {
                Image = Image.FromFile(imagePath),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(180, 180)
            };

            backgroundPanel.Controls.Add(pictureBox);

            // Center image once form is shown
            this.Shown += (s, e) =>
            {
                CenterImage();
            };

            // Optional: keep image centered if user resizes screen
            this.Resize += (s, e) =>
            {
                CenterImage();
            };
        }

        private void CenterImage()
        {
            if (pictureBox != null)
            {
                pictureBox.Location = new Point(
                    (this.ClientSize.Width - pictureBox.Width) / 2,
                    (this.ClientSize.Height - pictureBox.Height) / 2
                );
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Not needed if using Shown event
        }
    }
}
