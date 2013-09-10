using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using System.Collections.ObjectModel;
namespace SMT.SaaS.OA.Services
{
    public partial class SmtOACommonAdmin
    {
        
        [OperationContract]
        public List<T_OA_VEHICLE> GetVehicleInfoListByPage(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            IQueryable<T_OA_VEHICLE> infoList = vehicleManagerBll.GetVehicleInfoList(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
            if (infoList != null)
            {
                return infoList.ToList();
            }
            return null;
        }

        [OperationContract]
        public List<T_OA_VEHICLE> GetVehicleInfoList()
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            IQueryable<T_OA_VEHICLE> infoList = vehicleManagerBll.GetVehicleInfoList();
            if (infoList != null)
            {
                return infoList.ToList();
            }
            return null;
        }

        [OperationContract]
        public List<T_OA_VEHICLE> GetCanUseVehicleInfoList()
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            IEnumerable<T_OA_VEHICLE> infoList = vehicleManagerBll.GetCanUseVehicleInfoList();
            if (infoList != null)
            {
                return infoList.ToList();
            }
            return null;
        }

        [OperationContract]
        public int AddVehicle(T_OA_VEHICLE searchVehicleInfo)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            if (vehicleManagerBll.AddVehicleInfo(searchVehicleInfo))
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        public int AddVehicleList(List<T_OA_VEHICLE> searchVehicleList)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            foreach (T_OA_VEHICLE obj in searchVehicleList)
            {
                if (!vehicleManagerBll.AddVehicleInfo(obj))
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        public int UpdateVehicleList(List<T_OA_VEHICLE> searchVehicleList)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            foreach (T_OA_VEHICLE obj in searchVehicleList)
            {
                if (vehicleManagerBll.UpdateVehicleInfo(obj) == -1)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        public int UpdateVehicle(T_OA_VEHICLE searchVehicle)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            if (vehicleManagerBll.UpdateVehicleInfo(searchVehicle) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        public int DeleteVehicleList(List<T_OA_VEHICLE> searchVehicleList)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            foreach (T_OA_VEHICLE obj in searchVehicleList)
            {
                if (!vehicleManagerBll.DeleteVehicleInfo(obj.ASSETID))
                {
                    return -1;
                }
            }
            return 1;
        }
        #region  停车卡
        /// <summary>
        ///获取停车卡 修改界面
        [OperationContract]
        public List<T_OA_VEHICLECARD> Get_VICard(string parentID)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            return vehicleManagerBll.Get_VICard(parentID);
        }
        //更新停车卡
        [OperationContract]
        public int Save_VICard(List<T_OA_VEHICLECARD> lstAdd, List<T_OA_VEHICLECARD> lstUpd)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            int n = 0;
            if (lstAdd != null && lstAdd.Count > 0)
                n = vehicleManagerBll.Add_VICard(lstAdd);
            if (lstUpd != null && lstUpd.Count > 0)
                n += vehicleManagerBll.Upd_VICard(lstUpd,lstUpd[0].T_OA_VEHICLE);
            return n;
        }     
        //删除停车卡
        [OperationContract]
        public int Del_VICard(List<string> ids)
        {
            VehicleInfoManageBll vehicleManagerBll = new VehicleInfoManageBll();
            return vehicleManagerBll.Del_VICard(ids);
        }
        #endregion
    }
}