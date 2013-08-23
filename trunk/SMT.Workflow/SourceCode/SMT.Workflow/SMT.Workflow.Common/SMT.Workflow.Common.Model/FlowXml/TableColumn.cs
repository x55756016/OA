using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowXml
{
    /// <summary>
    /// 表字段自定义模型
    /// </summary>
    [DataContract]
    public class TableColumn
    {
        [DataMember]
        public string Key
        {
            get;
            set;
        }
        [DataMember]
        public string FieldName
        {
            get;
            set;
        }
        [DataMember]
        public string Description
        {
            get;
            set;
        }
        [DataMember]
        public string DataType
        {
            get;
            set;
        }
        [DataMember]
        public string DataValue
        {
            get;
            set;
        }
        [DataMember]
        public string Language
        {
            get;
            set;
        }
    }
}
