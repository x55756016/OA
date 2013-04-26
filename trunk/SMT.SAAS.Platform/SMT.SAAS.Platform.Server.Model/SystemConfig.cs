using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 描述系统配置信息的数据结构信息
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 描述系统配置信息的数据结构信息
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public class SystemConfig
    {
        #region Model
        private string _userconfigid;
        private string _userid;
        private string _configname;
        private string _configinfo;
        private string _configtype = "2";

        /// <summary>
        /// 配置ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string UserConfigID
        {
            set { _userconfigid = value; }
            get { return _userconfigid; }
        }
        /// <summary>
        /// 用户ID
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string UserID
        {
            set { _userid = value; }
            get { return _userid; }
        }
        /// <summary>
        /// 配置名
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ConfigName
        {
            set { _configname = value; }
            get { return _configname; }
        }
        /// <summary>
        /// 配置信息。使用自定义XML格式描述。
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ConfigInfo
        {
            set { _configinfo = value; }
            get { return _configinfo; }
        }
        /// <summary>
        /// 配置类型 0: 系统 1:管理员 2:用户
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string ConfigType
        {
            set { _configtype = value; }
            get { return _configtype; }
        }

        #endregion Model

    }
}

