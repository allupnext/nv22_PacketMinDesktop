using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NV22SpectralInteg.InactivityManager
{
    public partial class CountdownForm : Form
    {
        private Label lblMessage;

        public CountdownForm()
        {
            // Changed this call to the new method name
            SetupControls();
            PositionWindow();
        }

        // Renamed this method to avoid conflict with the designer's method
        private void SetupControls()
        {
            this.lblMessage = new Label();
            this.SuspendLayout();

            // 
            // lblMessage
            // 
            this.lblMessage.Dock = DockStyle.Fill;
            this.lblMessage.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = Color.White;
            this.lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            this.lblMessage.Name = "lblMessage";

            // 
            // CountdownForm
            // 
            this.BackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43))))); // Flat Red
            this.ClientSize = new Size(350, 60);
            this.Controls.Add(this.lblMessage);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "CountdownForm";
            this.Text = "Inactivity Warning";
            this.TopMost = true; // Always on top
            this.ShowInTaskbar = false; // Don't show in the taskbar
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Positions the form in the bottom-right corner of the primary screen.
        /// </summary>
        private void PositionWindow()
        {
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Left = workingArea.Right - this.Width - 10; // 10px margin
            this.Top = workingArea.Bottom - this.Height - 10; // 10px margin
        }

        /// <summary>
        /// Updates the text displayed on the form.
        /// </summary>
        public void UpdateMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateMessage), message);
                return;
            }
            lblMessage.Text = message;
        }
    }
}
