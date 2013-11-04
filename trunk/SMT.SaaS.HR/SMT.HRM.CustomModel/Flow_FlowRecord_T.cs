using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using SMT.SaaS.BLLCommonServices.FlowWFService;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 流程记录表实体
    /// </summary>
    public class Flow_FlowRecord_T : System.Data.Objects.DataClasses.EntityObject
    {
        public Flow_FlowRecord_T()
        {
            this.CreateCompanyID = string.Empty;
            this.CreateDepartmentID = string.Empty;
            this.CreatePostID = string.Empty;
            this.CreateUserID = string.Empty;
            this.EditUserName = string.Empty;
            this.EditUserID = string.Empty;
            this.Content = string.Empty;
            this.Flag = string.Empty;
            this.FlowCode = string.Empty;
            this.FormID = string.Empty;
            this.GUID = string.Empty;
            this.InstanceID = string.Empty;
            this.ModelCode = string.Empty;
            this.ParentStateCode = string.Empty;
            this.StateCode = string.Empty;
            this.XmlObject = "";
            this.SystemCode = string.Empty;
            this.BusinessObjectDefineXML = string.Empty;
        }
        #region 流程记录表属性

        public string CreateCompanyID { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateDepartmentID { get; set; }
        /// <summary>
        /// 创建人职位名
        /// </summary>
        public string CreatePostID { get; set; }
        /// <summary>
        /// 创建用户ID
        /// </summary>
        public string CreateUserID { get; set; }
        /// <summary>
        /// 创建用户名
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 编辑时间
        /// </summary>
        public DateTime? EditDate { get; set; }
        /// <summary>
        /// 编辑人ID
        /// </summary>
        public string EditUserID { get; set; }
        /// <summary>
        /// 编辑人姓名
        /// </summary>
        public string EditUserName { get; set; }
        /// <summary>
        /// 标志
        /// </summary>
        public string Flag { get; set; }
        /// <summary>
        /// 流程编码
        /// </summary>
        public string FlowCode { get; set; }
        /// <summary>
        /// 表单ID
        /// </summary>
        public string FormID { get; set; }
        /// <summary>
        /// 标识ID
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// 实例编码
        /// </summary>
        public string InstanceID { get; set; }
        /// <summary>
        /// 模块编码
        /// </summary>
        public string ModelCode { get; set; }
        /// <summary>
        /// 父状态编码
        /// </summary>
        public string ParentStateCode { get; set; }
        /// <summary>
        /// 状态编码
        /// </summary>
        public string StateCode { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 流程关联的XML数据
        /// </summary>
        public string XmlObject { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>

        public string SystemCode { get; set; }

        /// <summary>
        /// 模块对应的业务对象定义
        /// </summary>
        public string BusinessObjectDefineXML { get; set; }

        #endregion
    }

    /// <summary>
    /// 审核事件参数
    /// </summary>
    public class AuditEventArgs : EventArgs
    {
        public AuditEventArgs(AuditResult auditResult, DataResult innerResult)
        {
            result = auditResult;
            InnerResult = innerResult;
            this.Action = AuditAction.Audit;
        }
        private AuditResult result = AuditResult.Cancel;
        public AuditResult Result
        {
            get
            {
                return result;
            }
            set
            {
                result = value;
            }
        }

        public AuditAction Action { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DataResult InnerResult
        {
            get;
            set;
        }

        /// <summary>
        /// Auditing : 审核中
        /// Successful : 终审通过
        /// Fail : 终审不通过
        /// Cancel : 取消当前操作
        /// Error : 审核提交异常
        /// CancelSubmit:撤单
        /// </summary>
        public enum AuditResult
        {
            Auditing, Successful, Fail, Cancel, Error, Saved, CancelSubmit = 9
        }
        /// <summary>
        /// 审核动作
        /// Submit : 提交
        /// Audit : 审核
        /// </summary>
        public enum AuditAction
        {
            Submit, Audit
        }


        /// <summary>
        /// 审核操作
        /// </summary>
        protected enum AuditOperation
        {
            /// <summary>
            /// 新增
            /// </summary>
            Add,
            /// <summary>
            /// 修改
            /// </summary>
            Update,
            #region beyond
            Cancel = 5

            #endregion
        }
        ///// <summary>
        ///// 审核动作
        ///// </summary>
        //public enum AuditAction
        //{
        //    /// <summary>
        //    /// 审核不通过
        //    /// </summary>
        //    Fail = 0,
        //    /// <summary>
        //    /// 审核通过
        //    /// </summary>
        //    Pass = 1

        //}

    }

}
