using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_ContractApplications
    {
        public T_OA_CONTRACTAPP contractApp;
        public T_OA_CONTRACTPRINT contractPrint;
        private string guids;
        private string ownercompanyid;
        private string ownerdepartmentid;
        private string ownerpostid;
        private string ownerid;
        private string createuserid;
        public string contractType { get; set; }
        public string Guids
        {
            get { return guids; }
            set { guids = value; }
        }

        public string OWNERPOSTID
        {
            get { return ownerpostid; }
            set { ownerpostid = value; }
        }
        public string CREATEUSERID
        {
            get { return createuserid; }
            set { createuserid = value; }
        }
        public string OWNERID
        {
            get { return ownerid; }
            set { ownerid = value; }
        }
        public string OWNERDEPARTMENTID
        {
            get { return ownerdepartmentid; }
            set { ownerdepartmentid = value; }
        }
        public string OWNERCOMPANYID
        {
            get { return ownercompanyid; }
            set { ownercompanyid = value; }
        }
    }
}
