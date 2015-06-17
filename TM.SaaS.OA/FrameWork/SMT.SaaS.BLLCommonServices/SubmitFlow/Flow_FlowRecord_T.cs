using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.BLLCommonServices.SubmitFlow
{
    /// <summary>
    /// 流程记录表实体
    /// </summary>
    public class Flow_FlowRecord_T : EntityObject
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
}
