namespace SmtPortalSetUp
{
    partial class AttendCompany
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttendCompany));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtMessagebox = new System.Windows.Forms.TextBox();
            this.dataGridEmployees = new System.Windows.Forms.DataGridView();
            this.ColumnSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtCompanyName = new System.Windows.Forms.TextBox();
            this.txtCompanyid = new System.Windows.Forms.TextBox();
            this.btnCompanyAttend = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnPreviousMonth = new System.Windows.Forms.Button();
            this.txtEndDate = new System.Windows.Forms.TextBox();
            this.txtStartDate = new System.Windows.Forms.TextBox();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnCleanAll = new System.Windows.Forms.Button();
            this.BtnAttendBlance = new System.Windows.Forms.Button();
            this.btnCheckUnNormal = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btndel = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnInitAttendRecord = new System.Windows.Forms.Button();
            this.btnGetAllCompany = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEmployees)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(16, 439);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(936, 16);
            this.progressBar.TabIndex = 6;
            // 
            // txtMessagebox
            // 
            this.txtMessagebox.Location = new System.Drawing.Point(16, 459);
            this.txtMessagebox.Multiline = true;
            this.txtMessagebox.Name = "txtMessagebox";
            this.txtMessagebox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMessagebox.Size = new System.Drawing.Size(936, 104);
            this.txtMessagebox.TabIndex = 7;
            this.txtMessagebox.DoubleClick += new System.EventHandler(this.txtMessagebox_DoubleClick);
            // 
            // dataGridEmployees
            // 
            this.dataGridEmployees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridEmployees.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSelect});
            this.dataGridEmployees.Location = new System.Drawing.Point(6, 37);
            this.dataGridEmployees.Name = "dataGridEmployees";
            this.dataGridEmployees.RowTemplate.Height = 23;
            this.dataGridEmployees.Size = new System.Drawing.Size(848, 216);
            this.dataGridEmployees.TabIndex = 13;
            this.dataGridEmployees.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridEmployees_CellClick);
            // 
            // ColumnSelect
            // 
            this.ColumnSelect.HeaderText = "选择";
            this.ColumnSelect.Name = "ColumnSelect";
            // 
            // btnSelect
            // 
            this.btnSelect.BackColor = System.Drawing.Color.Silver;
            this.btnSelect.Location = new System.Drawing.Point(408, 6);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(120, 24);
            this.btnSelect.TabIndex = 4;
            this.btnSelect.Text = "查询公司信息";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtCompanyName
            // 
            this.txtCompanyName.Location = new System.Drawing.Point(-5, 9);
            this.txtCompanyName.Name = "txtCompanyName";
            this.txtCompanyName.Size = new System.Drawing.Size(148, 21);
            this.txtCompanyName.TabIndex = 14;
            this.txtCompanyName.Text = "公司名";
            // 
            // txtCompanyid
            // 
            this.txtCompanyid.Location = new System.Drawing.Point(165, 10);
            this.txtCompanyid.Name = "txtCompanyid";
            this.txtCompanyid.Size = new System.Drawing.Size(233, 21);
            this.txtCompanyid.TabIndex = 16;
            this.txtCompanyid.Text = "公司id";
            // 
            // btnCompanyAttend
            // 
            this.btnCompanyAttend.BackColor = System.Drawing.Color.Silver;
            this.btnCompanyAttend.Location = new System.Drawing.Point(565, 6);
            this.btnCompanyAttend.Name = "btnCompanyAttend";
            this.btnCompanyAttend.Size = new System.Drawing.Size(161, 24);
            this.btnCompanyAttend.TabIndex = 4;
            this.btnCompanyAttend.Text = "查询公司考勤初始化信息";
            this.btnCompanyAttend.UseVisualStyleBackColor = false;
            this.btnCompanyAttend.Click += new System.EventHandler(this.btnCompanyAttend_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(875, 429);
            this.tabControl1.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btndel);
            this.tabPage1.Controls.Add(this.btnStart);
            this.tabPage1.Controls.Add(this.btnPreviousMonth);
            this.tabPage1.Controls.Add(this.txtEndDate);
            this.tabPage1.Controls.Add(this.txtStartDate);
            this.tabPage1.Controls.Add(this.btnCleanAll);
            this.tabPage1.Controls.Add(this.BtnAttendBlance);
            this.tabPage1.Controls.Add(this.btnCheckUnNormal);
            this.tabPage1.Controls.Add(this.btnCheck);
            this.tabPage1.Controls.Add(this.btnCompanyAttend);
            this.tabPage1.Controls.Add(this.btnSelect);
            this.tabPage1.Controls.Add(this.dataGridEmployees);
            this.tabPage1.Controls.Add(this.txtCompanyid);
            this.tabPage1.Controls.Add(this.txtCompanyName);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(867, 403);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "处理公司考勤";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnInitAttendRecord);
            this.tabPage2.Controls.Add(this.btnGetAllCompany);
            this.tabPage2.Controls.Add(this.dataGridView1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(867, 403);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "处理所有公司考勤";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnPreviousMonth
            // 
            this.btnPreviousMonth.Location = new System.Drawing.Point(256, 268);
            this.btnPreviousMonth.Name = "btnPreviousMonth";
            this.btnPreviousMonth.Size = new System.Drawing.Size(75, 23);
            this.btnPreviousMonth.TabIndex = 30;
            this.btnPreviousMonth.Text = "处理上月";
            this.btnPreviousMonth.UseVisualStyleBackColor = true;
            // 
            // txtEndDate
            // 
            this.txtEndDate.Location = new System.Drawing.Point(123, 270);
            this.txtEndDate.Name = "txtEndDate";
            this.txtEndDate.Size = new System.Drawing.Size(100, 21);
            this.txtEndDate.TabIndex = 29;
            this.txtEndDate.Text = "2013-04-01";
            // 
            // txtStartDate
            // 
            this.txtStartDate.Location = new System.Drawing.Point(6, 270);
            this.txtStartDate.Name = "txtStartDate";
            this.txtStartDate.Size = new System.Drawing.Size(100, 21);
            this.txtStartDate.TabIndex = 28;
            this.txtStartDate.Text = "2013-03-01";
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackColor = System.Drawing.Color.Silver;
            this.btnPrevious.Location = new System.Drawing.Point(893, 4);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(63, 24);
            this.btnPrevious.TabIndex = 27;
            this.btnPrevious.Text = "上一步";
            this.btnPrevious.UseVisualStyleBackColor = false;
            // 
            // btnCleanAll
            // 
            this.btnCleanAll.BackColor = System.Drawing.Color.Silver;
            this.btnCleanAll.Location = new System.Drawing.Point(369, 267);
            this.btnCleanAll.Name = "btnCleanAll";
            this.btnCleanAll.Size = new System.Drawing.Size(227, 24);
            this.btnCleanAll.TabIndex = 23;
            this.btnCleanAll.Text = "清空指定时间段异常并重置考勤";
            this.btnCleanAll.UseVisualStyleBackColor = false;
            // 
            // BtnAttendBlance
            // 
            this.BtnAttendBlance.BackColor = System.Drawing.Color.Silver;
            this.BtnAttendBlance.Location = new System.Drawing.Point(443, 303);
            this.BtnAttendBlance.Name = "BtnAttendBlance";
            this.BtnAttendBlance.Size = new System.Drawing.Size(153, 24);
            this.BtnAttendBlance.TabIndex = 24;
            this.BtnAttendBlance.Text = "结算本月考勤";
            this.BtnAttendBlance.UseVisualStyleBackColor = false;
            // 
            // btnCheckUnNormal
            // 
            this.btnCheckUnNormal.BackColor = System.Drawing.Color.Silver;
            this.btnCheckUnNormal.Location = new System.Drawing.Point(256, 303);
            this.btnCheckUnNormal.Name = "btnCheckUnNormal";
            this.btnCheckUnNormal.Size = new System.Drawing.Size(153, 24);
            this.btnCheckUnNormal.TabIndex = 25;
            this.btnCheckUnNormal.Text = "检查异常考勤";
            this.btnCheckUnNormal.UseVisualStyleBackColor = false;
            // 
            // btnCheck
            // 
            this.btnCheck.BackColor = System.Drawing.Color.Silver;
            this.btnCheck.Location = new System.Drawing.Point(123, 303);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(100, 24);
            this.btnCheck.TabIndex = 26;
            this.btnCheck.Text = "检查请假及出差";
            this.btnCheck.UseVisualStyleBackColor = false;
            // 
            // btndel
            // 
            this.btndel.BackColor = System.Drawing.Color.Silver;
            this.btndel.Location = new System.Drawing.Point(609, 270);
            this.btndel.Name = "btndel";
            this.btndel.Size = new System.Drawing.Size(135, 24);
            this.btndel.TabIndex = 31;
            this.btndel.Text = "删除所有考勤初始化";
            this.btndel.UseVisualStyleBackColor = false;
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.Silver;
            this.btnStart.Location = new System.Drawing.Point(609, 306);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(135, 24);
            this.btnStart.TabIndex = 32;
            this.btnStart.Text = "初始化考勤";
            this.btnStart.UseVisualStyleBackColor = false;
            // 
            // btnInitAttendRecord
            // 
            this.btnInitAttendRecord.BackColor = System.Drawing.Color.Silver;
            this.btnInitAttendRecord.Location = new System.Drawing.Point(151, 3);
            this.btnInitAttendRecord.Name = "btnInitAttendRecord";
            this.btnInitAttendRecord.Size = new System.Drawing.Size(161, 24);
            this.btnInitAttendRecord.TabIndex = 14;
            this.btnInitAttendRecord.Text = "初始化所有公司考勤初始化信息";
            this.btnInitAttendRecord.UseVisualStyleBackColor = false;
            this.btnInitAttendRecord.Click += new System.EventHandler(this.btnInitAttendRecord_Click);
            // 
            // btnGetAllCompany
            // 
            this.btnGetAllCompany.BackColor = System.Drawing.Color.Silver;
            this.btnGetAllCompany.Location = new System.Drawing.Point(13, 3);
            this.btnGetAllCompany.Name = "btnGetAllCompany";
            this.btnGetAllCompany.Size = new System.Drawing.Size(120, 24);
            this.btnGetAllCompany.TabIndex = 15;
            this.btnGetAllCompany.Text = "查询所有公司";
            this.btnGetAllCompany.UseVisualStyleBackColor = false;
            this.btnGetAllCompany.Click += new System.EventHandler(this.btnGetAllCompany_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1});
            this.dataGridView1.Location = new System.Drawing.Point(13, 37);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(848, 360);
            this.dataGridView1.TabIndex = 16;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.HeaderText = "选择";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            // 
            // AttendCompany
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.ClientSize = new System.Drawing.Size(964, 575);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.txtMessagebox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnPrevious);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AttendCompany";
            this.Text = "欢迎使用协同办公服务器安装";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AttendCompany_FormClosed);
            this.Load += new System.EventHandler(this.Form_Second_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEmployees)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtMessagebox;
        private System.Windows.Forms.DataGridView dataGridEmployees;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtCompanyName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelect;
        private System.Windows.Forms.TextBox txtCompanyid;
        private System.Windows.Forms.Button btnCompanyAttend;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btndel;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnPreviousMonth;
        private System.Windows.Forms.TextBox txtEndDate;
        private System.Windows.Forms.TextBox txtStartDate;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnCleanAll;
        private System.Windows.Forms.Button BtnAttendBlance;
        private System.Windows.Forms.Button btnCheckUnNormal;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnInitAttendRecord;
        private System.Windows.Forms.Button btnGetAllCompany;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
    }
}

