using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 描述系统错误的数据结构信息
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 描述系统错误的数据结构信息
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public class SystemError
    {
        #region Model
        private string _errorid;
        private string _errorcode;
        private string _appname;
        private string _modelname;
        private string _errortitel;
        private string _category = "2";
        private string _priority = "2";
        private string _message;

        /// <summary>
        /// 错误日志ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ErrorID
        {
            set { _errorid = value; }
            get { return _errorid; }
        }
        /// <summary>
        /// 日志编号
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ErrorCode
        {
            set { _errorcode = value; }
            get { return _errorcode; }
        }
        /// <summary>
        /// 所属系统
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string AppName
        {
            set { _appname = value; }
            get { return _appname; }
        }
        /// <summary>
        /// 所属模块
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ModelName
        {
            set { _modelname = value; }
            get { return _modelname; }
        }
        /// <summary>
        /// 错误标题
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ErrorTitel
        {
            set { _errortitel = value; }
            get { return _errortitel; }
        }
        /// <summary>
        /// 错误类型
        /// 0: 调试
        /// 1: 异常
        /// 2: 消息
        /// 3: 警告
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Category
        {
            set { _category = value; }
            get { return _category; }
        }
        /// <summary>
        /// 错误等级，严重程度。
        /// 0: 无
        /// 1: 高
        /// 2: 中
        /// 3: 低
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Priority
        {
            set { _priority = value; }
            get { return _priority; }
        }
        /// <summary>
        /// 错误详细内容，有可能是自定义异常或系统产生的异常。
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Message
        {
            set { _message = value; }
            get { return _message; }
        }

        #endregion Model
    }
}

