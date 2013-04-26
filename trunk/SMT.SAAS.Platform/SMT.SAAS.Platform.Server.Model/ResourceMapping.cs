using System;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// T_PF_ResourceMapping:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public partial class ResourceMapping
    {
        public ResourceMapping()
        { }
        #region Model
        private string _mappingid;
        private string _hostid;
        private string _startip;
        private string _endip;

        /// <summary>
        /// 
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string MappingID
        {
            set { _mappingid = value; }
            get { return _mappingid; }
        }
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
        /// 起始IP地址端
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string StartIP
        {
            set { _startip = value; }
            get { return _startip; }
        }
        /// <summary>
        /// 结束IP地址端
        /// </summary>
#if !SILVERLIGHT
        [DataMember]
#endif
        public string EndIP
        {
            set { _endip = value; }
            get { return _endip; }
        }
        #endregion Model

    }
}

