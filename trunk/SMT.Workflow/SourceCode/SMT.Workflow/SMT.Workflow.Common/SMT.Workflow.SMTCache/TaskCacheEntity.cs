using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.SMTCache
{
    [DataContract]
    public class TaskCacheEntity
    {

        /// <summary>
        /// 是否有新待办
        /// </summary>
        [DataMember]
        public bool FonctionTask
        {
            get;
            set;
        }
        /// <summary>
        /// 是否有新待办
        /// </summary>
        [DataMember]
        public bool FonctionTaskPage
        {
            get;
            set;
        }
        /// <summary>
        /// 最后一次刷新时间
        /// </summary>
        [DataMember]
        public DateTime LastFreshTime
        {
            get;
            set;
        }
    }
}
