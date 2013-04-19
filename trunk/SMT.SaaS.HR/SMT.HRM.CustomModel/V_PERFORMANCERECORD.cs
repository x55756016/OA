/// <summary>
/// Log No.： 1
/// Create Desc： 绩效考核汇总显示用户信息
/// Creator： 冉龙军
/// Create Date： 2010-08-11
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;

namespace SMT.HRM.CustomModel
{
    public class V_PERFORMANCERECORD
    {
        public V_PERFORMANCERECORD() { }
        public T_HR_PERFORMANCERECORD T_HR_PERFORMANCERECORD { get; set; } //绩效明细
        
        public string EMPLOYEECODE { get; set; } //员工编号
        public string EMPLOYEECNAME { get; set; } //员工中文名
        public DateTime? SUMSTART { get; set; } //汇总开始时间
        public DateTime? SUMEND { get; set; } //汇总开始时间
        public string SUMID { get; set; }
        public string PERFORMANCEID { get; set; } 
        public string CREATEUSERID { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERCOMPANYID { get; set; }
       
        // 1s
        //正确
        //public T_HR_PERFORMANCERECORD T_HR_PERFORMANCERECORD { get; set; } //绩效明细
        //public T_HR_KPIRECORD T_HR_KPIRECORD { get; set; } //KPI明细
        
        //public string EMPLOYEECODE { get; set; } //员工编号
        //public string EMPLOYEECNAME { get; set; } //员工中文名
        // 1e
    }
}
