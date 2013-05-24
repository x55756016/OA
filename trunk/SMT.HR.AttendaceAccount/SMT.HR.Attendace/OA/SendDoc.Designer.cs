namespace SmtPortalSetUp
{
    partial class SendDocForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendDocForm));
            this.btnPrevious = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtMessagebox = new System.Windows.Forms.TextBox();
            this.TxtSendDocid = new System.Windows.Forms.TextBox();
            this.txtStartDate = new System.Windows.Forms.TextBox();
            this.dataGridSendDoc = new System.Windows.Forms.DataGridView();
            this.ColumnSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnSendDoc = new System.Windows.Forms.DataGridViewButtonColumn();
            this.btnSendAll = new System.Windows.Forms.Button();
            this.txtDocName = new System.Windows.Forms.TextBox();
            this.txtCompanyid = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSearchDoc = new System.Windows.Forms.Button();
            this.btnGenerateSalary = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridSendDoc)).BeginInit();
            this.SuspendLayout();
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackColor = System.Drawing.Color.Silver;
            this.btnPrevious.Location = new System.Drawing.Point(200, 46);
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
            // TxtSendDocid
            // 
            this.TxtSendDocid.Location = new System.Drawing.Point(325, 10);
            this.TxtSendDocid.Name = "TxtSendDocid";
            this.TxtSendDocid.Size = new System.Drawing.Size(257, 21);
            this.TxtSendDocid.TabIndex = 9;
            this.TxtSendDocid.Text = "公司发文id";
            // 
            // txtStartDate
            // 
            this.txtStartDate.Location = new System.Drawing.Point(73, 55);
            this.txtStartDate.Name = "txtStartDate";
            this.txtStartDate.Size = new System.Drawing.Size(100, 21);
            this.txtStartDate.TabIndex = 10;
            this.txtStartDate.Text = "2013-03-01";
            // 
            // dataGridSendDoc
            // 
            this.dataGridSendDoc.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridSendDoc.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSelect,
            this.ColumnSendDoc});
            this.dataGridSendDoc.Location = new System.Drawing.Point(16, 108);
            this.dataGridSendDoc.Name = "dataGridSendDoc";
            this.dataGridSendDoc.RowTemplate.Height = 23;
            this.dataGridSendDoc.Size = new System.Drawing.Size(731, 348);
            this.dataGridSendDoc.TabIndex = 13;
            this.dataGridSendDoc.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridSendDoc_CellContentClick);
            // 
            // ColumnSelect
            // 
            this.ColumnSelect.HeaderText = "选择";
            this.ColumnSelect.Name = "ColumnSelect";
            // 
            // ColumnSendDoc
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.NullValue = "发送给子孙公司";
            this.ColumnSendDoc.DefaultCellStyle = dataGridViewCellStyle1;
            this.ColumnSendDoc.HeaderText = "发送给子孙公司";
            this.ColumnSendDoc.Name = "ColumnSendDoc";
            // 
            // btnSendAll
            // 
            this.btnSendAll.BackColor = System.Drawing.Color.Silver;
            this.btnSendAll.Location = new System.Drawing.Point(343, 46);
            this.btnSendAll.Name = "btnSendAll";
            this.btnSendAll.Size = new System.Drawing.Size(199, 23);
            this.btnSendAll.TabIndex = 4;
            this.btnSendAll.Text = "发送给所有选中公司的子孙公司";
            this.btnSendAll.UseVisualStyleBackColor = false;
            this.btnSendAll.Click += new System.EventHandler(this.btnSendAll_Click);
            // 
            // txtDocName
            // 
            this.txtDocName.Location = new System.Drawing.Point(16, 12);
            this.txtDocName.Name = "txtDocName";
            this.txtDocName.Size = new System.Drawing.Size(290, 21);
            this.txtDocName.TabIndex = 14;
            this.txtDocName.Text = "公文名";
            // 
            // txtCompanyid
            // 
            this.txtCompanyid.Location = new System.Drawing.Point(16, 82);
            this.txtCompanyid.Name = "txtCompanyid";
            this.txtCompanyid.Size = new System.Drawing.Size(117, 21);
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
            // btnSearchDoc
            // 
            this.btnSearchDoc.BackColor = System.Drawing.Color.Silver;
            this.btnSearchDoc.Location = new System.Drawing.Point(588, 8);
            this.btnSearchDoc.Name = "btnSearchDoc";
            this.btnSearchDoc.Size = new System.Drawing.Size(159, 23);
            this.btnSearchDoc.TabIndex = 4;
            this.btnSearchDoc.Text = "查询";
            this.btnSearchDoc.UseVisualStyleBackColor = false;
            this.btnSearchDoc.Click += new System.EventHandler(this.btnSearchDoc_Click);
            // 
            // btnGenerateSalary
            // 
            this.btnGenerateSalary.BackColor = System.Drawing.Color.Silver;
            this.btnGenerateSalary.Location = new System.Drawing.Point(588, 46);
            this.btnGenerateSalary.Name = "btnGenerateSalary";
            this.btnGenerateSalary.Size = new System.Drawing.Size(159, 23);
            this.btnGenerateSalary.TabIndex = 4;
            this.btnGenerateSalary.Text = "开始结算员工薪资";
            this.btnGenerateSalary.UseVisualStyleBackColor = false;
            this.btnGenerateSalary.Click += new System.EventHandler(this.btnGenerateSalary_Click);
            // 
            // SendDocForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.ClientSize = new System.Drawing.Size(772, 575);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCompanyid);
            this.Controls.Add(this.txtDocName);
            this.Controls.Add(this.dataGridSendDoc);
            this.Controls.Add(this.txtStartDate);
            this.Controls.Add(this.TxtSendDocid);
            this.Controls.Add(this.txtMessagebox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.btnSearchDoc);
            this.Controls.Add(this.btnGenerateSalary);
            this.Controls.Add(this.btnSendAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SendDocForm";
            this.Text = "欢迎使用协同办公服务器安装";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AttendEmploeeBalance_FormClosed);
            this.Load += new System.EventHandler(this.AttendEmploeeBalance_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridSendDoc)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtMessagebox;
        private System.Windows.Forms.TextBox TxtSendDocid;
        private System.Windows.Forms.TextBox txtStartDate;
        private System.Windows.Forms.DataGridView dataGridSendDoc;
        private System.Windows.Forms.Button btnSendAll;
        private System.Windows.Forms.TextBox txtDocName;
        private System.Windows.Forms.TextBox txtCompanyid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSearchDoc;
        private System.Windows.Forms.Button btnGenerateSalary;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelect;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnSendDoc;
    }
}

