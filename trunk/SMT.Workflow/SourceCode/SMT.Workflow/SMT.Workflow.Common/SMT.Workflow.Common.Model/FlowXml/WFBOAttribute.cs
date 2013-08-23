using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowXml
{
    [DataContract(IsReference = true)]
    public partial class WFBOAttribute : Object
    {
        private string _Name;
        private string _Description;
        private string _DataType;
        /// <summary>
        /// 是否是选择项(条件信息是选择不是填写)
        /// </summary>
        private bool _IsSelected;

        //public WFBOAttribute(string name, string description, string dataType)
        //{
        //    this.Name = name;
        //    this.Description = description;
        //    this.DataType = dataType;
        //    //this.AttributeConfig = AttributeConfig.ToAttributeConfig(xeAttributeConfig);
        //}

        [DataMember]
        public string Name
        {
            set
            { _Name = value; }
            get
            { return _Name; }
        }

        [DataMember]
        public string Description
        {
            set
            { _Description = value; }
            get
            { return _Description; }
        }

        [DataMember]
        public string DataType
        {
            set
            { _DataType = value; }
            get
            { return _DataType; }
        }
        /// <summary>
        /// 是否是选择项(条件信息是选择不是填写)
        /// </summary>
        [DataMember]
        public bool IsSelected
        {
            set
            { _IsSelected = value; }
            get
            { return _IsSelected; }
        }
        //[DataMember]
        //public AttributeConfig AttributeConfig
        //{
        //    get;
        //    set;
        //}
    }
}
