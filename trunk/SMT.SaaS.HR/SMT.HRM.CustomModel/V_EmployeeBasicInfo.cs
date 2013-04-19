using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工报表 用来记录员工报表导出
    /// </summary>
    public class V_EmployeeBasicInfo
    {
        #region 主要信息
        
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEECNAME { get; set; }
        /// <summary>
        /// 由总公司名称  没有则填写神州通集团
        /// </summary>
        public string ORGANIZENAME { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string COMPANYNAME { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DEPARTMENTNAME { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string SEX { get; set; }
        /// <summary>
        /// 婚否
        /// </summary>
        public string ISMARRY { get; set; }
        /// <summary>
        /// 身份证
        /// </summary>
        public string IDNUMBER { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BIRTHDAY { get; set; }
        /// <summary>
        /// 社保缴交时间
        /// </summary>
        public string SOCIALSERVICEYEAR { get; set; }

        #endregion
        #region 岗位信息
        /// <summary>
        /// 岗位
        /// </summary>
        public string POSTNAME { get; set; }
        /// <summary>
        /// 入职时间
        /// </summary>
        public DateTime? ENTRYDATE { get; set; }
        /// <summary>
        /// 转正时间
        /// </summary>
        public DateTime? BEREGULARDATE { get; set; }
        /// <summary>
        /// 在公司的工龄
        /// </summary>
        public decimal WORKAGE { get; set; }
        /// <summary>
        /// 岗位所属公司
        /// </summary>
        public string OWNERPOSTCOMPANYID { get; set; }
        /// <summary>
        /// 岗位所属部门
        /// </summary>
        public string OWNERPOSTDEPARTMENT { get; set; }
        #endregion
        #region 教育信息
        /// <summary>
        /// 学历
        /// </summary>
        public string TOPEDUCATION { get; set; }
        /// <summary>
        /// 专业
        /// </summary>
        public string SPECIALTY { get; set; }
        /// <summary>
        /// 毕业院校
        /// </summary>
        public string GRADUATESCHOOL { get; set; }
        /// <summary>
        /// 毕业时间
        /// </summary>
        public DateTime? GRADUATEDATE { get; set; }
        #endregion
        #region 其它信息
        /// <summary>
        /// 籍贯
        /// </summary>
        public string PROVINCE { get; set; }
        /// <summary>
        /// 户籍地址
        /// </summary>
        public string REGRESIDENCE { get; set; }
        /// <summary>
        /// 现居住地址
        /// </summary>
        public string CURRENTADDRESS { get; set; }
        /// <summary>
        /// 家庭详细地址
        /// </summary>
        public string FAMILYADDRESS { get; set; }
        /// <summary>
        /// 紧急联系人
        /// </summary>
        public string URGENCYPERSON { get; set; }
        /// <summary>
        /// 紧急联系人方式
        /// </summary>
        public string URGENCYCONTACT { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string MOBILE { get; set; }
        /// <summary>
        /// 兴趣爱好
        /// </summary>
        public string INTERESTCONTENT { get; set; }
        #endregion
        #region 个人合同信息
        /// <summary>
        /// 第一次合同期限
        /// </summary>
        public string FIRSTCONTRACTDEADLINE { get; set; }
        /// <summary>
        /// 第一次合同终止时间
        /// </summary>
        public DateTime? FIRSTCONTRACTENDDATE { get; set; }

        /// <summary>
        /// 第二次合同期限
        /// </summary>
        public string SECONDCONTRACTDEADLINE { get; set; }
        /// <summary>
        /// 第二次合同终止时间
        /// </summary>
        public DateTime? SECONDCONTRACTENDDATE { get; set; }
        /// <summary>
        /// 第三次合同期限
        /// </summary>
        public string THIIRDCONTRACTDEADLINE { get; set; }
        /// <summary>
        /// 第三次合同终止时间
        /// </summary>
        public DateTime? THIRDCONTRACTENDDATE { get; set; }
        /// <summary>
        /// 培训协议终止时间
        /// </summary>
        public DateTime? LEARNERSHIPENDDATE { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
        
        #endregion
        #region 公共信息
        /// <summary>
        /// 所属人ID
        /// </summary>
        public string OWNERID { get; set; }
        /// <summary>
        /// 所属用户岗位ID
        /// </summary>
        public string OWNERPOSTID { get; set; }
        /// <summary>
        /// 所属部门ID
        /// </summary>
        public string OWNERDEPARTMENTID { get; set; }
        /// <summary>
        /// 所属公司ID
        /// </summary>
        public string OWNERCOMPANYID { get; set; }
        /// <summary>
        /// 添加人ID
        /// </summary>
        public string CREATEUSERID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATEDATE { get; set; }
        /// <summary>
        /// 异动时间
        /// </summary>
        public DateTime? CHANGETIME { get; set; }
        /// <summary>
        /// 异动前公司ID
        /// </summary>
        public string OLDCOMPANYID { get; set; }
        /// <summary>
        /// 异动后公司ID
        /// </summary>
        public string OLDDEPARTMENTID { get; set; }
        /// <summary>
        /// 异动前部门ID
        /// </summary>
        public string NEXTCOMPANYID { get; set; }
        /// <summary>
        /// 异动后部门ID
        /// </summary>
        public string NEXTDEPARTMENTID { get; set; }
        
        #endregion
        
    }
}
