/*
 * 文件名：V_COMPLAINRECORD.cs
 * 作  用：薪资发放实体扩展类
 * 创建人： 喻建华
 * 创建时间：2010年10月15日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;

namespace SMT.HRM.CustomModel
{
    public class V_COMPLAINRECORD
    {
        /// <summary>
        /// 申诉实体
        /// </summary>
        public T_HR_KPIRECORDCOMPLAIN T_HR_KPIRECORDCOMPLAIN { get; set; }
        /// <summary>
        /// KPI明细记录
        /// </summary>
        public T_HR_KPIRECORD T_HR_KPIRECORD { get; set; } 
        /// <summary>
        /// 员工编号
        /// </summary>
        public string EMPLOYEECODE { get; set; }       
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEECNAME { get; set; }
        /// <summary>
        /// 流程ID
        /// </summary>
        public string FLOWID { get; set; } 
        public string CREATEUSERID { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERCOMPANYID { get; set; }
    }
}
