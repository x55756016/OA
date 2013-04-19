using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;


namespace SMT.SaaS.OA.DAL.Views
{
    public class V_TravelReimbursement
    {
        public T_OA_TRAVELREIMBURSEMENT TravelReimbursement;
        private string guids;
        private string ownercompanyid;
        private string ownerdepartmentid;
        private string ownerpostid;
        private string ownerid;
        private string createuserid;
        private string cicty;
        private string cictys;
        private string startdate;
        private string endtime;

        public string Cicty
        {
            get { return cicty; }
            set { cicty = value; }
        }

        public string Cictys
        {
            get { return cictys; }
            set { cictys = value; }
        }

        public string Startdate
        {
            get { return startdate; }
            set { startdate = value; }
        }
        
        public string Endtime
        {
            get { return endtime; }
            set { endtime = value; }
        }

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
