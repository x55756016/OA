using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 描述子系统(应用)的数据结构信息
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 描述子系统(应用)的数据结构信息
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public class ModuleInfo
    {
        #region ModuleInfo成员
        private string _moduleid;
        private string _modulecode;
        private string _modulename;
        private string _moduletype;
        private string _parentmoduleid;
        private string _moduleicon;
        private string _version;
        private string _filename;
        private string _filepath;
        private string _enterassembly;
        private string _issave = "1";
        private string _clientid;
        private string _serverid;
        private string _usestate = "1";
        private string _hostaddress;
        private string _description;
        private string _systemtype;
        private Dictionary<string, string> _initparam=new Dictionary<string,string>();
        private Collection<string> _dependsOn =new Collection<string>();
        #endregion

        #region ModuleInfo属性
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
        /// 子项目编号
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ModuleCode
        {
            set { _modulecode = value; }
            get { return _modulecode; }
        }
        /// <summary>
        /// 子项目名称
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ModuleName
        {
            set { _modulename = value; }
            get { return _modulename; }
        }


        /// <summary>
		/// 模块类型，Type全称，包含DLL版本信息
        /// EG:TopNamespace.SubNameSpace.ContainingClass+NestedClass, MyAssembly, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b17a5c561934e089
		/// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
		public string ModuleType
		{
			set{ _moduletype=value;}
			get{return _moduletype;}
		}

		/// <summary>
		/// 所属父模块ID，可以为空
		/// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
		public string ParentModuleID
		{
			set{ _parentmoduleid=value;}
			get{return _parentmoduleid;}
		}

        /// <summary>
        /// 图标地址
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ModuleIcon
        {
            set { _moduleicon = value; }
            get { return _moduleicon; }
        }
        /// <summary>
        /// 当前版本号，版本号与项目部署文件有关系。
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
        /// 文件名称
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string FileName
        {
            set { _filename = value; }
            get { return _filename; }
        }
        /// <summary>
        /// 文件路径
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
        /// 入口程序集
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string EnterAssembly
        {
            set { _enterassembly = value; }
            get { return _enterassembly; }
        }
        /// <summary>
        /// 是否持久化 0:不进行持久化   1:持久化
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string IsSave
        {
            set { _issave = value; }
            get { return _issave; }
        }

        /// <summary>
        /// 客户端ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ClientID
        {
            set { _clientid = value; }
            get { return _clientid; }
        }
        /// <summary>
        /// 服务端ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ServerID
        {
            set { _serverid = value; }
            get { return _serverid; }
        }
        /// <summary>
        /// 应用状态 0:未启用 1:启用
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string UseState
        {
            set { _usestate = value; }
            get { return _usestate; }
        }
        /// <summary>
        /// 项目服务主机地址
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string HostAddress
        {
            set { _hostaddress = value; }
            get { return _hostaddress; }
        }
        
        /// <summary>
        /// 当前版本描述信息。更新后此信息将写入历史记录。
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string Description
        {
            set { _description = value; }
            get { return _description; }
        }

        /// <summary>
        /// 模块所属系统，若没有则为空。
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string SystemType
        {
            set { _systemtype = value; }
            get { return _systemtype; }
        }

        
        /// <summary>
        /// 模块参数列表。
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public Dictionary<string, string> InitParams
        {
            set { _initparam = value; }
            get { return _initparam; }
        }

        /// <summary>
        /// 模块依赖项。
        /// </summary>_dependsOn
#if !SILVERLIGHT
        [DataMember]
#endif
        public Collection<string> DependsOn
        {
            set { _dependsOn = value; }
            get { return _dependsOn; }
        }
        #endregion Model
    }
}

