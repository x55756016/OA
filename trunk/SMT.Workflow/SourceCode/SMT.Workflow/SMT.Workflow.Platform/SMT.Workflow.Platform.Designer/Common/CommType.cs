/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：zhangyh   
	 * 创建日期：2011/10/26 13:52   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer.Common 
	 * 描　　述： 公共类型定义
	 * 模块名称：工作流设计器
* ---------------------------------------------------------------------*/

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
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace SMT.Workflow.Platform.Designer.Common
{
   
    /// <summary>
    /// 流程类型: 0审批流程;1 任务流程 
    /// </summary>
    public enum FlowType
    {
        /// <summary>
        /// 审批流程
        /// </summary>
        Approval = 0,  //审批流程
        /// <summary>
        /// 任务流程
        /// </summary>
        Task = 1       //任务流程 
    }

    /// <summary>
    /// 上级类型
    /// </summary>
    public enum Higher
    {
        /// <summary>
        /// 直接上级
        /// </summary>
        Superior = 0,  //直接上级
        /// <summary>
        /// 隔级上级
        /// </summary>
        SuperiorSuperior = 1, //隔级上级
        /// <summary>
        /// 部门负责人
        /// </summary>
        DepartHead = 2  //部门负责人
    }

    /// <summary>
    /// 角色类型
    /// </summary>
    public class StateType
    {
        /// <summary>
        /// 角色类型代码
        /// </summary>
        public string StateCode { get; set; }
        /// <summary>
        /// 角色类型名称
        /// </summary>
        public string StateName { get; set; }
        /// <summary>
        /// 角色所属公司ID
        /// </summary>
        public string CompanyID { get; set; }

        /// <summary>
        /// 角色所属公司名称
        /// </summary>
        public string CompanyName { get; set; }
    }

    /// <summary>
    /// 用户类型
    /// </summary>
    public class UserType
    {
        /// <summary>
        /// 类型代码
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
    }
    #region  流程模型定义
    /// <summary>
    /// [流程模型定义对像]包括活动和连钱的集合
    /// </summary>
    public class FlowObject
    {
        /// <summary>
        /// 流程代码(唯一)
        /// </summary>
        public string FLOWCODE { get; set; }
        /// <summary>
        /// 连线 集合
        /// </summary>      
        public List<LineObject> LineList = new List<LineObject>();
        /// <summary>
        /// 活动 集合
        /// </summary>
        public List<ActivityObject> ActivityList = new List<ActivityObject>();
    }
    #endregion
    /// <summary>
    /// 设计器中每条连线对应的属性对象
    /// </summary>
    public class LineObject
    {
        /// <summary>
        ///连线ID
        /// </summary>
        public string LineId { get; set; }

        //added by jason, 02/16/2012
        /// <summary>
        /// 连线描述
        /// </summary>
        public string Remark { get; set; }
        //end added by jason, 02/16/2012

        /// <summary>
        /// 对像(通常是表名)
        /// </summary>
        public string Object { get; set; }
        /// <summary>
        /// 条件模式(AND或OR)
        /// </summary>
        public string CodiCombMode { get; set; }
        /// <summary>
        /// 条件规则列表
        /// </summary>
        public List<CompareCondition> ConditionList { get; set; }
    }

    /// <summary>
    /// 设计器中每个活动对应的属性对象
    /// </summary>
    public class ActivityObject
    {
        /// <summary>
        /// 活动ID
        /// </summary>
        public string ActivityId { get; set; }
        /// <summary>
        /// 活动名称
        /// </summary>
        public string Remark { get; set; }   
        /// <summary>
        /// 是否会签
        /// </summary>
        public bool IsCounterSign { get; set; }

        /// <summary>
        /// 是否制定公司
        /// </summary>
        public bool IsSpecifyCompany { get; set; }
        /// <summary>
        /// 制定公司ID
        /// </summary>
        public string OtherCompanyId { get; set; }
        /// <summary>
        /// 制定公司名称
        /// </summary>
        public string OtherCompanyName{ get; set; }

        /// <summary>
        /// 非会签时角色ID
        /// </summary>
        public string RoleId { get; set; }
        /// <summary>
        /// 非会签时角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 非会签时用户类型
        /// </summary>
        public string UserType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserTypeName { get; set; }

        /// <summary>
        /// 会签规则
        /// </summary>
        public string CounterType { get; set; }

        /// <summary>
        /// 会签角色列表
        /// </summary>
        public List<CounterSignRole> CounterSignRoleList { get; set; }
    }
    /// <summary>
    /// 会签角色
    /// </summary>
    public class CounterSignRole
    {      
        /// <summary>
        /// 角色代码
        /// </summary>
        public string StateCode { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public string StateName { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 是否指定公司
        /// </summary>
        public bool IsOtherCompany { get; set; }
        /// <summary>
        /// 指定公司ID
        /// </summary>
        public string OtherCompanyId { get; set; }

        /// <summary>
        /// 角色所属公司名称
        /// </summary>
        public string OtherCompanyName { get; set; }
    }

    
}
