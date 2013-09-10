using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_ArchivesLending 
    {
        public T_OA_ARCHIVES archives;
        public T_OA_LENDARCHIVES archivesLending;
        public V_FlowAPP flowApp;
        private string guids;

        public string Guids
        {
            get { return guids; }
            set { guids = value; }
        }

       

        //public T_OA_LENDINGARCHIVES ArchivesLending
        //{
        //    get { return archivesLending; }
        //    set { archivesLending = value; }
        //}

        //public T_OA_ARCHIVES Archives
        //{
        //    get { return archives; }
        //    set { archives = value; }
        //}
    }
}
