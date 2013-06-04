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
    public partial class AttendEmploee : Form
    {
        #region Form初始化
        private DataTable dtEmployee = new DataTable();
        private AttendanceServiceClient AttRdSvcClient;
        public AttendEmploee()
        {
            InitializeComponent();
            GlobalParameters.fromEmployee = this;
            this.Txtid.Text = GlobalParameters.employeeid;
            txtCompanyid.Text = GlobalParameters.employeeMasterCompanyid;
            txtStartDate.Text = GlobalParameters.StartDate;
            txtEndDate.Text = GlobalParameters.EndDate;
            txtEmployeeName.Text = GlobalParameters.employeeName;
            AttRdSvcClient = new AttendanceServiceClient();
        }

        private void Form_Second_Load(object sender, EventArgs e)
        {
            OracleHelp.MsgBox = this.txtMessagebox;
            
        }

        private void txtMessagebox_DoubleClick(object sender, EventArgs e)
        {
            txtMessagebox.SelectAll();
        }
        #endregion

        #region 初始化考勤
        private void btnStart_Click(object sender, EventArgs e)
        {
            DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("初始化月份为:" + GlobalParameters.StartDate.Substring(0, 7) 
                + ",是否覆盖该员工所有考勤初始化记录？",//对话框的显示内容 
            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                //Directory.SetCurrentDirectory(Directory.GetParent(binPath).FullName);
                //binPath = Directory.GetCurrentDirectory();
                try
                {
                    AttRdSvcClient.AsignAttendanceSolutionByOrgIDAndMonth("4", GlobalParameters.employeeid, GlobalParameters.StartDate.Substring(0, 7));
                    txtMessagebox.Text = "初始化考勤处理完成" + System.Environment.NewLine + txtMessagebox.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }
        #endregion

        #region 查询员工考勤记录并重置全局开始结束时间
        private void btnSelect_Click(object sender, EventArgs e)
        {
            GlobalParameters.StartDate = txtStartDate.Text;
            GlobalParameters.EndDate = txtEndDate.Text;
            GetAttendSolutionAssign();
            GetVacation();
            GetAttendInitData();
            getEmployeeAllAttendRecord();
        }
        string attendancesolutionid = string.Empty;
        private void GetAttendSolutionAssign()
        {      
            
            #region 考勤方案应用

            string sql = @"--查询员工分配的考勤方案
                            select * 
                            from(
                            select s.attendancesolutionname,s.ownercompanyid 分配考勤方案的公司
                            ,case when s.workdaytype='1' then '按当月实际工作日' else '固定方式'end 工作方式,s.workdays 固定方式下总天数
 ,s.AttendanceType 考勤方式,s.worktimeperday 每天工作时长,s.IsExpired 调休是否过期,s.AdjustExpiredValue 调休过期时长,s.CardType 打卡方式
                            ,a.startdate,a.enddate,a.checkstate,a.createdate,a.updatedate
                            ,a.ASSIGNEDOBJECTTYPE,'个人' Asingtype
                            ,s.attendancesolutionid
                            from smthrm.t_hr_attendancesolutionasign a
                            inner join smthrm.t_hr_attendancesolution s on a.attendancesolutionid=s.attendancesolutionid
                            inner  join smthrm.t_hr_employee e on instr(a.assignedobjectid,e.employeeid)>0
                            where a.assignedobjecttype=4--员工
                            and a.checkstate='2'
                            and e.employeeid='" + GlobalParameters.employeeid + @"'

                            union

                            --查询员工主岗位上分配的考勤方案
                            select s.attendancesolutionname,s.ownercompanyid 分配考勤方案的公司
                            ,case when s.workdaytype='1' then '按当月实际工作日' else '固定方式'end 工作方式,s.workdays 固定方式下总天数
 ,s.AttendanceType 考勤方式,s.worktimeperday 每天工作时长,s.IsExpired 调休是否过期,s.AdjustExpiredValue 调休过期时长,s.CardType 打卡方式
                            ,a.startdate,a.enddate,a.checkstate,a.createdate,a.updatedate
                            ,a.ASSIGNEDOBJECTTYPE,'岗位' Asingtype
                            ,s.attendancesolutionid
                            from smthrm.t_hr_attendancesolutionasign a
                            inner join smthrm.t_hr_attendancesolution s on a.attendancesolutionid=s.attendancesolutionid
                            inner  join smthrm.t_hr_post p on instr(a.assignedobjectid,p.postid)>0
                            where a.assignedobjecttype=3--岗位
                            and a.assignedobjectid in
                            (
                            select 
                                   ep.postid
                              from smthrm.t_hr_employee e
                             inner join smthrm.t_hr_employeepost ep
                                on e.employeeid = ep.employeeid
                             inner join smthrm.t_hr_post p
                                on ep.postid = p.postid
                             inner join smthrm.t_hr_company c
                                on p.companyid = c.companyid
                             inner join smthrm.t_hr_department d
                                on p.departmentid = d.departmentid
                             where  e.employeeid = '" + GlobalParameters.employeeid + @"'
                             and ep.editstate=1
                             and ep.checkstate=2
                             and ep.isagency=0
                            )

                            union

                            --查询员工主岗位部门上分配的考勤方案
                            select s.attendancesolutionname,s.ownercompanyid 分配考勤方案的公司
                            ,case when s.workdaytype='1' then '按当月实际工作日' else '固定方式'end 工作方式,s.workdays 固定方式下总天数
 ,s.AttendanceType 考勤方式,s.worktimeperday 每天工作时长,s.IsExpired 调休是否过期,s.AdjustExpiredValue 调休过期时长,s.CardType 打卡方式
                            ,a.startdate,a.enddate,a.checkstate,a.createdate,a.updatedate
                            ,a.ASSIGNEDOBJECTTYPE,'部门' Asingtype
                            ,s.attendancesolutionid
                            from smthrm.t_hr_attendancesolutionasign a
                            inner join smthrm.t_hr_attendancesolution s on a.attendancesolutionid=s.attendancesolutionid
                            inner  join smthrm.t_hr_department d on instr(a.assignedobjectid,d.departmentid)>0
                            where a.assignedobjecttype=2--部门
                            and a.assignedobjectid in
                            (
                            select d.departmentid
                              from smthrm.t_hr_employee e
                             inner join smthrm.t_hr_employeepost ep
                                on e.employeeid = ep.employeeid
                             inner join smthrm.t_hr_post p
                                on ep.postid = p.postid
                             inner join smthrm.t_hr_company c
                                on p.companyid = c.companyid
                             inner join smthrm.t_hr_department d
                                on p.departmentid = d.departmentid
                             where  e.employeeid = '" + GlobalParameters.employeeid + @"'
                             and ep.editstate=1
                             and ep.checkstate=2
                             and ep.isagency=0
                            )

                            union

                            --查询员工主岗位公司上分配的考勤方案
                            select s.attendancesolutionname,s.ownercompanyid 分配考勤方案的公司
                            ,case when s.workdaytype='1' then '按当月实际工作日' else '固定方式'end 工作方式,s.workdays 固定方式下总天数
 ,s.AttendanceType 考勤方式,s.worktimeperday 每天工作时长,s.IsExpired 调休是否过期,s.AdjustExpiredValue 调休过期时长,s.CardType 打卡方式
                            ,a.startdate,a.enddate,a.checkstate,a.createdate,a.updatedate
                            ,a.ASSIGNEDOBJECTTYPE,'公司' Asingtype
                            ,s.attendancesolutionid
                            from smthrm.t_hr_attendancesolutionasign a
                            inner join smthrm.t_hr_attendancesolution s on a.attendancesolutionid=s.attendancesolutionid
                            inner  join smthrm.t_hr_company c on instr(a.assignedobjectid,c.companyid)>0
                            where a.assignedobjecttype=1--公司
                            and a.assignedobjectid in
                            (
                            select c.companyid
                              from smthrm.t_hr_employee e
                             inner join smthrm.t_hr_employeepost ep
                                on e.employeeid = ep.employeeid
                             inner join smthrm.t_hr_post p
                                on ep.postid = p.postid
                             inner join smthrm.t_hr_company c
                                on p.companyid = c.companyid
                             inner join smthrm.t_hr_department d
                                on p.departmentid = d.departmentid
                             where  e.employeeid = '" + GlobalParameters.employeeid + @"'
                             and ep.editstate=1
                             and ep.checkstate=2
                             and ep.isagency=0
                            ) 
                            )aa
                            where aa.enddate>=sysdate 
                            and aa.checkstate='2'
                            order by aa.ASSIGNEDOBJECTTYPE desc ,aa.startdate";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dtAttsolutionAsign.DataSource = dt;
                if (dt.Rows.Count > 0) attendancesolutionid = dt.Rows[0]["attendancesolutionid"].ToString();
                OracleHelp.close();
                txtMessagebox.Text = "查询员工考勤方案分配完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询员工考勤方案分配完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion

        }

        private void GetVacation()
        {
            #region 获取假期设置

            string sql = @"select b.vacationname,b.vacationyear,a.outplanname,a.startdate,a.enddate,
                            case when a.daytype=1 then '假期' else '工作日' end 设置类型,a.daytype,a.days
                            ,b.assignedobjecttype,b.assignedobjectid from smthrm.T_HR_OutPlanDays a
                            inner join smthrm.T_HR_VacationSet b on a.vacationid=b.vacationid
                            where b.assignedobjectid='" + GlobalParameters.employeeMasterCompanyid + @"'
                            and b.vacationyear>= extract(year from sysdate)
                            order by a.startdate desc";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dtVacationDay.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询假期设置完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询假期设置完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion
        }

        private void GetAttendInitData()
        {
            #region 考勤初始化记录

            string sql = @"select c.cname,
                                    b.employeename,
                                    b.attendancedate,
                                    b.attendancestate,
                                    s.attendancesolutionname,
                                    t.shiftname,
                                    t.needfirstcard,
                                    t.firststarttime,
                                    t.firstendtime,
                                    t.needsecondcard,
                                    t.needsecondoffcard,
                                    t.secondstarttime,
                                    t.secondendtime,
                                    b.updatedate,
                                    c.companyid,
                                    b.attendancerecordid,
                                    b.employeeid,
                                    b.createuserid,
                                    b.createdate,
                                    b.remark
                                from smthrm.t_hr_company c
                                inner join smthrm.t_hr_attendancerecord b
                                on c.companyid = b.ownercompanyid
                                inner join smthrm.t_hr_attendancesolution s
                                on b.attendancesolutionid = s.attendancesolutionid
                                inner join smthrm.t_hr_shiftdefine t
                                on b.worktimesetid = t.shiftdefineid
                                where
                                b.employeeid='" + GlobalParameters.employeeid + @"'
                                and b.attendancedate>=to_date('" + GlobalParameters.StartDate + @"','yyyy-mm-dd')
                                and b.attendancedate<=to_date('" + GlobalParameters.EndDate + @"','yyyy-mm-dd')";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dataGridEmployees.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询员工初始化考勤完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询员工初始化考勤完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion
        }

        private void getEmployeeAllAttendRecord()
        {
            string sql = string.Empty;
            #region 打卡记录
            //打卡记录时间格式带小时分钟，需特殊处理
            DateTime dtEnd = DateTime.Parse(GlobalParameters.EndDate);
            string endDate = dtEnd.AddDays(1).ToString("yyyy-MM-dd");

            sql = @"select c.employeename,c.punchdate,c.punchtime,c.employeename,c.ownerid,c.ownercompanyid,c.clockinrecordid 
                    from smthrm.t_hr_employeeclockinrecord c
                    where c.employeeid='" + GlobalParameters.employeeid + @"'
                    and c.punchdate>=to_date('" + GlobalParameters.StartDate + @"','yyyy-mm-dd')
                    and c.punchdate<=to_date('" + endDate + @"','yyyy-mm-dd')
                    order by c.punchdate";
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                this.dtGClockRecord.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询员工打卡记录完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询员工打卡记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion

            #region 请假记录
            sql = @"select t.employeename,t.startdatetime,t.enddatetime,s.leavetypename,t.checkstate,t.employeename,t.updatedate,t.employeeid 
                        from smthrm.T_HR_EmployeeLeaveRecord t
                        inner join smthrm.t_hr_leavetypeset s on t.leavetypesetid=s.leavetypesetid
                        where t.employeeid='" + GlobalParameters.employeeid + @"'
                        and t.startdatetime>=to_date('" + GlobalParameters.StartDate + @"','yyyy-mm-dd')
                        and t.enddatetime<=to_date('" + GlobalParameters.EndDate + @"','yyyy-mm-dd')";
            dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                this.dtGEmployeeLeaveRecord.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询员工请假记录完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询员工请假记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion

            #region 出差记录
            GetEmployeeEvection();
            #endregion

            #region 异常考勤记录
            sql = @"select b.abnormaldate,b.abnormcategory,b.singinstate,b.abnormaltime,b.attendperiod,
                                b.singinstate,b.updatedate,b.abnormrecordid,b.ownerid 
                                from smthrm.T_HR_EMPLOYEEABNORMRECORD b
                                where b.ownerid='" + GlobalParameters.employeeid + @"'
                                and b.abnormaldate>=to_date('" + GlobalParameters.StartDate + @"','yyyy-mm-dd')
                                and b.abnormaldate<=to_date('" + GlobalParameters.EndDate + @"','yyyy-mm-dd')
                                order by b.abnormaldate";
            dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                this.dtgAbnormrecord.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询员工异常考勤记录完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询员工异常考勤记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion

            #region 员工签卡记录
            sql = @"select b.abnormaldate,b.abnormcategory,c.singinstate,a.checkstate,b.reasoncategory,b.abnormaltime,a.updatedate,
                    a.employeename,a.signinid,b.signindetailid,c.abnormrecordid from smthrm.t_hr_employeesigninrecord a
                    inner join smthrm.t_hr_employeesignindetail b on a.signinid=b.signinid
                    inner join smthrm.t_hr_employeeabnormrecord c on b.abnormrecordid=c.abnormrecordid
                    where b.ownerid='" + GlobalParameters.employeeid + @"'
                    and b.abnormaldate>=to_date('" + GlobalParameters.StartDate + @"','yyyy-mm-dd')
                    and b.abnormaldate<=to_date('" + GlobalParameters.EndDate + @"','yyyy-mm-dd')
                    order by b.abnormaldate";
            dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                this.dtGemployeesigninrecord.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询员工签卡记录完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询员工签卡记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion
        }


        #endregion

//        private void btnGetEmployeeid_Click(object sender, EventArgs e)
//        {
//            string sql = @"select t.employeeid,t.employeecname from smthrm.t_hr_employee t
//                    where t.employeecname='"+txtEmployeeName.Text+"'";
//            OracleHelp.Connect();
//            DataTable dt = OracleHelp.getTable(sql);
//            dataGridEmployees.DataSource = dt;
//            OracleHelp.close();
//            Txtid.Text = dt.Rows[0][0].ToString();
//        }

        #region 清空所有异常并重置考勤
        private void btnCleanAll_Click(object sender, EventArgs e)
        {

            OracleHelp.Connect();
            int i = OracleHelp.Excute(CleareEployeesignindetail1());
            txtMessagebox.Text = "第一步清除签卡明细完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
             i = OracleHelp.Excute(CleareEployeesigninPersonalrecord());
             txtMessagebox.Text = "第二步清除签卡我的单据完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
             i = OracleHelp.Excute(Clearet_wf_dotask3());
             txtMessagebox.Text = "第三步清除签卡待办完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
             i = OracleHelp.Excute(CleareEmployeesigninrecordMaster4());
             txtMessagebox.Text = "第四步清除签卡主表完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
             i = OracleHelp.Excute(CleareEmployeeabnormrecord5());
             txtMessagebox.Text = "第五步清除考勤异常完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
             i = OracleHelp.Excute(UpdateEmployeeAttendancerecord6());
             txtMessagebox.Text = "第六步还原考勤初始化记录状态完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;

            OracleHelp.close();
            MessageBox.Show("处理完成");
        }

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
        private string CleareEployeesigninPersonalrecord()
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
        private string CleareEmployeesigninrecordMaster4()
        {
            string sql = @"delete from smthrm.t_hr_employeesigninrecord s
                         where s.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                           and s.ownerid = '" + GlobalParameters.employeeid + @"'
                           and s.signinid not in
                               (select d.signinid
                                  from smthrm.t_hr_employeesignindetail d)";
                                   //where d.ownerid = '" + GlobalParameters.employeeid + @"')";
            //d.ownercompanyid ='" + GlobalParameters.ownerCompanyid + @"'
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
        private string UpdateEmployeeAttendancerecord6()
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

        #region 删除员工考勤初始化记录
        private void btndel_Click(object sender, EventArgs e)
        {
            DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("是否确认删除该员工所有考勤初始化记录，删除后将不可恢复？",//对话框的显示内容 

            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                string sql = @"delete smthrm.t_hr_attendancerecord a
                     where a.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                       and a.ownerid = '" + GlobalParameters.employeeid + @"'
                       and a.attendancedate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                       and a.attendancedate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')";
                OracleHelp.Connect();
                int i = OracleHelp.Excute(sql);
                txtMessagebox.Text = "处理完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
                OracleHelp.close();
            }
        }
        #endregion

        #region 检查出差请假
        private void btnCheck_Click(object sender, EventArgs e)
        {
            AttRdSvcClient.UpdateAttendRecordByEvectionAndLeaveRd(GlobalParameters.employeeMasterCompanyid, GlobalParameters.StartDate.Substring(0, 7));
            txtMessagebox.Text = "检查所有请假出差完成" + System.Environment.NewLine + txtMessagebox.Text;
            MessageBox.Show("检查所有请假出差完毕！");
           
        }
        #endregion

        #region 检查异常考勤
        private void btnCheckUnNormal_Click(object sender, EventArgs e)
        {

            string strMsg = string.Empty;
            try
            {
                DateTime dtstart = DateTime.Parse(txtStartDate.Text);
                DateTime dtEnd = DateTime.Parse(txtEndDate.Text);
                if (dtEnd >= DateTime.Now)
                {
                    DialogResult MsgBoxResult;//设置对话框的返回值
                    MsgBoxResult = MessageBox.Show("检查异常的结束日期大于当前日期，确认继续？",//对话框的显示内容 
                    "提示",//对话框的标题 
                    MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
                    MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
                    MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                    if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                    {
                        AttRdSvcClient.CheckAbnormRdForEmployeesByDate(GlobalParameters.employeeid, dtstart, dtEnd, ref strMsg);
                        txtMessagebox.Text = "检查所有异常完成:" + strMsg + System.Environment.NewLine + txtMessagebox.Text;
                    }
                }
                else
                {
                    AttRdSvcClient.CheckAbnormRdForEmployeesByDate(GlobalParameters.employeeid, dtstart, dtEnd, ref strMsg);
                    txtMessagebox.Text = "检查所有异常完成:" + strMsg + System.Environment.NewLine + txtMessagebox.Text;

                    MessageBox.Show("检查所有异常完毕！");
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("日期选择错误，请重新选择。"+ex.ToString());
            }


        }
        #endregion

        #region Form导航
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Form_Second form = GlobalParameters.fromSecond;
            form.Show();
            this.Hide();
        }

        private void BtnAttendBlance_Click(object sender, EventArgs e)
        {
            GlobalParameters.employeeName = txtEmployeeName.Text;
            GlobalParameters.employeeid = Txtid.Text;
            GlobalParameters.employeeMasterCompanyid = txtCompanyid.Text;
            AttendEmploeeBalance form = GlobalParameters.fromEmployeeBalance;
            if (form == null) form = new AttendEmploeeBalance();
            form.Show();
            this.Hide();
        }
        #endregion

        #region Datagrid考勤初始化状态更新


        private void dataGridEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            for (int i = 0; i < dataGridEmployees.RowCount; i++)
            {
                string selectValue = dataGridEmployees.Rows[i].Cells["ColumnSelect"].EditedFormattedValue.ToString();
                if (selectValue == "True")
                {
                    GlobalParameters.StartDate = dataGridEmployees.Rows[i].Cells["attendancedate"].EditedFormattedValue.ToString();
                    GlobalParameters.EndDate = dataGridEmployees.Rows[i].Cells["attendancedate"].EditedFormattedValue.ToString();

                    getEmployeeAllAttendRecord();


                    GlobalParameters.StartDate = txtStartDate.Text;
                    GlobalParameters.EndDate = txtEndDate.Text;
                }
            }


            if (e.RowIndex >= 0 && e.ColumnIndex>=0)
            {
                DataGridViewColumn column = dataGridEmployees.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "ColumnUpdateAttendstates")
                    {
                        DialogResult MsgBoxResult
                            = MessageBox.Show("确认是否继续操作？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                        if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                        {
                            string attendancerecordid = dataGridEmployees.Rows[e.RowIndex].Cells["attendancerecordid"].EditedFormattedValue.ToString();
                            UpdateEmployeeAttendancerecordById(attendancerecordid);
                        }
                    }
                    if (column.Name == "ClUpdateToNormal")
                    {
                        DialogResult MsgBoxResult
                              = MessageBox.Show("确认是否继续操作？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                        if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                        {
                            string attendancerecordid = dataGridEmployees.Rows[e.RowIndex].Cells["attendancerecordid"].EditedFormattedValue.ToString();
                            UpdateAttendancerecordToNormal(attendancerecordid);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 强制更新考勤记录为空（初始化）
        /// </summary>
        /// <param name="attendancerecordid"></param>
        private void UpdateEmployeeAttendancerecordById(string attendancerecordid)
        {
            string sql = @"update smthrm.t_hr_attendancerecord a
                       set a.attendancestate = ''
                     where a.attendancerecordid = '" + attendancerecordid + @"'";
            int i = OracleHelp.Excute(sql);
            txtMessagebox.Text = "强制更新考勤初始化记录状态完毕，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
            GetAttendInitData();
        }

        /// <summary>
        /// 强制更新考勤记录为1（正常出勤）
        /// </summary>
        /// <param name="attendancerecordid"></param>
        private void UpdateAttendancerecordToNormal(string attendancerecordid)
        {
            string sql = @"update smthrm.t_hr_attendancerecord a
                       set a.attendancestate = '1'
                     where a.attendancerecordid = '" + attendancerecordid + @"'";
            int i = OracleHelp.Excute(sql);
            txtMessagebox.Text = "强制更新考勤初始化记录状态完毕，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
            GetAttendInitData();
        }

        #endregion

        #region 生成带薪假
        private void btnGenerVacation_Click(object sender, EventArgs e)
        {
            MessageBox.Show("生成带薪假需要有生效的主岗位，有生效的考勤方案，并且员工状态为在职状态。");
            txtMessagebox.Text = "开始生成带薪假:" + System.Environment.NewLine + txtMessagebox.Text;
            AttRdSvcClient.CalculateEmployeeLevelDayCountByOrgID("4", GlobalParameters.employeeid);
            txtMessagebox.Text = "生成带薪假完成:" + System.Environment.NewLine + txtMessagebox.Text;
            MessageBox.Show("生成带薪年假完成！");
        }
        #endregion

        #region  异常考勤dataGrid删除指定的异常
        private void dtgAbnormrecord_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = dtgAbnormrecord.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "btnColumnAbnormalDelet")
                    {
                        //这里可以编写你需要的任意关于按钮事件的操作~
                        string abnormrecordid = dtgAbnormrecord.Rows[e.RowIndex].Cells["abnormrecordid"].EditedFormattedValue.ToString();
                        if (string.IsNullOrEmpty(abnormrecordid))
                        {
                            MessageBox.Show("请选择不为空的异常记录进行操作");
                            return;
                        }
                          DialogResult MsgBoxResult 
                        = MessageBox.Show("确认是否继续操作？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                          if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                          {
                              try
                              {
                                  int i = OracleHelp.Excute(DeletSignindetailFromAbnormalId(abnormrecordid));
                                  txtMessagebox.Text = "第一步清除签卡明细完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
                              }
                              catch (Exception ex)
                              {
                                  txtMessagebox.Text = "第一步清除签卡异常：" + ex.ToString()
                                      + System.Environment.NewLine + txtMessagebox.Text;

                              }
                              try
                              {
                                  int i = OracleHelp.Excute(DeletAbnormrecordFromAbnormalId(abnormrecordid));
                                  txtMessagebox.Text = "第二步清除异常考勤完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
                              }
                              catch (Exception ex)
                              {
                                  txtMessagebox.Text = "第二步清除异常考勤异常：" + ex.ToString()
                                        + System.Environment.NewLine + txtMessagebox.Text;
                              }
                              MessageBox.Show("处理完毕！");
                          }
                        //UpdateEmployeeAttendancerecordById(attendancerecordid);
                    }
                }
            }
        }


        //第一步
        private string DeletSignindetailFromAbnormalId(string abnormrecordid)
        {
            string sql = @"delete from smthrm.t_hr_employeesignindetail d
                where d.abnormrecordid = '" + abnormrecordid + @"'";
            return sql;
        }
        //第二步
        private string DeletAbnormrecordFromAbnormalId(string abnormrecordid)
        {
            string sql = @"delete from smthrm.t_hr_employeeabnormrecord a
                             where a.abnormrecordid = '" + abnormrecordid + @"'";
            return sql;
        }
        #endregion

        private void AttendEmploee_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnPreviousMonth_Click(object sender, EventArgs e)
        {

            DateTime dtstart = new DateTime(DateTime.Now.Year, DateTime.Now.Month-1, 1);
            DateTime dtPrivious = DateTime.Now;
            DateTime dtEnd = new DateTime(dtPrivious.Year, dtPrivious.Month, 1).AddDays(-1);
            txtStartDate.Text = dtstart.ToString("yyyy-MM-dd");
            txtEndDate.Text = dtEnd.ToString("yyyy-MM-dd");

            GlobalParameters.StartDate = dtstart.ToString("yyyy-MM-dd");
            GlobalParameters.EndDate = dtEnd.ToString("yyyy-MM-dd");
        }

        #region 查询带薪假
        string leavetypesetid = string.Empty;
        private void btnCheckVacationName_Click(object sender, EventArgs e)
        {
           
            string sql = @"select l.leavetypename,l.leavetypesetid                            
                            from    smthrm.T_HR_AttendFreeLeave a
                            inner join smthrm.t_hr_leavetypeset l on a.leavetypesetid=l.leavetypesetid
                            inner join smthrm.t_hr_attendancesolution s on a.attendancesolutionid=s.attendancesolutionid
                            where s.attendancesolutionid='" + attendancesolutionid + @"' 
                            and l.leavetypename like '%" + txtVacationName.Text + @"%'";

            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt.Rows.Count>0)
            {
                txtVacationName.Text = dt.Rows[0]["leavetypename"].ToString();
                leavetypesetid = dt.Rows[0]["leavetypesetid"].ToString();
               
                txtMessagebox.Text = "检查带薪假名称完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "检查带薪假名称完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
                MessageBox.Show("检查带薪假名称完成，共：0条数据");
                return;
            }
            txtVacationStart.Text = DateTime.Now.Year + "-01-01";

            sql = @"select sum(t.days) days
                    from smthrm.T_HR_EMPLOYEELEVELDAYCOUNT t
                    inner join smthrm.T_HR_LEAVETYPESET b on t.leavetypesetid=b.leavetypesetid
                    where 
                    b.leavetypesetid='" + leavetypesetid + @"'
                    and t.employeeid='" + GlobalParameters.employeeid + @"'
                    and t.terminatedate>to_date('" + DateTime.Now.ToShortDateString() + @"','yyyy-MM-dd')
                    group by t.employeename,b.leavetypename";

            dt = OracleHelp.getTable(sql);
            if (dt.Rows.Count>0)
            {
                txtVacationTotolDay.Text = dt.Rows[0]["days"].ToString();
            }
            OracleHelp.close();
        }

        private void btnSelectVacation_Click(object sender, EventArgs e)
        {
            //查询休假记录来源
            string sql = @"select t.recordid,t.employeename,b.leavetypename,t.days,
                            t.efficdate,t.terminatedate,p.cname,p.departmentname,p.postname,
                            t.remark,t.createuserid,t.createdate,t.ownercompanyid
                            from smthrm.T_HR_EMPLOYEELEVELDAYCOUNT t
                            inner join smthrm.T_HR_LEAVETYPESET b on t.leavetypesetid=b.leavetypesetid
                            inner join smthrm.v_oragnization p on t.ownerpostid=p.postid
                            where  b.leavetypesetid='" + leavetypesetid+@"'
                            and t.employeeid='" + GlobalParameters.employeeid + @"'
                            and t.ownercompanyid='" + GlobalParameters.employeeMasterCompanyid + @"'
                            and t.terminatedate>to_date('" + (DateTime.Now.Year-1)+"-01-01" + @"','yyyy-MM-dd')";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dtVacationFrom.DataSource = dt;              
                txtMessagebox.Text = "查询休假记录来源完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询休假记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }

            //查询请假记录
            sql = @"--请假记录
                            select t.employeename,t.totalhours,t.startdatetime,t.enddatetime
                            ,s.leavetypename,t.checkstate,t.updatedate
                            ,t.employeeid,t.ownercompanyid,t.leaverecordid 
                            from smthrm.T_HR_EmployeeLeaveRecord t
                            inner join smthrm.t_hr_leavetypeset s on t.leavetypesetid=s.leavetypesetid
                            where s.leavetypesetid='" + leavetypesetid+@"'
                            and t.employeeid='" + GlobalParameters.employeeid + @"'
                            --and t.ownercompanyid='" + GlobalParameters.employeeMasterCompanyid + @"'
                            and t.startdatetime>=to_date('"+txtVacationStart.Text+@"','yyyy-MM-dd')
                            order by t.enddatetime desc";
            OracleHelp.Connect();
            DataTable dts = OracleHelp.getTable(sql);
            if (dts != null)
            {
                dtVacation.DataSource = dts;               
                txtMessagebox.Text = "查询休假记录完成，共：" + dts.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询休假记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            //txtVacationTotolDay.Text=decimal.Parse(txtVacationTotolDay.Text)-
            OracleHelp.close();
            
        }
        #endregion

        private void btnDeleEmployeeClockin_Click(object sender, EventArgs e)
        {
            DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("是否确认删除该员工所有打卡记录？时间：" + GlobalParameters.StartDate +"-"+ GlobalParameters.EndDate,//对话框的显示内容 

            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                string sql = @"delete smthrm.t_hr_employeeclockinrecord a
                     where a.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                       and a.ownerid = '" + GlobalParameters.employeeid + @"'
                       and a.punchdate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                       and a.punchdate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')";
                OracleHelp.Connect();
                int i = OracleHelp.Excute(sql);
                txtMessagebox.Text = "处理完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
                OracleHelp.close();
            }
        }

        #region 执行sql
        private void btnExcute_Click(object sender, EventArgs e)
        {
            string sql=txtSql.Text;
            OracleHelp.Connect();
            int i = OracleHelp.Excute(sql);
            txtMessagebox.Text = "处理完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
            OracleHelp.close();
        }
        #endregion

        private void dtVacationFrom_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = dtVacationFrom.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "ColumnDel")
                    {
                        //这里可以编写你需要的任意关于按钮事件的操作~
                        string recordid = dtVacationFrom.Rows[e.RowIndex].Cells["recordid"].EditedFormattedValue.ToString();
                        if (string.IsNullOrEmpty(recordid))
                        {
                            MessageBox.Show("请选择不为空的记录进行操作");
                            return;
                        }
                        DialogResult MsgBoxResult
                            = MessageBox.Show("确认是否继续操作？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                        if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                        {
                            try
                            {
                                string sql = @" delete from smthrm.T_HR_EMPLOYEELEVELDAYCOUNT t
                                                where t.recordid='" + recordid + "'";
                                int i = OracleHelp.Excute(sql);
                                txtMessagebox.Text = "处理完成，处理了：" + i + "条数据！" + System.Environment.NewLine + txtMessagebox.Text;
                            }
                            catch (Exception ex)
                            {
                                txtMessagebox.Text = "处理异常：" + ex.ToString()
                                    + System.Environment.NewLine + txtMessagebox.Text;

                            }                           
                        }
                        //UpdateEmployeeAttendancerecordById(attendancerecordid);
                    }
                }
            }
        }


        #region 根据设置的公共假期及工作日检查考勤初始化记录及考勤异常
        private void btnCheckAttByVacationSet_Click(object sender, EventArgs e)
        {
            DialogResult MsgBoxResult;//设置对话框的返回值
            MsgBoxResult = MessageBox.Show("开始处理选择日期的考勤异常，确认继续？",//对话框的显示内容 
            "提示",//对话框的标题 
            MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
            MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
            MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                Thread t = new Thread(new ThreadStart(startDealAttByVacationset));
                t.Start();
            }            
        }

        private void startDealAttByVacationset()
        {
            string strMsg =  "____________开始处理当前用户所在公司的公共假期设置考勤异常............";
            SetLog(strMsg);
            bool aleadyCheckEmployeeleved = false;

            OracleHelp.Connect();
            DataTable dtEmployees = OracleHelp.getTable(GetallEmployeeByCompanyid(GlobalParameters.employeeMasterCompanyid));
                    

            for (int i = 0; i < dtVacationDay.RowCount; i++)
            {
                string selectValue = dtVacationDay.Rows[i].Cells["dtVacationColumnSelect"].EditedFormattedValue.ToString();

                if (selectValue == "True")
                {
                    string daytype = dtVacationDay.Rows[i].Cells["daytype"].EditedFormattedValue.ToString();
                    string startdate = dtVacationDay.Rows[i].Cells["startdate"].EditedFormattedValue.ToString();
                    string enddate = dtVacationDay.Rows[i].Cells["enddate"].EditedFormattedValue.ToString();
                    DateTime dtstart = DateTime.Parse(startdate);
                    DateTime dtEnd = DateTime.Parse(enddate);
                    startdate = dtstart.ToString("yyyy-MM-dd");
                    enddate = dtEnd.ToString("yyyy-MM-dd");

                   if (dtEmployees != null && dtEmployees.Rows.Count > 0)
                    {
                        for (int j = 0; j < dtEmployees.Rows.Count; j++)
                        {                            
                            string employeeName = dtEmployees.Rows[j]["cname"].ToString() + "-" + dtEmployees.Rows[j]["departmentname"].ToString()
                                + dtEmployees.Rows[j]["postname"].ToString() + dtEmployees.Rows[j]["employeecname"].ToString();

                            GlobalParameters.StartDate = startdate;
                            GlobalParameters.EndDate = enddate;
                            GlobalParameters.employeeid = dtEmployees.Rows[j]["employeeid"].ToString();

                        
                            switch (daytype)
                            {
                                case "1"://假期，消除异常考勤并消除初始化考勤记录
                                    SetLog("*********开始处理员工：" + employeeName);
                                    dealVactionAtt();
                                    SetLog("*********结算处理员工的考勤异常：");
                                    break;
                                case "2"://工作日，初始化考勤并检查请假及异常
                                    if (!aleadyCheckEmployeeleved)
                                    {
                                        try
                                        {
                                            //初始化公司考勤记录
                                            AsignAttendanceSolutionForCompany();
                                        }
                                        catch (Exception ex)
                                        {
                                            SetLog("初始化公司考勤异常：" + ex.ToString());
                                            aleadyCheckEmployeeleved = true;
                                            return;
                                        }
                                        //检查请假及异常,整个公司只需要一次
                                        AttRdSvcClient.UpdateAttendRecordByEvectionAndLeaveRd(GlobalParameters.employeeMasterCompanyid, GlobalParameters.StartDate.Substring(0, 7));
                                        SetLog("检查整个公司请假出差完成");
                                        //检查整个公司考勤异常
                                        AttRdSvcClient.CheckAbnormRdForCompanyByDate(GlobalParameters.employeeMasterCompanyid, dtstart, dtEnd, ref strMsg);
                                        SetLog("检查公司" + GlobalParameters.employeeMasterCompanyid + " " + startdate + "---" + enddate + " 所有异常完成:" + strMsg);
                                        aleadyCheckEmployeeleved = true;
                                    }
                                  
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            OracleHelp.close();
            SetLog("____________结束处理当前用户所在公司的所有员工公共假期设置考勤异常......");
        }

        private void dealVactionAtt()
        {
            int j = OracleHelp.Excute(CleareEployeesignindetail1());
            SetLog("第一步清除签卡明细完成，处理了：" + j + "条数据！");
            j = OracleHelp.Excute(CleareEployeesigninPersonalrecord());
            SetLog("第二步清除签卡我的单据完成，处理了：" + j + "条数据！");
            j = OracleHelp.Excute(Clearet_wf_dotask3());
            SetLog("第三步清除签卡待办完成，处理了：" + j + "条数据！");
            j = OracleHelp.Excute(CleareEmployeesigninrecordMaster4());
            SetLog("第四步清除签卡主表完成，处理了：" + j + "条数据！");
            j = OracleHelp.Excute(CleareEmployeeabnormrecord5());
            SetLog("第五步清除考勤异常完成，处理了：" + j + "条数据！");
            j = OracleHelp.Excute(deleteAttanRecordByDate());
            SetLog("第六步删除考勤初始化记录完成，处理了：" + j + "条数据！");            

            //MessageBox.Show("处理完成");
        }
        /// <summary>
        /// 通过公司id获取所有主岗位在职员工
        /// </summary>
        /// <param name="companyid"></param>
        /// <returns></returns>
        private string GetallEmployeeByCompanyid(string companyid)
        {
            string sql = @"
                            select 
                            e.employeeid,
                            e.editstate,
                            e.employeecname,
                            pd.postname,
                            dd.departmentname,
                            c.cname 
                            from smthrm.t_hr_company c
                            left join smthrm.t_hr_department d on c.companyid =d.companyid
                            left join smthrm.t_hr_departmentdictionary dd on d.departmentdictionaryid=dd.departmentdictionaryid
                            left join smthrm.t_hr_post p on d.departmentid=p.departmentid
                            left join smthrm.t_hr_postdictionary pd on p.postdictionaryid=pd.postdictionaryid
                            left join smthrm.t_hr_employeepost ep on ep.postid=p.postid
                            left join smthrm.t_hr_employee e on e.employeeid=ep.employeeid
                            where c.companyid='" + companyid + @"'
                            and ep.checkstate=2
                            and ep.editstate=1
                            and ep.isagency=0
                            order by c.cname";

            return sql;
        }
        /// <summary>
        /// 删除员工指定日期考勤初始化记录
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="Companyid"></param>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        private string deleteAttanRecordByDate()
        {
            string sql = @"delete smthrm.t_hr_attendancerecord a
                     where a.ownercompanyid = '" + GlobalParameters.employeeMasterCompanyid + @"'
                       and a.ownerid = '" + GlobalParameters.employeeid + @"'
                       and a.attendancedate >= To_Date('" + GlobalParameters.StartDate + @"', 'yyyy-MM-dd')
                       and a.attendancedate <= To_Date('" + GlobalParameters.EndDate + @"', 'yyyy-MM-dd')";

            return sql;

        }

        /// <summary>
        /// 初始化整个公司考勤
        /// </summary>
        private void AsignAttendanceSolutionForCompany()
        {
            SetLog("====================================开始初始化整个公司考勤记录");
            AttendanceServiceClient AttRdSvc = new AttendanceServiceClient();
            AttRdSvc.AsignAttendanceSolutionByOrgIDAndMonth("1", GlobalParameters.employeeMasterCompanyid, GlobalParameters.StartDate.Substring(0, 7));
            SetLog("====================================初始化整个公司考勤记录完毕");
        }
        #endregion




        #region 跨线程调用UI控件显示消息
        delegate void DelShow(String Msg); //代理
        //将对控件的操作写到一个函数中 
        public void SetLog(String para)
        {
            if (!txtMessagebox.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                txtMessagebox.Text = DateTime.Now.ToLongTimeString() + " " + para + System.Environment.NewLine + txtMessagebox.Text;
            }
            else //非创建线程，用代理进行操作
            {
                DelShow ds = new DelShow(SetLog);
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

        #region 处理调休假及生效日期
        private void btnUpdateEffectDate_Click(object sender, EventArgs e)
        {
            DialogResult MsgBoxResult
                             = MessageBox.Show("确认是否修改当前员工指定假期生效日期？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
            {
                string sql = @"update smthrm.T_HR_EMPLOYEELEVELDAYCOUNT t 
                            set t.terminatedate=add_months(t.efficdate," + txtAvailableMonth.Text + @")
                            where t.leavetypesetid='" + leavetypesetid + @"'
                            and t.employeeid='" + GlobalParameters.employeeid + @"'
                            and t.ownercompanyid='" + GlobalParameters.employeeMasterCompanyid + @"'";

                OracleHelp.Connect();
                int i = OracleHelp.Excute(sql);
                SetLog("更新了" + i + "条数据");
                OracleHelp.close();
            }
        }

        private void btnUpdateAllEffectDate_Click(object sender, EventArgs e)
        {
              DialogResult MsgBoxResult
                            = MessageBox.Show("确认是否修改当前员工所在公司所有员工指定假期生效日期？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
              if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
              {
                  string sql = @"update smthrm.T_HR_EMPLOYEELEVELDAYCOUNT t 
                            set t.terminatedate=add_months(t.efficdate," + txtAvailableMonth.Text + @")
                            where t.leavetypesetid='" + leavetypesetid + @"'                           
                            and t.ownercompanyid='" + GlobalParameters.employeeMasterCompanyid + @"'";

                  OracleHelp.Connect();
                  int i = OracleHelp.Excute(sql);
                  SetLog("更新了" + i + "条数据");
                  OracleHelp.close();
              }
        }
        #endregion

        #region 出差记录删除
        private void dtGEmployeeEvectionRecord_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //for (int i = 0; i < dtGEmployeeEvectionRecord.RowCount; i++)
            //{
            //    string selectValue = dtGEmployeeEvectionRecord.Rows[i].Cells["dtEvectionCheckBoxColumn"].EditedFormattedValue.ToString();

            //    if (selectValue == "True")
            //    {
            //        string evectionrecordid = dtGEmployeeEvectionRecord.Rows[i].Cells["evectionrecordid"].EditedFormattedValue.ToString();
            //    }
            //}

            if (e.RowIndex >= 0)
            {
                 string selectValue = dtGEmployeeEvectionRecord.Rows[e.RowIndex].Cells["dtEvectionCheckBoxColumn"].EditedFormattedValue.ToString();

                 if (selectValue == "True")
                 {
                     DataGridViewColumn column = dtGEmployeeEvectionRecord.Columns[e.ColumnIndex];
                     if (column is DataGridViewButtonColumn)
                     {
                         if (column.Name == "ColumnDelete")
                         {
                             DialogResult MsgBoxResult
                                 = MessageBox.Show("确认是否删除选中的出差记录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                             if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                             {
                                 string evectionrecordid = dtGEmployeeEvectionRecord.Rows[e.RowIndex].Cells["evectionrecordid"].EditedFormattedValue.ToString();
                                 deleteEvection(evectionrecordid);
                             }
                         }
                     }
                 }
            }

        }

        private void deleteEvection(string evectionrecordid)
        {
            string sql = @"delete from smthrm.T_HR_EmployeeEvectionRecord t
                        where t.evectionrecordid='" + evectionrecordid + "'";
            OracleHelp.Connect();
            int i = OracleHelp.Excute(sql);
            SetLog("删除了" + i + "条数据");
            OracleHelp.close();
            GetEmployeeEvection();
        }

        /// <summary>
        /// 获取出差记录
        /// </summary>
        private void GetEmployeeEvection()
        {
            string sql = @"select t.employeename,t.startdate,t.enddate,t.starttime,t.endtime,t.totaldays,t.employeename,t.evectionreason 
                        ,t.evectionrecordid
                        from smthrm.T_HR_EmployeeEvectionRecord t
                        where t.employeeid='" + GlobalParameters.employeeid + @"'
                        and t.startdate>=to_date('" + GlobalParameters.StartDate + @"','yyyy-mm-dd')
                        and t.enddate<=to_date('" + GlobalParameters.EndDate + @"','yyyy-mm-dd')    ";
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                this.dtGEmployeeEvectionRecord.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询员工出差记录完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询员工出差记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
        }
        #endregion


        #region 查询加班及生产调休假
        private void btnGetOvertimeRecord_Click(object sender, EventArgs e)
        {
            GetOvertimeRecord();
        }

        private void GetOvertimeRecord()
        {
            #region 获取假期设置
            string datestar = DateTime.Now.Year + "-01-01";
            string sql = @"select t.employeename,
                                   t.startdate,
                                   t.enddate,
                                   t.overtimehours,
                                   t.paycategory,
                                   t.overtimerecordid,
                                   t.checkstate,
                                   t.createdate,
                                   t.updatedate
                              from smthrm.T_HR_EmployeeOverTimeRecord t
                            where t.employeeid ='" + GlobalParameters.employeeid + @"'
                            and t.startdate >=  to_date('"+datestar+@"', 'yyyy-mm-dd')
                            order by t.startdate desc";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                dtOverTime.DataSource = dt;
                OracleHelp.close();
                txtMessagebox.Text = "查询加班完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                txtMessagebox.Text = "查询加班完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion
        }
        #endregion

        private void btnGenarat_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtOverTime.RowCount; i++)
            {
                string selectValue = dtOverTime.Rows[i].Cells["dtOverTimeCheckBoxColumn"].EditedFormattedValue.ToString();

                if (selectValue == "True")
                {
                    string evectionrecordid = dtOverTime.Rows[i].Cells["overtimerecordid"].EditedFormattedValue.ToString();
                    string checkState = dtOverTime.Rows[i].Cells["checkstate"].EditedFormattedValue.ToString();
                    AttRdSvcClient.AuditOverTimeRd(evectionrecordid, checkState);
                    SetLog("生成调休假完成：" + " evectionrecordid:" + evectionrecordid + " checkState:" + checkState);

                }
            }
        }
    }

    
}
