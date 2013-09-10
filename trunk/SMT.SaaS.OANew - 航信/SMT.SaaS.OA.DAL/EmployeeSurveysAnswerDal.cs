using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class EmployeeSurveysAnswerDal : CommDaL<T_OA_REQUIREDETAIL>
    {
        public EmployeeSurveysAnswerDal()
        {

        }

        public bool AddAnswer(T_OA_REQUIREDETAIL requireAnswer)
        {
            try
            {
                base.Add(requireAnswer);
                int i = SaveContextChanges();
                if (i > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public bool UpdateAnswer(T_OA_REQUIREDETAIL requireAnswer)
        {
            try
            {
                T_OA_REQUIREDETAIL tmpobj = base.GetObjectByEntityKey(requireAnswer.EntityKey) as T_OA_REQUIREDETAIL;
                base.Update(requireAnswer);
                int i = SaveContextChanges();
                if (i < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}