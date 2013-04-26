using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 描述WebPart的数据结构信息
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 描述WebPart的数据结构信息
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public class WebPart
    {
        #region Model
        private string _webpartid;
        private string _moduleid;
        private string _iconpath;
        private string _titel;
        private string _fullname;
        private string _assemplyname;
        private string _issysneed = "0";
        private string _params;
        private string _templatename;
        private string _userstate = "0";

        /// <summary>
        /// WebPartID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string WebPartID
        {
            set { _webpartid = value; }
            get { return _webpartid; }
        }
        /// <summary>
        /// 子项目ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ModuleID
        {
            set { _moduleid = value; }
            get { return _moduleid; }
        }
        /// <summary>
        /// 图标路径
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string IconPath
        {
            set { _iconpath = value; }
            get { return _iconpath; }
        }
        /// <summary>
        /// 标题
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Titel
        {
            set { _titel = value; }
            get { return _titel; }
        }
        /// <summary>
        /// 完整路径
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string FullName
        {
            set { _fullname = value; }
            get { return _fullname; }
        }
        /// <summary>
        /// 所属程序集名
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string AssemplyName
        {
            set { _assemplyname = value; }
            get { return _assemplyname; }
        }
        /// <summary>
        /// 是否可删除 0:不可以，1：可以
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string IsSysNeed
        {
            set { _issysneed = value; }
            get { return _issysneed; }
        }
        /// <summary>
        /// 初始参数
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Params
        {
            set { _params = value; }
            get { return _params; }
        }
        /// <summary>
        /// 默认模版
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string TemplateName
        {
            set { _templatename = value; }
            get { return _templatename; }
        }

        /// <summary>
        /// 0: 未启用 1: 启用
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string UserState
        {
            set { _userstate = value; }
            get { return _userstate; }
        }

        #endregion Model
    }
}

