using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// 描述系统更新日志的数据结构信息
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public class ModuleUpdateLog
    {
        #region Model
        private string _modulelogid;
        private string _moduleid;
        private string _version;
        private string _description;
        /// <summary>
        /// 更新记录ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ModuleLogID
        {
            set { _modulelogid = value; }
            get { return _modulelogid; }
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
        /// 版本ID，子项目的更新版本。版本号与项目部署文件有关系。
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
        /// 版本描述信息。对当前版本更新内容进行描述。
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

