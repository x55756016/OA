using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;
namespace SMT.SaaS.OA.DAL
{
    public class VehicleDispatchManageDal : CommDaL<T_OA_VEHICLEDISPATCH>
    {
        public int AddVehicleDispatch(T_OA_VEHICLEDISPATCH vehicleDispatchInfo)
        {
            try
            {
                vehicleDispatchInfo.T_OA_VEHICLE = base.GetObjectByEntityKey(vehicleDispatchInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.AddToContext(vehicleDispatchInfo);
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

        public int AddVehicleDispatchAndDetail(T_OA_VEHICLEDISPATCH vehicleDispatchInfo, List<T_OA_VEHICLEDISPATCHDETAIL> vddList)
        {
            try
            {
                vehicleDispatchInfo.T_OA_VEHICLE = base.GetObjectByEntityKey(vehicleDispatchInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.AddToContext(vehicleDispatchInfo);
                
               

                foreach (T_OA_VEHICLEDISPATCHDETAIL obj in vddList)
                {
                    obj.T_OA_VEHICLEUSEAPP = base.GetObjectByEntityKey(obj.T_OA_VEHICLEUSEAPP.EntityKey) as T_OA_VEHICLEUSEAPP;
                    obj.T_OA_VEHICLEDISPATCH = base.GetObjectByEntityKey(vehicleDispatchInfo.EntityKey) as T_OA_VEHICLEDISPATCH;
                    base.AddToContext(obj);
                }
                int i = SaveContextChanges();
                if (i < 1)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public int UpdateVehicleDispatchAndDetail(T_OA_VEHICLEDISPATCH vehicleDispatchInfo, List<T_OA_VEHICLEDISPATCHDETAIL> vddList)
        {
            try
            {
                vehicleDispatchInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_VEHICLEDISPATCH", "VEHICLEDISPATCHID", vehicleDispatchInfo.VEHICLEDISPATCHID);

                T_OA_VEHICLEDISPATCH tmpobj = base.GetObjectByEntityKey(vehicleDispatchInfo.EntityKey) as T_OA_VEHICLEDISPATCH;
                tmpobj.T_OA_VEHICLE = base.GetObjectByEntityKey(vehicleDispatchInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.UpdateFromContext(vehicleDispatchInfo);
                foreach (T_OA_VEHICLEDISPATCHDETAIL obj in vddList)
                {
                    obj.T_OA_VEHICLEUSEAPP = base.GetObjectByEntityKey(obj.T_OA_VEHICLEUSEAPP.EntityKey) as T_OA_VEHICLEUSEAPP;
                    obj.T_OA_VEHICLEDISPATCH = base.GetObjectByEntityKey(vehicleDispatchInfo.EntityKey) as T_OA_VEHICLEDISPATCH;
                    base.AddToContext(obj);
                }
                int i = SaveContextChanges();
                if (i < 1)
                {
                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public int UpdateVehicleDispatch(T_OA_VEHICLEDISPATCH vehicleDispatchInfo)
        {
            try
            {
                T_OA_VEHICLEDISPATCH tmpobj = base.GetObjectByEntityKey(vehicleDispatchInfo.EntityKey) as T_OA_VEHICLEDISPATCH;
                tmpobj.T_OA_VEHICLE = base.GetObjectByEntityKey(vehicleDispatchInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.Update(vehicleDispatchInfo);
                int i = SaveContextChanges();
                return i;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //添加调度记录
        public int Add_VDRecord(List<T_OA_VEHICLEDISPATCHRECORD> vddList)
        {
            try
            {
                foreach (T_OA_VEHICLEDISPATCHRECORD obj in vddList)
                {
                    obj.T_OA_VEHICLEDISPATCHDETAIL = base.GetObjectByEntityKey(obj.T_OA_VEHICLEDISPATCHDETAIL.EntityKey) as T_OA_VEHICLEDISPATCHDETAIL;
                    base.AddToContext(obj);
                }
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
    }
}
