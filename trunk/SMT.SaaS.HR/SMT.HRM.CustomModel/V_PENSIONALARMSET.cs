using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
   public class V_PENSIONALARMSET
    {
       public T_HR_PENSIONALARMSET T_HR_PENSIONALARMSET { get; set; }
       public string CNAME { get; set; }
       public string CREATEUSERID {get;set;}
       public string OWNERID{get;set;}
       public string OWNERPOSTID {get;set;}
       public string OWNERDEPARTMENTID{get;set;}
       public string OWNERCOMPANYID{get;set;}
    }
}
