
using System.Collections.Generic;
using System;
using SMT.HRM.UI.Views.Attendance;
using SMT.HRM.UI.Form.Attendance;
namespace SMT.HRM.UI
{
    public partial class UIDictionary
    {
        private static void GetUIDictionary_AKH()
        {
            #region 考勤记录

            //员工打卡记录View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ClockInRd), new List<string> { "CHECKSTATE", "CLOCKINRDUPLOADTYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ImportEmpClockInRdForm), new List<string> { "ASSIGNEDOBJECTTYPE", "CLOCKINRDUPLOADTYPE" }));
            //异常考勤查询View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ExceptionAttRd), new List<string> { "CHECKSTATE", "ABNORMCATEGORY", "ATTENDPERIOD", "SINGINSTATE" }));
            //异常签卡申请View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SignInRd), new List<string> { "REASONCATEGORY", "ABNORMCATEGORY", "ATTENDPERIOD", "SINGINSTATE", }));
            //异常签卡申请Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SignInRdForm), new List<string> { "REASONCATEGORY", "ABNORMCATEGORY", "ATTENDPERIOD", "SINGINSTATE", }));
            //员工请假申请View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeLeaveRecord), new List<string> { "CHECKSTATE", }));
            //员工请假申请Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeLeaveRecordForm), new List<string> { "CHECKSTATE", "LEVELDAYVACATIONTYPE" ,"ISCHECKED" }));
            //出差记录统计View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Evection), new List<string> { "SUBSIDYTYPE", "CHECKSTATE", }));
            //出差报告View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EvectionReport), new List<string> { "SUBSIDYTYPE", "CHECKSTATE", }));
            //员工加班申请View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(OverTime), new List<string> { "CHECKSTATE", }));
            //员工加班申请Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(OverTimeForm), new List<string> { "CHECKSTATE", }));
            //员工销假申请View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TerminateLeave), new List<string> { "CHECKSTATE", }));
            //员工销假申请Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TerminateLeaveForm), new List<string> { "CHECKSTATE", }));
            //带薪假期View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeLeaveDays), new List<string> { "CHECKSTATE", "ISPERFECTATTENDANCEFACTOR" ,"LEAVETYPEVALUE" }));

            #endregion

            #region 考勤设置

            //公共假期设置View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PublicVacation), new List<string> { "CHECKSTATE", "COUNTYTYPE", "OUTPLANDAYSTYPE","ASSIGNEDOBJECTTYPE" }));
            //公共假期设置Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PublicVacationForm), new List<string> { "CHECKSTATE", "COUNTYTYPE", "OUTPLANDAYSTYPE" }));
            //假期标准设置View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(LeaveTypeSet), new List<string> { "CHECKSTATE", "LEAVETYPEVALUE", "POSTLEVEL", "LEAVEFINETYPE", "ISCHECKED" }));
            //假期标准设置Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(LeaveTypeSetForm), new List<string> { "CHECKSTATE", "LEAVETYPEVALUE", "POSTLEVEL", "LEAVEFINETYPE", "ISCHECKED" }));
            //带薪假期统计View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FreeLeaveDaySet), new List<string> { "CHECKSTATE", "OFFESTTYPE" }));
            //带薪假期统计Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FreeLeaveDaySetForm), new List<string> { "CHECKSTATE", "OFFESTTYPE" }));
            //考勤扣款标准View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendanceDeductMaster), new List<string> { "CHECKSTATE", "ATTENDABNORMALTYPE", "ATTEXFINETYPE", "ISCHECKED" }));
            //考勤扣款标准Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendanceDeductMasterForm), new List<string> { "ATTENDABNORMALTYPE", "ATTEXFINETYPE", "ISCHECKED" }));
            //考勤扣款标准明细Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendanceDeductDetailForm), new List<string> { "ATTENDABNORMALTYPE", "ATTEXFINETYPE", "ISCHECKED" }));
            //加班类别设置View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(OvertimeRewardSet), new List<string> { "CHECKSTATE", "OVERTIMECATE", "OVERTIMEPAYTYPE" }));
            //加班类别设置Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(OvertimeRewardSetForm), new List<string> { "CHECKSTATE", "OVERTIMECATE", "OVERTIMEPAYTYPE" }));
            //打卡时间设置View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ShiftDefine), new List<string> { "CHECKSTATE" }));
            //打卡时间设置Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ShiftDefineForm), new List<string> { "CHECKSTATE" }));
            //作息方案设置View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SchedulingTemplate), new List<string> { "CHECKSTATE", "SCHEDULINGCIRCLETYPE" }));
            //作息方案设置Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SchedulingTemplateForm), new List<string> { "CHECKSTATE", "SCHEDULINGCIRCLETYPE" }));

            #endregion

            //#region 考勤方案

            ////考勤方案定义View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttSolRd), new List<string> { "CHECKSTATE", "ATTENDANCETYPE", "CARDTYPE", "WORKDAYTYPE", "ISCHECKED" }));
            ////考勤方案分配View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendanceSolutionAsign), new List<string> { "CHECKSTATE", "ASSIGNEDOBJECTTYPE" }));

            //#endregion

            #region 考勤结算

            ////月度考勤结算View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendMonthlyBalance), new List<string> { "CHECKSTATE" }));

            //月度考勤结算导入
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CalculateEmployeeAttendanceMonthlyForm), new List<string> { "ASSIGNEDOBJECTTYPE" }));

            //月度考勤结算导入
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ImportAttendbalanceForm), new List<string> { "ASSIGNEDOBJECTTYPE"}));

            //月度考勤结算批量审核
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendMonthlyBalanceAudit), new List<string> { "ASSIGNEDOBJECTTYPE" }));


            ////年度考勤结算View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AttendYearlyBalance), new List<string> { "CHECKSTATE" }));

            #endregion
        }
    }

}
