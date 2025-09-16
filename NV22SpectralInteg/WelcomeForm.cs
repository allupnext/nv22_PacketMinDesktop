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
                Size = new Size(300, 300),
                // Set a background color to match the panel to avoid potential rendering artifacts
                BackColor = Color.Transparent
            };

            // Add the PictureBox to the background panel
            backgroundPanel.Controls.Add(pictureBox);

            // Center the image once the form is loaded and displayed.
            // Using the 'Shown' event ensures the form has its final dimensions.
            this.Shown += (s, e) =>
            {
                CenterImage();
            };

            // Keep the image centered if the screen resolution changes while the form is open.
            this.Resize += (s, e) =>
            {
                CenterImage();
            };
        }

        /// <summary>
        /// Calculates the center position of the form and moves the PictureBox to it.
        /// </summary>
        private void CenterImage()
        {
            // The null check is important because pictureBox is nullable
            if (pictureBox != null && this.Parent == null) // The parent check prevents issues in the designer
            {
                // Calculate the point to place the top-left corner of the PictureBox
                int x = (this.ClientSize.Width - pictureBox.Width) / 2;
                int y = (this.ClientSize.Height - pictureBox.Height) / 2;
                pictureBox.Location = new Point(x, y);
            }
        }

        // This event handler is not needed, so it can be removed for cleaner code.
        // private void Form1_Load(object sender, EventArgs e) {}
    }
}
