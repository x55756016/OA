using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif

namespace SMT.SAAS.Platform.Model
{
#if !SILVERLIGHT
    [DataContract]
#endif
    public class UserConfig
    {
#if !SILVERLIGHT
        [DataMember]
#endif
        public string XmlConfigInfo { get; set; }

#if !SILVERLIGHT
        [DataMember]
#endif
        public List<ShortCut> ShortCutList { get; set; }

#if !SILVERLIGHT
        [DataMember]
#endif
        public List<WebPart> WebPartList { get; set; }
    }
}
