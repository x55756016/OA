using System.ComponentModel;
using Wintellect.Sterling.Serialization;
using System.Collections.ObjectModel;
using System;

namespace SMT.SaaS.LocalData.Tables
{
    public class V_PostInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string UserModuleID { get; set; }

        /// <summary>
        /// 员工ID
        /// </summary>
        public string UserID { get; set; }

        public string CHECKSTATE { get; set; }
        
        public string DEPARTMENTID { get; set; }
        
        public string EDITSTATE { get; set; }
        
        public string FATHERPOSTID { get; set; }
        
        public string POSTID { get; set; }
        
        public string POSTNAME { get; set; }
    }
}
