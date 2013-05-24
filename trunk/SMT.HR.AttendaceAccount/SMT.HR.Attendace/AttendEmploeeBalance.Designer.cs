namespace SmtPortalSetUp
{
    partial class AttendEmploeeBalance
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttendEmploeeBalance));
            this.btnPrevious = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtMessagebox = new System.Windows.Forms.TextBox();
            this.Txtid = new System.Windows.Forms.TextBox();
            this.txtStartDate = new System.Windows.Forms.TextBox();
            this.dataGridEmployees = new System.Windows.Forms.DataGridView();
            this.ColumnSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtEmployeeName = new System.Windows.Forms.TextBox();
            this.txtCompanyid = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectBalance = new System.Windows.Forms.Button();
            this.btnGenerateSalary = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEmployees)).BeginInit();
            this.SuspendLayout();
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackColor = System.Drawing.Color.Silver;
            this.btnPrevious.Location = new System.Drawing.Point(222, 59);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(106, 24);
            this.btnPrevious.TabIndex = 5;
            this.btnPrevious.Text = "上一步";
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(16, 345);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(731, 16);
            this.progressBar.TabIndex = 6;
            // 
            // txtMessagebox
            // 
            this.txtMessagebox.Location = new System.Drawing.Point(16, 462);
            this.txtMessagebox.Multiline = true;
            this.txtMessagebox.Name = "txtMessagebox";
            this.txtMessagebox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMessagebox.Size = new System.Drawing.Size(731, 101);
            this.txtMessagebox.TabIndex = 7;
            this.txtMessagebox.DoubleClick += new System.EventHandler(this.txtMessagebox_DoubleClick);
            // 
            // Txtid
            // 
            this.Txtid.Location = new System.Drawing.Point(133, 10);
            this.Txtid.Name = "Txtid";
            this.Txtid.Size = new System.Drawing.Size(195, 21);
            this.Txtid.TabIndex = 9;
            this.Txtid.Text = "处理id";
            // 
            // txtStartDate
            // 
            this.txtStartDate.Location = new System.Drawing.Point(73, 55);
            this.txtStartDate.Name = "txtStartDate";
            this.txtStartDate.Size = new System.Drawing.Size(100, 21);
            this.txtStartDate.TabIndex = 10;
            this.txtStartDate.Text = "2013-03-01";
            // 
            // dataGridEmployees
            // 
            this.dataGridEmployees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridEmployees.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSelect});
            this.dataGridEmployees.Location = new System.Drawing.Point(16, 108);
            this.dataGridEmployees.Name = "dataGridEmployees";
            this.dataGridEmployees.RowTemplate.Height = 23;
            this.dataGridEmployees.Size = new System.Drawing.Size(731, 348);
            this.dataGridEmployees.TabIndex = 13;
            // 
            // ColumnSelect
            // 
            this.ColumnSelect.HeaderText = "选择";
            this.ColumnSelect.Name = "ColumnSelect";
            // 
            // btnSelect
            // 
            this.btnSelect.BackColor = System.Drawing.Color.Silver;
            this.btnSelect.Location = new System.Drawing.Point(363, 60);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(145, 23);
            this.btnSelect.TabIndex = 4;
            this.btnSelect.Text = "开始结算员工考勤";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtEmployeeName
            // 
            this.txtEmployeeName.Location = new System.Drawing.Point(16, 12);
            this.txtEmployeeName.Name = "txtEmployeeName";
            this.txtEmployeeName.Size = new System.Drawing.Size(100, 21);
            this.txtEmployeeName.TabIndex = 14;
            this.txtEmployeeName.Text = "员工姓名";
            // 
            // txtCompanyid
            // 
            this.txtCompanyid.Location = new System.Drawing.Point(344, 10);
            this.txtCompanyid.Name = "txtCompanyid";
            this.txtCompanyid.Size = new System.Drawing.Size(233, 21);
            this.txtCompanyid.TabIndex = 16;
            this.txtCompanyid.Text = "公司id";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "结算年月";
            // 
            // btnSelectBalance
            // 
            this.btnSelectBalance.BackColor = System.Drawing.Color.Silver;
            this.btnSelectBalance.Location = new System.Drawing.Point(588, 10);
            this.btnSelectBalance.Name = "btnSelectBalance";
            this.btnSelectBalance.Size = new System.Drawing.Size(159, 23);
            this.btnSelectBalance.TabIndex = 4;
            this.btnSelectBalance.Text = "查询员工结算记录";
            this.btnSelectBalance.UseVisualStyleBackColor = false;
            this.btnSelectBalance.Click += new System.EventHandler(this.btnSelectBalance_Click);
            // 
            // btnGenerateSalary
            // 
            this.btnGenerateSalary.BackColor = System.Drawing.Color.Silver;
            this.btnGenerateSalary.Location = new System.Drawing.Point(588, 60);
            this.btnGenerateSalary.Name = "btnGenerateSalary";
            this.btnGenerateSalary.Size = new System.Drawing.Size(159, 23);
            this.btnGenerateSalary.TabIndex = 4;
            this.btnGenerateSalary.Text = "开始结算员工薪资";
            this.btnGenerateSalary.UseVisualStyleBackColor = false;
            this.btnGenerateSalary.Click += new System.EventHandler(this.btnGenerateSalary_Click);
            // 
            // AttendEmploeeBalance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.ClientSize = new System.Drawing.Size(772, 575);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCompanyid);
            this.Controls.Add(this.txtEmployeeName);
            this.Controls.Add(this.dataGridEmployees);
            this.Controls.Add(this.txtStartDate);
            this.Controls.Add(this.Txtid);
            this.Controls.Add(this.txtMessagebox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.btnSelectBalance);
            this.Controls.Add(this.btnGenerateSalary);
            this.Controls.Add(this.btnSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AttendEmploeeBalance";
            this.Text = "欢迎使用协同办公服务器安装";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AttendEmploeeBalance_FormClosed);
            this.Load += new System.EventHandler(this.AttendEmploeeBalance_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEmployees)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtMessagebox;
        private System.Windows.Forms.TextBox Txtid;
        private System.Windows.Forms.TextBox txtStartDate;
        private System.Windows.Forms.DataGridView dataGridEmployees;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtEmployeeName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelect;
        private System.Windows.Forms.TextBox txtCompanyid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectBalance;
        private System.Windows.Forms.Button btnGenerateSalary;
    }
}

