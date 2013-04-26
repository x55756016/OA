using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 描述系统资源的数据结构信息
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 描述系统资源的数据结构信息
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public class Resource
    {
        #region Model
        private string _resourceid;
        private string _resourcename;
        private string _filepath;
        private string _version;
        private string _type;
        private string _state = "0";
        private string _description;

        /// <summary>
        /// 资源ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ResourceID
        {
            set { _resourceid = value; }
            get { return _resourceid; }
        }
        /// <summary>
        /// 资源名称
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ResourceName
        {
            set { _resourcename = value; }
            get { return _resourcename; }
        }
        /// <summary>
        /// 资源路径
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string FilePath
        {
            set { _filepath = value; }
            get { return _filepath; }
        }
        /// <summary>
        /// 资源版本
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Version
        {
            set { _version = value; }
            get { return _version; }
        }
        /// <summary>
        /// 资源类型
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Type
        {
            set { _type = value; }
            get { return _type; }
        }
        /// <summary>
        /// 资源状态
        /// 0:未启用
        /// 1:启用
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string State
        {
            set { _state = value; }
            get { return _state; }
        }
        /// <summary>
        /// 资源描述
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Description
        {
            set { _description = value; }
            get { return _description; }
        }
        #endregion Model

    }
}

