using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SMT.Workflow.Common.Model.FlowXml;

namespace SMT.Workflow.Common.Model
{
    [DataContract]
    public class FlowTriggerRules
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string DOTASKRULEDETAILID { get; set; }
        /// <summary>
        /// 引擎编码
        /// </summary>
        [DataMember]
        public string DOTASKRULEID { get; set; }

        /// <summary>
        /// 可处理日期
        /// </summary>
        [DataMember]
        public decimal AVAILABILITYPROCESSDATES { get; set; }

        /// <summary>
        /// 触发条件
        /// </summary>
        [DataMember]
        public decimal TRIGGERCONDITION { get; set; }
        /// <summary>
        /// 缺省消息
        /// </summary>
        [DataMember]
        public string DEFAULTMSG { get; set; }
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
        /// 系统模块(其它来源)
        /// </summary>
        [DataMember]
        public List<AppModel> APPMODELS { get; set; }
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
        /// 功能赋值
        /// </summary>
        [DataMember]
        public List<TableColumn> TABLECOLUMN { get; set; }
        /// <summary>
        /// Select功能赋值
        /// </summary>
        [DataMember]
        public TableColumn SELECTTABLECOLUMN { get; set; }

        /// <summary>
        /// 消息显示
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 设置参数
        /// </summary>
        [DataMember]
        public List<Param> PARAMCOLLECTION { get; set; }

        /// <summary>
        /// Select系统代码
        /// </summary>
        [DataMember]
        public AppSystem SELECTMCODE { get; set; }

        /// <summary>
        /// Select模块
        /// </summary>
        [DataMember]
        public AppModel SELECTMODEL { get; set; }


        /// <summary>
        /// 其它来源
        /// </summary>
        [DataMember]
        public string OTHER { get; set; }

        /// <summary>
        /// IsEnabled
        /// </summary>
        [DataMember]
        public string ISENABLED { get; set; }

        /// <summary>
        /// 岗位ID
        /// </summary>
        [DataMember]
        public string OWNERPOSTID { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        [DataMember]
        public string RECEIVEUSERNAME { get; set; }
        
        /// <summary>
        /// 创建人ID
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember]
        public DateTime CREATEDATETIME { get; set; }


        /// <summary>
        /// 系统代码（主表）
        /// </summary>
        [DataMember]
        public List<AppSystem> SYSTEMCODEMASTER { get; set; }
        /// <summary>
        /// Select系统代码(主表)
        /// </summary>
        [DataMember]
        public AppSystem SELECTSYSTEMCODEMASTER { get; set; }

        /// <summary>
        /// 系统模块
        /// </summary>
        [DataMember]
        public List<AppModel> APPMODELMASTER { get; set; }
        /// <summary>
        /// Select模块
        /// </summary>
        [DataMember]
        public AppModel SELECTAPPMODELMASTER { get; set; }

     }
}
