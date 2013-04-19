using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class VehicleDispatchDetailDal : CommDaL<T_OA_VEHICLEDISPATCHDETAIL>
    {
        public int AddVehicleDispatchDetail(T_OA_VEHICLEDISPATCHDETAIL vehicleDispatchDetailInfo)
        {
            try
            {
                vehicleDispatchDetailInfo.T_OA_VEHICLEDISPATCH = base.GetObjectByEntityKey(vehicleDispatchDetailInfo.T_OA_VEHICLEDISPATCH.EntityKey) as T_OA_VEHICLEDISPATCH;
                vehicleDispatchDetailInfo.T_OA_VEHICLEUSEAPP = base.GetObjectByEntityKey(vehicleDispatchDetailInfo.T_OA_VEHICLEUSEAPP.EntityKey) as T_OA_VEHICLEUSEAPP;
                base.Add(vehicleDispatchDetailInfo);
                int i = SaveContextChanges();
                if (i > 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public bool UpdateVehicleDispatchDetail(T_OA_VEHICLEDISPATCHDETAIL vehicleDispatchDetailInfo)
        {
            try
            {
                T_OA_VEHICLEDISPATCHDETAIL tmpobj = base.GetObjectByEntityKey(vehicleDispatchDetailInfo.EntityKey) as T_OA_VEHICLEDISPATCHDETAIL;
                tmpobj.T_OA_VEHICLEDISPATCH = base.GetObjectByEntityKey(vehicleDispatchDetailInfo.T_OA_VEHICLEDISPATCH.EntityKey) as T_OA_VEHICLEDISPATCH;
                tmpobj.T_OA_VEHICLEUSEAPP = base.GetObjectByEntityKey(vehicleDispatchDetailInfo.T_OA_VEHICLEUSEAPP.EntityKey) as T_OA_VEHICLEUSEAPP;
                base.Update(vehicleDispatchDetailInfo);
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
