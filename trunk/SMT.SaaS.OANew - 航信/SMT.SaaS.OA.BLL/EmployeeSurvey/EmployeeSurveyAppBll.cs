using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL.Views;
using SMT.Foundation.Core;
using SMT.SaaS.BLLCommonServices.EngineConfigWS;
using System.Reflection;
using System.Data.Objects;
using SMT.SaaS.BLLCommonServices;

namespace SMT.SaaS.OA.BLL
{
    public class EmployeeSurveyAppBll : BaseBll<T_OA_REQUIRE>
    {
        #region 新增
        /// <summary>
        /// 新增员工调查申请
        ///// </summary>
        public bool AddRequire(V_EmployeeSurveyApp addApp)
        {
            try
            {
                base.BeginTransaction();
                bool add = base.Add(addApp.requireEntity);
                if (add)
                {
                    foreach (var item in addApp.distributeuserList)
                    {
                        dal.Add(item);
                    }  
                        base.CommitTransaction();
                        return true;                  
                }
                base.RollbackTransaction();
                return false;
                           }
            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("员工调查申请EmployeeSurveyAppBll-AddRequire" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改员工调查申请:分发范围待完善
        /// </summary>
        public bool UpdRequire(V_EmployeeSurveyApp updApp)
        {
            string _masterID = updApp.MasterId;
            string _requireID = updApp.requireEntity.REQUIREID;
            string _checkState = updApp.requireEntity.CHECKSTATE;
            try
            {
                base.BeginTransaction();
                foreach (var ents in updApp.oldDistributeuserList)
                {
                    var ent = from chlidData in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                              where
                                  new {chlidData.MODELNAME, chlidData.FORMID, chlidData.VIEWER, chlidData.VIEWTYPE} ==
                                  new {ents.MODELNAME, ents.FORMID, ents.VIEWER, ents.VIEWTYPE}
                              select chlidData;

                    if (ent.Count() > 0)
                    {
                        var _delData = ent.FirstOrDefault();
                        dal.DeleteFromContext(_delData);
                    }
                }
                int _delFlag = dal.SaveContextChanges();
                if (_delFlag > 0)
                {
                    var data = from items in dal.GetObjects<T_OA_REQUIRE>()
                               where items.REQUIREID ==_requireID
                               select items;
                    if (data.Count() > 0)
                    {
                        //建立entityKey,表示数据库不更新此字段(主键不允许更新)
                       updApp.requireEntity.EntityKey = Utility.AddEntityKey("T_OA_REQUIRE", "REQUIREID", _requireID);
                        if (updApp.requireEntity.T_OA_REQUIREMASTERReference.EntityKey == null)
                        {
                            updApp.requireEntity.T_OA_REQUIREMASTERReference.EntityKey = Utility.AddEntityKey("T_OA_REQUIREMASTER", "REQUIREMASTERID", _masterID); 
                        }
                        int updFlag = dal.Update(updApp.requireEntity);
                        if (updFlag > 0)
                        {
                            foreach (var users in updApp.distributeuserList)
                            {
                                dal.Add(users);
                            }
                            base.CommitTransaction();
                            if (_checkState == ((int)CheckStates.Approved).ToString())
                            {
                                foreach (var items in updApp.distributeuserList)
                                {
                                    CreateNotifyTask(items);
                                }
                            }
                            return true;
                        }

                    }
                }
                base.RollbackTransaction();
                return false;
                
            }
            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("员工调查申请EmployeeSurveyAppBll-UpdRequire" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

        #region 页面加载时查询
        /// <summary>
        /// 员工调查申请子页面加载时查询
        /// </summary>
        public V_EmployeeSurveyApp GetRequireData(string requireKey)
        {
            try
            {
                var data = from ent in dal.GetObjects<T_OA_REQUIRE>()
                           .Include("T_OA_REQUIREMASTER")
                           join ents in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                           on ent.REQUIREID equals ents.FORMID into list
                           where ent.REQUIREID == requireKey
                           orderby ent.CREATEDATE
                           select new V_EmployeeSurveyApp
                           {
                               requireEntity = ent,
                               distributeuserList = list,
                               MasterId = ent.T_OA_REQUIREMASTER.REQUIREMASTERID,
                               
                           };
                return data != null ? data.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查申请EmployeeSurveyAppBll-GetRequireData" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 选择申请时查询
        /// <summary>
        /// 选择员工调查申请时查询，并排序分页
        /// </summary>
        public IQueryable<V_EmployeeSurveyApp> GetEmployeeSurveyAppByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] datetimes)
        {
            try
            {
                DateTime startDate = datetimes[0];
                DateTime endDate = datetimes[1];
                var data = from ents in dal.GetObjects<T_OA_REQUIRE>()
                           .Include("T_OA_REQUIRERESULT")
                           where ents.STARTDATE >= startDate && ents.ENDDATE <= endDate && ents.CHECKSTATE == checkstate                 orderby ents.CREATEDATE
                           select new V_EmployeeSurveyApp
                           {
                               CreateDate = ents.CREATEDATE,
                               ResultList=ents.T_OA_REQUIRERESULT,
                               RequireId = ents.REQUIREID,
                               RequireContent = ents.CONTENT,
                               SurveyTitle = ents.APPTITLE,
                               OwnerName = ents.OWNERNAME
                           };
                if (data.Count() > 0)//返回只有参与过调查的申请
                {
                    List<V_EmployeeSurveyApp> _delList = data.ToList();
                    foreach (var ent in _delList)
                    {
                        if (ent.ResultList.Count() > 0)
                        {
                            _delList.Remove(ent);
                        }
                    }
                    data = _delList.AsQueryable();
                }
                
                return data != null ? Utility.Pager<V_EmployeeSurveyApp>(data, pageIndex, pageSize, ref pageCount) : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查申请EmployeeSurveyAppBll-GetAppBySelect" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 生成待办任务
        /// </summary>
        private void CreateNotifyTask(T_OA_DISTRIBUTEUSER notifyfeedback)
        {
            EngineWcfGlobalFunctionClient engClient = new EngineWcfGlobalFunctionClient();
            CustomUserMsg[] cust = new CustomUserMsg[1];
            cust[0] = new CustomUserMsg()
            {
                FormID = notifyfeedback.DISTRIBUTEUSERID,
                UserID = notifyfeedback.OWNERID
            };
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = ObjListToXml<T_OA_DISTRIBUTEUSER>(notifyfeedback, "OA");
            engClient.ApplicationMsgTrigger(cust, "OA", "T_OA_DISTRIBUTEUSER", strXmlObjectSource, MsgType.Task);
        }

        /// <summary>        
        ///引擎需要的XML形式的实体字符串转化        
        ///  </summary> 
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

        #endregion
    }
}
        