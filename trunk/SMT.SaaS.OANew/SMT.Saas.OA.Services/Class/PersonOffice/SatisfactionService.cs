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
using SMT.SaaS.BLLCommonServices.FlowWFService;

namespace SMT.SaaS.OA.Services
{
    /// <summary>
    ///   满意度调查  copy员工调查
    /// </summary>
    public partial class SmtOAPersonOffice
    {
        #region 满意度调查方案
        [OperationContract]
        public bool AddSatisfactionsMaster(T_OA_SATISFACTIONMASTER key)
        {
            SatisfactionBll  masterBll=new SatisfactionBll();
            bool bl = masterBll.Add_SatisfactionMaster(key);
            if (bl == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        [OperationContract]
        public int Add_SSurvey(V_Satisfaction v)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Add_SSurvey(v);
            }
        }

        #region 查询满意调查方案
        /// <summary> 查询视图V_Satisfaction
        ///  根据调查方案ID查询视图数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [OperationContract]
        public IQueryable<V_Satisfaction> GetSurvey(string key)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
               IQueryable<V_Satisfaction> satList = satBll.Get_SSurvey(key);
               if (satList == null)
               {
                   return null;
               }
               else
               {
                   return satList;
 
               }
            }
        }
        #endregion

        //获取满意度调查方案
        [OperationContract]
        public List<V_Satisfaction> Get_SSurveys(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                IQueryable<V_Satisfaction> infoViewList = null;
                if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
                {
                    infoViewList = satBll.Get_SSurveys(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
                }
                else
                {
                    string isView = "1";
                    if (checkState == "4")
                    {
                        isView = "0";
                    }
                    ServiceClient workFlowWS = new ServiceClient();
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_SATISFACTIONMASTER", companyId, userId);
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
                    infoViewList = satBll.Get_SSurveys(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
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
        public V_Satisfaction Get_SSurvey(string requireMasterID)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                IQueryable<V_Satisfaction> lst = satBll.Get_SSurvey(requireMasterID);
                if (lst != null)
                    return lst.ToList()[0];
                else
                    return null;
            }
        }
       //调查申请,选择审核通过的调查方案，时用
        [OperationContract]
        public List<V_Satisfaction> Get_SSurveyChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                IQueryable<V_Satisfaction> infoViewList = satBll.Get_SSurveyChecked(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, "2");
                if (infoViewList != null)
                    return infoViewList.ToList();
                return null;
            }
        }
        //删除方案
        [OperationContract]
        public int Del_SSurveys(List<V_Satisfaction> infoViewList)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Del_SSurveys(infoViewList);
            }
        }
        //删除题目视图
        [OperationContract]
        public int Del_SSurveySub(List<T_OA_SATISFACTIONDETAIL> lst)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                int n = 0;
                foreach (T_OA_SATISFACTIONDETAIL i in lst)
                    n += satBll.Del_SSurveySub(i.SATISFACTIONDETAILID);
                return n;
            }
        }
        //更新审核状态
        [OperationContract]
        public int Upd_SSurveyChecked(V_Satisfaction infoView)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Upd_SSurveyChecked(infoView);
            }
        }
        //更新满意度方案、题目
        [OperationContract]
        public int Upd_SSurvey(V_Satisfaction infoView)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                if (satBll.Upd_SSurvey(infoView) == 1)
                {
                    return 1;
                }
                return -1;
            }
        }
      
        #endregion

        #region 调查申请
        [OperationContract]
        private T_OA_SATISFACTIONREQUIRE Get_SSurveyApp(string id)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Get_SSurveyApp(id);
            }
        }
        [OperationContract]
        public List<T_OA_SATISFACTIONREQUIRE> Get_SSurveyApps(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                IQueryable<T_OA_SATISFACTIONREQUIRE> infoViewList = null;
                if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
                {
                    infoViewList = satBll.Get_SSurveyApps(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
                }
                else
                {
                    string isView = "1";
                    if (checkState == "4")
                    {
                        isView = "0";
                    }
                    ServiceClient workFlowWS = new ServiceClient();
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_SATISFACTIONREQUIRE", companyId, userId);
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
                    infoViewList = satBll.Get_SSurveyApps(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
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
        public List<T_OA_SATISFACTIONREQUIRE> Get_SSurveyAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                IQueryable<T_OA_SATISFACTIONREQUIRE> infoViewList = satBll.Get_SSurveyAppChecked(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, "2");
                if (infoViewList != null)
                    return infoViewList.ToList();
                return null;
            }
        }

        //满意度调查申请, 获取参与调查的数据 2010-8-4
        [OperationContract]
        public List<V_MyStaticfaction> Get_StaticfactionSurveyAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState, string postID, string companyID, string departmentID)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                IQueryable<V_MyStaticfaction> infoViewList = satBll.Get_SaticfactionSurveyAppChecked(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, "2", postID, companyID, departmentID);
                if (infoViewList != null)
                    return infoViewList.ToList();
                return null;
            }
        }

        [OperationContract]
        public bool Add_SSurveyApp(T_OA_SATISFACTIONREQUIRE info)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                int bl = satBll.Add_SSurveyApp(info);
                if (bl == -1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        /// <summary>
        /// 修改调查申请  
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [OperationContract]
        public int Upd_SSurveyApp(T_OA_SATISFACTIONREQUIRE info)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Upd_SSurveyApp(info);
            }
        }
        [OperationContract]
        public int Del_SSurveyApp(List<T_OA_SATISFACTIONREQUIRE> lst)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Del_SSurveyApp(lst);
            }
        }
        #endregion 

        #region 调查发布
        [OperationContract]
        private T_OA_SATISFACTIONDISTRIBUTE Get_SSurveyResult(string id)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Get_SSurveyResult(id);
            }
        }
        [OperationContract]
        public List<T_OA_SATISFACTIONDISTRIBUTE> Get_SSurveyResults(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                IQueryable<T_OA_SATISFACTIONDISTRIBUTE> infoViewList = null;
                if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
                {
                    infoViewList = satBll.Get_SSurveyResults(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
                }
                else
                {
                    string isView = "1";
                    if (checkState == "4")
                    {
                        isView = "0";
                    }
                    ServiceClient workFlowWS = new ServiceClient();
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_SATISFACTIONDISTRIBUTE", companyId, userId);
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
                    infoViewList = satBll.Get_SSurveyResults(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
                }
                if (infoViewList != null)
                {
                    return infoViewList.ToList();
                }
                return null;
            }
        }
        [OperationContract]
        public int Add_SSurveyResult(T_OA_SATISFACTIONDISTRIBUTE info)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Add_SSurveyResult(info);
            }
        }        
        /// 修改调查申请       
        [OperationContract]
        public int Upd_SSurveyResult(T_OA_SATISFACTIONDISTRIBUTE info)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Upd_SSurveyResult(info);
            }
        }
        [OperationContract]
        public int Del_SSurveyResult(List<T_OA_SATISFACTIONDISTRIBUTE> lst)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Del_SSurveyResult(lst);
            }
        }
        #endregion 

        #region 调查结果 与统计
        //统计每个调查申请单的结果明细
        [OperationContract]
        public List<V_SatisfactionResult> Result_SurveyByRequireID(string surveyID)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Result_SurveyByRequireID(surveyID);
            }
        }       
        //统计每个题目的结果
        [OperationContract]
        public int Result_SurveySubID(string surveyID, string subID, string resultValue)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                int resultCount = satBll.Result_SurveySubID(surveyID, subID, resultValue);
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
        //参与调查，保存填写人的调查结果
        [OperationContract]
        public int Result_Save(List<T_OA_SATISFACTIONRESULT> resultInfoList)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Result_Save(resultInfoList);
            }
        }
        //获取每个人的调查结果
        [OperationContract]
        public List<T_OA_SATISFACTIONRESULT> Result_SubByUserID(string userID, string surveyID)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Result_SubByUserID(userID, surveyID);
            }
        }
        //获取考试人员名单
        [OperationContract]
        public List<V_EmployeeID> Result_EmployeeByRequireID(string resqId)
        {
            using (SatisfactionBll satBll = new SatisfactionBll())
            {
                return satBll.Result_EmployeeByRequireID(resqId);
            }
        }
        #endregion
    }
}