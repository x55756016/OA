using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class EmployeeSurveysResultDal : CommDaL<T_OA_REQUIRERESULT>
    {
        public bool AddRequireResult(T_OA_REQUIRERESULT surveyResult)
        {
            try
            {
                surveyResult.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveyResult.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                surveyResult.T_OA_REQUIRE = base.GetObjectByEntityKey(surveyResult.T_OA_REQUIRE.EntityKey) as T_OA_REQUIRE;
                base.Add(surveyResult);
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

        public bool UpdateRequireResult(T_OA_REQUIRERESULT surveyResult)
        {
            try
            {
                if (surveyResult.EntityKey == null)
                {
                    surveyResult.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveyResult.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                    surveyResult.T_OA_REQUIRE = base.GetObjectByEntityKey(surveyResult.T_OA_REQUIRE.EntityKey) as T_OA_REQUIRE;
                    base.Add(surveyResult);
                }
                else
                {
                    T_OA_REQUIRERESULT tmpobj = base.GetObjectByEntityKey(surveyResult.EntityKey) as T_OA_REQUIRERESULT;
                    tmpobj.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveyResult.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                    tmpobj.T_OA_REQUIRE = base.GetObjectByEntityKey(surveyResult.T_OA_REQUIRE.EntityKey) as T_OA_REQUIRE;
                    base.Update(surveyResult);
                }
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
