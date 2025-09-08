namespace NV22SpectralInteg
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnHalt = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.resetValidatorBtn = new System.Windows.Forms.Button();
            this.logTickBox = new System.Windows.Forms.CheckBox();
            this.tbNumNotes = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.chkHold = new System.Windows.Forms.CheckBox();
            this.btnReturn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(14, 419);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "&Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnHalt
            // 
            this.btnHalt.Location = new System.Drawing.Point(106, 419);
            this.btnHalt.Name = "btnHalt";
            this.btnHalt.Size = new System.Drawing.Size(75, 23);
            this.btnHalt.TabIndex = 2;
            this.btnHalt.Text = "&Halt";
            this.btnHalt.UseVisualStyleBackColor = true;
            this.btnHalt.Click += new System.EventHandler(this.btnHalt_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.textBox1.Location = new System.Drawing.Point(12, 27);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(280, 272);
            this.textBox1.TabIndex = 4;
            // 
            // resetValidatorBtn
            // 
            this.resetValidatorBtn.Location = new System.Drawing.Point(14, 337);
            this.resetValidatorBtn.Name = "resetValidatorBtn";
            this.resetValidatorBtn.Size = new System.Drawing.Size(141, 23);
            this.resetValidatorBtn.TabIndex = 15;
            this.resetValidatorBtn.Text = "R&eset Validator";
            this.resetValidatorBtn.UseVisualStyleBackColor = true;
            this.resetValidatorBtn.Click += new System.EventHandler(this.resetValidatorBtn_Click);
            // 
            // logTickBox
            // 
            this.logTickBox.AutoSize = true;
            this.logTickBox.Checked = true;
            this.logTickBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logTickBox.Location = new System.Drawing.Point(214, 423);
            this.logTickBox.Name = "logTickBox";
            this.logTickBox.Size = new System.Drawing.Size(81, 17);
            this.logTickBox.TabIndex = 16;
            this.logTickBox.Text = "Comms Log";
            this.logTickBox.UseVisualStyleBackColor = true;
            this.logTickBox.CheckedChanged += new System.EventHandler(this.logTickBox_CheckedChanged);
            // 
            // tbNumNotes
            // 
            this.tbNumNotes.Location = new System.Drawing.Point(161, 305);
            this.tbNumNotes.Name = "tbNumNotes";
            this.tbNumNotes.Size = new System.Drawing.Size(100, 20);
            this.tbNumNotes.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 307);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Number of Notes Accepted:";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(160, 337);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 23);
            this.btnClear.TabIndex = 21;
            this.btnClear.Text = "Clear Totals";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // chkHold
            // 
            this.chkHold.AutoSize = true;
            this.chkHold.Location = new System.Drawing.Point(160, 376);
            this.chkHold.Name = "chkHold";
            this.chkHold.Size = new System.Drawing.Size(97, 17);
            this.chkHold.TabIndex = 22;
            this.chkHold.Text = "Hold in Escrow";
            this.chkHold.UseVisualStyleBackColor = true;
            this.chkHold.CheckedChanged += new System.EventHandler(this.chkHold_CheckedChanged);
            // 
            // btnReturn
            // 
            this.btnReturn.Location = new System.Drawing.Point(14, 376);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(140, 23);
            this.btnReturn.TabIndex = 23;
            this.btnReturn.Text = "Return Note";
            this.btnReturn.UseVisualStyleBackColor = true;
            this.btnReturn.Click += new System.EventHandler(this.btnReturn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 454);
            this.Controls.Add(this.btnReturn);
            this.Controls.Add(this.chkHold);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbNumNotes);
            this.Controls.Add(this.logTickBox);
            this.Controls.Add(this.resetValidatorBtn);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnHalt);
            this.Controls.Add(this.btnRun);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Validator eSSP C# example";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnHalt;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button resetValidatorBtn;
        private System.Windows.Forms.CheckBox logTickBox;
        private System.Windows.Forms.TextBox tbNumNotes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.CheckBox chkHold;
        private System.Windows.Forms.Button btnReturn;

    }
}
