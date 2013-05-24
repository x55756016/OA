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
    public partial class SalaryBalanceForm : Form
    {
        private AttendaceAccount.SalaryWS.SalaryServiceClient salaryClient;
        private DataTable dtEmployee = new DataTable();
        public SalaryBalanceForm()
        {
            InitializeComponent();
            GlobalParameters.salaryBalanceForm = this;           
            salaryClient = new AttendaceAccount.SalaryWS.SalaryServiceClient();
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
            //if (!progressBar.InvokeRequired) //不需要唤醒，就是创建控件的线程
            ////如果是创建控件的线程，直接正常操作 
            //{
            //    progressBar.Value = para;
            //}
            //else //非创建线程，用代理进行操作
            //{
            //    DelShowProgress ds = new DelShowProgress(ShowProgress);
            //    //唤醒主线程，可以传递参数，也可以为null，即不传参数
            //    Invoke(ds, new object[] { para });
            //}
        }


        delegate void DelMaxProgress(int Msg); //代理
        //将对控件的操作写到一个函数中 
        public void SetProgressMaxValue(int para)
        {
            //if (!progressBar.InvokeRequired) //不需要唤醒，就是创建控件的线程
            ////如果是创建控件的线程，直接正常操作 
            //{
            //    progressBar.Maximum = para;
            //}
            //else //非创建线程，用代理进行操作
            //{
            //    DelMaxProgress ds = new DelMaxProgress(SetProgressMaxValue);
            //    //唤醒主线程，可以传递参数，也可以为null，即不传参数
            //    Invoke(ds, new object[] { para });
            //}
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
           //AttendaceAccount.SalaryWS.SalaryServiceClient  ServiceClient AttRdSvc = new AttendanceServiceClient();
            //AttRdSvc.CalculateEmployeeAttendanceMonthlyByEmployeeID(year + "-" + month, GlobalParameters.employeeid);
            //结算薪资
            Dictionary<string, string> GeneratePrameter = new Dictionary<string, string>();
            GeneratePrameter.Add("GernerateType", txtGenerateType.Text);//0://发薪机构 1://指定组织架构  2://离职薪资 3://结算岗位薪资
            GeneratePrameter.Add("GenerateEmployeePostid", GlobalParameters.employeeMasterPostid);
            GeneratePrameter.Add("GenerateCompanyid", GlobalParameters.employeeMasterCompanyid);

            string strGenerInfo=GetCreateInfor();
            salaryClient.SalaryRecordAccountCheck(GeneratePrameter, int.Parse(txtOrgType.Text), TxtGenerID.Text, year, month, strGenerInfo);
            txtMessagebox.Text = "结算完成！"+ System.Environment.NewLine +txtMessagebox.Text;
            //GetEmployeeBlance();
        }

        private string GetCreateInfor()
        {
            string sbstr = GlobalParameters.employeeid + ";";
            sbstr += GlobalParameters.employeeMasterPostid + ";";
            sbstr += GlobalParameters.employeeMasterDepartmentid + ";";
            sbstr += GlobalParameters.employeeMasterCompanyid + ";";

            sbstr += GlobalParameters.employeeid + ";";
            sbstr += GlobalParameters.employeeMasterPostid + ";";
            sbstr += GlobalParameters.employeeMasterDepartmentid + ";";
            sbstr += GlobalParameters.employeeMasterCompanyid;
            return sbstr;
        }

        private void GetEmployeeBlance()
        {
            int year = int.Parse(GlobalParameters.StartDate.Substring(0, 4));
            int month = int.Parse(GlobalParameters.StartDate.Substring(5, 2));
            string sql = @"select 
                            t.EMPLOYEENAME
                            ,t.BALANCEYEAR
                            ,t.BALANCEMONTH
                            ,t.BALANCEDATE 结算日期
                            ,t.WORKTIMEPERDAY 每日工作时长
                            ,t.NEEDATTENDDAYS 应出勤天数
                            ,t.REALNEEDATTENDDAYS 实际出勤天数
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
                            from smthrm.T_HR_ATTENDMONTHLYBALANCE t
                            where t.employeename='" + GlobalParameters.employeeName + @"'
                            and t.balanceyear=" + year + @"
                            and t.balancemonth=" + month;
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dtSalaryRecord.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：" + dt.Rows.Count.ToString() + "条数据";
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
            dtSalaryRecord.DataSource = dt;
            OracleHelp.close();
            TxtEmployeeid.Text = dt.Rows[0][0].ToString();
        }



        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Form_Second form = GlobalParameters.fromSecond;
            form.Show();
            this.Hide();
        }

        private void AttendEmploeeBalance_Load(object sender, EventArgs e)
        {
          
        }

        private void btnGeneryFunds_Click(object sender, EventArgs e)
        {
              DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("是否确认生成公司活动经费？时间：" + GlobalParameters.StartDate +"-"+ GlobalParameters.EndDate,//对话框的显示内容 

            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                txtMessagebox.Text = "开始生成活动经费" + System.Environment.NewLine + txtMessagebox.Text;
                string strMsg = string.Empty;
                salaryClient.AssignPersonMoney("2", GlobalParameters.employeeMasterCompanyid, ref strMsg);
                txtMessagebox.Text = "结算生成活动经费：" + strMsg + System.Environment.NewLine + txtMessagebox.Text;
            }
        }

        private void SalaryBalanceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        #region 查询薪资档案
        private void btnGetSalaryAchive_Click(object sender, EventArgs e)
        {
            string sql = @"select e.employeeid,s.salaryarchiveid,e.employeecname,c.cname 发薪机构,
                            s.attendanceorgid,ac.cname 考勤机构,
                            s.balancepostid 结算岗位,
                            s.balancepostname 结算岗位名称,
                            e.employeestate,
                            s.balancepostid,s.balancepostname,
                            ec.cname 员工档案所属公司,ec.companyid,
                            s.othersubjoin 生效年份,
                            s.otheradddeduct 生效月份,
                            s.checkstate,
                            s.salaryarchiveid 
                            from smthrm.t_hr_employee e
                            inner join smthrm.t_hr_salaryarchive s on e.employeeid=s.employeeid
                            left join smthrm.t_hr_company c on s.paycompany=c.companyid
                            left join smthrm.t_hr_company ec on e.ownercompanyid=ec.companyid
                            left join smthrm.t_hr_company ac on s.attendanceorgid=ac.companyid
                            where 
                            s.employeeid='" + GlobalParameters.employeeid + @"'
                            --and s.checkstate='2'
                            order by s.othersubjoin desc,s.otheradddeduct desc";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtSalaryAchive.DataSource = dt;
            OracleHelp.close();         
        }
        #endregion

        #region 查询薪资结算记录
        private void btnGetSalaryRecord_Click(object sender, EventArgs e)
        {
            DateTime date = DateTime.Parse(txtStartDate.Text);
            string sql = @"select distinct a.employeesalaryrecordid,
                a.employeename,
                a.salaryyear,
                a.salarymonth,
                a.actuallypay,
                e.employeecname as 结算人,
                a.createdate,
                a.createuserid,
                a.ownerid,
                a.ownerpostid,
                a.ownerdepartmentid,
                a.ownercompanyid,
                a.*
                from smthrm.t_hr_employeesalaryrecord a
                inner join smthrm.t_hr_employeesalaryrecorditem b
                on a.employeesalaryrecordid = b.employeesalaryrecordid
                inner join smthrm.t_hr_employee e
                on a.createuserid = e.employeeid
                where a.employeeid='" + GlobalParameters.employeeid + @"'
                and a.salaryyear = " + date.Year + @"
                and a.salarymonth = " + date.Month + @"
                order by a.employeename";

            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtSalaryRecord.DataSource = dt;
            OracleHelp.close();      
            
        }
        #endregion


        #region 查询社保缴交记录
        private void btnGetPentionMaster_Click(object sender, EventArgs e)
        {
            DateTime date = DateTime.Parse(txtStartDate.Text);

            string sql = @"select * from smthrm.t_hr_pensiondetail t
                            where t.employeeid='" + GlobalParameters.employeeid + @"'
                            and t.pensionyear="+date.Year+@"
                            and t.pensionmoth=" + date.Month + @"";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtPentionRecord.DataSource = dt;
            OracleHelp.close();      
        }
        #endregion

        private void dtSalaryAchive_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = dtSalaryAchive.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "ColumnUpdateBlancePost")
                    {
                        DialogResult MsgBoxResult
                            = MessageBox.Show("确认是否继续更新员工薪资档案？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                        if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                        {
                            string salaryarchiveid = dtSalaryAchive.Rows[e.RowIndex].Cells["salaryarchiveid"].EditedFormattedValue.ToString();
                            UpdateSalaryBlancePostid(salaryarchiveid);
                        }
                    }
                }
            }
        }
        #region 更新薪资档案发薪机构，考勤机构，结算岗位
        private void UpdateSalaryBlancePostid(string salaryarchiveid)
        {
            bool needAdd = false;
            DateTime date = DateTime.Parse(txtStartDate.Text);

            string sql = @"update smthrm.t_hr_salaryarchive t
                            set ";

            if (!string.IsNullOrEmpty(txBalancePostid.Text))
            {
                sql += @" t.balancepostid='" + txBalancePostid.Text + @"'
                            ,t.balancepostname='" + txtBlancePostName.Text + @"'";
                needAdd = true;
            }


            if(!string.IsNullOrEmpty(txtPayCompany.Text))
            {
                if (needAdd) sql += @" ,";
                sql+=@"   t.paycompany='" + txtPayCompany.Text + @"'";
                needAdd = true;
            }
            if (!string.IsNullOrEmpty(txtAttendCompany.Text))
            {
                if (needAdd) sql += @" ,";
                sql += @"  t.attendanceorgid='" + txtAttendCompany.Text + @"'
                                        ,t.attendanceorgname='" + txtAttendCompanyName.Text + @"'";
            }

                        
            sql+=@" where t.salaryarchiveid='" + salaryarchiveid + @"'";
            OracleHelp.Connect();
            int i = OracleHelp.Excute(sql);
            txtMessagebox.Text = "更新员工薪资档案信息成功，影响记录数："+i;
            OracleHelp.close();      
        }
        /// <summary>
        /// 获取结算岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetBalancePost_Click(object sender, EventArgs e)
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
                         e.employeecname like '%" + txtBalanceEmployeeName.Text + @"%'
                         and ep.editstate='1'
                         and ep.isagency='0'
                         and ep.checkstate=2
                         order by ep.isagency,ep.editstate desc ";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt.Rows.Count == 1)
            {
                txBalancePostid.Text = dt.Rows[0]["postid"].ToString();
                if (txBalancePostid.Text == GlobalParameters.employeeMasterPostid)
                {
                    MessageBox.Show("不需要更新结算岗位，两人在同一家公司");
                }
                txtBlancePostName.Text = dt.Rows[0]["postname"].ToString()
                    +"-"+ dt.Rows[0]["departmentname"].ToString()
                    + "-" + dt.Rows[0]["cname"].ToString();
            }
            else
            {
                MessageBox.Show("结算员工有同名或主岗位异常！");
            }
           
            OracleHelp.close();
            txtMessagebox.Text = "获取员工信息完成！";
        }

        /// <summary>
        /// 验证发薪机构及考勤机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCompanyCheck_Click(object sender, EventArgs e)
        {
            string sql = @"select c.companyid,c.cname from smthrm.t_hr_company c
                            where c.cname like '%"+txtPayCompanyName.Text+"%'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt.Rows.Count == 1)
            {
                txtPayCompanyName.Text = dt.Rows[0]["cname"].ToString();
                txtPayCompany.Text = dt.Rows[0]["companyid"].ToString();
            }
            else
            {
                MessageBox.Show("发薪机构多条或没有找到记录！");
            }


            sql = @"select c.companyid,c.cname from smthrm.t_hr_company c
                            where c.cname like '%" + txtAttendCompanyName.Text+ "%'";
           
            DataTable dt2 = OracleHelp.getTable(sql);
            if (dt2.Rows.Count == 1)
            {
                txtAttendCompanyName.Text = dt2.Rows[0]["cname"].ToString();
                txtAttendCompany.Text = dt2.Rows[0]["companyid"].ToString();
            }
            else
            {
                MessageBox.Show("考勤机构多条或没有找到记录！");
            }

            OracleHelp.close();
            txtMessagebox.Text = "获取组织架构信息完成！";
        }
        #endregion

        private void SalaryBalanceForm_Shown(object sender, EventArgs e)
        {
            TxtGenerID.Text = GlobalParameters.employeeMasterCompanyid;
            this.TxtEmployeeid.Text = GlobalParameters.employeeid;
            txtCompanyid.Text = GlobalParameters.employeeMasterCompanyid;
            txtStartDate.Text = GlobalParameters.StartDate;
            //txtEndDate.Text = Employee.EndDate;
            txtEmployeeName.Text = GlobalParameters.employeeName;

        }

        private void dtSalaryRecord_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = dtSalaryRecord.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "ColumnDelSalary")
                    {
                        DialogResult MsgBoxResult
                            = MessageBox.Show("确认是否继续删除员工薪资结算记录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                        if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                        {
                            string salaryrecordid = dtSalaryRecord.Rows[e.RowIndex].Cells["employeesalaryrecordid"].EditedFormattedValue.ToString();
                            delSalaryrecord(salaryrecordid);
                        }
                    }
                }
            }

        }
        private void delSalaryrecord(string salaryrecordid)
        {
            string sql = @"delete from smthrm.T_HR_EmployeeSalaryRecordItem t
                            where t.employeesalaryrecordid in
                            (
                                select a.employeesalaryrecordid from smthrm.t_hr_employeesalaryrecord a
                                  where a.EMPLOYEESALARYRECORDID='"+salaryrecordid+@"'
                            )";

            OracleHelp.Connect();
            int i = OracleHelp.Excute(sql);
            txtMessagebox.Text = "删除员工薪资记录项目成功，影响记录数：" + i + System.Environment.NewLine + txtMessagebox.Text;


            sql = @"delete from smthrm.t_hr_employeesalaryrecord a
            where a.EMPLOYEESALARYRECORDID ='" + salaryrecordid + @"'";
           
            i = OracleHelp.Excute(sql);
            txtMessagebox.Text = "删除员工薪资记录成功，影响记录数：" + i + System.Environment.NewLine + txtMessagebox.Text; ;

            OracleHelp.close();      
        }

        private void btnGetSalaryRecordByEmployee_Click(object sender, EventArgs e)
        {
          


            DateTime date = DateTime.Parse(txtStartDate.Text);
            string sql = @"select distinct a.employeesalaryrecordid,
                a.employeename,
                a.salaryyear,
                a.salarymonth,
                a.actuallypay,
                e.employeecname as 结算人,
                a.createdate,
                a.createuserid,
                a.ownerid,
                a.ownerpostid,
                a.ownerdepartmentid,
                a.ownercompanyid,
                a.*
                from smthrm.t_hr_employeesalaryrecord a
                inner join smthrm.t_hr_employeesalaryrecorditem b
                on a.employeesalaryrecordid = b.employeesalaryrecordid
                inner join smthrm.t_hr_employee e
                on a.createuserid = e.employeeid
                where a.createuserid='" + txtGenerEmployeeid.Text + @"'
                and a.salaryyear = " + date.Year + @"
                and a.salarymonth = " + date.Month + @"
                order by a.employeename,a.createdate desc";

            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            dtSalaryRecord.DataSource = dt;
            OracleHelp.close();    
        }

        private void btnGetEmployeeId_Click_1(object sender, EventArgs e)
        {
            string sql = @"select e.employeeename,e.employeeid
                          from smthrm.t_hr_employee e
                         where  
                         e.employeecname like '%" + txtGenEmployeeName.Text + @"%'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt.Rows.Count == 1)
            {
                txtGenerEmployeeid.Text = dt.Rows[0]["employeeid"].ToString();
                txtGenEmployeeName.Text = dt.Rows[0]["employeeename"].ToString();
            }
            else
            {
                MessageBox.Show("员工不存在或有同名！");
            }
            OracleHelp.close();
        }

    }

    
}
