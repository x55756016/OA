using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace SMT.SAAS.Platform.Model
{
    [DataContract]
    public class T_PF_NEWSListView
    {

        [DataMember]
        public string NEWSID { get; set; }
        [DataMember]
        public string NEWSTATE { get; set; }
        [DataMember]
        public string NEWSTITEL { get; set; }
        [DataMember]
        public string NEWSTYPEID { get; set; }
        [DataMember]
        public DateTime? UPDATEDATE { get; set; }
    }
}
