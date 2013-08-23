using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowXml
{
     /// <summary>
     /// 设置参数
     /// </summary>
    [DataContract]
    public class Param
    {
        [DataMember]
        public string ParamName { get; set; }
        [DataMember]
        public string ParamID{ get;set;}
        [DataMember]
        public string FieldName{get;set; }
        [DataMember]
        public string FieldID{ get;set;}
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string TableName { get; set; }
    }
}
