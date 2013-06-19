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
    public partial class AttendCompany : Form
    {
        private AttendanceServiceClient AttRdSvcClient;
        private DataTable dtEmployee = new DataTable();
        public AttendCompany()
        {
            InitializeComponent();
            GlobalParameters.fromCompany = this;
            DateTime dtstart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime dtNext = DateTime.Now.AddMonths(1);
            DateTime dtEnd = new DateTime(dtNext.Year, dtNext.Month, 1).AddDays(-1);
            txtStartDate.Text = dtstart.ToString("yyyy-MM-dd");
            txtEndDate.Text = dtEnd.ToString("yyyy-MM-dd");
            AttRdSvcClient = new AttendanceServiceClient();
            //this.Txtid.Text = GlobalParameters.employeeid;
            
            //txtCompanyName.Text = GlobalParameters.employeeName;
        }

        private void Form_Second_Load(object sender, EventArgs e)
        {
            GlobalParameters.StartDate = txtStartDate.Text;
            GlobalParameters.EndDate = txtEndDate.Text;
        }

        private void txtMessagebox_DoubleClick(object sender, EventArgs e)
        {
            txtMessagebox.SelectAll();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(AsignAttendanceSolution));
            t.Start();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            string sql = @"select c.cname,c.companyid from smthrm.t_hr_company c
                            where c.cname like '%" + txtCompanyName.Text + @"%'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dataGridEmployees.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：" + dt.Rows.Count.ToString() + "条数据";
            }
            else
            {
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：0条数据";
            }




        }

        private void btnCleanAll_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(startClearAndInitAttendRecord));
            t.Start();
          
        }

        private void btndel_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(delAttendrecordAll));
            t.Start();

        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(UpdateAttendRecord));
            t.Start();

        }

        private void btnCheckUnNormal_Click(object sender, EventArgs e)
        {

            DateTime dtstart = DateTime.Parse(txtStartDate.Text);
            DateTime dtEnd = DateTime.Parse(txtEndDate.Text);
              DialogResult MsgBoxResult;//设置对话框的返回值
                MsgBoxResult = MessageBox.Show("开始检查：" + dtstart.ToShortDateString() + "-" + dtEnd.ToShortDateString()
                    + "的异常，确认继续？",//对话框的显示内容 
                "提示",//对话框的标题 
                MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
                MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
                MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                {
                    if (dtEnd >= DateTime.Now)
                    {
                        //DialogResult MsgBoxResult;//设置对话框的返回值
                        MsgBoxResult = MessageBox.Show("检查异常的结束日期大于当前日期，确认继续？",//对话框的显示内容 
                        "提示",//对话框的标题 
                        MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
                        MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
                        MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                        if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                        {
                            Thread t = new Thread(new ThreadStart(CheckAbnormRd));
                            t.Start();
                        }
                    }
                    else
                    {
                        Thread t = new Thread(new ThreadStart(CheckAbnormRd));
                        t.Start();
                    }
                }           
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Form_First form = GlobalParameters.fromFisrt;
            form.Show();
            this.Hide();
        }

        private void BtnAttendBlance_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(CalculateEmployeeAttendanceMonthly));
            t.Start();
        }

        private void btnCompanyAttend_Click(object sender, EventArgs e)
        {
            GlobalParameters.employeeMasterCompanyid = txtCompanyid.Text;

            GlobalParameters.employeeMasterCompanyid = txtCompanyid.Text;
            GlobalParameters.StartDate = txtStartDate.Text;
            GlobalParameters.EndDate = txtEndDate.Text;


            string sql = @"select t.cname, b.employeename,b.employeeid,count(b.attendancedate)
                              from smthrm.t_hr_company t
                             inner join smthrm.t_hr_attendancerecord b
                                on t.companyid = b.ownercompanyid
                             where t.companyid='" + GlobalParameters.employeeMasterCompanyid + @"'
                             and b.attendancedate >= to_date('" + GlobalParameters.StartDate + @"', 'yyyy-mm-dd')
                             and b.attendancedate <= to_date('" + GlobalParameters.EndDate + @"', 'yyyy-mm-dd')
                             group by t.cname, b.employeename,b.employeeid
                             order by t.cname, b.employeename";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dataGridEmployees.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：" + dt.Rows.Count.ToString() + "条数据";
            }
            else
            {
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：0条数据";
            }
        }

        private void dataGridEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            for (int i = 0; i < dataGridEmployees.RowCount; i++)
            {
                string selectValue = dataGridEmployees.Rows[i].Cells["ColumnSelect"].EditedFormattedValue.ToString();
                if (selectValue == "True")
                {
                    //Txtid.Text = dataGridEmployees.Rows[i].Cells["employeeid"].EditedFormattedValue.ToString(); ;
                    txtCompanyid.Text = dataGridEmployees.Rows[i].Cells["companyid"].EditedFormattedValue.ToString();
                    GlobalParameters.employeeMasterCompanyid = txtCompanyid.Text;
                    this.txtCompanyName.Text = dataGridEmployees.Rows[i].Cells["cname"].EditedFormattedValue.ToString();
                }
            }
        }


        #region 跨线程调用UI控件显示消息
        delegate void DelShow(String Msg); //代理
        //将对控件的操作写到一个函数中 
        public void ShowMessageAsny(String para)
        {
            if (!txtMessagebox.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                txtMessagebox.Text = DateTime.Now.ToLongTimeString() +" "+ para + System.Environment.NewLine + txtMessagebox.Text;
            }
            else //非创建线程，用代理进行操作
            {
                DelShow ds = new DelShow(ShowMessageAsny);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }

        delegate void DelShowProgress(int Msg); //代理
        //将对控件的操作写到一个函数中 
        public void ShowProgressAsny(int para)
        {
            if (!progressBar.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                progressBar.Value = para;
            }
            else //非创建线程，用代理进行操作
            {
                DelShowProgress ds = new DelShowProgress(ShowProgressAsny);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }

        delegate void DelMaxProgress(int Msg); //代理
        //将对控件的操作写到一个函数中 
        public void SetProgressMaxValueAsny(int para)
        {
            if (!progressBar.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                progressBar.Maximum = para;
            }
            else //非创建线程，用代理进行操作
            {
                DelMaxProgress ds = new DelMaxProgress(SetProgressMaxValueAsny);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }
        #endregion 


        #region 清理异常及考勤初始化
        //第一步
        private string CleareEployeesignindetail1()
        {
            string sql = @"delete from smthrm.t_hr_employeesignindetail d
                where d.abnormaldate >= To_Date('"+GlobalParameters.StartDate+@"', 'yyyy-MM-dd')
               and d.abnormaldate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')
                and d.ownercompanyid ='" + GlobalParameters.employeeMasterCompanyid + @"'
               and d.ownerid = '" + GlobalParameters.employeeid + @"'";
            return sql;
        }
        //第二步
        private string ClearePersonalrecord()
        {
            string sql = @"delete from smtwf.t_wf_personalrecord p
                     where p.systype = 'HR'
                       and p.modelcode = 'smthrm.t_hr_EMPLOYEESIGNINRECORD'
                       and p.modelid in
                           (select s.signinid
                              from smthrm.t_hr_employeesigninrecord s
                             where s.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                               and s.ownerid = '" + GlobalParameters.employeeid + @"'
                               and s.signinid not in
                                   (select d.signinid
                                      from smthrm.t_hr_employeesignindetail d
                                     where d.abnormaldate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                                       and d.abnormaldate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')
                           and d.ownerid = '" + GlobalParameters.employeeid + @"'))";
            return sql;
        }
        //第三步
        private string Clearet_wf_dotask3()
        {
            string sql = @"delete from smtwf.t_wf_dotask p
                     where p.dotaskstatus = 0
                       and p.modelcode = 'T_HR_EMPLOYEESIGNINRECORD'
                       and p.orderid in
                           (select s.signinid
                              from smthrm.t_hr_employeesigninrecord s
                             where s.ownercompanyid = '"+GlobalParameters.employeeMasterCompanyid+@"'
                               and s.ownerid = '" + GlobalParameters.employeeid + @"'
                               and s.signintime >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                               and s.signinid not in
                                   (select d.signinid
                                      from smthrm.t_hr_employeesignindetail d
                                     where d.abnormaldate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                                       and d.abnormaldate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')
                                       and d.ownercompanyid =
                                           '" + GlobalParameters.employeeMasterCompanyid + @"'
                                       and d.ownerid = '" + GlobalParameters.employeeid + @"'))";
            return sql;
        }
        //第四步
        private string CleareEmployeesigninrecord4()
        {
            string sql = @"delete from smthrm.t_hr_employeesigninrecord s
                         where s.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                           and s.ownerid = '" + GlobalParameters.employeeid + @"'
                           and s.signinid not in
                               (select d.signinid
                                  from smthrm.t_hr_employeesignindetail d
                                   where d.ownercompanyid ='" + GlobalParameters.employeeMasterCompanyid + @"'
                                   and d.ownerid = '" + GlobalParameters.employeeid + @"')";
            return sql;
        }
        //第五步
        private string CleareEmployeeabnormrecord5()
        {
            string sql = @"delete from smthrm.t_hr_employeeabnormrecord a
                             where a.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                               and a.ownerid = '" + GlobalParameters.employeeid + @"'
                               and a.abnormaldate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                               and a.abnormaldate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')";
            return sql;
        }
        //第六步
        private string CleareEmployeeabnormrecord6()
        {
            string sql = @"update smthrm.t_hr_attendancerecord a
                       set a.attendancestate = ''
                     where a.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                       and a.ownerid = '" + GlobalParameters.employeeid + @"'
                       and a.attendancedate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                       and a.attendancedate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')
                       and a.attendancestate = '2'";
            return sql;
        }

        #endregion


        private void startClearAndInitAttendRecord()
        {
            DateTime dtstart = DateTime.Parse(txtStartDate.Text);
            DateTime dtEnd = DateTime.Parse(txtEndDate.Text);
            DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("开始处理：" + dtstart.ToShortDateString() + "-" + dtEnd.ToShortDateString()
                + "的异常及考勤，确认继续？",//对话框的显示内容 
            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                ShowMessageAsny("*********************************开始处理指定公司员工的考勤异常及初始化考勤记录");
                int j = 0;
                OracleHelp.Connect();
                for (int i = 0; i < dataGridEmployees.RowCount; i++)
                {
                    try
                    {
                        GlobalParameters.employeeName = dataGridEmployees.Rows[i].Cells["employeename"].EditedFormattedValue.ToString();
                        GlobalParameters.employeeid = dataGridEmployees.Rows[i].Cells["employeeid"].EditedFormattedValue.ToString();

                        string sql = CleareEployeesignindetail1()
                            + ClearePersonalrecord()
                            + Clearet_wf_dotask3()
                            + CleareEmployeesigninrecord4()
                            + CleareEmployeeabnormrecord5()
                            + CleareEmployeeabnormrecord6();
                        ShowMessageAsny("*******开始处理：" + GlobalParameters.employeeName + " 清理考勤异常及初始化考勤记录");
                        j = OracleHelp.Excute(CleareEployeesignindetail1());
                        ShowMessageAsny("第一步完成，处理了：" + j + "条数据！");
                        j = OracleHelp.Excute(ClearePersonalrecord());
                        ShowMessageAsny("第二步完成，处理了：" + j + "条数据！");
                        j = OracleHelp.Excute(Clearet_wf_dotask3());
                        ShowMessageAsny("第三步完成，处理了：" + j + "条数据！");
                        j = OracleHelp.Excute(CleareEmployeesigninrecord4());
                        ShowMessageAsny("第四步完成，处理了：" + j + "条数据！");
                        j = OracleHelp.Excute(CleareEmployeeabnormrecord5());
                        ShowMessageAsny("第五步完成，处理了：" + j + "条数据！");
                        j = OracleHelp.Excute(CleareEmployeeabnormrecord6());
                        ShowMessageAsny("第六步完成，处理了：" + j + "条数据！");
                        ShowMessageAsny("*******处理完成：" + GlobalParameters.employeeName + " 清理考勤异常及初始化考勤记录");
                    }
                    catch (Exception ex)
                    {
                        ShowMessageAsny("处理异常：" + GlobalParameters.employeeName + " 异常信息：" + ex.ToString());
                    }
                }
                OracleHelp.close();
                ShowMessageAsny("*********************************已处理完成所有指定公司员工的考勤异常及初始化考勤记录");
            }
        }

        private void delAttendrecordAll()
        {
            ShowMessageAsny("----------------------开始删除员工考勤初始化记录......");
            OracleHelp.Connect();
            for (int i = 0; i < dataGridEmployees.RowCount; i++)
            {
                GlobalParameters.employeeid = dataGridEmployees.Rows[i].Cells["employeeid"].EditedFormattedValue.ToString();
                GlobalParameters.employeeName = dataGridEmployees.Rows[i].Cells["employeename"].EditedFormattedValue.ToString();

                string sql = @"delete smthrm.t_hr_attendancerecord a
                     where a.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                       and a.ownerid = '" + GlobalParameters.employeeid + @"'
                       and a.attendancedate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                       and a.attendancedate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')";

                int j = OracleHelp.Excute(sql);
                ShowMessageAsny("删除员工:"+GlobalParameters.employeeName+" 考勤初始化记录完成，处理了：" + j + "条数据！");

            }
            OracleHelp.close();
            ShowMessageAsny("----------------------删除员工考勤初始化记录完成!");
        }

        private void CheckAbnormRd()
        {
            ShowMessageAsny("++++++++++++++++++++++++开始检查公司所有员工考勤异常......");
            AttendanceServiceClient AttRdSvc = new AttendanceServiceClient();
            string strMsg = string.Empty;
            DateTime dtstart = DateTime.Parse(txtStartDate.Text);
            DateTime dtEnd = DateTime.Parse(txtEndDate.Text); 

            for (int i = 0; i < dataGridEmployees.RowCount; i++)
            {
                GlobalParameters.employeeid = dataGridEmployees.Rows[i].Cells["employeeid"].EditedFormattedValue.ToString();
                GlobalParameters.employeeName = dataGridEmployees.Rows[i].Cells["employeename"].EditedFormattedValue.ToString();
                AttRdSvc.CheckAbnormRdForEmployeesByDate(GlobalParameters.employeeid, dtstart, dtEnd, ref strMsg);
                ShowMessageAsny("检查员工：" + GlobalParameters.employeeName + " 考勤异常完毕:"+strMsg);
            }
            ShowMessageAsny("++++++++++++++++++++++++检查公司所有员工考勤异常完成!");
        }

        private void AsignAttendanceSolution()
        {
            try
            {
                ShowMessageAsny("====================================开始初始化考勤记录");
                AttendanceServiceClient AttRdSvc = new AttendanceServiceClient();
                //for (int i = 0; i < dataGridEmployees.RowCount; i++)
                //{
                //GlobalParameters.employeeid = dataGridEmployees.Rows[i].Cells["employeeid"].EditedFormattedValue.ToString();
                //GlobalParameters.employeeName = dataGridEmployees.Rows[i].Cells["employeename"].EditedFormattedValue.ToString();
                ShowMessageAsny("========开始初始化公司考勤记录");
                AttRdSvc.AsignAttendanceSolutionByOrgIDAndMonth("1", GlobalParameters.employeeMasterCompanyid, GlobalParameters.StartDate.Substring(0, 7));
                ShowMessageAsny("========初始化公司考勤记录完成！");
                //}
                ShowMessageAsny("====================================初始化考勤记录完毕");
            }
            catch (Exception ex)
            {
                ShowMessageAsny("始化考勤记录异常：" + ex.ToString());
            }
        }

        private void UpdateAttendRecord()
        {
            ShowMessageAsny("++++++++++++++++++++++++++++++++++++++++++++++++++开始检查公司所有员工请假及出差");
            AttendanceServiceClient AttRdSvc = new AttendanceServiceClient();
            AttRdSvc.UpdateAttendRecordByEvectionAndLeaveRd(GlobalParameters.employeeMasterCompanyid, GlobalParameters.StartDate.Substring(0, 7));
            //txtMessagebox.Text += System.Environment.NewLine + "检查所有请假及出差完成";
            ShowMessageAsny("++++++++++++++++++++++++++++++++++++++++++++++++++检查公司所有员工请假及出差完毕！");
        }

        private void CalculateEmployeeAttendanceMonthly()
        {
            ShowMessageAsny("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$开始结算公司月度考勤");
            int year = int.Parse(GlobalParameters.StartDate.Substring(0, 4));
            int month = int.Parse(GlobalParameters.StartDate.Substring(5, 2));
            AttendanceServiceClient AttRdSvc = new AttendanceServiceClient();
            AttRdSvc.CalculateEmployeeAttendanceMonthlyByCompanyID(year + "-" + month, GlobalParameters.employeeMasterCompanyid);
            ShowMessageAsny("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$结算公司月度考勤完毕！");
        }

        private void AttendCompany_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

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

        private void btnInitAttendRecord_Click(object sender, EventArgs e)
        {

        }

        private void btnGetAllCompany_Click(object sender, EventArgs e)
        {
            string sql = @"select t.cname, b.employeename,b.employeeid,count(b.attendancedate)
                              from smthrm.t_hr_company t
                             inner join smthrm.t_hr_attendancerecord b
                                on t.companyid = b.ownercompanyid
                             where 
                             --t.companyid='" + GlobalParameters.employeeMasterCompanyid + @"'
                             b.attendancedate >= to_date('2013-05-01', 'yyyy-mm-dd')
                             and b.attendancedate <= to_date('2013-05-31', 'yyyy-mm-dd')
                             group by t.cname, b.employeename,b.employeeid
                             order by t.cname, b.employeename";
        }

        #region 生成公司带薪假
        private void btnGenerVacation_Click(object sender, EventArgs e)
        {
            DialogResult MsgBoxResult
                              = MessageBox.Show("确认是否继续操作？生成带薪假需要有生效的主岗位，有生效的考勤方案，并且员工状态为在职状态。", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                txtMessagebox.Text = "开始生成带薪假:" + System.Environment.NewLine + txtMessagebox.Text;
                AttRdSvcClient.CalculateEmployeeLevelDayCountByOrgID("1", txtCompanyid.Text);
                txtMessagebox.Text = "生成带薪假完成:" + System.Environment.NewLine + txtMessagebox.Text;
                MessageBox.Show("生成带薪年假完成！");
            }
        }
        #endregion

        private void btnPrevious_Click_1(object sender, EventArgs e)
        {
            Form_Second form = GlobalParameters.fromSecond;
            form.Show();
            this.Hide();
        }

    }

    
}
