using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowXml
{
    /// <summary>
    /// 系统方法实体
    /// </summary>
    [DataContract]
    public class AppFunc
    {
        [DataMember]
        public string Language
        {
            get;
            set;
        }
        [DataMember]
        //方法名称
        public string FuncName
        {
            get;
            set;
        }
        [DataMember]
        public List<Parameter> Parameter
        {
            get;
            set;
        }
        [DataMember]
        //地址
        public string Address
        { get; set; }
        [DataMember]
        //绑定型式
        public string Binding
        { get; set; }
        [DataMember]
        //参数分格符
        public string SplitChar
        { get; set; }
    }
}
