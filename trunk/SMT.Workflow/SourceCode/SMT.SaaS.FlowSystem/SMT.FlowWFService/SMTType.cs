using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.EntityFlowSys;
using System.Runtime.Serialization;

namespace SMT.FlowWFService
{
    /// <summary>
    /// 直接上线和隔级上有定义:0直接上级;1隔级上级;2部门负责人
    /// </summary>
    public enum Higher
    {
        Superior = 0,  //直接上级
        SuperiorSuperior = 1,  //隔级上级
        DepartHead = 2    //部门负责人
    }

    /// <summary>
    /// 检查是否已提交返回结果(0:不启动流程，直接返回。1:启动流程)
    /// </summary>
    public class CheckResult
    {
        public DataResult APPDataResult;
        public int Flag;// 0:不启动流程，直接返回。1:启动流程
        public List<FLOW_FLOWRECORDDETAIL_T> fd;
    }

    public class ModelInfo
    {
        public string SysCode;
        public string ModelName;
    }

    /// <summary>
    /// 规则与处理人类型:UserType:CreateUser(建单人);EditUser(审批人)
    /// </summary>
    /// 
    [DataContract]
    public class Role_UserType
    {
        /// <summary>
        /// 活动ID
        /// </summary>
        [DataMember]
        public string Name { set; get; }
        /// <summary>
        /// 角色ID
        /// </summary>
        [DataMember]        
        public string RoleName { set; get; }
        /// <summary>
        /// 用户类型
        /// </summary>
        [DataMember]
        public string UserType { set; get; }
        /// <summary>
        /// 角色名称(在流程代码Layout会签中,角色ID是StateType ;角色名称是RoleName)
        /// </summary>
        [DataMember]
        public string Remark { get; set; }

        [DataMember]
        public bool? IsOtherCompany //是否是特定公司
        {
            get;
            set;
        }

        [DataMember]
        public string OtherCompanyID //特定公司id
        {
            get;
            set;
        }
    }


    //引擎消息数据格式
    public class MessageData
    {
        public string MessageSystemCode { set; get; }
        public string SystemCode { set; get; }
        public string CompanyID { set; get; }
        public string ModelCode { set; get; }
        public string ModelName { set; get; }
        public string FormID { set; get; }
        public string StateCode { set; get; }
        public string CheckState { set; get; }
        public string IsTask { set; get; }
        public string AppUserID { set; get; }
        public string AppUserName { set; get; }
        public string KPITime { get; set; }
        public MessageData(string strMessageSystemCode, string strSystemCode, string strCompanyID, string strModelCode,
            string strModelName, string strFormID, string strStateCode, string strCheckState, string strIsTask, string strAppUserID,
            string strAppUserName, string strKPITime)
        {
            MessageSystemCode = strMessageSystemCode;
            SystemCode = strSystemCode;
            CompanyID = strCompanyID;
            ModelCode = strModelCode;
            ModelName = strModelName;
            FormID = strFormID;
            StateCode = strStateCode;
            CheckState = strCheckState;
            IsTask = strIsTask;
            AppUserID = strAppUserID;
            AppUserName = strAppUserName;
            KPITime = strKPITime;
        }
    }
}
    

