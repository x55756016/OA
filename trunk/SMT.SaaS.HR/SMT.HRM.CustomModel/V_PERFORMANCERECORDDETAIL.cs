
/// <summary>
/// Log No.： 1
/// Create Desc： 个人绩效考核明细
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
    public class V_PERFORMANCERECORDDETAIL
    {
        public T_HR_PERFORMANCERECORD T_HR_PERFORMANCERECORD { get; set; } //绩效明细
        public T_HR_KPIRECORD T_HR_KPIRECORD { get; set; } //KPI明细

        public string EMPLOYEECODE { get; set; } //员工编号
        public string EMPLOYEECNAME { get; set; } //员工中文名
        public string FLOWNAME { get; set; } //员工中文名
        public string PERFORMANCEID { get; set; } 
    }
}
