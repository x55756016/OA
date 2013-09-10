using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_BumfCompanySendDoc
    {
        public T_OA_SENDDOC senddoc;
        
        public T_OA_SENDDOCTYPE doctype;
        public T_OA_DISTRIBUTEUSER distrbuteuser;
        public V_FlowAPP flowApp;
        private string guids;
        private string ownercompanyid;
        private string ownerdepartmentid;
        private string ownerpostid;
        private string ownerid;        
        private string createuserid;
        private DateTime createdate;
        //private string senddoctitle;
        //private string senddocid;
        //private string createusername;
        //private string grade;
        //private string priorities;
        //private string senddoctype;
        //private string num;
        //private string keywords;
        //private string checkstate;
        //private string issave;
        //private string isdistrbute;
        private byte[] arrbyte;
        private byte[] Arrbyte
        {
            get
            {
                arrbyte = new byte[0]; 
                return arrbyte;
            }
        }

        public T_OA_SENDDOC OACompanySendDoc
        {

            get 
            {
                
                senddoc.CONTENT = Arrbyte;
                senddoc.T_OA_SENDDOCTYPE = null;
                
                return senddoc; 
            }
            set 
            {
                
                senddoc = value;
                senddoc.CONTENT = Arrbyte;
                senddoc.T_OA_SENDDOCTYPE = null;
            }
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
        public DateTime CREATEDATE
       {
           get { return createdate; }
           set { createdate = value; }
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

        public string Guids
        {
            get { return guids; }
            set { guids = value; }
        }

        //public string SENDDOCTITLE
        //{
        //    get { return senddoctitle; }
        //    set { senddoctitle = value; }
        //}

        //public string SENDDOCTYPE
        //{
        //    get { return senddoctype; }
        //    set { senddoctype = value; }
        //}

        //public string GRADED
        //{
        //    get { return grade; }
        //    set { grade = value; }
        //}
        //public string PRIORITIES
        //{
        //    get { return priorities; }
        //    set { priorities = value; }
        //}
        //public string NUM
        //{
        //    get { return num; }
        //    set { num = value; }
        //}
        //public string KEYWORDS
        //{
        //    get { return keywords; }
        //    set { keywords = value; }
        //}
        //public string CREATEUSERNAME
        //{
        //    get { return createusername; }
        //    set { createusername = value; }
        //}
        //public string SENDDOCID
        //{
        //    get { return senddocid; }
        //    set { senddocid = value; }
        //}

        //public string ISSAVE
        //{
        //    get { return issave; }
        //    set { issave = value; }
        //}
        //public string ISDISTRIBUTE
        //{
        //    get { return isdistrbute; }
        //    set { isdistrbute = value; }
        //}

        //public string CHECKSTATE
        //{
        //    get { return checkstate; }
        //    set { checkstate = value; }
        //}
    }
}
