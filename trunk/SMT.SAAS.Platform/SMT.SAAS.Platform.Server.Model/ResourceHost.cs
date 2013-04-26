using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 描述快捷方式的数据结构信息
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 资源服务器主机信息
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public class ResourceHost
    {
        #region Model
        private string _hostid;
        private string _hostname;
        private string _hostaddress;
        private string _description;
        private string _hostversion;
        private string _ismainhost = "0";
        private string _isaccess = "0";
        private DateTime? _syncdate;

        /// <summary>
        /// 主机ID,唯一,主键
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string HostID
        {
            set { _hostid = value; }
            get { return _hostid; }
        }
        /// <summary>
        /// 资源主机名称
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string HostName
        {
            set { _hostname = value; }
            get { return _hostname; }
        }
        /// <summary>
        /// 主机地址，用于根据主机地址请求同步
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
        /// 描述信息，对主机情况的概要描述
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
        /// 主机版本,描述当前的主机版本.
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string HostVersion
        {
            set { _hostversion = value; }
            get { return _hostversion; }
        }
        /// <summary>
        /// 主服务器，如果为主服务器，其他服务器可以请求主服务器进行资源同步。
        ///0：非主服务器，默认
        ///1：主服务器
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string IsMainHost
        {
            set { _ismainhost = value; }
            get { return _ismainhost; }
        }
        /// <summary>
        /// 主机是否开放给外网访问，当可访问状态下，主服务器可以通知子服务器进行更新。
        ///0：不可访问，默认
        ///1：可访问
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string IsAccess
        {
            set { _isaccess = value; }
            get { return _isaccess; }
        }
        /// <summary>
        /// 最后同步时间。描述主服务器与子服务器的更新时间差异。
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public DateTime? SyncDate
        {
            set { _syncdate = value; }
            get { return _syncdate; }
        }

        #endregion Model

    }
}
