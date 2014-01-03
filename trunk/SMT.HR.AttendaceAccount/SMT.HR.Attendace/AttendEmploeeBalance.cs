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
using AttendaceAccount.AttendWS;

namespace SmtPortalSetUp
{
    public partial class AttendEmploeeBalance : Form
    {

        private DataTable dtEmployee = new DataTable();
        public AttendEmploeeBalance()
        {
            InitializeComponent();
            GlobalParameters.fromEmployeeBalance = this;
            this.Txtid.Text = GlobalParameters.employeeid;
            txtCompanyid.Text = GlobalParameters.employeeMasterCompanyid;
            txtStartDate.Text = GlobalParameters.StartDate;
            //txtEndDate.Text = Employee.EndDate;
            txtEmployeeName.Text = GlobalParameters.employeeName;
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

        private void btnSelect_Click(object sender, EventArgs e)
        {
            txtMessagebox.Text = "开始结算" + System.Environment.NewLine + txtMessagebox.Text;
            int year=int.Parse(txtStartDate.Text.Substring(0, 4));
            int month = int.Parse(txtStartDate.Text.Substring(5, 2));
            AttendanceServiceClient AttRdSvc = new AttendanceServiceClient();
            AttRdSvc.CalculateEmployeeAttendanceMonthlyByEmployeeID(year + "-" + month, GlobalParameters.employeeid);
            txtMessagebox.Text = "结算完成！"+ System.Environment.NewLine +txtMessagebox.Text;
            GetEmployeeBlance();
        }

        private void GetEmployeeBlance()
        {
            int year = int.Parse(txtStartDate.Text.Substring(0, 4));
            int month = int.Parse(txtStartDate.Text.Substring(5, 2));
            string sql = @"select 
                            c.cname 考勤记录所属公司
                            ,t.EMPLOYEENAME
                            ,t.BALANCEYEAR
                            ,t.BALANCEMONTH
                            ,t.BALANCEDATE 结算日期
                            ,t.WORKTIMEPERDAY 每日工作时长
                            ,t.NEEDATTENDDAYS 应出勤天数
                            ,t.REALNEEDATTENDDAYS 实际应出勤天数
                            ,t.REALATTENDDAYS 实际出勤天数
                            ,t.LATEDAYS 迟到天数
                            ,t.LEAVEEARLYDAYS 早退天数
                            ,t.ABSENTDAYS 旷工天数
                            ,t.AFFAIRLEAVEDAYS 事假天数
                            ,t.LATETIMES 迟到次数
                            ,t.LATEMINUTES 迟到分钟数
                            ,t.leaveearlytimes 早退次数
                            ,t.FORGETCARDTIMES 漏打卡次数
                            ,t.ABSENTDAYS 旷工天数
                            ,t.ABSENTMINUTES 旷工分钟数
                            ,t.AFFAIRLEAVEDAYS 事假天数
                            ,t.SICKLEAVEDAYS 病假天数
                            ,t.OTHERLEAVEDAYS  其他假期天数
                            ,t.ANNUALLEVELDAYS 年休假天数
                            ,t.LEAVEUSEDDAYS 调休假天数
                            ,t.MARRYDAYS 婚假天数
                            ,t.MATERNITYLEAVEDAYS 产假天数
                            ,t.NURSESDAYS 看护假天数
                            ,t.FUNERALLEAVEDAYS 丧假天数
                            ,t.TRIPDAYS 路程假天数
                            ,t.INJURYLEAVEDAYS 工伤假天数
                            ,t.PRENATALCARELEAVEDAYS 产前检查假天数
                            ,t.EVECTIONTIME 出差时长
                            ,t.CHECKSTATE 审核状态
                            ,t.MONTHLYBALANCEID 主键
                            from smthrm.T_HR_ATTENDMONTHLYBALANCE t
                            inner join smthrm.t_hr_company c
                            on t.ownercompanyid=c.companyid
                            where t.employeename='" + GlobalParameters.employeeName + @"'
                            and t.balanceyear=" + year + @"
                            and t.balancemonth=" + month;
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dataGridEmployees.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：" + dt.Rows.Count.ToString() + "条数据";
                txtMessagebox.Text += System.Environment.NewLine + sql;
            }
            else
            {
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：0条数据";
            }
        }

        private void btnGetEmployeeid_Click(object sender, EventArgs e)
        {
            string sql = @"select t.employeeid,t.employeecname from smthrm.t_hr_employee t
                    where t.employeecname='"+txtEmployeeName.Text+"'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dataGridEmployees.DataSource = dt;
            OracleHelp.close();
            Txtid.Text = dt.Rows[0][0].ToString();
        }



        private void btnPrevious_Click(object sender, EventArgs e)
        {
            AttendEmploee form = GlobalParameters.fromEmployee;
            form.Show();
            this.Hide();
        }

        private void AttendEmploeeBalance_Load(object sender, EventArgs e)
        {
            //GetEmployeeBlance();
        }

        private void btnSelectBalance_Click(object sender, EventArgs e)
        {
            GetEmployeeBlance();
        }

        private void btnGenerateSalary_Click(object sender, EventArgs e)
        {
            SalaryBalanceForm form = GlobalParameters.salaryBalanceForm;
            if (form == null) form = new SalaryBalanceForm();
            form.Show();
            this.Hide();
        }

        private void AttendEmploeeBalance_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

    }

    
}
