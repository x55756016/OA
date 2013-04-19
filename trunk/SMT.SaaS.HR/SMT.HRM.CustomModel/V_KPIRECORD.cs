/// <summary>
/// Log No.： 1
/// Create Desc： 显示用户信息（流程+模块取不到）
/// Creator： 冉龙军
/// Create Date： 2010-08-10
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;

namespace SMT.HRM.CustomModel
{
    public class V_KPIRECORD
    {
        public V_KPIRECORD() { }
        public T_HR_KPIRECORD T_HR_KPIRECORD { get; set; } //KPI明细
        public string EMPLOYEECODE { get; set; } //员工编号
        public string EMPLOYEECNAME { get; set; } //员工中文名
        public string KPIPOINTREMARK { get; set; } //KPI点描述
        public string FLOWID { get; set; } //流程
        public string CREATEUSERID { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERCOMPANYID { get; set; }
    }
}
