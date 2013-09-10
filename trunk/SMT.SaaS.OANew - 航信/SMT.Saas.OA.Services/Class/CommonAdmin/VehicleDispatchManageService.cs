using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
//using SMT.SaaS.OA.Services.FlowService;
using SMT.SaaS.BLLCommonServices.FlowWFService;

namespace SMT.SaaS.OA.Services
{
    //派车管理
    public partial class SmtOACommonAdmin
    {
        

        /// <summary>
        /// 获取派车单
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_VEHICLEDISPATCH> Gets_VDInfo(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            IQueryable<T_OA_VEHICLEDISPATCH> infoList = null;
            if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                infoList = vehicleDispatchManagerBll.Gets_VDInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, null, checkState);
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (checkState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_VEHICLEDISPATCH", loginUserInfo.companyID, loginUserInfo.userID);
                if (flowList == null)
                {
                    return null;
                }
                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                {
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T.FORMID);
                }
                if (guidStringList.Count < 1)
                {
                    return null;
                }
                infoList = vehicleDispatchManagerBll.Gets_VDInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, guidStringList, checkState);
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
        //获取审核通过的派车单
        [OperationContract]
        public List<T_OA_VEHICLEDISPATCH> Gets_VDChecked(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, LoginUserInfo loginInfo)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            return vehicleDispatchManagerBll.Gets_VDChecked(pageIndex, pageSize, sort, filterString, paras, ref pageCount, loginInfo.userID, "2");
        }
        //获取已经派车的申请单及明细
        [OperationContract]
        public List<T_OA_VEHICLEDISPATCH> Gets_VDAndDetail(string sort, string filterString, IList<object> paras)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            return vehicleDispatchManagerBll.Gets_VDAndDetail(sort, filterString, paras);
        }
        //获取已经派车的申请单
        [OperationContract]
        public List<T_OA_VEHICLEUSEAPP> Get_ByParentID(string parentID)
        {
            VehicleDispatchDetailBll vddBll = new VehicleDispatchDetailBll();
            return vddBll.Get_ByParentID(parentID);
        }

        [OperationContract]
        public int AddVehicleDispatch(T_OA_VEHICLEDISPATCH vehicleDispatchInfo)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            if (vehicleDispatchManagerBll.AddVehicleDispatch(vehicleDispatchInfo) == 1)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        public int AddVehicleDispatchList(List<T_OA_VEHICLEDISPATCH> vehicleDispatchList)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            foreach (T_OA_VEHICLEDISPATCH obj in vehicleDispatchList)
            {
                if (vehicleDispatchManagerBll.AddVehicleDispatch(obj) == -1)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        public int UpdateVehicleDispatchList(List<T_OA_VEHICLEDISPATCH> vehicleDispatchList)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            foreach (T_OA_VEHICLEDISPATCH obj in vehicleDispatchList)
            {
                if (vehicleDispatchManagerBll.UpdateVehicleDispatch(obj) == -1)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        public int UpdateVehicleDispatch(T_OA_VEHICLEDISPATCH vehicleDispatch)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            if (vehicleDispatchManagerBll.UpdateVehicleDispatch(vehicleDispatch) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        public int DeleteVehicleDispatchList(List<T_OA_VEHICLEDISPATCH> vehicleDispatchList)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            VehicleDispatchDetailBll vddBll = new VehicleDispatchDetailBll();
            foreach (T_OA_VEHICLEDISPATCH obj in vehicleDispatchList)
            {
                if (vddBll.DeleteVehicleDispatchDetailByDiaspatchId(obj.VEHICLEDISPATCHID) == -1)
                {
                    return -1;
                }
                if (!vehicleDispatchManagerBll.DeleteVehicleDispatch(obj.VEHICLEDISPATCHID))
                {
                    return -1;
                }
            }
            return 1;
        }
        
        //删除派车明细
        [OperationContract]
        public int Del_VDDetails(string[] ids)
        {
            
            VehicleDispatchDetailBll vddBll = new VehicleDispatchDetailBll();
            return vddBll.Del_VDDetail(ids);
        }

        [OperationContract]
        public int UpdateVehicleDispatchAndDetail(T_OA_VEHICLEDISPATCH vehicleDispatch, List<T_OA_VEHICLEDISPATCHDETAIL> vddList)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            if (vehicleDispatchManagerBll.UpdateVehicleDispatchAndDetail(vehicleDispatch, vddList) != 1)
            {
                return -1;
            }
            return 1;
        }
        //添加派车及明细
        [OperationContract]
        public int AddVehicleDispatchAndDetail(T_OA_VEHICLEDISPATCH vehicleDispatch, List<T_OA_VEHICLEDISPATCHDETAIL> vddList)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            if (vehicleDispatchManagerBll.AddVehicleDispatchAndDetail(vehicleDispatch, vddList) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        public List<T_OA_VEHICLEDISPATCHDETAIL> GetDetailListByDispatchId(string dispatchId)
        {
            VehicleDispatchDetailBll vddBll = new VehicleDispatchDetailBll();
            IQueryable<T_OA_VEHICLEDISPATCHDETAIL> detailList = vddBll.GetDetailByDispatchId(dispatchId);
            if (detailList == null)
            {
                return null;
            }
            return detailList.ToList();
        }

        [OperationContract]
        public T_OA_VEHICLEDISPATCH GetVehicleDispatchById(string entityId)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            T_OA_VEHICLEDISPATCH VehicleDispatchById = vehicleDispatchManagerBll.GetVehicleDispatchById(entityId);
            return VehicleDispatchById == null ? null : VehicleDispatchById;
        }

        #region 调度记录
        [OperationContract]
        public List<T_OA_VEHICLEDISPATCHRECORD> Gets_VDRecord(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            IQueryable<T_OA_VEHICLEDISPATCHRECORD> infoList = null;
            if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
                infoList = vehicleDispatchManagerBll.Gets_VDRecord(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, null, checkState);
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (checkState == "4")
                    isView = "0";
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_VEHICLEDISPATCHRECORD", loginUserInfo.companyID, loginUserInfo.userID);
                if (flowList == null) return null;

                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T.FORMID);
                if (guidStringList.Count < 1)
                    return null;
                infoList = vehicleDispatchManagerBll.Gets_VDRecord(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, guidStringList, checkState);
            }
            if (infoList == null) return null;
            else
                return infoList.ToList();
        }

        /// 调度记录用  添加 ||平台审核 进入
        [OperationContract]
        public List<T_OA_VEHICLEDISPATCH> Get_VDInfo(string id)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            return vehicleDispatchManagerBll.Get_VDInfo(id);
        }
        //调度记录用 修改
        [OperationContract]
        public List<T_OA_VEHICLEDISPATCHRECORD> Get_VDRecord(string id)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            return vehicleDispatchManagerBll.Get_VDRecord(id);
        }
        /// <summary>
        /// 更新调度记录
        /// </summary>
        /// <param name="vddList"></param>
        /// <returns></returns>
        [OperationContract]
        public int Upd_VDRecord(List<T_OA_VEHICLEDISPATCHRECORD> vddList)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            if (vehicleDispatchManagerBll.Upd_VDRecord(vddList) != 1)
            {
                return -1;
            }
            return 1;
        }
        //添加调度记录
        [OperationContract]
        public int Add_VDRecord(List<T_OA_VEHICLEDISPATCHRECORD> vddList)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            return vehicleDispatchManagerBll.Add_VDRecord(vddList);
        }
        [OperationContract]
        public int Del_VDRecord(List<string> id)
        {
            VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
            return vehicleDispatchManagerBll.Del_VDRecord(id);
        }
        #endregion 调度记录
    }
}
