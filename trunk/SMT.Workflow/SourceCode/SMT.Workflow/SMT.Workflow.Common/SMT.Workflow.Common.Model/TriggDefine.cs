using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SMT.Workflow.Common.Model.FlowXml;

namespace SMT.Workflow.Common.Model
{
    [DataContract]
    public class TriggDefine
    {
        [DataMember]
        public string PROCESSID { get; set; }
        /// <summary>
        /// 定时触发名称
        /// </summary>
        [DataMember]
        public string TRIGGERNAME { get; set; }

        /// <summary>
        /// 定时触发描述
        /// </summary>
        [DataMember]
        public string TRIGGERDESCRIPTION { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public List<AppSystem> SYSTEMCODE { get; set; }
        /// <summary>
        /// Select系统代码
        /// </summary>
        [DataMember]
        public AppSystem SELECTSYSTEMCODE { get; set; }

        /// <summary>
        /// 系统模块
        /// </summary>
        [DataMember]
        public List<AppModel> APPMODEL { get; set; }
        /// <summary>
        /// Select模块
        /// </summary>
        [DataMember]
        public AppModel SELECTAPPMODEL { get; set; }

        /// <summary>
        /// 系统功能
        /// </summary>
        [DataMember]
        public List<AppFunc> APPFUNC { get; set; }
        /// <summary>
        /// Select系统功能
        /// </summary>
        [DataMember]
        public AppFunc SELECTAPPFUNC { get; set; }

        /// <summary>
        /// 功能参数
        /// </summary>
        [DataMember]
        public List<Parameter> PARAMETER { get; set; }
        /// <summary>
        /// Select功能参数
        /// </summary>
        [DataMember]
        public Parameter SELECTPARAMETER { get; set; }
        /// <summary>
        /// 功能参数赋值
        /// </summary>
        [DataMember]
        public string PARAMETERVALUE { get; set; }

        /// <summary>
        /// 处理周期
        /// </summary>
        [DataMember]
        public List<Cycle> CYCLETYPE { get; set; }
        /// <summary>
        /// Select周期
        /// </summary>
        [DataMember]
        public Cycle SELECTCYCLETYPE { get; set; }

         /// <summary>
        /// 设置参数
        /// </summary>
        [DataMember]
        public List<Param> PARAMCOLLECTION { get; set; }

        /// <summary>
        /// 系统级别
        /// </summary>
        [DataMember]
        public bool SYSTEMTRIGGER { get; set; }
        /// <summary>
        /// 用户级别
        /// </summary>
        [DataMember]
        public bool USERTRIGGERTYPE { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember]
        public DateTime PROCESSSTARTDATES { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember]
        public DateTime CREATEDATETIME { get; set; }

        /// <summary>
        /// 触发活动类型
        /// </summary>
        [DataMember]
        public decimal TRIGGERACTIVITYTYPE { get; set; }

        /// <summary>
        /// 短信活动
        /// </summary>
        [DataMember]
        public bool SMSTYPE { get; set; }
        /// <summary>
        /// 服务活动
        /// </summary>
        [DataMember]
        public bool SERVICETYPE { get; set; }
       
     }
}
