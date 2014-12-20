namespace VoltageCurrentGraphApp
{
    partial class VoltageCurrentGraphUI
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
            this.lblUsbConnected = new System.Windows.Forms.Label();
            this.voltageLabel = new System.Windows.Forms.Label();
            this.currentLabel = new System.Windows.Forms.Label();
            this.tmrGraphUpdater = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tmrGraphScroller = new System.Windows.Forms.Timer(this.components);
            this.zgcCurrent = new ZedGraph.ZedGraphControl();
            this.zgcVoltage = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // lblUsbConnected
            // 
            this.lblUsbConnected.AutoSize = true;
            this.lblUsbConnected.Location = new System.Drawing.Point(12, 436);
            this.lblUsbConnected.Name = "lblUsbConnected";
            this.lblUsbConnected.Size = new System.Drawing.Size(79, 13);
            this.lblUsbConnected.TabIndex = 0;
            this.lblUsbConnected.Text = "Not Connected";
            // 
            // voltageLabel
            // 
            this.voltageLabel.AutoSize = true;
            this.voltageLabel.Location = new System.Drawing.Point(9, 9);
            this.voltageLabel.Name = "voltageLabel";
            this.voltageLabel.Size = new System.Drawing.Size(35, 13);
            this.voltageLabel.TabIndex = 1;
            this.voltageLabel.Text = "label1";
            // 
            // currentLabel
            // 
            this.currentLabel.AutoSize = true;
            this.currentLabel.Location = new System.Drawing.Point(85, 9);
            this.currentLabel.Name = "currentLabel";
            this.currentLabel.Size = new System.Drawing.Size(35, 13);
            this.currentLabel.TabIndex = 2;
            this.currentLabel.Text = "label2";
            // 
            // tmrGraphUpdater
            // 
            this.tmrGraphUpdater.Enabled = true;
            this.tmrGraphUpdater.Interval = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(347, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(437, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "label2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(493, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "label3";
            // 
            // tmrGraphScroller
            // 
            this.tmrGraphScroller.Enabled = true;
            this.tmrGraphScroller.Interval = 10;
            // 
            // zgcCurrent
            // 
            this.zgcCurrent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.zgcCurrent.IsPrintKeepAspectRatio = false;
            this.zgcCurrent.Location = new System.Drawing.Point(8, 232);
            this.zgcCurrent.Name = "zgcCurrent";
            this.zgcCurrent.PanButtons = System.Windows.Forms.MouseButtons.None;
            this.zgcCurrent.PanButtons2 = System.Windows.Forms.MouseButtons.Left;
            this.zgcCurrent.ScrollGrace = 0D;
            this.zgcCurrent.ScrollMaxX = 0D;
            this.zgcCurrent.ScrollMaxY = 0D;
            this.zgcCurrent.ScrollMaxY2 = 0D;
            this.zgcCurrent.ScrollMinX = 0D;
            this.zgcCurrent.ScrollMinY = 0D;
            this.zgcCurrent.ScrollMinY2 = 0D;
            this.zgcCurrent.Size = new System.Drawing.Size(700, 200);
            this.zgcCurrent.TabIndex = 4;
            this.zgcCurrent.ZoomButtons = System.Windows.Forms.MouseButtons.None;
            this.zgcCurrent.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zgcCurrent_DoubleClickEvent);
            // 
            // zgcVoltage
            // 
            this.zgcVoltage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.zgcVoltage.IsPrintKeepAspectRatio = false;
            this.zgcVoltage.Location = new System.Drawing.Point(8, 27);
            this.zgcVoltage.Name = "zgcVoltage";
            this.zgcVoltage.PanButtons = System.Windows.Forms.MouseButtons.None;
            this.zgcVoltage.PanButtons2 = System.Windows.Forms.MouseButtons.Left;
            this.zgcVoltage.ScrollGrace = 0D;
            this.zgcVoltage.ScrollMaxX = 0D;
            this.zgcVoltage.ScrollMaxY = 0D;
            this.zgcVoltage.ScrollMaxY2 = 0D;
            this.zgcVoltage.ScrollMinX = 0D;
            this.zgcVoltage.ScrollMinY = 0D;
            this.zgcVoltage.ScrollMinY2 = 0D;
            this.zgcVoltage.Size = new System.Drawing.Size(700, 200);
            this.zgcVoltage.TabIndex = 3;
            this.zgcVoltage.ZoomButtons = System.Windows.Forms.MouseButtons.None;
            this.zgcVoltage.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zgcVoltage_DoubleClickEvent);
            // 
            // VoltageCurrentGraphUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(716, 458);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.zgcCurrent);
            this.Controls.Add(this.zgcVoltage);
            this.Controls.Add(this.currentLabel);
            this.Controls.Add(this.voltageLabel);
            this.Controls.Add(this.lblUsbConnected);
            this.Name = "VoltageCurrentGraphUI";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblUsbConnected;
        private System.Windows.Forms.Label voltageLabel;
        private System.Windows.Forms.Label currentLabel;
        private ZedGraph.ZedGraphControl zgcCurrent;
        private ZedGraph.ZedGraphControl zgcVoltage;
        private System.Windows.Forms.Timer tmrGraphUpdater;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer tmrGraphScroller;
    }
}

