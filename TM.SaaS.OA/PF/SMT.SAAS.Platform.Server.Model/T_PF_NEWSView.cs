using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TM_SaaS_OA_EFModel;

namespace SMT.SAAS.Platform.Model
{
    [DataContract]
    public class T_PF_NEWSView
    {
        [DataMember]
        public List<T_PF_DISTRIBUTEUSER> T_PF_DISTRIBUTEUSERS { get; set; }
        [DataMember]
        public T_PF_NEWS T_PF_NEWS { get; set; }
    }
}
