using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
     public class V_REQUIRERESULTMODE
    {
        /// <summary>
        /// 员工调查详表
        /// </summary>
        public T_OA_REQUIREDETAIL T_OA_REQUIREDETAIL
        {
            get;
            set;
        }

         /// <summary>
         /// 结果
         /// </summary>
         public bool RESULT
         {
             get;
             set;
         }

         /// <summary>
         /// 调查结果
         /// </summary>
         public T_OA_REQUIRERESULT T_OA_REQUIRERESULT
         {
             get;
             set;
         }

        
    }
}
