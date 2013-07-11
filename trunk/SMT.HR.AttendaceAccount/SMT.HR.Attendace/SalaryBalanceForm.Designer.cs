namespace SmtPortalSetUp
{
    partial class SalaryBalanceForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SalaryBalanceForm));
            this.btnPrevious = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.txtMessagebox = new System.Windows.Forms.TextBox();
            this.TxtEmployeeid = new System.Windows.Forms.TextBox();
            this.txtStartDate = new System.Windows.Forms.TextBox();
            this.dtSalaryRecord = new System.Windows.Forms.DataGridView();
            this.ColumnSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnDelSalary = new System.Windows.Forms.DataGridViewButtonColumn();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtEmployeeName = new System.Windows.Forms.TextBox();
            this.txtCompanyid = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtOrgType = new System.Windows.Forms.TextBox();
            this.TxtGenerID = new System.Windows.Forms.TextBox();
            this.btnGeneryFunds = new System.Windows.Forms.Button();
            this.txtGenerateType = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnGetEmployeeId = new System.Windows.Forms.Button();
            this.btnGetSalaryRecordByEmployee = new System.Windows.Forms.Button();
            this.btnGetSalaryRecord = new System.Windows.Forms.Button();
            this.txtGenerEmployeeid = new System.Windows.Forms.TextBox();
            this.txtGenEmployeeName = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAttendCompanyName = new System.Windows.Forms.TextBox();
            this.txtAttendCompany = new System.Windows.Forms.TextBox();
            this.txtPayCompanyName = new System.Windows.Forms.TextBox();
            this.txtPayCompany = new System.Windows.Forms.TextBox();
            this.txtBalanceEmployeeName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBlancePostName = new System.Windows.Forms.TextBox();
            this.txBalancePostid = new System.Windows.Forms.TextBox();
            this.dtSalaryAchive = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnUpdateBlancePost = new System.Windows.Forms.DataGridViewButtonColumn();
            this.btnCompanyCheck = new System.Windows.Forms.Button();
            this.btnGetBalancePost = new System.Windows.Forms.Button();
            this.btnGetSalaryAchive = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dtPentionRecord = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnGetPentionMaster = new System.Windows.Forms.Button();
            this.btnInitSalaryItem = new System.Windows.Forms.Button();
            this.txtSalaryItemCompanyid = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.textSalaryItemCompany = new System.Windows.Forms.TextBox();
            this.btnGetCompanyid = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dtSalaryRecord)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtSalaryAchive)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtPentionRecord)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackColor = System.Drawing.Color.Silver;
            this.btnPrevious.Location = new System.Drawing.Point(579, 5);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(118, 33);
            this.btnPrevious.TabIndex = 5;
            this.btnPrevious.Text = "上一步";
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // txtMessagebox
            // 
            this.txtMessagebox.Location = new System.Drawing.Point(16, 440);
            this.txtMessagebox.Multiline = true;
            this.txtMessagebox.Name = "txtMessagebox";
            this.txtMessagebox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMessagebox.Size = new System.Drawing.Size(1106, 86);
            this.txtMessagebox.TabIndex = 7;
            this.txtMessagebox.DoubleClick += new System.EventHandler(this.txtMessagebox_DoubleClick);
            // 
            // TxtEmployeeid
            // 
            this.TxtEmployeeid.Location = new System.Drawing.Point(133, 10);
            this.TxtEmployeeid.Name = "TxtEmployeeid";
            this.TxtEmployeeid.Size = new System.Drawing.Size(195, 21);
            this.TxtEmployeeid.TabIndex = 9;
            this.TxtEmployeeid.Text = "处理id";
            // 
            // txtStartDate
            // 
            this.txtStartDate.Location = new System.Drawing.Point(87, 43);
            this.txtStartDate.Name = "txtStartDate";
            this.txtStartDate.Size = new System.Drawing.Size(100, 21);
            this.txtStartDate.TabIndex = 10;
            this.txtStartDate.Text = "2013-03-01";
            // 
            // dtSalaryRecord
            // 
            this.dtSalaryRecord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtSalaryRecord.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSelect,
            this.ColumnDelSalary});
            this.dtSalaryRecord.Location = new System.Drawing.Point(-12, 43);
            this.dtSalaryRecord.Name = "dtSalaryRecord";
            this.dtSalaryRecord.RowTemplate.Height = 23;
            this.dtSalaryRecord.Size = new System.Drawing.Size(719, 289);
            this.dtSalaryRecord.TabIndex = 13;
            this.dtSalaryRecord.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dtSalaryRecord_CellContentClick);
            // 
            // ColumnSelect
            // 
            this.ColumnSelect.HeaderText = "选择";
            this.ColumnSelect.Name = "ColumnSelect";
            // 
            // ColumnDelSalary
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.NullValue = "删除";
            this.ColumnDelSalary.DefaultCellStyle = dataGridViewCellStyle1;
            this.ColumnDelSalary.HeaderText = "删除";
            this.ColumnDelSalary.Name = "ColumnDelSalary";
            // 
            // btnSelect
            // 
            this.btnSelect.BackColor = System.Drawing.Color.Silver;
            this.btnSelect.Location = new System.Drawing.Point(266, 6);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(130, 31);
            this.btnSelect.TabIndex = 4;
            this.btnSelect.Text = "开始结算";
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
            this.label1.Location = new System.Drawing.Point(14, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "结算年月";
            // 
            // txtOrgType
            // 
            this.txtOrgType.Location = new System.Drawing.Point(193, 43);
            this.txtOrgType.Name = "txtOrgType";
            this.txtOrgType.Size = new System.Drawing.Size(100, 21);
            this.txtOrgType.TabIndex = 10;
            this.txtOrgType.Text = "0公司，4员工";
            // 
            // TxtGenerID
            // 
            this.TxtGenerID.Location = new System.Drawing.Point(312, 43);
            this.TxtGenerID.Name = "TxtGenerID";
            this.TxtGenerID.Size = new System.Drawing.Size(100, 21);
            this.TxtGenerID.TabIndex = 10;
            this.TxtGenerID.Text = "结算对象ID";
            // 
            // btnGeneryFunds
            // 
            this.btnGeneryFunds.BackColor = System.Drawing.Color.Silver;
            this.btnGeneryFunds.Location = new System.Drawing.Point(431, 6);
            this.btnGeneryFunds.Name = "btnGeneryFunds";
            this.btnGeneryFunds.Size = new System.Drawing.Size(130, 33);
            this.btnGeneryFunds.TabIndex = 5;
            this.btnGeneryFunds.Text = "生成活动经费";
            this.btnGeneryFunds.UseVisualStyleBackColor = false;
            this.btnGeneryFunds.Click += new System.EventHandler(this.btnGeneryFunds_Click);
            // 
            // txtGenerateType
            // 
            this.txtGenerateType.Location = new System.Drawing.Point(444, 43);
            this.txtGenerateType.Name = "txtGenerateType";
            this.txtGenerateType.Size = new System.Drawing.Size(303, 21);
            this.txtGenerateType.TabIndex = 10;
            this.txtGenerateType.Text = "结算类型 0:发薪机构,1:组织架构,2:离职薪资,3:结算岗位薪资";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 70);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1114, 364);
            this.tabControl1.TabIndex = 18;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dtSalaryRecord);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.btnGetEmployeeId);
            this.tabPage1.Controls.Add(this.btnGetSalaryRecordByEmployee);
            this.tabPage1.Controls.Add(this.btnGetSalaryRecord);
            this.tabPage1.Controls.Add(this.btnSelect);
            this.tabPage1.Controls.Add(this.btnGeneryFunds);
            this.tabPage1.Controls.Add(this.txtGenerEmployeeid);
            this.tabPage1.Controls.Add(this.txtGenEmployeeName);
            this.tabPage1.Controls.Add(this.btnPrevious);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1106, 338);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "结算薪资";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(765, 55);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 17;
            this.label9.Text = "结算人Id";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(765, 55);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 17;
            this.label8.Text = "结算人";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(765, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 17;
            this.label7.Text = "结算人";
            // 
            // btnGetEmployeeId
            // 
            this.btnGetEmployeeId.BackColor = System.Drawing.Color.Silver;
            this.btnGetEmployeeId.Location = new System.Drawing.Point(929, 73);
            this.btnGetEmployeeId.Name = "btnGetEmployeeId";
            this.btnGetEmployeeId.Size = new System.Drawing.Size(171, 31);
            this.btnGetEmployeeId.TabIndex = 4;
            this.btnGetEmployeeId.Text = "获取员工id";
            this.btnGetEmployeeId.UseVisualStyleBackColor = false;
            this.btnGetEmployeeId.Click += new System.EventHandler(this.btnGetEmployeeId_Click_1);
            // 
            // btnGetSalaryRecordByEmployee
            // 
            this.btnGetSalaryRecordByEmployee.BackColor = System.Drawing.Color.Silver;
            this.btnGetSalaryRecordByEmployee.Location = new System.Drawing.Point(929, 130);
            this.btnGetSalaryRecordByEmployee.Name = "btnGetSalaryRecordByEmployee";
            this.btnGetSalaryRecordByEmployee.Size = new System.Drawing.Size(171, 31);
            this.btnGetSalaryRecordByEmployee.TabIndex = 4;
            this.btnGetSalaryRecordByEmployee.Text = "查询结算人结算结果";
            this.btnGetSalaryRecordByEmployee.UseVisualStyleBackColor = false;
            this.btnGetSalaryRecordByEmployee.Click += new System.EventHandler(this.btnGetSalaryRecordByEmployee_Click);
            // 
            // btnGetSalaryRecord
            // 
            this.btnGetSalaryRecord.BackColor = System.Drawing.Color.Silver;
            this.btnGetSalaryRecord.Location = new System.Drawing.Point(18, 7);
            this.btnGetSalaryRecord.Name = "btnGetSalaryRecord";
            this.btnGetSalaryRecord.Size = new System.Drawing.Size(130, 31);
            this.btnGetSalaryRecord.TabIndex = 4;
            this.btnGetSalaryRecord.Text = "查询结果";
            this.btnGetSalaryRecord.UseVisualStyleBackColor = false;
            this.btnGetSalaryRecord.Click += new System.EventHandler(this.btnGetSalaryRecord_Click);
            // 
            // txtGenerEmployeeid
            // 
            this.txtGenerEmployeeid.Location = new System.Drawing.Point(831, 46);
            this.txtGenerEmployeeid.Name = "txtGenerEmployeeid";
            this.txtGenerEmployeeid.Size = new System.Drawing.Size(269, 21);
            this.txtGenerEmployeeid.TabIndex = 10;
            // 
            // txtGenEmployeeName
            // 
            this.txtGenEmployeeName.Location = new System.Drawing.Point(831, 13);
            this.txtGenEmployeeName.Name = "txtGenEmployeeName";
            this.txtGenEmployeeName.Size = new System.Drawing.Size(269, 21);
            this.txtGenEmployeeName.TabIndex = 10;
            this.txtGenEmployeeName.Text = "郭雪梅";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.txtAttendCompanyName);
            this.tabPage2.Controls.Add(this.txtAttendCompany);
            this.tabPage2.Controls.Add(this.txtPayCompanyName);
            this.tabPage2.Controls.Add(this.txtPayCompany);
            this.tabPage2.Controls.Add(this.txtBalanceEmployeeName);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.txtBlancePostName);
            this.tabPage2.Controls.Add(this.txBalancePostid);
            this.tabPage2.Controls.Add(this.dtSalaryAchive);
            this.tabPage2.Controls.Add(this.btnCompanyCheck);
            this.tabPage2.Controls.Add(this.btnGetBalancePost);
            this.tabPage2.Controls.Add(this.btnGetSalaryAchive);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1106, 338);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "查询薪资档案";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 21;
            this.label6.Text = "考勤机构";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 87);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 21;
            this.label5.Text = "发薪机构";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(174, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 21;
            this.label4.Text = "结算人";
            // 
            // txtAttendCompanyName
            // 
            this.txtAttendCompanyName.Location = new System.Drawing.Point(68, 114);
            this.txtAttendCompanyName.Name = "txtAttendCompanyName";
            this.txtAttendCompanyName.Size = new System.Drawing.Size(259, 21);
            this.txtAttendCompanyName.TabIndex = 20;
            // 
            // txtAttendCompany
            // 
            this.txtAttendCompany.Location = new System.Drawing.Point(360, 114);
            this.txtAttendCompany.Name = "txtAttendCompany";
            this.txtAttendCompany.Size = new System.Drawing.Size(327, 21);
            this.txtAttendCompany.TabIndex = 20;
            // 
            // txtPayCompanyName
            // 
            this.txtPayCompanyName.Location = new System.Drawing.Point(68, 87);
            this.txtPayCompanyName.Name = "txtPayCompanyName";
            this.txtPayCompanyName.Size = new System.Drawing.Size(259, 21);
            this.txtPayCompanyName.TabIndex = 20;
            // 
            // txtPayCompany
            // 
            this.txtPayCompany.Location = new System.Drawing.Point(360, 87);
            this.txtPayCompany.Name = "txtPayCompany";
            this.txtPayCompany.Size = new System.Drawing.Size(327, 21);
            this.txtPayCompany.TabIndex = 20;
            // 
            // txtBalanceEmployeeName
            // 
            this.txtBalanceEmployeeName.Location = new System.Drawing.Point(257, 3);
            this.txtBalanceEmployeeName.Name = "txtBalanceEmployeeName";
            this.txtBalanceEmployeeName.Size = new System.Drawing.Size(259, 21);
            this.txtBalanceEmployeeName.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(174, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 19;
            this.label3.Text = "结算岗位名称";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 19;
            this.label2.Text = "结算岗位id";
            // 
            // txtBlancePostName
            // 
            this.txtBlancePostName.Location = new System.Drawing.Point(257, 57);
            this.txtBlancePostName.Name = "txtBlancePostName";
            this.txtBlancePostName.Size = new System.Drawing.Size(464, 21);
            this.txtBlancePostName.TabIndex = 18;
            // 
            // txBalancePostid
            // 
            this.txBalancePostid.Location = new System.Drawing.Point(257, 30);
            this.txBalancePostid.Name = "txBalancePostid";
            this.txBalancePostid.Size = new System.Drawing.Size(259, 21);
            this.txBalancePostid.TabIndex = 18;
            // 
            // dtSalaryAchive
            // 
            this.dtSalaryAchive.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtSalaryAchive.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1,
            this.ColumnUpdateBlancePost});
            this.dtSalaryAchive.Location = new System.Drawing.Point(6, 196);
            this.dtSalaryAchive.Name = "dtSalaryAchive";
            this.dtSalaryAchive.RowTemplate.Height = 23;
            this.dtSalaryAchive.Size = new System.Drawing.Size(715, 136);
            this.dtSalaryAchive.TabIndex = 17;
            this.dtSalaryAchive.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dtSalaryAchive_CellContentClick);
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.HeaderText = "选择";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            // 
            // ColumnUpdateBlancePost
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.NullValue = "更新结算岗位";
            this.ColumnUpdateBlancePost.DefaultCellStyle = dataGridViewCellStyle2;
            this.ColumnUpdateBlancePost.HeaderText = "更新结算岗位";
            this.ColumnUpdateBlancePost.Name = "ColumnUpdateBlancePost";
            // 
            // btnCompanyCheck
            // 
            this.btnCompanyCheck.BackColor = System.Drawing.Color.Silver;
            this.btnCompanyCheck.Location = new System.Drawing.Point(11, 141);
            this.btnCompanyCheck.Name = "btnCompanyCheck";
            this.btnCompanyCheck.Size = new System.Drawing.Size(180, 22);
            this.btnCompanyCheck.TabIndex = 14;
            this.btnCompanyCheck.Text = "验证发薪考勤机构";
            this.btnCompanyCheck.UseVisualStyleBackColor = false;
            this.btnCompanyCheck.Click += new System.EventHandler(this.btnCompanyCheck_Click);
            // 
            // btnGetBalancePost
            // 
            this.btnGetBalancePost.BackColor = System.Drawing.Color.Silver;
            this.btnGetBalancePost.Location = new System.Drawing.Point(541, 1);
            this.btnGetBalancePost.Name = "btnGetBalancePost";
            this.btnGetBalancePost.Size = new System.Drawing.Size(180, 22);
            this.btnGetBalancePost.TabIndex = 14;
            this.btnGetBalancePost.Text = "获取结算人主岗位";
            this.btnGetBalancePost.UseVisualStyleBackColor = false;
            this.btnGetBalancePost.Click += new System.EventHandler(this.btnGetBalancePost_Click);
            // 
            // btnGetSalaryAchive
            // 
            this.btnGetSalaryAchive.BackColor = System.Drawing.Color.Silver;
            this.btnGetSalaryAchive.Location = new System.Drawing.Point(6, 6);
            this.btnGetSalaryAchive.Name = "btnGetSalaryAchive";
            this.btnGetSalaryAchive.Size = new System.Drawing.Size(130, 31);
            this.btnGetSalaryAchive.TabIndex = 14;
            this.btnGetSalaryAchive.Text = "查询薪资档案";
            this.btnGetSalaryAchive.UseVisualStyleBackColor = false;
            this.btnGetSalaryAchive.Click += new System.EventHandler(this.btnGetSalaryAchive_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dtPentionRecord);
            this.tabPage3.Controls.Add(this.btnGetPentionMaster);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1106, 338);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "社保导入记录";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dtPentionRecord
            // 
            this.dtPentionRecord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtPentionRecord.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn2});
            this.dtPentionRecord.Location = new System.Drawing.Point(6, 43);
            this.dtPentionRecord.Name = "dtPentionRecord";
            this.dtPentionRecord.RowTemplate.Height = 23;
            this.dtPentionRecord.Size = new System.Drawing.Size(715, 289);
            this.dtPentionRecord.TabIndex = 19;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.HeaderText = "选择";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            // 
            // btnGetPentionMaster
            // 
            this.btnGetPentionMaster.BackColor = System.Drawing.Color.Silver;
            this.btnGetPentionMaster.Location = new System.Drawing.Point(6, 6);
            this.btnGetPentionMaster.Name = "btnGetPentionMaster";
            this.btnGetPentionMaster.Size = new System.Drawing.Size(130, 31);
            this.btnGetPentionMaster.TabIndex = 18;
            this.btnGetPentionMaster.Text = "查询社保记录";
            this.btnGetPentionMaster.UseVisualStyleBackColor = false;
            this.btnGetPentionMaster.Click += new System.EventHandler(this.btnGetPentionMaster_Click);
            // 
            // btnInitSalaryItem
            // 
            this.btnInitSalaryItem.BackColor = System.Drawing.Color.Silver;
            this.btnInitSalaryItem.Location = new System.Drawing.Point(453, 91);
            this.btnInitSalaryItem.Name = "btnInitSalaryItem";
            this.btnInitSalaryItem.Size = new System.Drawing.Size(130, 31);
            this.btnInitSalaryItem.TabIndex = 4;
            this.btnInitSalaryItem.Text = "初始化薪资项目";
            this.btnInitSalaryItem.UseVisualStyleBackColor = false;
            this.btnInitSalaryItem.Click += new System.EventHandler(this.btnInitSalaryItem_Click);
            // 
            // txtSalaryItemCompanyid
            // 
            this.txtSalaryItemCompanyid.Location = new System.Drawing.Point(620, 11);
            this.txtSalaryItemCompanyid.Name = "txtSalaryItemCompanyid";
            this.txtSalaryItemCompanyid.Size = new System.Drawing.Size(233, 21);
            this.txtSalaryItemCompanyid.TabIndex = 16;
            this.txtSalaryItemCompanyid.Text = "公司id";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.textSalaryItemCompany);
            this.tabPage4.Controls.Add(this.btnGetCompanyid);
            this.tabPage4.Controls.Add(this.btnInitSalaryItem);
            this.tabPage4.Controls.Add(this.txtSalaryItemCompanyid);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1106, 338);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "初始化薪资项目";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 20);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(101, 12);
            this.label10.TabIndex = 19;
            this.label10.Text = "薪资项目来源公司";
            // 
            // textSalaryItemCompany
            // 
            this.textSalaryItemCompany.Location = new System.Drawing.Point(143, 17);
            this.textSalaryItemCompany.Name = "textSalaryItemCompany";
            this.textSalaryItemCompany.Size = new System.Drawing.Size(269, 21);
            this.textSalaryItemCompany.TabIndex = 18;
            this.textSalaryItemCompany.Text = "集团本部";
            // 
            // btnGetCompanyid
            // 
            this.btnGetCompanyid.BackColor = System.Drawing.Color.Silver;
            this.btnGetCompanyid.Location = new System.Drawing.Point(453, 11);
            this.btnGetCompanyid.Name = "btnGetCompanyid";
            this.btnGetCompanyid.Size = new System.Drawing.Size(130, 31);
            this.btnGetCompanyid.TabIndex = 4;
            this.btnGetCompanyid.Text = "获取公司id";
            this.btnGetCompanyid.UseVisualStyleBackColor = false;
            this.btnGetCompanyid.Click += new System.EventHandler(this.btnGetCompanyid_Click);
            // 
            // SalaryBalanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.ClientSize = new System.Drawing.Size(1150, 554);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCompanyid);
            this.Controls.Add(this.txtEmployeeName);
            this.Controls.Add(this.txtGenerateType);
            this.Controls.Add(this.TxtGenerID);
            this.Controls.Add(this.txtOrgType);
            this.Controls.Add(this.txtStartDate);
            this.Controls.Add(this.TxtEmployeeid);
            this.Controls.Add(this.txtMessagebox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SalaryBalanceForm";
            this.Text = "欢迎使用协同办公服务器安装";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SalaryBalanceForm_FormClosed);
            this.Load += new System.EventHandler(this.AttendEmploeeBalance_Load);
            this.Shown += new System.EventHandler(this.SalaryBalanceForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dtSalaryRecord)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtSalaryAchive)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtPentionRecord)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox txtMessagebox;
        private System.Windows.Forms.TextBox TxtEmployeeid;
        private System.Windows.Forms.TextBox txtStartDate;
        private System.Windows.Forms.DataGridView dtSalaryRecord;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtEmployeeName;
        private System.Windows.Forms.TextBox txtCompanyid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtOrgType;
        private System.Windows.Forms.TextBox TxtGenerID;
        private System.Windows.Forms.Button btnGeneryFunds;
        private System.Windows.Forms.TextBox txtGenerateType;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnGetSalaryAchive;
        private System.Windows.Forms.DataGridView dtSalaryAchive;
        private System.Windows.Forms.Button btnGetSalaryRecord;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dtPentionRecord;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.Button btnGetPentionMaster;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnUpdateBlancePost;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBalanceEmployeeName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBlancePostName;
        private System.Windows.Forms.TextBox txBalancePostid;
        private System.Windows.Forms.Button btnGetBalancePost;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtAttendCompany;
        private System.Windows.Forms.TextBox txtPayCompany;
        private System.Windows.Forms.Button btnCompanyCheck;
        private System.Windows.Forms.TextBox txtAttendCompanyName;
        private System.Windows.Forms.TextBox txtPayCompanyName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelect;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnDelSalary;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnGetSalaryRecordByEmployee;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtGenerEmployeeid;
        private System.Windows.Forms.TextBox txtGenEmployeeName;
        private System.Windows.Forms.Button btnGetEmployeeId;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textSalaryItemCompany;
        private System.Windows.Forms.Button btnGetCompanyid;
        private System.Windows.Forms.Button btnInitSalaryItem;
        private System.Windows.Forms.TextBox txtSalaryItemCompanyid;
    }
}

