using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class ConserVationManagementDal : CommDaL<T_OA_CONSERVATION>
    {
        public bool AddConserVation(T_OA_CONSERVATION conserVationInfo)
        {
            try
            {
                conserVationInfo.T_OA_VEHICLE = base.GetObjectByEntityKey(conserVationInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;

                base.AddToContext(conserVationInfo);
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

        public bool UpdateConserVation(T_OA_CONSERVATION conserVationInfo)
        {
            try
            {
                if (conserVationInfo.EntityKey == null)
                    conserVationInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_CONSERVATION", "CONSERVATIONID", conserVationInfo.CONSERVATIONID);
                T_OA_CONSERVATION tmpobj = base.GetObjectByEntityKey(conserVationInfo.EntityKey) as T_OA_CONSERVATION;
                tmpobj.T_OA_VEHICLE = base.GetObjectByEntityKey(conserVationInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.UpdateFromContext(conserVationInfo);
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
        #region 保养记录

        public int Add_VCRecord(T_OA_CONSERVATIONRECORD info)
        {
            int n = 0;
            try
            {
                info.T_OA_CONSERVATION = base.GetObjectByEntityKey(info.T_OA_CONSERVATION.EntityKey) as T_OA_CONSERVATION;
                base.AddToContext(info);
                n = SaveContextChanges();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return n;
        }

        public int Upd_VCRecord(T_OA_CONSERVATIONRECORD info)
        {
            int n = 0;
            try
            {
                T_OA_CONSERVATIONRECORD tmpobj = base.GetObjectByEntityKey(info.EntityKey) as T_OA_CONSERVATIONRECORD;
                tmpobj.T_OA_CONSERVATION = base.GetObjectByEntityKey(info.T_OA_CONSERVATION.EntityKey) as T_OA_CONSERVATION;
                n = base.Update(info);
                //n = SaveContextChanges();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return n;
        }
        #endregion
    }
}
