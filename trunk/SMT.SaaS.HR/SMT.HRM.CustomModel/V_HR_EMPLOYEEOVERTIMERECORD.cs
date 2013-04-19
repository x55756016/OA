/*
 * 文件名：V_HR_EMPLOYEEOVERTIMERECORD.cs
 * 作  用：加班记录实体扩展类
 * 创建人：吴鹏
 * 创建时间：2010年1月26日, 15:13:31
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_HR_EMPLOYEEOVERTIMERECORD
    {
        public V_HR_EMPLOYEEOVERTIMERECORD()
        { }

        /// <summary>
        /// 打卡记录号
        /// </summary>
        public string OVERTIMERECORDID { get; set; }

        /// <summary>
        /// 加班开始时间
        /// </summary>
        public string STARTDATETIME { get; set; }

        /// <summary>
        /// 加班结束时间
        /// </summary>
        public string ENDDATETIME { get; set; }

        /// <summary>
        /// 加班时长
        /// </summary>
        public decimal? OVERTIMEHOURS { get; set; }

        /// <summary>
        /// 加班类别
        /// </summary>
        public string OVERTIMECATE { get; set; }

        /// <summary>
        /// 加班薪酬方式
        /// </summary>
        public string PAYCATEGORY { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public string CHECKSTATE { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }

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
