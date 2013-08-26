using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using AttendaceAccount;

namespace SmtPortalSetUp
{
    public partial class Form_Second : Form
    {

        private DataTable dtEmployee = new DataTable();
        public Form_Second()
        {
            InitializeComponent();
            GlobalParameters.fromSecond = this;
        }

        private void Form_Second_Load(object sender, EventArgs e)
        {
            DateTime dtstart = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);
            DateTime dtNext = DateTime.Now.AddMonths(1);
            DateTime dtEnd = new DateTime(dtNext.Year, dtNext.Month, 1).AddDays(-1);
            txtStartDate.Text = dtstart.ToString("yyyy-MM-dd");
            txtEndDate.Text = dtEnd.ToString("yyyy-MM-dd");
            GlobalParameters.StartDate = txtStartDate.Text;
            GlobalParameters.EndDate = txtEndDate.Text;
        }

        private void startCopy()
        {
           
            //Utility.DeleteFile(setUpPath);
            try
            {
                //Utility.CopyDirectory(SourcePath, setUpPath, replacePra,txtConnectString.Text);
                MessageBox.Show("安装完毕！");
                //Utility.EditDirectoryFiles(setUpPath, replacePra);
            }
            catch (Exception ex)
            {
                ShowMessage("安装文件异常：" + ex.ToString());
                ShowProgress(0);
                MessageBox.Show("安装文件异常，请查看错误日志！");

            }
        }

        delegate void DelShow(String Msg); //代理
        //将对控件的操作写到一个函数中 
        public void ShowMessage(String para)
        {
            if (!txtMessagebox.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                txtMessagebox.Text = DateTime.Now.ToLongTimeString() +" "+ para + System.Environment.NewLine + txtMessagebox.Text;
            }
            else //非创建线程，用代理进行操作
            {
                DelShow ds = new DelShow(ShowMessage);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }

        delegate void DelShowProgress(int Msg); //代理
        //将对控件的操作写到一个函数中 
        public void ShowProgress(int para)
        {
            if (!progressBar.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                progressBar.Value = para;
            }
            else //非创建线程，用代理进行操作
            {
                DelShowProgress ds = new DelShowProgress(ShowProgress);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }


        delegate void DelMaxProgress(int Msg); //代理
        //将对控件的操作写到一个函数中 
        public void SetProgressMaxValue(int para)
        {
            if (!progressBar.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                progressBar.Maximum = para;
            }
            else //非创建线程，用代理进行操作
            {
                DelMaxProgress ds = new DelMaxProgress(SetProgressMaxValue);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }

        private void txtMessagebox_DoubleClick(object sender, EventArgs e)
        {
            txtMessagebox.SelectAll();

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            GlobalParameters.employeeid = Txtid.Text;
            GlobalParameters.StartDate = txtStartDate.Text;
            GlobalParameters.EndDate = txtEndDate.Text;
            GlobalParameters.employeeMasterCompanyid = txtCompanyid.Text;
            GlobalParameters.employeeName = txtEmployeeName.Text;
            AttendEmploee form = new AttendEmploee();
            form.Show();
            this.Hide();

        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            string sql = @"select e.employeeename,
                       e.employeecname,
                       e.employeestate 员工状态,-- 0使用，1在职，2已离职，3离职中
                       --e.editstate 员工状态,
                       ep.isagency 岗位类型,--0主岗位，1兼职岗位
                       ep.editstate 岗位生效状态,--0失效，1生效,
                       ep.checkstate 岗位审核状态,--0未提交，1审核中，2审核通过，3审核不通过
                       ep.postlevel,
                       ep.postid,
                       ep.employeepostid 员工岗位主键,
                       pd.postname,
                       dp.departmentname,
                       c.cname,
                       c.briefname,
                       c.companyid
                  from smthrm.t_hr_employee e
                 inner join smthrm.t_hr_employeepost ep
                    on e.employeeid = ep.employeeid
                 inner join smthrm.t_hr_post p
                    on ep.postid = p.postid
                 inner join smthrm.t_hr_postdictionary pd
                    on p.postdictionaryid = pd.postdictionaryid
                 inner join smthrm.t_hr_company c
                    on p.companyid = c.companyid
                 inner join smthrm.t_hr_department d
                    on p.departmentid = d.departmentid
                 inner join smthrm.t_hr_departmentdictionary dp
                    on d.departmentdictionaryid = dp.departmentdictionaryid
                 where  
                 e.employeeid='" + Txtid.Text+ @"'
                 and ep.editstate=1
                 and ep.checkstate=2;
                ";


            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dataGridEmployees.DataSource = dt;
            OracleHelp.close();
        }

        private void btnGetEmployeeid_Click(object sender, EventArgs e)
        {
            string sql = @"select e.employeeename,
                               e.employeecname,
                               case when ep.isagency='0' then '主岗位' else '兼职岗位' end 岗位类型,
                               --ep.isagency 岗位类型 ,--0主岗位 1兼职岗位
                               case when ep.editstate='0' then '已失效' else '生效中' end 生效状态,
                               --ep.editstate 岗位生效状态,--0失效，1生效,
                               ep.checkstate 岗位审核状态,--0未提交，1审核中，2审核通过，3审核不通过
                               pd.postname,
                               dp.departmentname,
                               c.cname,
                               case
                                 when e.employeestate = '0' then '试用'
                                 when e.employeestate = '1' then '在职'
                                 when e.employeestate = '2' then '已离职'
                                 when e.employeestate = '3' then '离职中'
                               end 员工状态,-- 0使用，1在职，2已离职，3离职中
                               ep.postlevel 岗位级别,       
                               e.fingerprintid 指纹编码,
                               ep.employeepostid,  
                               e.employeeid,    
                               ep.postid,
                               d.departmentid,
                               c.companyid,
                               et.entrydate 入职日期,
                               et.checkstate 入职业审核状态
                          from smthrm.t_hr_employee e
                         inner join smthrm.t_hr_employeepost ep
                            on e.employeeid = ep.employeeid
                         inner join smthrm.t_hr_employeeentry et
                            on e.employeeid=et.employeeid
                         inner join smthrm.t_hr_post p
                            on ep.postid = p.postid
                         inner join smthrm.t_hr_postdictionary pd
                            on p.postdictionaryid = pd.postdictionaryid
                         inner join smthrm.t_hr_company c
                            on p.companyid = c.companyid
                         inner join smthrm.t_hr_department d
                            on p.departmentid = d.departmentid
                         inner join smthrm.t_hr_departmentdictionary dp
                            on d.departmentdictionaryid = dp.departmentdictionaryid
                         where  
                         e.employeecname like '%" + txtEmployeeName.Text +@"%'
                         --and ep.editstate=1
                         and ep.checkstate=2
                         order by ep.isagency,ep.editstate desc ";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dataGridEmployees.DataSource = dt;
            OracleHelp.close();
            txtMessagebox.Text="获取员工信息完成！" ;
        }

        #region 员工基本信息
        private string GetEmployeeBasicInfo()
        {
            string str = @"";
            return str;
        }

        #endregion

        #region 员工入职信息


        private string GetEmployeeEntry(string employeeid)
        {
            string str = @"select e.employeecname,
                                   et.employeeentryid,
                                   et.entrydate,
                                   pd.postname,
                                   ep.postlevel,
                                   et.checkstate,
                                   et.remark 入职备注,
                                   et.editstate,
                                   et.employeepostid
                              from smthrm.t_hr_employeeentry et
                             inner join smthrm.t_hr_employee e
                                on et.employeeid = e.employeeid
                             left join smthrm.t_hr_employeepost ep
                                on et.employeepostid = ep.employeepostid
                             left join smthrm.t_hr_post p
                                on ep.postid = p.postid
                             left join smthrm.t_hr_postdictionary pd
                                on p.postdictionaryid = pd.postdictionaryid
                                 where e.employeeid = '" + employeeid + "'";
            return str;
        }

        #endregion

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Form_First form = GlobalParameters.fromFisrt;
            form.Show();
            this.Hide();
        }

        private void dataGridEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = dataGridEmployees.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "ColumnDealPost")
                    {
                        //这里可以编写你需要的任意关于按钮事件的操作~
                        string isAgencePost = "0";
                        string isActivedPost = "0";

                        string strEmployeePostid = dataGridEmployees.Rows[e.RowIndex].Cells["employeepostid"].EditedFormattedValue.ToString(); ;
                        string strIsAgence = dataGridEmployees.Rows[e.RowIndex].Cells["ColumnIsAgence"].EditedFormattedValue.ToString();
                        string strIsActived = dataGridEmployees.Rows[e.RowIndex].Cells["ColumnIsActived"].EditedFormattedValue.ToString();

                        if (strIsAgence == "True")
                        {
                            isAgencePost = "1";
                        }
                        if (strIsActived == "True")
                        {
                            isActivedPost = "1";
                        }
                        UpdatePost(isAgencePost, isActivedPost, strEmployeePostid);
                    } if (column.Name == "ColumnUpEmplooState")
                    {

                    }
                }
            }
            for (int i = 0; i < dataGridEmployees.RowCount; i++)
            {
                string selectValue = dataGridEmployees.Rows[i].Cells["ColumnSelect"].EditedFormattedValue.ToString();
                if (selectValue == "True")
                {
                    Txtid.Text = dataGridEmployees.Rows[i].Cells["employeeid"].EditedFormattedValue.ToString();
                    txtCompanyid.Text = dataGridEmployees.Rows[i].Cells["companyid"].EditedFormattedValue.ToString();
                    txtEmployeeName.Text = dataGridEmployees.Rows[i].Cells["employeecname"].EditedFormattedValue.ToString();
                    
                    GlobalParameters.employeeid = Txtid.Text;
                    GlobalParameters.employeeName = txtEmployeeName.Text;
                    GlobalParameters.employeeMasterPostid = dataGridEmployees.Rows[i].Cells["postid"].EditedFormattedValue.ToString();
                    GlobalParameters.employeeMasterDepartmentid = dataGridEmployees.Rows[i].Cells["departmentid"].EditedFormattedValue.ToString();
                    GlobalParameters.employeeMasterCompanyid = dataGridEmployees.Rows[i].Cells["companyid"].EditedFormattedValue.ToString();

                    OracleHelp.Connect();
                    DataTable dt = OracleHelp.getTable(GetEmployeeEntry(Txtid.Text));
                    dtEmployeeEntry.DataSource = dt;
                    OracleHelp.close();
                    txtMessagebox.Text = txtMessagebox.Text+System.Environment.NewLine+"获取员工入职信息完成！";
                }
            }
        }

        private void UpdatePost(string isAgence,string isActived,string employeePostId)
        {
            DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("是否确认更新此员工岗位？",//对话框的显示内容 

            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                string sql = @"update smthrm.t_hr_employeepost t set t.isagency='"+isAgence
                    + @"',t.editstate='" + isActived + @"'
                where t.employeepostid='" + employeePostId + "'";
                OracleHelp.Connect();
                int i = OracleHelp.Excute(sql);
                txtMessagebox.Text = "处理完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
                OracleHelp.close();
            }
        }
        private void UpdateEmployeeState(string employeeId)
        {
            DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("是否更新此员工状态为在职？",//对话框的显示内容 

            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                string sql = @"update smthrm.t_hr_employee t set t.employeestate='1'
                where t.employeeid='" + GlobalParameters.employeeid + "'";
                OracleHelp.Connect();
                int i = OracleHelp.Excute(sql);
                txtMessagebox.Text = "更新员工状态完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
                OracleHelp.close();
            }
        }

        private void btnGetPassword_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://portal.smt-online.net/new/services/pm/");
        }

        private void btnGenerateSalary_Click(object sender, EventArgs e)
        {
            //GlobalParameters.employeeid = Txtid.Text;
            //GlobalParameters.StartDate = txtStartDate.Text;
            //GlobalParameters.EndDate = txtEndDate.Text;
            //GlobalParameters.employeeMasterCompanyid = txtCompanyid.Text;
            //GlobalParameters.employeeName = txtEmployeeName.Text;
            SalaryBalanceForm form = GlobalParameters.salaryBalanceForm;
            if (form == null) form = new SalaryBalanceForm();
            form.Show();
            this.Hide();
        }

        private void Form_Second_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        #region 查询社保信息
        private void btnSelectPention_Click(object sender, EventArgs e)
        {
            string sql = @"select s.SOCIALSERVICEYEAR 社保缴纳起始时间
                            ,e.idnumber 身份证号, s.cardid  社保卡号,s.computerno 电脑号,s.startdate,s.checkstate,
                            s.createdate,s.updatedate from smthrm.t_hr_employee e
                            inner join smthrm.t_hr_pensionmaster s on e.employeeid=s.employeeid
                            where e.employeeid='" + GlobalParameters.employeeid + @"'
                            order by s.updatedate
                            ";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtPention.DataSource = dt;
            OracleHelp.close();
            txtMessagebox.Text = "获取员工社保信息完成！";
        }
        #endregion

        private void btnPreviousMonth_Click(object sender, EventArgs e)
        {
            DateTime dtstart = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
            DateTime dtPrivious = DateTime.Now;
            DateTime dtEnd = new DateTime(dtPrivious.Year, dtPrivious.Month, 1).AddDays(-1);
            txtStartDate.Text = dtstart.ToString("yyyy-MM-dd");
            txtEndDate.Text = dtEnd.ToString("yyyy-MM-dd");

            GlobalParameters.StartDate = dtstart.ToString("yyyy-MM-dd");
            GlobalParameters.EndDate = dtEnd.ToString("yyyy-MM-dd");
        }

        private void btnImportPunchRecord_Click(object sender, EventArgs e)
        {
            AttendLogForm form = GlobalParameters.attendLogForm;
            if (form == null) form = new AttendLogForm();
            form.Show();
            this.Hide();
        }

        //发布公文
        private void btnSendDocument_Click(object sender, EventArgs e)
        {
            Form form = GlobalParameters.sendDocForm; ;
            if (form == null) form = new SendDocForm();
            form.Show();
            this.Hide();
        }

        #region  出差相关查询
        private void btnGetTraveSolution_Click(object sender, EventArgs e)
        {
            string sql = @"--查询指定公司应用的考勤方案
                select a.companyid,b.travelsolutionsid,b.programmename from smtoa.T_OA_PROGRAMAPPLICATIONS a--出差方案应用
                inner join smtoa.T_OA_TRAVELSOLUTIONS b --出差方案
                on a.travelsolutionsid=b.travelsolutionsid
                inner join smthrm.t_hr_company c on c.companyid=a.companyid
                where c.companyid='" + txtCompanyid.Text + "'";

            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtTravSolution.DataSource = dt;
            OracleHelp.close();
            if (dt.Rows.Count > 0)
            {
                txtTraveSolutionid.Text = dt.Rows[0]["travelsolutionsid"].ToString();
            }
        }
        #endregion

        private void btnGetTravlTools_Click(object sender, EventArgs e)
        {
            string sql = @"--查询可乘坐的交通级别
                    select t.endpostlevel 岗位级别,t.typeoftraveltools 交通工具类型,
                    d.dictionaryname,t.TAKETHETOOLLEVEL 交通工具级别,
                    dl.dictionaryname 
                    from smtoa.T_OA_TAKETHESTANDARDTRANSPORT t
                    inner join smtsystem.t_sys_dictionary d on t.typeoftraveltools = d.dictionaryvalue and d.dictioncategoryname='交通工具类型'
                    inner join smtsystem.t_sys_dictionary dl on t.TAKETHETOOLLEVEL = dl.dictionaryvalue and dl.dictioncategory='VICHILELEVEL'--'交通工具类型'
                    where t.travelsolutionsid='"+txtTraveSolutionid.Text+@"'
                    order by t.typeoftraveltools";

            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtTravSolution.DataSource = dt;
            OracleHelp.close();
        }

        private void btnGetTravButie_Click(object sender, EventArgs e)
        {
            string sql = @"
                    --查询地区补贴
                    select c.city,a.travelsolutionsid,a.postlevel
                    ,a.Accommodation 住宿补贴
                    ,a.Transportationsubsidies 交通补贴
                    ,a.Mealsubsidies 餐费补贴 
                    from smtoa.T_OA_AreaAllowance a--地区差异分类补贴
                    inner join smtoa.T_OA_AreaDifference b --地区差异分类表
                    on b.areadifferenceid=a.areadifferenceid
                    inner join smtoa.T_OA_AreaCity c--地区分类城市
                    on c.areadifferenceid=b.areadifferenceid
                    where 
                    c.city=(
                    select t.dictionaryvalue from smtsystem.t_sys_dictionary t--查询城市字典值
                    where t.dictionaryname='"+txtTraveCityName.Text+@"')
                    and a.postlevel in (
                    select --查询员工出差岗位级别,所属公司
                           ep.postlevel      
                      from smthrm.t_hr_employee e
                     inner join smthrm.t_hr_employeepost ep
                        on e.employeeid = ep.employeeid
                     where  
                     e.employeeid = '" + Txtid.Text + @"'
                     and ep.editstate=1 and ep.isagency=0 and ep.checkstate=2
                    )
                    and 
                    a.travelsolutionsid='"+txtTraveSolutionid.Text+"'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtTravSolution.DataSource = dt;
            OracleHelp.close();
        }

        private void btnGetTravalReme_Click(object sender, EventArgs e)
        {
            string sql = @"select t.COMPUTINGTIME,t.* from smtoa.T_OA_TRAVELREIMBURSEMENT t
where t.nobudgetclaims='" + txtTravNomber.Text + "'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtTravSolution.DataSource = dt;
            OracleHelp.close();
        }
    }

    
}
