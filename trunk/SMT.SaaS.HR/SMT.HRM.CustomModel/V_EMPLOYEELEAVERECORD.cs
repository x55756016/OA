using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEELEAVERECORD
    {
        public V_EMPLOYEELEAVERECORD()
        { }
        
        //当前请假记录表
        public SMT_HRM_EFModel.T_HR_EMPLOYEELEAVERECORD EmployeeLeaveRecord { get; set; }
        //当前请假调休记录表
        public List<SMT_HRM_EFModel.T_HR_ADJUSTLEAVE> AdjustLeave { get; set; }
        //当前请假调休记录表
        public List<V_EMPLOYEELEAVE> EmployeeLeave { get; set; }
        //工作时长
        public decimal WorkTimePerDay { get; set; }
    }
}
