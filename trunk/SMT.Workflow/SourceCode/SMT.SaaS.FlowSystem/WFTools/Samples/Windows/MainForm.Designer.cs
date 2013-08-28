namespace WFTools.Samples.Windows
{
    partial class MainForm
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
            this.btnCreateSequentialWorkflow = new System.Windows.Forms.Button();
            this.lblTraceOutput = new System.Windows.Forms.Label();
            this.cboPersistenceService = new System.Windows.Forms.ComboBox();
            this.lblPersistenceService = new System.Windows.Forms.Label();
            this.chkModifyWorkflow = new System.Windows.Forms.CheckBox();
            this.cmdUpdateTrackingProfile = new System.Windows.Forms.Button();
            this.txtTraceOutput = new System.Windows.Forms.TextBox();
            this.cboTrackingService = new System.Windows.Forms.ComboBox();
            this.lblTrackingService = new System.Windows.Forms.Label();
            this.chkUseLocalTransactions = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnCreateSequentialWorkflow
            // 
            this.btnCreateSequentialWorkflow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateSequentialWorkflow.Location = new System.Drawing.Point(370, 3);
            this.btnCreateSequentialWorkflow.Name = "btnCreateSequentialWorkflow";
            this.btnCreateSequentialWorkflow.Size = new System.Drawing.Size(169, 23);
            this.btnCreateSequentialWorkflow.TabIndex = 0;
            this.btnCreateSequentialWorkflow.Text = "Create Sequential WorkFlow";
            this.btnCreateSequentialWorkflow.UseVisualStyleBackColor = true;
            this.btnCreateSequentialWorkflow.Click += new System.EventHandler(this.btnCreateSequentialWorkflow_Click);
            // 
            // lblTraceOutput
            // 
            this.lblTraceOutput.AutoSize = true;
            this.lblTraceOutput.Location = new System.Drawing.Point(9, 102);
            this.lblTraceOutput.Name = "lblTraceOutput";
            this.lblTraceOutput.Size = new System.Drawing.Size(73, 13);
            this.lblTraceOutput.TabIndex = 2;
            this.lblTraceOutput.Text = "Trace Output:";
            // 
            // cboPersistenceService
            // 
            this.cboPersistenceService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPersistenceService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPersistenceService.FormattingEnabled = true;
            this.cboPersistenceService.Location = new System.Drawing.Point(122, 5);
            this.cboPersistenceService.Name = "cboPersistenceService";
            this.cboPersistenceService.Size = new System.Drawing.Size(242, 21);
            this.cboPersistenceService.TabIndex = 3;
            // 
            // lblPersistenceService
            // 
            this.lblPersistenceService.AutoSize = true;
            this.lblPersistenceService.Location = new System.Drawing.Point(12, 8);
            this.lblPersistenceService.Name = "lblPersistenceService";
            this.lblPersistenceService.Size = new System.Drawing.Size(104, 13);
            this.lblPersistenceService.TabIndex = 4;
            this.lblPersistenceService.Text = "Persistence Service:";
            // 
            // chkModifyWorkflow
            // 
            this.chkModifyWorkflow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkModifyWorkflow.AutoSize = true;
            this.chkModifyWorkflow.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkModifyWorkflow.Location = new System.Drawing.Point(434, 61);
            this.chkModifyWorkflow.Name = "chkModifyWorkflow";
            this.chkModifyWorkflow.Size = new System.Drawing.Size(105, 17);
            this.chkModifyWorkflow.TabIndex = 5;
            this.chkModifyWorkflow.Text = "Modify Workflow";
            this.chkModifyWorkflow.UseVisualStyleBackColor = true;
            // 
            // cmdUpdateTrackingProfile
            // 
            this.cmdUpdateTrackingProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdUpdateTrackingProfile.Location = new System.Drawing.Point(370, 32);
            this.cmdUpdateTrackingProfile.Name = "cmdUpdateTrackingProfile";
            this.cmdUpdateTrackingProfile.Size = new System.Drawing.Size(169, 23);
            this.cmdUpdateTrackingProfile.TabIndex = 6;
            this.cmdUpdateTrackingProfile.Text = "Update Tracking Profile";
            this.cmdUpdateTrackingProfile.UseVisualStyleBackColor = true;
            this.cmdUpdateTrackingProfile.Click += new System.EventHandler(this.cmdUpdateTrackingProfile_Click);
            // 
            // txtTraceOutput
            // 
            this.txtTraceOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTraceOutput.BackColor = System.Drawing.SystemColors.Window;
            this.txtTraceOutput.Location = new System.Drawing.Point(12, 118);
            this.txtTraceOutput.Multiline = true;
            this.txtTraceOutput.Name = "txtTraceOutput";
            this.txtTraceOutput.ReadOnly = true;
            this.txtTraceOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTraceOutput.Size = new System.Drawing.Size(527, 261);
            this.txtTraceOutput.TabIndex = 7;
            // 
            // cboTrackingService
            // 
            this.cboTrackingService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTrackingService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrackingService.FormattingEnabled = true;
            this.cboTrackingService.Location = new System.Drawing.Point(122, 32);
            this.cboTrackingService.Name = "cboTrackingService";
            this.cboTrackingService.Size = new System.Drawing.Size(242, 21);
            this.cboTrackingService.TabIndex = 8;
            // 
            // lblTrackingService
            // 
            this.lblTrackingService.AutoSize = true;
            this.lblTrackingService.Location = new System.Drawing.Point(12, 35);
            this.lblTrackingService.Name = "lblTrackingService";
            this.lblTrackingService.Size = new System.Drawing.Size(91, 13);
            this.lblTrackingService.TabIndex = 9;
            this.lblTrackingService.Text = "Tracking Service:";
            // 
            // chkUseLocalTransactions
            // 
            this.chkUseLocalTransactions.AutoSize = true;
            this.chkUseLocalTransactions.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkUseLocalTransactions.Location = new System.Drawing.Point(401, 84);
            this.chkUseLocalTransactions.Name = "chkUseLocalTransactions";
            this.chkUseLocalTransactions.Size = new System.Drawing.Size(138, 17);
            this.chkUseLocalTransactions.TabIndex = 10;
            this.chkUseLocalTransactions.Text = "Use Local Transactions";
            this.chkUseLocalTransactions.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 391);
            this.Controls.Add(this.chkUseLocalTransactions);
            this.Controls.Add(this.lblTrackingService);
            this.Controls.Add(this.cboTrackingService);
            this.Controls.Add(this.txtTraceOutput);
            this.Controls.Add(this.cmdUpdateTrackingProfile);
            this.Controls.Add(this.chkModifyWorkflow);
            this.Controls.Add(this.lblPersistenceService);
            this.Controls.Add(this.cboPersistenceService);
            this.Controls.Add(this.lblTraceOutput);
            this.Controls.Add(this.btnCreateSequentialWorkflow);
            this.Name = "MainForm";
            this.Text = "Windows WorkFlow Test";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCreateSequentialWorkflow;
        private System.Windows.Forms.Label lblTraceOutput;
        private System.Windows.Forms.ComboBox cboPersistenceService;
        private System.Windows.Forms.Label lblPersistenceService;
        private System.Windows.Forms.CheckBox chkModifyWorkflow;
        private System.Windows.Forms.Button cmdUpdateTrackingProfile;
        private System.Windows.Forms.TextBox txtTraceOutput;
        private System.Windows.Forms.ComboBox cboTrackingService;
        private System.Windows.Forms.Label lblTrackingService;
        private System.Windows.Forms.CheckBox chkUseLocalTransactions;
    }
}

