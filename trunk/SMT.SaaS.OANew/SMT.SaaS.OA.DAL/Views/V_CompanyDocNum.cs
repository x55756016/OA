using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    /// <summary>
    /// 用来获取公司发文的 公文编号信息  用来自动生成
    /// </summary>
    public class V_CompanyDocNum
    {
        
        
        private string ownercompanyid;
        private string ownerdepartmentid;
        private string ownerpostid;
        private string ownerid;
        private string createuserid;
        private DateTime createdate;        
        private string num;        
        private string checkstate;        
       
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
        public string NUM
        {
            get { return num; }
            set { num = value; }
        }
        public string CHECKSTATE
        {
            get { return checkstate; }
            set { checkstate = value; }
        }
        
        
    }
}
