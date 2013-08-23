/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：CustomUserMsg.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/23 11:43:07   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Common.Model.FlowEngine 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowEngine
{
    [DataContract]
    public class CustomUserMsg
    {
        [DataMember]
        public string UserID
        {
            get;
            set;
        }
        [DataMember]
        public string FormID
        {
            get;
            set;
        }

    }
}
