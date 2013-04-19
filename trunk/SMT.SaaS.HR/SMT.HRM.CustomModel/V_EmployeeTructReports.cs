using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工结构报表
    /// </summary>
    public class V_EmployeeTructReports
    {
        /// <summary>
        /// 机构/部门
        /// </summary>
        public string ORGANIZATIONNAME { get; set; }
        /// <summary>
        /// 在编人数
        /// </summary>
        public decimal ONPOSITIONCOUNT { get; set; }
        /// <summary>
        /// 已婚人数
        /// </summary>
        public decimal MARRIEDCOUNT { get; set; }
        /// <summary>
        /// 未婚人数
        /// </summary>
        public decimal NOMARRYCOUNT { get; set; }
        /// <summary>
        /// 男性数量
        /// </summary>
        public decimal MANCOUNT { get; set; }
        /// <summary>
        /// 女性数量
        /// </summary>
        public decimal WOMANCOUNT { get; set; }
        /// <summary>
        /// 大专及其以下人数
        /// </summary>
        public decimal EDUCATIONASSOCIATECOUNT {get;set;}
        /// <summary>
        /// 本科人数
        /// </summary>
        public decimal EDUCATIONUNDERGRADUTECOUNT {get;set;}
        /// <summary>
        /// 硕士人数
        /// </summary>
        public decimal EDUCATIONMASTERCOUNT { get; set; }
        /// <summary>
        /// 博士人数
        /// </summary>
        public decimal EDUCATIONDOCTORCOUNT { get; set; }
        /// <summary>
        /// 25岁或以下人数(男)
        /// </summary>
        public decimal MANAGETWENTYFIVECOUNT { get; set; }
        /// <summary>
        /// 26-35岁人数(男)
        /// </summary>
        public decimal MANAGETHIRTYFIVECOUNT { get; set; }
        /// <summary>
        /// 36-45岁人数(男)
        /// </summary>
        public decimal MANAGEFORTYFIVECOUNT { get; set; }
        /// <summary>
        /// 46-45岁人数(男)
        /// </summary>
        public decimal MANAGEFIFTYFIVECOUNT { get; set; }
        /// <summary>
        /// 55岁以上人数(男)
        /// </summary>
        public decimal MANAGEOVERFIFTYFIVECOUNT { get; set; }
        /// <summary>
        /// 25岁或以下人数(女)
        /// </summary>
        public decimal WOMANAGETWENTYFIVECOUNT { get; set; }
        /// <summary>
        /// 26-35岁人数(女)
        /// </summary>
        public decimal WOMANAGETHIRTYFIVECOUNT { get; set; }
        /// <summary>
        /// 36-45岁人数(女)
        /// </summary>
        public decimal WOMANAGEFORTYFIVECOUNT { get; set; }
        /// <summary>
        /// 46-45岁人数(女)
        /// </summary>
        public decimal WOMANAGEFIFTYFIVECOUNT { get; set; }
        /// <summary>
        /// 55岁以上人数(女)
        /// </summary>
        public decimal WOMANAGEOVERFIFTYFIVECOUNT { get; set; }
        /// <summary>
        /// 工龄1年人数(含1年)
        /// </summary>
        public decimal WORKAGEONE { get; set; }
        /// <summary>
        /// 工龄1-3年（含3年）人数
        /// </summary>
        public decimal WORKAGETHREE { get; set; }
        /// <summary>
        /// 工龄3-5年(含5年)
        /// </summary>
        public decimal WORKAGEFIVE { get; set; }
        /// <summary>
        /// 工龄5-8年(含8年)
        /// </summary>
        public decimal WORKAGEEIGHT { get; set; }
        /// <summary>
        /// 工龄8-10年(不含10年)
        /// </summary>
        public decimal WORKAGETEN { get; set; }
        /// <summary>
        /// 超过10年(含10年)
        /// </summary>
        public decimal WORKAGEOVERTEN { get; set; }
        /// <summary>
        /// 员工层人数(不包括管理层)
        /// </summary>
        public decimal EMPLOYEECOUNT { get; set; }
        /// <summary>
        /// 基层管理者人数(由工作岗位决定)
        /// </summary>
        public decimal PRIMARYMANAGERCOUNT { get; set; }
        /// <summary>
        /// 中层管理者人数(由工作岗位决定)
        /// </summary>
        public decimal MIDDLEMANAGERCOUNT { get; set; }
        /// <summary>
        /// 高层管理者人数(由工作岗位决定)
        /// </summary>
        public decimal ADVANCEMANAGERCOUNT { get; set; }
        /// <summary>
        /// 领导层人数
        /// </summary>
        public decimal LEADERSHIPCOUNT { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
        

    }
}
