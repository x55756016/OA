using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
namespace SMT.HRM.CustomModel
{
    [DataContract]
    public class DataSetData
    {
        [DataMember]
        public ObservableCollection<DataTableInfo> Tables { get; set; }
        [DataMember]
        public string DataXML { get; set; }

    }
}
