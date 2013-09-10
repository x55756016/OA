using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_Travelmanagement
    {
        public T_OA_BUSINESSTRIP Travelmanagement;
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
        private string stateReimbursement;
        private string noClaims;
        private string traveAppCheckState;
        private int tdetail;

        public int Tdetail
        {
            get { return tdetail; }
            set { tdetail = value; }
        }

        /// <summary>
        /// 出差申请审核状态
        /// </summary>
        public string TraveAppCheckState
        {
            get { return traveAppCheckState; }
            set { traveAppCheckState = value; }
        }

        /// <summary>
        /// 报销单号
        /// </summary>
        public string NoClaims
        {
            get { return noClaims; }
            set { noClaims = value; }
        }

        /// <summary>
        /// 显示是否已报销或为报销
        /// </summary>
        public string StateReimbursement
        {
            get { return stateReimbursement; }
            set { stateReimbursement = value; }
        }
        /// <summary>
        /// //出差报告ID
        /// </summary>
        private string reportId;

        public string ReportId
        {
            get { return reportId; }
            set { reportId = value; }
        }
        /// <summary>
        /// //出差报告状态
        /// </summary>
        private string reportCheckState;

        public string ReportCheckState
        {
            get { return reportCheckState; }
            set { reportCheckState = value; }
        }
        /// <summary>
        /// //出差报销ID
        /// </summary>
        private string trId;

        public string TrId
        {
            get { return trId; }
            set { trId = value; }
        }
        /// <summary>
        /// //出差报销状态
        /// </summary>
        private string trCheckState;

        public string TrCheckState
        {
            get { return trCheckState; }
            set { trCheckState = value; }
        }

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
