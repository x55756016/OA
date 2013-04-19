using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.HRM.UI
{
    public class ReportsType
    {
        public enum Reports
        { 
            /// <summary>
            /// 员工报表
            /// </summary>
            EmployeeInfos,
            /// <summary>
            /// 离职报表
            /// </summary>
            EmployeeLeftOffice,
            /// <summary>
            /// 员工异动报表
            /// </summary>
            EmployeeChange,
            /// <summary>
            /// 员工统计报表
            /// </summary>
            EmployeeCollect,
            /// <summary>
            /// 员工结构报表
            /// </summary>
            EmployeeTruct,
            /// <summary>
            /// 员工薪酬明细报表
            /// </summary>
            EmployeePension
        }

    }
}
