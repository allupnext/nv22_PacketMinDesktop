using System;
using System.Drawing;
using System.IO; // Required for Path and File operations
using System.Windows.Forms;

namespace NV22SpectralInteg
{
    public partial class WelcomeForm : Form
    {
        // Declare the PictureBox as nullable (?) to resolve the CS8618 warning.
        // This tells the compiler that we intend for it to be potentially null.
        private PictureBox? pictureBox;
        private Label? waitLabel;

        public WelcomeForm()
        {
            InitializeComponent();

            // Make the form transparent and borderless
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ColorTranslator.FromHtml("#25c866"); ; // Using a standard color name
            //this.TransparencyKey = Color.LimeGreen;

            // Set size and position to cover the entire screen
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            // Prevent resizing by the user (though maximized already handles this)
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create a panel with a green background that fills the form
            Panel backgroundPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#25C866")
            };
            this.Controls.Add(backgroundPanel);

            // Define the path to the logo image
            string imagePath = Path.Combine(Application.StartupPath, "Image", "Logo.png");

            // Check if the image file exists before trying to use it
            if (!File.Exists(imagePath))
            {
                MessageBox.Show("Image not found:\n" + imagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Close the welcome form if the logo is missing
                this.Close();
                return;
            }


            // Initialize the PictureBox to display the logo
            pictureBox = new PictureBox
            {
                Image = Image.FromFile(imagePath),
                SizeMode = PictureBoxSizeMode.Zoom,
                // Increased size of the logo container
                Size = new Size(250, 250),
                // Set a background color to match the panel to avoid potential rendering artifacts
                BackColor = Color.Transparent
            };

            // Add the PictureBox to the background panel
            backgroundPanel.Controls.Add(pictureBox);

            waitLabel = new Label
            {
                Text = "Please wait...",
                Font = new Font("Poppins", 26, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true
            };

            backgroundPanel.Controls.Add(waitLabel);



            this.Shown += (s, e) =>
            {
                CenterControls();
            };



            // Recenter controls if the screen resolution changes

            this.Resize += (s, e) =>

            {
                CenterControls();
            };

        }

        private void CenterControls()
        {
            if (pictureBox != null && waitLabel != null && this.Parent == null)
            {

                // Calculate the total height of the controls and the spacing
                int totalHeight = pictureBox.Height + waitLabel.Height + 20; // 20 pixels of space between them
                // Calculate the vertical starting position to center the group
                int topY = (this.ClientSize.Height - totalHeight) / 2;

                // Position the PictureBox at the calculated top
                int pictureBoxX = (this.ClientSize.Width - pictureBox.Width) / 2;
                pictureBox.Location = new Point(pictureBoxX, topY);

                // Position the label below the PictureBox
                int labelX = (this.ClientSize.Width - waitLabel.Width) / 2;
                int labelY = pictureBox.Location.Y + pictureBox.Height + 20;
                waitLabel.Location = new Point(labelX, labelY);
            }
        }
    }
}