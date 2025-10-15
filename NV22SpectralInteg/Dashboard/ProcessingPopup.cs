using System;
using System.Drawing;
using System.Drawing.Drawing2D; // For rounded corners
using System.IO;
using System.Windows.Forms;

namespace NV22SpectralInteg.Dashboard
{
    public partial class ProcessingPopup : Form
    {
        // Define your accent color (from the loader)
        private Color AccentColor = ColorTranslator.FromHtml("#25C866");
        // Define a slightly softer dark background
        private Color BackgroundColor = Color.FromArgb(35, 35, 35); // Darker than 45,45,45 for more contrast

        public ProcessingPopup()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BackgroundColor; // Use the harmonized background color
            this.ShowInTaskbar = false;

            // Optional: For rounded corners, we need to handle the Paint event
            this.Paint += new PaintEventHandler(ProcessingPopup_Paint);

            // Calculate responsive size
            // Get the working area (screen bounds minus taskbar, etc.)
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

            // Set minimum and max desired size
            int desiredWidth = 700;
            int desiredHeight = 400;

            // Optionally scale down if screen is smaller
            int finalWidth = Math.Min(desiredWidth, workingArea.Width - 100);
            int finalHeight = Math.Min(desiredHeight, workingArea.Height - 100);

            this.Size = new Size(finalWidth, finalHeight);

            // Center the form on screen manually
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                (workingArea.Width - this.Width) / 2,
                (workingArea.Height - this.Height) / 2
            );



            // 1. Message Label
            Label messageLabel = new Label
            {
                Text = "Processing transaction,\nplease wait...",
                ForeColor = AccentColor, // Text color matches the loader!
                Font = new Font("Poppins", 20F, FontStyle.Bold), // Slightly larger font
                TextAlign = ContentAlignment.MiddleCenter,
                // Use a TableLayoutPanel for better control over spacing and centering vertically
                Dock = DockStyle.None, // No dock initially for better manual positioning
                AutoSize = false,
                Size = new Size(finalWidth, (int)(finalHeight * 0.3)), // Take up top 30% of form height
                Location = new Point(0, 0) // Position at top
            };
            this.Controls.Add(messageLabel);

            // 2. Spinner PictureBox
            string imagePath = Path.Combine(Application.StartupPath, "Image", "Spinner.gif");

            PictureBox spinner = new PictureBox
            {
                Image = File.Exists(imagePath) ? Image.FromFile(imagePath) : null, // Check if file exists
                SizeMode = PictureBoxSizeMode.Zoom, // Use Zoom to scale it nicely within bounds
                // Position and size the spinner dynamically
                Width = (int)(finalWidth * 0.4), // 40% of popup width
                Height = (int)(finalWidth * 0.5), // Keep it square
                // Center the spinner horizontally and below the message label
                Location = new Point((finalWidth - (int)(finalWidth * 0.4)) / 2, messageLabel.Bottom)
            };
            this.Controls.Add(spinner);

            // Optional: Add a subtle loading text below the spinner
            Label loadingTextLabel = new Label
            {
                Text = "Loading...", // Or "Connecting to server..." etc.
                ForeColor = Color.LightGray, // A subtle gray
                Font = new Font("Poppins", 14F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = finalWidth,
                Height = 40,
                Location = new Point(0, spinner.Bottom + 5) // Below the spinner
            };
            //this.Controls.Add(loadingTextLabel);


            // Adjust positions if elements overlap or are not centered as expected
            // For robust layout, consider a TableLayoutPanel or FlowLayoutPanel
            // but for a simple popup, direct positioning often suffices.
        }


        // --- Method for Rounded Corners ---
        // This makes the form itself rounded.
        private const int WS_EX_TOOLWINDOW = 0x80; // Prevents the form from showing in the taskbar when hidden
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        private void ProcessingPopup_Paint(object sender, PaintEventArgs e)
        {
            int radius = 20; // Radius for rounded corners
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90); // Top-left
            path.AddArc(this.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90); // Top-right
            path.AddArc(this.Width - radius * 2, this.Height - radius * 2, radius * 2, radius * 2, 0, 90); // Bottom-right
            path.AddArc(0, this.Height - radius * 2, radius * 2, radius * 2, 90, 90); // Bottom-left
            path.CloseAllFigures();
            this.Region = new Region(path);

        }
    }
}