namespace AttendaceAccount
{
    partial class AttendLogFormHanWang
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtFingerPrintID = new System.Windows.Forms.TextBox();
            this.lblFingerPrintID = new System.Windows.Forms.Label();
            this.lblDateTo = new System.Windows.Forms.Label();
            this.dpDateTo = new System.Windows.Forms.DateTimePicker();
            this.dpDateFrom = new System.Windows.Forms.DateTimePicker();
            this.lblDateFrom = new System.Windows.Forms.Label();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnUploadGeneralLogData = new System.Windows.Forms.Button();
            this.btnGetGeneralLogData = new System.Windows.Forms.Button();
            this.lvLogs = new System.Windows.Forms.ListView();
            this.lvLogsch1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvLogsch2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvLogsch3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvLogsch4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnGetAttPrintRecord = new System.Windows.Forms.Button();
            this.txtEmployeeName = new System.Windows.Forms.TextBox();
            this.txtEmployeeId = new System.Windows.Forms.TextBox();
            this.btnGetEmployeeId = new System.Windows.Forms.Button();
            this.dtAttRecord = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn7 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnUploadAgain = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtAttRecord)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tabControl1);
            this.groupBox2.Controls.Add(this.lblState);
            this.groupBox2.Location = new System.Drawing.Point(10, 33);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(467, 188);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "考勤机连接设置";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(6, 20);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(449, 143);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage1.Controls.Add(this.txtFingerPrintID);
            this.tabPage1.Controls.Add(this.lblFingerPrintID);
            this.tabPage1.Controls.Add(this.lblDateTo);
            this.tabPage1.Controls.Add(this.dpDateTo);
            this.tabPage1.Controls.Add(this.dpDateFrom);
            this.tabPage1.Controls.Add(this.lblDateFrom);
            this.tabPage1.Controls.Add(this.txtIP);
            this.tabPage1.Controls.Add(this.txtPort);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.btnConnect);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabPage1.ForeColor = System.Drawing.Color.DarkBlue;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(441, 117);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "TCP/IP";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtFingerPrintID
            // 
            this.txtFingerPrintID.Location = new System.Drawing.Point(87, 65);
            this.txtFingerPrintID.Name = "txtFingerPrintID";
            this.txtFingerPrintID.Size = new System.Drawing.Size(296, 21);
            this.txtFingerPrintID.TabIndex = 17;
            // 
            // lblFingerPrintID
            // 
            this.lblFingerPrintID.AutoSize = true;
            this.lblFingerPrintID.Location = new System.Drawing.Point(25, 71);
            this.lblFingerPrintID.Name = "lblFingerPrintID";
            this.lblFingerPrintID.Size = new System.Drawing.Size(65, 12);
            this.lblFingerPrintID.TabIndex = 16;
            this.lblFingerPrintID.Text = "指纹编号：";
            // 
            // lblDateTo
            // 
            this.lblDateTo.AutoSize = true;
            this.lblDateTo.Location = new System.Drawing.Point(219, 43);
            this.lblDateTo.Name = "lblDateTo";
            this.lblDateTo.Size = new System.Drawing.Size(17, 12);
            this.lblDateTo.TabIndex = 15;
            this.lblDateTo.Text = "至";
            // 
            // dpDateTo
            // 
            this.dpDateTo.Location = new System.Drawing.Point(262, 38);
            this.dpDateTo.Name = "dpDateTo";
            this.dpDateTo.Size = new System.Drawing.Size(121, 21);
            this.dpDateTo.TabIndex = 14;
            // 
            // dpDateFrom
            // 
            this.dpDateFrom.Location = new System.Drawing.Point(87, 38);
            this.dpDateFrom.Name = "dpDateFrom";
            this.dpDateFrom.Size = new System.Drawing.Size(121, 21);
            this.dpDateFrom.TabIndex = 13;
            // 
            // lblDateFrom
            // 
            this.lblDateFrom.AutoSize = true;
            this.lblDateFrom.Location = new System.Drawing.Point(25, 43);
            this.lblDateFrom.Name = "lblDateFrom";
            this.lblDateFrom.Size = new System.Drawing.Size(65, 12);
            this.lblDateFrom.TabIndex = 11;
            this.lblDateFrom.Text = "导入日期：";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(87, 12);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(121, 21);
            this.txtIP.TabIndex = 6;
            this.txtIP.Text = "192.168.1.201";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(262, 12);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(121, 21);
            this.txtPort.TabIndex = 7;
            this.txtPort.Text = "4370";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(219, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "端口：";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(183, 90);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "IP地址：";
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.ForeColor = System.Drawing.Color.Crimson;
            this.lblState.Location = new System.Drawing.Point(183, 166);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(95, 12);
            this.lblState.TabIndex = 2;
            this.lblState.Text = "连接状态:未连接";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExport);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnUploadGeneralLogData);
            this.groupBox1.Controls.Add(this.btnGetGeneralLogData);
            this.groupBox1.Controls.Add(this.lvLogs);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.ForeColor = System.Drawing.Color.DarkBlue;
            this.groupBox1.Location = new System.Drawing.Point(10, 224);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(467, 343);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "下载、上传打卡记录";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(12, 314);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(137, 23);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "导出记录";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(154, 319);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(155, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "打卡记录导出为Excel文件。";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(155, 269);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "从考勤机获取打卡记录。";
            // 
            // btnUploadGeneralLogData
            // 
            this.btnUploadGeneralLogData.Location = new System.Drawing.Point(12, 288);
            this.btnUploadGeneralLogData.Name = "btnUploadGeneralLogData";
            this.btnUploadGeneralLogData.Size = new System.Drawing.Size(137, 23);
            this.btnUploadGeneralLogData.TabIndex = 4;
            this.btnUploadGeneralLogData.Text = "上传记录";
            this.btnUploadGeneralLogData.UseVisualStyleBackColor = true;
            this.btnUploadGeneralLogData.Click += new System.EventHandler(this.btnUploadGeneralLogData_Click);
            // 
            // btnGetGeneralLogData
            // 
            this.btnGetGeneralLogData.Location = new System.Drawing.Point(12, 263);
            this.btnGetGeneralLogData.Name = "btnGetGeneralLogData";
            this.btnGetGeneralLogData.Size = new System.Drawing.Size(137, 23);
            this.btnGetGeneralLogData.TabIndex = 1;
            this.btnGetGeneralLogData.Text = "下载记录";
            this.btnGetGeneralLogData.UseVisualStyleBackColor = true;
            this.btnGetGeneralLogData.Click += new System.EventHandler(this.btnGetGeneralLogData_Click);
            // 
            // lvLogs
            // 
            this.lvLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvLogsch1,
            this.lvLogsch2,
            this.lvLogsch3,
            this.lvLogsch4});
            this.lvLogs.GridLines = true;
            this.lvLogs.Location = new System.Drawing.Point(9, 21);
            this.lvLogs.Name = "lvLogs";
            this.lvLogs.Size = new System.Drawing.Size(449, 234);
            this.lvLogs.TabIndex = 0;
            this.lvLogs.UseCompatibleStateImageBehavior = false;
            this.lvLogs.View = System.Windows.Forms.View.Details;
            // 
            // lvLogsch1
            // 
            this.lvLogsch1.Text = "序号";
            this.lvLogsch1.Width = 45;
            // 
            // lvLogsch2
            // 
            this.lvLogsch2.Text = "指纹编号";
            this.lvLogsch2.Width = 69;
            // 
            // lvLogsch3
            // 
            this.lvLogsch3.Text = "验证方式";
            this.lvLogsch3.Width = 76;
            // 
            // lvLogsch4
            // 
            this.lvLogsch4.Text = "打卡时间";
            this.lvLogsch4.Width = 75;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(154, 293);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "上传打卡记录到服务器。";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AttendaceAccount.Properties.Resources.top550;
            this.pictureBox1.Location = new System.Drawing.Point(0, -2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(956, 19);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // btnGetAttPrintRecord
            // 
            this.btnGetAttPrintRecord.Location = new System.Drawing.Point(498, 60);
            this.btnGetAttPrintRecord.Name = "btnGetAttPrintRecord";
            this.btnGetAttPrintRecord.Size = new System.Drawing.Size(138, 23);
            this.btnGetAttPrintRecord.TabIndex = 12;
            this.btnGetAttPrintRecord.Text = "获取员工打卡记录";
            this.btnGetAttPrintRecord.UseVisualStyleBackColor = true;
            this.btnGetAttPrintRecord.Click += new System.EventHandler(this.btnGetAttPrintRecord_Click);
            // 
            // txtEmployeeName
            // 
            this.txtEmployeeName.Location = new System.Drawing.Point(498, 31);
            this.txtEmployeeName.Name = "txtEmployeeName";
            this.txtEmployeeName.Size = new System.Drawing.Size(162, 21);
            this.txtEmployeeName.TabIndex = 13;
            this.txtEmployeeName.Text = "员工姓名";
            // 
            // txtEmployeeId
            // 
            this.txtEmployeeId.Location = new System.Drawing.Point(788, 33);
            this.txtEmployeeId.Name = "txtEmployeeId";
            this.txtEmployeeId.Size = new System.Drawing.Size(168, 21);
            this.txtEmployeeId.TabIndex = 14;
            // 
            // btnGetEmployeeId
            // 
            this.btnGetEmployeeId.Location = new System.Drawing.Point(688, 31);
            this.btnGetEmployeeId.Name = "btnGetEmployeeId";
            this.btnGetEmployeeId.Size = new System.Drawing.Size(75, 23);
            this.btnGetEmployeeId.TabIndex = 12;
            this.btnGetEmployeeId.Text = "获取员工Id";
            this.btnGetEmployeeId.UseVisualStyleBackColor = true;
            this.btnGetEmployeeId.Click += new System.EventHandler(this.btnGetEmployeeId_Click);
            // 
            // dtAttRecord
            // 
            this.dtAttRecord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtAttRecord.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn7});
            this.dtAttRecord.Location = new System.Drawing.Point(498, 106);
            this.dtAttRecord.Name = "dtAttRecord";
            this.dtAttRecord.RowTemplate.Height = 23;
            this.dtAttRecord.Size = new System.Drawing.Size(449, 373);
            this.dtAttRecord.TabIndex = 15;
            // 
            // dataGridViewCheckBoxColumn7
            // 
            this.dataGridViewCheckBoxColumn7.HeaderText = "选择";
            this.dataGridViewCheckBoxColumn7.Name = "dataGridViewCheckBoxColumn7";
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point(818, 60);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(138, 23);
            this.btnPrevious.TabIndex = 12;
            this.btnPrevious.Text = "上一步";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnUploadAgain
            // 
            this.btnUploadAgain.Location = new System.Drawing.Point(499, 488);
            this.btnUploadAgain.Name = "btnUploadAgain";
            this.btnUploadAgain.Size = new System.Drawing.Size(137, 23);
            this.btnUploadAgain.TabIndex = 4;
            this.btnUploadAgain.Text = "再次上传打卡记录";
            this.btnUploadAgain.UseVisualStyleBackColor = true;
            this.btnUploadAgain.Click += new System.EventHandler(this.btnUploadAgain_Click);
            // 
            // AttendLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(979, 573);
            this.Controls.Add(this.dtAttRecord);
            this.Controls.Add(this.txtEmployeeId);
            this.Controls.Add(this.txtEmployeeName);
            this.Controls.Add(this.btnUploadAgain);
            this.Controls.Add(this.btnGetEmployeeId);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.btnGetAttPrintRecord);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Location = new System.Drawing.Point(7, 186);
            this.Name = "AttendLogForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Text = "AttendLog";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtAttRecord)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label lblDateFrom;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDateTo;
        private System.Windows.Forms.DateTimePicker dpDateTo;
        private System.Windows.Forms.DateTimePicker dpDateFrom;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnUploadGeneralLogData;
        private System.Windows.Forms.Button btnGetGeneralLogData;
        private System.Windows.Forms.ListView lvLogs;
        private System.Windows.Forms.ColumnHeader lvLogsch1;
        private System.Windows.Forms.ColumnHeader lvLogsch2;
        private System.Windows.Forms.ColumnHeader lvLogsch3;
        private System.Windows.Forms.ColumnHeader lvLogsch4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtFingerPrintID;
        private System.Windows.Forms.Label lblFingerPrintID;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnGetAttPrintRecord;
        private System.Windows.Forms.TextBox txtEmployeeName;
        private System.Windows.Forms.TextBox txtEmployeeId;
        private System.Windows.Forms.Button btnGetEmployeeId;
        private System.Windows.Forms.DataGridView dtAttRecord;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn7;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnUploadAgain;
    }
}

