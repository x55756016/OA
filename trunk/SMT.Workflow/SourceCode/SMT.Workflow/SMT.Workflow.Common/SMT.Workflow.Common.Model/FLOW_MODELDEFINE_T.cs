 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：T.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:56:55   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[模块定义表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
namespace SMT.Workflow.Common.Model
{
    #region  模块定义表
    
    /// <summary>
     /// [模块定义表]
     /// </summary>
    [DataContract]
    public class FLOW_MODELDEFINE_T
    {
    	 
        /// <summary>
        /// 模块ID
        /// </summary>
        [DataMember]
        public string  MODELDEFINEID { get; set; }
         
        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string  SYSTEMCODE { get; set; }

        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string  MODELCODE { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        [DataMember]
        public string SYSTEMNAME { get; set; }
         
        /// <summary>
        /// 上级模块代码
        /// </summary>
        [DataMember]
        public string  PARENTMODELCODE { get; set; }
         
        /// <summary>
        /// 模块描述
        /// </summary>
        [DataMember]
        public string  DESCRIPTION { get; set; }
         
        /// <summary>
        /// 创建人ID
        /// </summary>
        [DataMember]
        public string  CREATEUSERID { get; set; }
         
        /// <summary>
        /// 创建人名
        /// </summary>
        [DataMember]
        public string  CREATEUSERNAME { get; set; }
         
        /// <summary>
        /// 创建公司ID
        /// </summary>
        [DataMember]
        public string  CREATECOMPANYID { get; set; }
         
        /// <summary>
        /// 创建部门ID
        /// </summary>
        [DataMember]
        public string  CREATEDEPARTMENTID { get; set; }
         
        /// <summary>
        /// 创建岗位ID
        /// </summary>
        [DataMember]
        public string  CREATEPOSTID { get; set; }
         
        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public DateTime? CREATEDATE { get; set; }
         
        /// <summary>
        /// 修改人ID
        /// </summary>
        [DataMember]
        public string  EDITUSERID { get; set; }
         
        /// <summary>
        /// 修改人用户名
        /// </summary>
        [DataMember]
        public string  EDITUSERNAME { get; set; }
         
        /// <summary>
        /// 修改时间
        /// </summary>
        [DataMember]
        public DateTime? EDITDATE { get; set; }

        /// <summary>
        /// 哪些公司在模块中可以允许自选流程 列表
        /// </summary>
        [DataMember]
        public List<FLOW_MODELDEFINE_FREEFLOW> FreeFlowCompanyList { get; set; }

        /// <summary>
        /// [哪些公司在模块中可以允许提单人撒回流程]
        /// </summary>
        [DataMember]
        public List<FLOW_MODELDEFINE_FLOWCANCLE> FlowCancelCompanyList { get; set; }
    }
    #endregion
    #region 哪些公司在模块中可以允许自选流程
    /// <summary>
    /// [哪些公司在模块中可以允许自选流程]
    /// </summary>
    [DataContract]
    public class FLOW_MODELDEFINE_FREEFLOW
    {

        /// <summary>
        /// GUID
        /// </summary>
        [DataMember]
        public string MODELDEFINEFREEFLOWID { get; set; }

        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 允许自选流程公司名称
        /// </summary>
        [DataMember]
        public string COMPANYNAME { get; set; }

        /// <summary>
        /// 允许自选流程公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }

        /// <summary>
        /// 创建人名
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 创建公司ID
        /// </summary>
        [DataMember]
        public string CREATECOMPANYID { get; set; }

        /// <summary>
        /// 创建部门ID
        /// </summary>
        [DataMember]
        public string CREATEDEPARTMENTID { get; set; }

        /// <summary>
        /// 创建岗位ID
        /// </summary>
        [DataMember]
        public string CREATEPOSTID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public DateTime CREATEDATE { get; set; }

    }
    #endregion
    #region  [哪些公司在模块中可以允许提单人撒回流程]
    /// <summary>
    /// [哪些公司在模块中可以允许提单人撒回流程]
    /// </summary>
    [DataContract]
    public class FLOW_MODELDEFINE_FLOWCANCLE
    {

        /// <summary>
        /// GUID
        /// </summary>
        [DataMember]
        public string MODELDEFINEFLOWCANCLEID { get; set; }

        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 允许提单人撒回流程公司名称
        /// </summary>
        [DataMember]
        public string COMPANYNAME { get; set; }

        /// <summary>
        /// 允许提单人撒回流程公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }

        /// <summary>
        /// 创建人名
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 创建公司ID
        /// </summary>
        [DataMember]
        public string CREATECOMPANYID { get; set; }

        /// <summary>
        /// 创建部门ID
        /// </summary>
        [DataMember]
        public string CREATEDEPARTMENTID { get; set; }

        /// <summary>
        /// 创建岗位ID
        /// </summary>
        [DataMember]
        public string CREATEPOSTID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public DateTime CREATEDATE { get; set; }

    }
#endregion
}