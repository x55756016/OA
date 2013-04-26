
using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 模块信息
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    public partial class ModuleInfo
    {
        #region ModuleInfo属性
        /// <summary>
        /// 子项目ID
        /// </summary>
        public string ModuleID
        {
            get;
            set;
        }
        /// <summary>
        /// 子项目编号
        /// </summary>
        public string ModuleCode
        {
            get;
            set;
        }
        ///// <summary>
        ///// 子项目名称
        ///// </summary>
        //public string ModuleName
        //{
        //    get;
        //    set;
        //}


        ///// <summary>
        ///// 模块类型，Type全称，包含DLL版本信息
        ///// EG:TopNamespace.SubNameSpace.ContainingClass+NestedClass, MyAssembly, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b17a5c561934e089
        ///// </summary>
        //public string ModuleType
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// 所属父模块ID，可以为空
        /// </summary>
        public string ParentModuleID
        {
            get;
            set;
        }

        /// <summary>
        /// 图标地址
        /// </summary>
        public string ModuleIcon
        {
            get;
            set;
        }
        /// <summary>
        /// 当前版本号，版本号与项目部署文件有关系。
        /// </summary>
        public string Version
        {
            get;
            set;
        }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName
        {
            get;
            set;
        }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath
        {
            get;
            set;
        }
        /// <summary>
        /// 入口程序集
        /// </summary>
        public string EnterAssembly
        {
            get;
            set;
        }
        /// <summary>
        /// 是否持久化 0:不进行持久化   1:持久化
        /// </summary>
        public string IsSave
        {
            get;
            set;
        }

        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientID
        {
            get;
            set;
        }
        /// <summary>
        /// 服务端ID
        /// </summary>
        public string ServerID
        {
            get;
            set;
        }
        /// <summary>
        /// 应用状态 0:未启用 1:启用
        /// </summary>
        public string UseState
        {
            get;
            set;
        }
        /// <summary>
        /// 项目服务主机地址
        /// </summary>
        public string HostAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 当前版本描述信息。更新后此信息将写入历史记录。
        /// </summary>
        public string Description
        {
            get;
            set;
        }
      
        /// <summary>
        /// 标识模块是通过WebClient下载，还是从服务下载。
        /// </summary>
        public bool IsOnWeb
        {
            get;
            set;
        }

        public Dictionary<string, string> InitParams
        {
            get;
            set;
        }

        public string SystemType
        {
            get;
            set;
        }
        #endregion Model
    }
}
