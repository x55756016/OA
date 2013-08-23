/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：MailParams.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/21 9:09:10   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services.DAL 
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
    /// <summary>
    /// 邮件发送参数
    /// </summary>
    [DataContract]
    public class MailParams
    {
        [DataMember]
        public string ReceiveUserMail
        { get; set; }
        [DataMember]
        public string MailTitle
        { get; set; }
        [DataMember]
        public string MailContent
        { get; set; }
    }
}
