using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.BLLCommonServices.EngineConfigWS;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.BLL
{
    public class EmployeeSurveysBLL : BaseBll<T_OA_REQUIREMASTER>
    {
        /// <summary>
        /// 查询员工调查方案
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userId"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public IQueryable<T_OA_REQUIREMASTER> GetEmployeeSurveys(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, string checkState)
        {
            var ents = from t in dal.GetObjects<T_OA_REQUIREMASTER>() select t;

            List<object> queryParas = new List<object>();
            if (paras != null)
            {
                queryParas.AddRange(paras);
            }
            // 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            if (checkState != "4")
            {
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "OAREQUIREMASTER");

                if (checkState != "5")
                {
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        if (!string.IsNullOrEmpty(filterString))
                        {
                            filterString += " AND";
                        }

                        filterString += " CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(checkState);
                    }
                }
            }
            else
            {
                UtilityClass.SetFilterWithflow("REQUIREMASTERID", "OAREQUIREMASTER", userId, ref checkState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count())
                {
                    return null;
                }
            }

            if (queryParas.Count > 0)
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_OA_REQUIREMASTER>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }


        /// <summary>
        /// 删除员工调查方案
        /// </summary>
        /// <param name="requiremasterIDs"></param>
        /// <returns></returns>
        public bool DeleteEmployeeSurveys(string[] requiremasterIDs)
        {
            try
            {
                var ents = from t in dal.GetObjects<T_OA_REQUIREMASTER>()
                           where requiremasterIDs.Contains(t.REQUIREMASTERID)
                           select t;

                BeginTransaction();

                if (ents.Count() > 0)
                {
                    foreach (var itemMaster in ents)
                    {
                     var ent = from t in dal.GetObjects<T_OA_REQUIREDETAIL>()
                                  where itemMaster.REQUIREMASTERID == t.REQUIREMASTERID
                                  select t;
                        foreach (var itemDetail in ent)
                        {
                            // 删除子表
                            dal.DeleteFromContext(itemDetail);
                        }

                        var ent1 = from t in dal.GetObjects<T_OA_REQUIREDETAIL2>()
                                   where itemMaster.REQUIREMASTERID == t.REQUIREMASTERID
                                   select t;
                        foreach (var detail in ent1)
                        {
                            // 删除子表
                            dal.DeleteFromContext(detail);
                        }

                        // 删除主表
                        dal.DeleteFromContext(itemMaster);
                    }
                }
                dal.SaveContextChanges();
                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Tracer.Debug("调查方案EmployeeSurveysBLL-DeleteEmployeeSurveys" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

       /// <summary>
        /// 新增调查方案
       /// </summary>
       /// <param name="master"></param>
       /// <returns></returns>
        public bool AddEmployeeSurveys(T_OA_REQUIREMASTER master)
        {
            try
            {
                Utility.RefreshEntity(master);
                dal.Add(master);
                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveysBLL-AddEmployeeSurveys" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 修改调查方案
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
        public bool UpdateEmployeeSurveys(V_EmployeeSurveyMaster master)
        {
            try
            {
                string[] requiremasterIDs = new string[1];
                requiremasterIDs[0] = master.masterEntity.REQUIREMASTERID;
                DeleteEmployeeSurveys(requiremasterIDs);

                EmployeeSurveyBll bbl = new EmployeeSurveyBll();
                bbl.AddRequireMaster(master);

                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveysBLL-UpdateEmployeeSurveys" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
       /// 根据申请ID查询方案题目
       /// </summary>
       /// <param name="requireID"></param>
       /// <returns></returns>
        public V_EmployeeSurveysModel GetDataByRequireID(string requireID)
        {
            try
            {
                V_EmployeeSurveysModel model = new V_EmployeeSurveysModel();

                var ents = from t in dal.GetObjects<T_OA_REQUIRE>() //申请
                          where t.REQUIREID ==requireID
                          select t;

                // 题目
                var ents1 = from t in dal.GetObjects<T_OA_REQUIRE>()
                            join t1 in dal.GetObjects<T_OA_REQUIREDETAIL2>() on t.T_OA_REQUIREMASTER.REQUIREMASTERID
                                equals t1.REQUIREMASTERID into t2
                            from t3 in t2.DefaultIfEmpty()
                            where t.REQUIREID == requireID
                            select t3;

                // 方案
                var ents2 = from t in dal.GetObjects<T_OA_REQUIRE>()
                            join t1 in dal.GetObjects<T_OA_REQUIREMASTER>() on t.T_OA_REQUIREMASTER.REQUIREMASTERID equals t1.REQUIREMASTERID into t2
                            from t3 in t2.DefaultIfEmpty()
                            where t.REQUIREID == requireID
                            select t3;

                model.T_OA_REQUIRE = ents.FirstOrDefault();
                model.T_OA_REQUIREDETAIL2 = ents1.ToList();
                model.T_OA_REQUIREMASTER = ents2.FirstOrDefault();
                //model.T_OA_REQUIRERESULT = ents.FirstOrDefault().T_OA_REQUIREDISTRIBUTE;
                return model;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工参与调查EmployeeSurveysBLL-GetDataByRequireID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 查询题目选项，答案表
        /// </summary>
        /// <param name="requiremasterID">方案ID</param>
        /// <param name="subjectID">题号</param>
        /// <returns></returns>
        public List<V_REQUIRERESULTMODE> GetAnswer(string requiremasterID, decimal subjectID, string createuserID)
        {
            try
            {
                List<V_REQUIRERESULTMODE> listMode=new List<V_REQUIRERESULTMODE>();
                // 题目选项
                var ents = from t in dal.GetObjects<T_OA_REQUIREDETAIL>()
                           where t.REQUIREMASTERID == requiremasterID
                                 && t.SUBJECTID == subjectID
                           select new V_REQUIRERESULTMODE
                                      {
                                          T_OA_REQUIREDETAIL = t,
                                          RESULT = false
                                      };
                // 答案表
                var ents1 = from t in dal.GetObjects<T_OA_REQUIRERESULT>()
                            where t.T_OA_REQUIREMASTER.REQUIREMASTERID == requiremasterID
                                  && t.SUBJECTID == subjectID
                                  && t.CREATEUSERID == createuserID
                            select t;

                if (ents.Count() > 0)
                {
                    foreach (var ent in ents)
                    {
                        foreach (var requireresult in ents1)
                        {
                            if (ent.T_OA_REQUIREDETAIL.CODE == requireresult.CODE)
                            {
                                ent.RESULT = true;
                                ent.T_OA_REQUIRERESULT = requireresult;
                            }
                        }
                        listMode.Add(ent);
                    }
                }
                return  listMode;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工参与调查EmployeeSurveysBLL-GetDataByRequireID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 添加调查结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool AddRequireresult(List<T_OA_REQUIRERESULT> result)
        {
            try
            {
                foreach (var requireresult in result)
                {
                    Utility.RefreshEntity(requireresult);
                    dal.Add(requireresult);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工参与调查EmployeeSurveysBLL-AddRequireresult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 删除调查结果
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public bool DeleteRequireresult(string[] IDs)
        {
            try
            {
                var ents = from t in dal.GetObjects<T_OA_REQUIRERESULT>()
                           where IDs.Contains(t.REQUIRERESULTID)
                           select t;

                BeginTransaction();
                if (ents.Count() > 0)
                {
                    foreach (var ent in ents)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }
                dal.SaveContextChanges();
                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工参与调查EmployeeSurveysBLL-DeleteRequireresult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                RollbackTransaction();
                return false;
            }
        }

        /// <summary>
        ///  审核员工调查申请表 生成待办任务
        /// </summary>
        /// <param name="require"></param>
        /// <returns></returns>
        public bool CheckRequire(T_OA_REQUIRE require)
        {
            try
            {
                var entity =
                   dal.GetObjects<T_OA_REQUIRE>().Where(
                       s => s.REQUIREID == require.REQUIREID).
                       FirstOrDefault();

                if (entity != null)
                {
                    // 克隆实体
                    CloneEntity(require, entity);
                }
                dal.Update(entity);

                if (require.CHECKSTATE == "2") // 审核通过时，生成待办任务
                {
                    List<T_OA_REQUIREDISTRIBUTE> listUte = new List<T_OA_REQUIREDISTRIBUTE>();
                    listUte = require.T_OA_REQUIREDISTRIBUTE.ToList();
                   
                    
                    for (int i = 0; i < listUte.Count; i++)
                    {
                        // 添加到员工调查发布申请表
                        AddRequireDistribute(listUte[i]);

                        // 生成待办任务
                        CreateNotifyTask(listUte[i]);
                       
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工参与调查EmployeeSurveysBLL-CheckRequire" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 添加到员工调查发布申请表
        /// </summary>
        /// <param name="distribute"></param>
        /// <returns></returns>
        public bool AddRequireDistribute(T_OA_REQUIREDISTRIBUTE distribute)
        {
            try
            {
                Utility.RefreshEntity(distribute);
                dal.Add(distribute);
                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工参与调查EmployeeSurveysBLL-CheckRequire" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// 生成待办任务
        /// </summary>
        /// <param name="notifyfeedback"></param>
        private void CreateNotifyTask(T_OA_REQUIREDISTRIBUTE distribute)
        {
            EngineWcfGlobalFunctionClient engClient = new EngineWcfGlobalFunctionClient();
            CustomUserMsg[] cust = new CustomUserMsg[1];
            cust[0] = new CustomUserMsg() { FormID = distribute.REQUIREDISTRIBUTEID, UserID = distribute.OWNERID };
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = ObjListToXml<T_OA_REQUIREDISTRIBUTE>(distribute, "OA");
            engClient.ApplicationMsgTrigger(cust, "OA", "T_OA_REQUIREDISTRIBUTE", strXmlObjectSource, MsgType.Task);
        }


        /// <summary>
        /// 引擎需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, string SystemCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<System>");
            Type objtype = objectdata.GetType();
            sb.AppendLine("<Name>" + SystemCode + "</Name>");
            sb.AppendLine("<Object Name=\"Approval\" Description=\"\">");
            PropertyInfo[] propinfos = objtype.GetProperties();
            foreach (PropertyInfo propinfo in propinfos)
            {
                if (propinfo.Name.ToUpper() != "CHECKSTATE")
                {
                    sb.AppendLine("<Attribute Name=\"" + propinfo.Name + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + propinfo.GetValue(objectdata, null) + "\"/>");
                }
            }
            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"001\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
        }


        /// <summary>
        /// 查询员工调查发布申请
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userId"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public IQueryable<T_OA_REQUIREDISTRIBUTE> GetRequireDistribute(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, string checkState)
        {
            try
            {
                var ents = from t in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>().Include("T_OA_REQUIRE") select t;
               

                List<object> queryParas = new List<object>();
                if (paras != null)
                {
                    queryParas.AddRange(paras);
                }
                // 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
                if (checkState != "4")
                {
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_REQUIREDISTRIBUTE");

                    if (checkState != "5")
                    {
                        if (!string.IsNullOrEmpty(checkState))
                        {
                            if (!string.IsNullOrEmpty(filterString))
                            {
                                filterString += " AND";
                            }

                            filterString += " CHECKSTATE == @" + queryParas.Count();
                            queryParas.Add(checkState);
                        }
                    }
                }
                else
                {
                    UtilityClass.SetFilterWithflow("REQUIREDISTRIBUTEID", "T_OA_REQUIREDISTRIBUTE", userId, ref checkState, ref filterString, ref queryParas);
                    if (queryParas.Count() == paras.Count())
                    {
                        return null;
                    }
                }

                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);

                ents = Utility.Pager<T_OA_REQUIREDISTRIBUTE>(ents, pageIndex, pageSize, ref pageCount);
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工参与调查EmployeeSurveysBLL-GetRequireDistribute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
           
        }

    }
}
