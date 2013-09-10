using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
//using SMT.SaaS.OA.Services.FlowService;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.SaaS.OA.DAL;
using System.Collections.ObjectModel;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {

        #region 调查方案
        [OperationContract]
        public int AddEmployeeSurveyView(V_EmployeeSurvey employeeSurveyView)
        {
            using (EmployeeSurveysMasterBll employeemaster = new EmployeeSurveysMasterBll())
            {
                return employeemaster.AddEmployeeSurveysView(employeeSurveyView);
            }
        }

        [OperationContract]
        public List<V_EmployeeSurvey> GetEmployeeSurveyViewList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                IQueryable<V_EmployeeSurvey> infoViewList = null;
                if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
                {
                    infoViewList = employeeSurveyViewBll.GetEmployeeSurveyViewListByFlag(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
                }
                else
                {
                    string isView = "1";
                    if (checkState == "4")
                    {
                        isView = "0";
                    }
                    ServiceClient workFlowWS = new ServiceClient();
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_REQUIREMASTER", companyId, userId);
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
                    infoViewList = employeeSurveyViewBll.GetEmployeeSurveyViewListByFlag(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
                }
                if (infoViewList != null)
                {
                    return infoViewList.ToList();
                }
                return null;
            }
        }
        //参与调查时用 
        [OperationContract]
        public V_EmployeeSurvey Get_ESurvey(string requireMasterID)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                IQueryable<V_EmployeeSurvey> lst = employeeSurveyViewBll.Get_ESurvey(requireMasterID);
                if (lst != null)
                    return lst.ToList()[0];
                else
                    return null;
            }
        }
       //调查申请,选择审核通过的调查方案，时用
        [OperationContract]
        public List<V_EmployeeSurvey> Get_ESurveyChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                IQueryable<V_EmployeeSurvey> infoViewList = employeeSurveyViewBll.Get_ESurveyChecked(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, "2");
                if (infoViewList != null)
                    return infoViewList.ToList();
                return null;
            }
        }

        [OperationContract]
        public int DeleteEmployeeSurveyViewList(List<V_EmployeeSurvey> infoViewList)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                if (employeeSurveyViewBll.DeleteEmployeeSurveyViewList(infoViewList) == 1)
                {
                    return 1;
                }
                return -1;
            }
        }
        //删除题目视图
        [OperationContract]
        public int DeleteEmployeeSurveySubjectView(List<V_EmployeeSurveySubject> lst)
        {
            using (EmployeeSurveySubjectViewBll _sub = new EmployeeSurveySubjectViewBll())
            {
                return _sub.DeleteEmployeeSurveySubjectView(lst[0]);
            }
        }

        //更新方案 新
        [OperationContract]
        public int Upd_ESurvey(T_OA_REQUIREMASTER survey,List<V_EmployeeSurveySubject> addLst,List<V_EmployeeSurveySubject> updLst)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Upd_ESurvey(survey, addLst, updLst);
            }
        }
        //更新方案 旧
        [OperationContract]
        public int UpdateEmployeeSurveyView(V_EmployeeSurvey infoView)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                if (employeeSurveyViewBll.UpdateEmployeeSurveyView(infoView) == 1)
                {
                    return 1;
                }
                return -1;
            }
        }

       
        [OperationContract]
        public List<T_OA_REQUIRERESULT> GetResultByUserID(string userID, string masterID)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.GetSubjectResultByUserID(userID, masterID);
            }
        }

        [OperationContract]
        public int SubmitResult(List<T_OA_REQUIRERESULT> resultInfoList)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                if (employeeSurveyViewBll.AddResultInfoList(resultInfoList) == 1)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }
        [OperationContract]
        public List<V_EmployeeID> GetEmployeeList(string resqId)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.GetEmployeeList(resqId);
            }
        }
#endregion 

        #region 调查申请
        [OperationContract]
        private T_OA_REQUIRE Get_ESurveyApp(string id)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Get_ESurveyApp(id);
            }
        }
        [OperationContract]
        public List<T_OA_REQUIRE> Get_ESurveyApps(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                List<T_OA_REQUIRE> infoViewList = null;
                if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
                {
                    infoViewList = employeeSurveyViewBll.Get_ESurveyApps(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
                }
                else
                {
                    string isView = "1";
                    if (checkState == "4")
                    {
                        isView = "0";
                    }
                    ServiceClient workFlowWS = new ServiceClient();
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_REQUIRE", companyId, userId);
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
                    infoViewList = employeeSurveyViewBll.Get_ESurveyApps(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
                }
                if (infoViewList != null)
                {
                    return infoViewList.ToList();
                }
                return null;
            }
        }
        //调查申请,选择审核通过的调查方案，时用
        [OperationContract]
        public List<T_OA_REQUIRE> Get_ESurveyAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                IQueryable<T_OA_REQUIRE> infoViewList = employeeSurveyViewBll.Get_ESurveyAppChecked(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, "2");
                if (infoViewList != null)
                    return infoViewList.ToList();
                return null;
            }
        }

        //获取我参与的员工调查  获取参与调查的数据 2010-8-4
        [OperationContract]
        public List<V_MyEusurvey> Get_MyVisistedSurvey(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState, string postID, string companyID, string departmentID)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                IQueryable<V_MyEusurvey> infoViewList = employeeSurveyViewBll.Get_MyVisistedSurvey(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, "2", postID, companyID, departmentID);
                if (infoViewList != null && infoViewList.Count() > 0)
                {
                    return infoViewList.ToList();
                }
                return null;
            }
        }


        [OperationContract]
        public int Add_EsurveyApp(T_OA_REQUIRE info)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Add_EsurveyApp(info);
            }
        }
        /// <summary>
        /// 修改调查申请
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [OperationContract]
        public int Upd_ESurveyApp(T_OA_REQUIRE info)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Upd_ESurveyApp(info);
            }
        }
        [OperationContract]
        public int Del_ESurveyApp( List<T_OA_REQUIRE> lst)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Del_ESurveyApp(lst);
            }
        }
        /// <summary>
        /// 获取员工参与调查的结果
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_SATISFACTIONRESULT> GetStaticfactonVisistedByEmployID(string UserID)
        { 
            SatisfactionBll bll = new SatisfactionBll();
            IQueryable<T_OA_SATISFACTIONRESULT> result = bll.GetStaticfactonVisistedByEmployID(UserID);
            return result.Count() > 0 ? result.ToList() : null;
        }
        #endregion 

        #region 调查发布

        [OperationContract]
        private T_OA_REQUIREDISTRIBUTE Get_ESurveyResult(string id)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Get_ESurveyResult(id);
            }
        }

        [OperationContract]
        public List<T_OA_REQUIREDISTRIBUTE> Get_ESurveyResults(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                IQueryable<T_OA_REQUIREDISTRIBUTE> infoViewList = null;
                if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
                {
                    infoViewList = employeeSurveyViewBll.Get_ESurveyResults(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
                }
                else
                {
                    string isView = "1";
                    if (checkState == "4")
                    {
                        isView = "0";
                    }
                    ServiceClient workFlowWS = new ServiceClient();
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_REQUIREDISTRIBUTE", companyId, userId);
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
                    infoViewList = employeeSurveyViewBll.Get_ESurveyResults(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
                }
                if (infoViewList != null)
                {
                    return infoViewList.ToList();
                }
                return null;
            }
        }
        [OperationContract]
        public int Add_ESurveyResult(T_OA_REQUIREDISTRIBUTE info)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Add_ESurveyResult(info);
            }
        }        
        /// 修改调查申请       
        [OperationContract]
        public int Upd_ESurveyResult(T_OA_REQUIREDISTRIBUTE info, ObservableCollection<T_OA_DISTRIBUTEUSER> distributeuser)
        {
            EmployeeDistrbuteBll distrbute = new EmployeeDistrbuteBll();
            return distrbute.Upd_ESurveyResult(info);
            //return employeeSurveyViewBll.Upd_ESurveyResult(info,DistributeListObj);
        }
        [OperationContract]
        public int Del_ESurveyResult(List<T_OA_REQUIREDISTRIBUTE> lst)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Del_ESurveyResult(lst);
            }
        }

        [OperationContract]
        ///返回员工调查发布和满意度调查发布
        ///2010-09-20 liujin
        public List<V_SystemNotice> GetStaffSurveyInfos(string userID, string postID, string companyID, string departmentID)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                IQueryable<V_SystemNotice> ent = employeeSurveyViewBll.GetStaffSurveyInfo(userID, postID, companyID, departmentID);
                return ent != null ? ent.ToList() : null;
            }
        }
        #endregion 

        #region 调查结果
        //无用 10-7-8
        [OperationContract]
        public int GetResultCount(T_OA_REQUIREDETAIL answerInfo)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                int resultCount = employeeSurveyViewBll.GetResultCount(answerInfo);
                if (resultCount > 0)
                {
                    return resultCount;
                }
                else
                {
                    return 0;
                }
            }
        }
        //(员工调查)统计每个调查申请单的结果明细
        [OperationContract]
        public List<V_SatisfactionResult> Result_ESurveyByRequireID(string id)
        {
            using (EmployeeSurveyViewBll employeeSurveyViewBll = new EmployeeSurveyViewBll())
            {
                return employeeSurveyViewBll.Result_SurveyByRequireID(id);
            }
        }
        #endregion 
    }
}