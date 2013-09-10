using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class EmployeeSurveysMasterDal : CommDaL<T_OA_REQUIREMASTER>
    {
        public bool AddRequireMaster(T_OA_REQUIREMASTER requireRequireMaster)
        {
            try
            {
                requireRequireMaster = base.GetObjectByEntityKey(requireRequireMaster.EntityKey) as T_OA_REQUIREMASTER;
                base.Add(requireRequireMaster);
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

        public bool UpdateRequireMaster(T_OA_REQUIREMASTER requireRequireMaster)
        {
            try
            {
                T_OA_REQUIREMASTER tmpobj = base.GetObjectByEntityKey(requireRequireMaster.EntityKey) as T_OA_REQUIREMASTER;
                base.Update(requireRequireMaster);
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
