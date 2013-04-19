using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 模拟Dataset结构
    /// </summary>
    [DataContract]
    public class DataTableInfo
    {
        [DataMember]
        public string TableName { get; set; }

        [DataMember]
        public ObservableCollection<DataColumnInfo> Columns { get; set; }

    }
}
