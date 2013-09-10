using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class MaintenanceAPPDal : CommDaL<T_OA_MAINTENANCEAPP>
    {
        public bool AddMaintenanceApp(T_OA_MAINTENANCEAPP maintenanceAppInfo)
        {
            try
            {
                maintenanceAppInfo.T_OA_VEHICLE = base.GetObjectByEntityKey(maintenanceAppInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.AddToContext(maintenanceAppInfo);
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

        public bool UpdateMaintenanceApp(T_OA_MAINTENANCEAPP maintenanceAppInfo)
        {
            try
            {
                if (maintenanceAppInfo.EntityKey == null)
                {
                    var ents = from ent in base.GetObjects<T_OA_MAINTENANCEAPP>().Include("T_OA_VEHICLE")
                               where ent.MAINTENANCEAPPID == maintenanceAppInfo.MAINTENANCEAPPID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        maintenanceAppInfo.EntityKey = ents.FirstOrDefault().EntityKey;
                    }
                }
                T_OA_MAINTENANCEAPP tmpobj = base.GetObjectByEntityKey(maintenanceAppInfo.EntityKey) as T_OA_MAINTENANCEAPP;
                tmpobj.T_OA_VEHICLE = base.GetObjectByEntityKey(maintenanceAppInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                base.UpdateFromContext(maintenanceAppInfo);
                
                
                 //base.UpdateFromContext(maintenanceAppInfo);
                 int i = base.SaveContextChanges();
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

        public int Add_VMRecord(T_OA_MAINTENANCERECORD info)
        {
            int n = 0;
            try
            {
                info.T_OA_MAINTENANCEAPP = base.GetObjectByEntityKey(info.T_OA_MAINTENANCEAPP.EntityKey) as T_OA_MAINTENANCEAPP;
                base.AddToContext(info);
                n = SaveContextChanges();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return n;
        }

        public int Upd_VMRecord(T_OA_MAINTENANCERECORD maintenanceAppInfo)
        {
            int n = 0;
            try
            {
                T_OA_MAINTENANCERECORD tmpobj = base.GetObjectByEntityKey(maintenanceAppInfo.EntityKey) as T_OA_MAINTENANCERECORD;
                tmpobj.T_OA_MAINTENANCEAPP = base.GetObjectByEntityKey(maintenanceAppInfo.T_OA_MAINTENANCEAPP.EntityKey) as T_OA_MAINTENANCEAPP;
                base.UpdateFromContext(maintenanceAppInfo);
                n = SaveContextChanges();     
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return n;
        }
    }
}