/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：MsgParms.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/19 9:48:38   
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

namespace SMT.Global.IEngineContract
{

    [DataContract]
    public class ReceiveUserAndContent
    {
        [DataMember]
        public string ReceiveUser
        { get; set; }
        [DataMember]
        public string Content
        { get; set; }
    }

    public enum MsgType
    {
        Msg,//消息
        Task,//代办任务
        Cancel//撤消 
    }
    /// <summary>
    /// 接收用户的表单实体
    /// </summary>
    [DataContract]
    public class ReceiveUserForm
    {
        /// <summary>
        /// 接收用户ID
        /// </summary>
        [DataMember]
        public string ReceiveUser
        {
            get;
            set;
        }
        /// <summary>
        /// 接收用户所对应的表单
        /// </summary>
        [DataMember]
        public List<string> FormID
        {
            get;
            set;
        }
    }
   
    /// <summary>
    /// 待办任务参数
    /// </summary>
    [DataContract]
    public class MsgParms
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [DataMember]
        public string UserID
        {
            get;
            set;
        }
        /// <summary>
        /// 消息状态（Close,Open）
        /// </summary>
        [DataMember]
        public string Status
        {
            get;
            set;
        }
        /// <summary>
        /// Top多少条
        /// </summary>
        [DataMember]
        public int Top
        {
            get;
            set;
        }
        /// <summary>
        ///系统当前时间往前推 最近多少天
        /// </summary>
        [DataMember]
        public int LastDay
        {
            get;
            set;
        }
        [DataMember]
        public int PageSize
        {
            get;
            set;
        }
        [DataMember]
        public int PageIndex
        {
            get;
            set;
        }
        [DataMember]
        public string MessageBody
        {
            get;
            set;
        }
        [DataMember]
        public DateTime BeginDate
        {
            get;
            set;
        }
        [DataMember]
        public DateTime EndDate
        {
            get;
            set;
        }

        [DataMember]
        public string MessageId
        {
            get;
            set;
        }
    }
}
