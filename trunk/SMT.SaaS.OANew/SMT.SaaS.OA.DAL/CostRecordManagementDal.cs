using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class CostRecordManagementDal : CommDaL<T_OA_COSTRECORD>
    {
        public bool AddCostRecord(T_OA_COSTRECORD costRecordInfo)
        {
            try
            {
              
                costRecordInfo.T_OA_VEHICLE = base.GetObjectByEntityKey(costRecordInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                int i = base.Add(costRecordInfo);
                //int i = SaveContextChanges();
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

        public bool UpdateCostRecord(T_OA_COSTRECORD costRecordInfo)
        {
            try
            {
                T_OA_COSTRECORD tmpobj = base.GetObjectByEntityKey(costRecordInfo.EntityKey) as T_OA_COSTRECORD;
                tmpobj.T_OA_VEHICLE = base.GetObjectByEntityKey(costRecordInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.Update(costRecordInfo);
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
