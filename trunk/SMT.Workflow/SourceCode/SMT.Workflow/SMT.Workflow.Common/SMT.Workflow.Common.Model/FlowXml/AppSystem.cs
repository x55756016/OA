using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowXml
{ 
    /// <summary>
    /// 系统自定义模型
    /// </summary>
    [DataContract]
    public class AppSystem
    {
        [DataMember]
        public string Name
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
        public string ObjectFolder
        {
            get;
            set;
        }

    }
}
