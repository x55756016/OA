using System;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 描述系统更新日志的数据结构信息
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 项目依赖关系
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public partial class ModuleDepends
    {
        public ModuleDepends()
        { }
        #region Model
        private string _moduleid;
        private string _dependid;
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
        /// 依赖的子系统ID。单个项目可能会有多个依赖项。
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string DependID
        {
            set { _dependid = value; }
            get { return _dependid; }
        }
        #endregion Model

    }
}

