using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.BLLCommonServices.FlowWFService;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOACommonAdmin
    {
        
        //平台审核 进入
        [OperationContract]
        public T_OA_VEHICLEUSEAPP Get_VehicleUseApp(string id)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            return vehicleUseManagerBll.Get_VehicleUseApp(id);
        }
        [OperationContract]
        public List<T_OA_VEHICLEUSEAPP> GetVehicleUseAppInfoList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            IEnumerable<T_OA_VEHICLEUSEAPP> infoList = null;
            if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                infoList = vehicleUseManagerBll.GetVehicleInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (checkState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_VEHICLEUSEAPP", companyId, userId);
                if (flowList == null)
                {
                    return null;
                }
                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                {
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T .FORMID );
                }
                if (guidStringList.Count < 1)
                {
                    return null;
                }
                infoList = vehicleUseManagerBll.GetVehicleInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
            }
            if (infoList == null)
            {
                return null;
            }
            else
            {
                return infoList.ToList();
            }
        }
        //获取已经通过审核的申请用车单
        [OperationContract]
        public List<T_OA_VEHICLEUSEAPP> GetCanUseVehicleUseAppInfoList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            IQueryable<T_OA_VEHICLEUSEAPP> objList= vehicleUseManagerBll.GetCanUseVehicleUseAppInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, "2");
            if (objList == null)
            {
                return null;
            }
            return objList.ToList();
        }
      
        [OperationContract]
        public int AddVehicleUseApp(T_OA_VEHICLEUSEAPP vehicleUseAppInfo)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            if (vehicleUseManagerBll.AddVehicleUseApp(vehicleUseAppInfo))
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        public int AddVehicleUseAppList(List<T_OA_VEHICLEUSEAPP> vehicleUseAppList)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            foreach (T_OA_VEHICLEUSEAPP obj in vehicleUseAppList)
            {
                if (!vehicleUseManagerBll.AddVehicleUseApp(obj))
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        public int UpdateVehicleUseAppList(List<T_OA_VEHICLEUSEAPP> vehicleUseAppList)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            foreach (T_OA_VEHICLEUSEAPP obj in vehicleUseAppList)
            {
                if (vehicleUseManagerBll.UpdateVehicleUseApp(obj) == -1)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        public int UpdateVehicleUseApp(T_OA_VEHICLEUSEAPP vehicleUseApp)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            if (vehicleUseManagerBll.UpdateVehicleUseApp(vehicleUseApp) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        public int DeleteVehicleUseAppList(List<T_OA_VEHICLEUSEAPP> vehicleUseAppList)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            foreach (T_OA_VEHICLEUSEAPP obj in vehicleUseAppList)
            {
                if (!vehicleUseManagerBll.DeleteVehicleUseApp(obj.VEHICLEUSEAPPID))
                {
                    return -1;
                }
            }
            return 1;
        }
        [OperationContract]
        public T_OA_VEHICLEUSEAPP GetVehicleUseAppById(string VehicleUseAppId)
        {
            VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
            T_OA_VEHICLEUSEAPP VehicleUseAppById = vehicleUseManagerBll.GetVehicleUseAppById(VehicleUseAppId);
            return VehicleUseAppById == null ? null : VehicleUseAppById;
        }
    }
}
