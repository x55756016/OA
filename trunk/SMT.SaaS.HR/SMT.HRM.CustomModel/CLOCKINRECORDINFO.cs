/*
 * 文件名：CLOCKINRECORDINFO.cs
 * 作  用：日常考勤打卡实体扩展类
 * 创建人：吴鹏
 * 创建时间：2010年1月21日, 15:37:12
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class CLOCKINRECORDINFO
    {
        public CLOCKINRECORDINFO()
        {
 
        }

        /// <summary>
        /// 打卡记录号
        /// </summary>
        public string CLOCKINRECORDID { get; set; }

        /// <summary>
        /// 打卡日期
        /// </summary>
        public DateTime? CLOCKINDATE { get; set; }

        /// <summary>
        /// 打卡时间
        /// </summary>
        public string CLOCKINTIME { get; set; }

        /// <summary>
        /// 员工编号
        /// </summary>
        public string EMPLOYEECODE { get; set; }

        /// <summary>
        /// 员工记录号
        /// </summary>
        public string EMPLOYEEID { get; set; }

        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEENAME { get; set; }

        /// <summary>
        /// 部门序号
        /// </summary>
        public string DEPARTMENTID { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DEPARTMENTNAME { get; set; }

        /// <summary>
        /// 公司序号
        /// </summary>
        public string COMPANYID { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string COMPANYNAME { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        public string POSITIONNAME { get; set; }
    }
}
