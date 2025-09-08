using NV22SpectralInteg.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using ITLlib;

namespace NV22SpectralInteg
{
    public partial class Form1 : Form
    {
        // Variables used by this program.

        bool Running = false; // Indicates the status of the main poll loop
        int pollTimer = 250; // Timer in ms between polls
        int reconnectionAttempts = 10, reconnectionInterval = 3; // Connection info to deal with retrying connection to validator
        volatile bool Connected = false, ConnectionFail = false; // Threading bools to indicate status of connection with validator
        CValidator Validator; // The main validator class - used to send commands to the unit
        bool FormSetup = false; // Boolean so the form will only be setup once
        System.Windows.Forms.Timer reconnectionTimer = new System.Windows.Forms.Timer(); // Timer used to give a delay between reconnect attempts
        Thread ConnectionThread; // Thread used to connect to the validator

        // Constructor
        public Form1()
        {
            InitializeComponent();
            timer1.Interval = pollTimer;
            reconnectionTimer.Tick += new EventHandler(reconnectionTimer_Tick);
        }

        // This updates UI variables such as textboxes etc.
        void UpdateUI()
        {
            // update text boxes
            tbNumNotes.Text = Validator.NumberOfNotesStacked.ToString();
        }

        // The main program loop, this is to control the validator, it polls at
        // a value set in this class (pollTimer).
        void MainLoop()
        {
            btnRun.Enabled = false;
            Validator.CommandStructure.ComPort = Global.ComPort;
            Validator.CommandStructure.SSPAddress = Global.SSPAddress;
            Validator.CommandStructure.Timeout = 3000;

            // connect to the validator
            if (ConnectToValidator())
            {
                Running = true;
                textBox1.AppendText("\r\nPoll Loop\r\n*********************************\r\n");
                btnHalt.Enabled = true;
            }

            while (Running)
            {
                // if the poll fails, try to reconnect
                if (!Validator.DoPoll(textBox1))
                {
                    textBox1.AppendText("Poll failed, attempting to reconnect...\r\n");
                    Connected = false;
                    ConnectionThread = new Thread(ConnectToValidatorThreaded);
                    ConnectionThread.Start();
                    while (!Connected)
                    {
                        if (ConnectionFail)
                        {
                            textBox1.AppendText("Failed to reconnect to validator\r\n");
                            return;
                        }
                        Application.DoEvents();
                    }
                    textBox1.AppendText("Reconnected successfully\r\n");
                }

                timer1.Enabled = true;
                // update form
                UpdateUI();
                // setup dynamic elements of win form once
                if (!FormSetup)
                {
                    SetupFormLayout();
                    FormSetup = true;
                }
                while (timer1.Enabled)
                {
                    Application.DoEvents();
                    Thread.Sleep(1); // Yield to free up CPU
                }
            }

            //close com port and threads
            Validator.SSPComms.CloseComPort();

            btnRun.Enabled = true;
            btnHalt.Enabled = false;
        }

        // This is a one off function that is called the first time the MainLoop()
        // function runs, it just sets up a few of the UI elements that only need
        // updating once.
        private void SetupFormLayout()
        {
            // need validator class instance
            if (Validator == null)
            {
                MessageBox.Show("Validator class is null.", "ERROR");
                return;
            }
        }

        // This function opens the com port and attempts to connect with the validator. It then negotiates
        // the keys for encryption and performs some other setup commands.
        private bool ConnectToValidator()
        {
            // setup the timer
            reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (int i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                reconnectionTimer.Enabled = true;

                // close com port in case it was open
                Validator.SSPComms.CloseComPort();

                // turn encryption off for first stage
                Validator.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (Validator.OpenComPort(textBox1) && Validator.NegotiateKeys(textBox1))
                {
                    Validator.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    byte maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        Validator.SetProtocolVersion(maxPVersion, textBox1);
                    }
                    else
                    {
                        MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
                        return false;
                    }
                    // get info from the validator and store useful vars
                    Validator.ValidatorSetupRequest(textBox1);
                    // Get Serial number
                    Validator.GetSerialNumber(textBox1);
                    // check this unit is supported by this program
                    if (!IsUnitTypeSupported(Validator.UnitType))
                    {
                        MessageBox.Show("Unsupported unit type, this SDK supports the BV series and the NV series (excluding the NV11)");
                        Application.Exit();
                        return false;
                    }
                    // inhibits, this sets which channels can receive notes
                    Validator.SetInhibits(textBox1);
                    // enable, this allows the validator to receive and act on commands
                    Validator.EnableValidator(textBox1);

                    return true;
                }
                while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
            }
            return false;
        }

        // This is the same as the above function but set up differently for threading.
        private void ConnectToValidatorThreaded()
        {
            // setup the timer
            reconnectionTimer.Interval = reconnectionInterval * 1000; // for ms

            // run for number of attempts specified
            for (int i = 0; i < reconnectionAttempts; i++)
            {
                // reset timer
                reconnectionTimer.Enabled = true;

                // close com port in case it was open
                Validator.SSPComms.CloseComPort();

                // turn encryption off for first stage
                Validator.CommandStructure.EncryptionStatus = false;

                // open com port and negotiate keys
                if (Validator.OpenComPort() && Validator.NegotiateKeys())
                {
                    Validator.CommandStructure.EncryptionStatus = true; // now encrypting
                    // find the max protocol version this validator supports
                    byte maxPVersion = FindMaxProtocolVersion();
                    if (maxPVersion > 6)
                    {
                        Validator.SetProtocolVersion(maxPVersion);
                    }
                    else
                    {
                        MessageBox.Show("This program does not support units under protocol version 6, update firmware.", "ERROR");
                        Connected = false;
                        return;
                    }
                    // get info from the validator and store useful vars
                    Validator.ValidatorSetupRequest();
                    // inhibits, this sets which channels can receive notes
                    Validator.SetInhibits();
                    // enable, this allows the validator to operate
                    Validator.EnableValidator();

                    Connected = true;
                    return;
                }
                while (reconnectionTimer.Enabled) Application.DoEvents(); // wait for reconnectionTimer to tick
            }
            Connected = false;
            ConnectionFail = true;
        }

        // This function finds the maximum protocol version that a validator supports. To do this
        // it attempts to set a protocol version starting at 6 in this case, and then increments the
        // version until error 0xF8 is returned from the validator which indicates that it has failed
        // to set it. The function then returns the version number one less than the failed version.
        private byte FindMaxProtocolVersion()
        {
            // not dealing with protocol under level 6
            // attempt to set in validator
            byte b = 0x06;
            while (true)
            {
                Validator.SetProtocolVersion(b);
                if (Validator.CommandStructure.ResponseData[0] == CCommands.SSP_RESPONSE_FAIL)
                    return --b;
                b++;
                if (b > 20)
                    return 0x06; // return default if protocol 'runs away'
            }
        }

        // This function checks whether the type of validator is supported by this program. This program only
        // supports Note Validators so any other type should be rejected.
        private bool IsUnitTypeSupported(char type)
        {
            if (type == (char)0x00)
                return true;
            return false;
        }

        // Event handlers.

        private void Form1_Load(object sender, EventArgs e)
        {
            // create an instance of the validator info class
            Validator = new CValidator();
            btnHalt.Enabled = false;

            // Position comms window
            Point p = new Point(Location.X, Location.Y);
            p.X += this.Width;
            //Validator.CommsLog.Location = p;

            if (Properties.Settings.Default.CommWindow)
            {
                //Validator.CommsLog.Show();
                logTickBox.Checked = true;
            }
            else
                logTickBox.Checked = false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // hide this and show opening menu
            Hide();
            frmOpenMenu menu = new frmOpenMenu(this);
            menu.Show();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            MainLoop();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form formSettings = new frmSettings();
            formSettings.ShowDialog();
            Running = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void btnHalt_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("Poll loop stopped\r\n");
            Running = false;
        }

        private void resetValidatorBtn_Click(object sender, EventArgs e)
        {
            if (Validator != null)
            {
                Validator.Reset(textBox1);
                Validator.SSPComms.CloseComPort();
            }

        }

        private void logTickBox_CheckedChanged(object sender, EventArgs e)
        {
            //if (logTickBox.Checked)
            //    Validator.CommsLog.Show();
            //else
            //    Validator.CommsLog.Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Running = false;
            Properties.Settings.Default.CommWindow = logTickBox.Checked;
            Properties.Settings.Default.ComPort = Global.ComPort;
            Properties.Settings.Default.Save();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSettings f = new frmSettings();
            f.Show();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Validator.NumberOfNotesStacked = 0;
        }

        private void reconnectionTimer_Tick(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Timer)
            {
                System.Windows.Forms.Timer t = sender as System.Windows.Forms.Timer;
                t.Enabled = false;
            }
        }

        private void chkHold_CheckedChanged(object sender, EventArgs e)
        {
            if (chkHold.Checked == true)
            {
                Validator.HoldNumber = 10;
            }
            else
            {
                Validator.HoldNumber = 0;
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            Validator.ReturnNote(textBox1);
        }
    }
}
