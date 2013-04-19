using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 模拟dataset结构
    /// </summary>
    [DataContract]
    public class DataColumnInfo
    {
        [DataMember]
        public string ColumnName { get; set; }

        [DataMember]
        public string ColumnTitle { get; set; }

        [DataMember]
        public string DataTypeName { get; set; }

        [DataMember]
        public bool IsRequired { get; set; }

        [DataMember]
        public bool IsShow { get; set; }

        [DataMember]
        public bool IsKey { get; set; }

        [DataMember]
        public bool IsReadOnly { get; set; }

        [DataMember]
        public int DisplayIndex { get; set; }

        [DataMember]
        public string EditControlType { get; set; }

        [DataMember]
        public int MaxLength { get; set; }

        [DataMember]
        public bool IsEncrypt { get; set; }

        [DataMember]
        public bool IsNeedSum { get; set; }
    }
}
