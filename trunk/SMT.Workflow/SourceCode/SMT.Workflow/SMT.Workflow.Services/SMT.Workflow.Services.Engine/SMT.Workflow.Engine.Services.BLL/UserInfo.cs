using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Engine.Services.BLL
{
    /// <summary>
    /// 返回人员信息
    /// </summary>
    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string CompanyID { get; set; }
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string DepartmentID { get; set; }
        [DataMember]
        public string DepartmentName { get; set; }
        [DataMember]
        public string PostID { get; set; }
        [DataMember]
        public string PostName { get; set; }

        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        public string UserName { get; set; }

    }
}
