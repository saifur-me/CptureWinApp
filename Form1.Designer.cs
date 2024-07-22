namespace CptureWinApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblSelectCameraPreview;
        private System.Windows.Forms.ComboBox cmbCameraList;
        private System.Windows.Forms.Panel panelCameraPreview;
        private System.Windows.Forms.Label lblIntervalTimesCapture;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnStop;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblSelectCameraPreview = new Label();
            cmbCameraList = new ComboBox();
            panelCameraPreview = new Panel();
            lblIntervalTimesCapture = new Label();
            txtInterval = new TextBox();
            btnStart = new Button();
            txtLog = new TextBox();
            btnStop = new Button();
            SuspendLayout();
            // 
            // lblSelectCameraPreview
            // 
            lblSelectCameraPreview.AutoSize = true;
            lblSelectCameraPreview.Location = new Point(12, 9);
            lblSelectCameraPreview.Name = "lblSelectCameraPreview";
            lblSelectCameraPreview.Size = new Size(144, 15);
            lblSelectCameraPreview.TabIndex = 0;
            lblSelectCameraPreview.Text = "Select Camera for Preview";
            // 
            // cmbCameraList
            // 
            cmbCameraList.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCameraList.FormattingEnabled = true;
            cmbCameraList.Location = new Point(12, 27);
            cmbCameraList.Name = "cmbCameraList";
            cmbCameraList.Size = new Size(231, 23);
            cmbCameraList.TabIndex = 1;
            // 
            // panelCameraPreview
            // 
            panelCameraPreview.BorderStyle = BorderStyle.FixedSingle;
            panelCameraPreview.Location = new Point(12, 56);
            panelCameraPreview.Name = "panelCameraPreview";
            panelCameraPreview.Size = new Size(800, 477);
            panelCameraPreview.TabIndex = 2;
            // 
            // lblIntervalTimesCapture
            // 
            lblIntervalTimesCapture.AutoSize = true;
            lblIntervalTimesCapture.Location = new Point(818, 56);
            lblIntervalTimesCapture.Name = "lblIntervalTimesCapture";
            lblIntervalTimesCapture.Size = new Size(174, 15);
            lblIntervalTimesCapture.TabIndex = 3;
            lblIntervalTimesCapture.Text = "Interval Time Capture (seconds)";
            // 
            // txtInterval
            // 
            txtInterval.Location = new Point(818, 74);
            txtInterval.Name = "txtInterval";
            txtInterval.Size = new Size(174, 23);
            txtInterval.TabIndex = 4;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(892, 482);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(100, 23);
            btnStart.TabIndex = 5;
            btnStart.Text = "Start Capture";
            btnStart.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(12, 541);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(800, 150);
            txtLog.TabIndex = 6;
            // 
            // btnStop
            // 
            btnStop.BackColor = SystemColors.Control;
            btnStop.Location = new Point(892, 511);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(100, 23);
            btnStop.TabIndex = 7;
            btnStop.Text = "Stop Capture";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1024, 693);
            Controls.Add(txtLog);
            Controls.Add(btnStart);
            Controls.Add(btnStop);
            Controls.Add(txtInterval);
            Controls.Add(lblIntervalTimesCapture);
            Controls.Add(panelCameraPreview);
            Controls.Add(cmbCameraList);
            Controls.Add(lblSelectCameraPreview);
            Name = "Form1";
            Text = "Camera Capture App";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}