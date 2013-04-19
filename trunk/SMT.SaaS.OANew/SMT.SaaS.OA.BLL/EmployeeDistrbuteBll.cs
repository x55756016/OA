using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects.DataClasses;
using SMT.Foundation.Log;
namespace SMT.SaaS.OA.BLL
{
    public class EmployeeDistrbuteBll : BaseBll<T_OA_REQUIREDISTRIBUTE>
    {
        public int Upd_ESurveyResult(T_OA_REQUIREDISTRIBUTE info)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.REQUIREDISTRIBUTEID == info.REQUIREDISTRIBUTEID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();

                    info.EntityKey = user.EntityKey;
                    T_OA_REQUIREDISTRIBUTE tmpobj = dal.GetObjectByEntityKey(info.EntityKey) as T_OA_REQUIREDISTRIBUTE;
                    //dal.UpdateFromContext(info);

                    //int i = dal.SaveContextChanges();
                    int i = Update(info);

                    if (i > 0)
                    {
                        return i;
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeDistrbuteBll-Upd_ESurveyResult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
                throw (ex);
            }
        }
    }
}
