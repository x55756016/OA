using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model
{
    [DataContract]
    public class CustomPerfix
    {
        /// <summary>
        ///  字首类型ID
        /// </summary>
        [DataMember]
        public string PrefixTypeId
        {
            get;
            set;
        }
        /// <summary>
        /// 字首ID 
        /// </summary>
        [DataMember]
        public string PrefixId
        {
            get;
            set;
        }
    }
}
