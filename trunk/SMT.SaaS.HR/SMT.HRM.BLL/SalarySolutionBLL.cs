using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using BLLCommonServices = SMT.SaaS.BLLCommonServices;

namespace SMT.HRM.BLL
{
    public class SalarySolutionBLL : BaseBll<T_HR_SALARYSOLUTION>, ILookupEntity, IOperate
    {
        public void SalarySolutionAdd(T_HR_SALARYSOLUTION obj)
        {
            try
            {
                T_HR_SALARYSOLUTION ent = new T_HR_SALARYSOLUTION();
                Utility.CloneEntity<T_HR_SALARYSOLUTION>(obj, ent);
                if (obj.T_HR_SALARYSYSTEM != null)
                {
                    ent.T_HR_SALARYSYSTEMReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSYSTEM", "SALARYSYSTEMID", obj.T_HR_SALARYSYSTEM.SALARYSYSTEMID);

                }
                if (obj.T_HR_AREADIFFERENCE != null)
                {
                    ent.T_HR_AREADIFFERENCEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID);

                }

                dal.Add(ent);

                BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYSOLUTION>(obj);
            }
            catch (Exception ex)
            {
                Utility.SaveLog("保存薪资方案失败，原因：" + ex.ToString());
            }
        }

        /// <summary>
        /// 搜索相同的薪资方案名
        /// </summary>
        /// <param name="solutionName">薪资方案名</param>
        public bool SalarySolutionSameSearch(string solutionName)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYSOLUTION>()
                       where a.SALARYSOLUTIONNAME == solutionName
                       select a;
            if (ents.Count() > 0) return true;
            return false;
        }

        /// <summary>
        /// 更新薪资方案
        /// </summary>
        /// <param name="entity"></param>
        public void SalarySolutionUpdate(T_HR_SALARYSOLUTION obj)
        {

            var ent = from a in dal.GetTable()
                      where a.SALARYSOLUTIONID == obj.SALARYSOLUTIONID
                      select a;
            if (ent.Count() > 0)
            {
                T_HR_SALARYSOLUTION tmpEnt = ent.FirstOrDefault();

                //Utility.CloneEntity<T_HR_SALARYSOLUTION>(obj, tmpEnt);
                if (obj.T_HR_SALARYSYSTEM != null)
                {
                    obj.T_HR_SALARYSYSTEMReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSYSTEM", "SALARYSYSTEMID", obj.T_HR_SALARYSYSTEM.SALARYSYSTEMID);
                    obj.T_HR_SALARYSYSTEM.EntityKey =
                  new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSYSTEM", "SALARYSYSTEMID", obj.T_HR_SALARYSYSTEM.SALARYSYSTEMID);

                }
                if (obj.T_HR_AREADIFFERENCE != null)
                {
                    obj.T_HR_AREADIFFERENCEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID);
                    obj.T_HR_AREADIFFERENCE.EntityKey =
                  new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID);

                }
                dal.Update(obj);
                BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYSOLUTION>(obj);
            }

        }

        public int SalarySolutionDelete(string[] IDs)
        {
            SalarySolutionItemBLL bll = new SalarySolutionItemBLL();
            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYSOLUTION>()
                           where e.SALARYSOLUTIONID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                if (ent != null)
                {

                    bll.SalarySolutionItemsDeleteBySID(ent.SALARYSOLUTIONID);
                    var taxItems = from c in dal.GetObjects<T_HR_SALARYTAXES>()
                                   where c.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == ent.SALARYSOLUTIONID
                                   select c;
                    if (taxItems.Count() > 0)
                    {
                        foreach (var taxitem in taxItems)
                        {
                            dal.DeleteFromContext(taxitem);
                        }
                        dal.SaveContextChanges();
                    }
                    dal.DeleteFromContext(ent);
                    BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYSOLUTION>(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_SALARYSOLUTION> ents = from a in DataContext.T_HR_SALARYSOLUTION
        //                                           select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYSOLUTION");

            IQueryable<T_HR_SALARYSOLUTION> ents = from a in dal.GetObjects<T_HR_SALARYSOLUTION>()
                                                   select a;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYSOLUTION>(ents, pageIndex, pageSize, ref pageCount);

            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        public T_HR_SALARYSOLUTION GetSalarySolutionByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYSOLUTION>().Include("T_HR_SALARYSYSTEM").Include("T_HR_AREADIFFERENCE")
                       //    join c in DataContext.T_HR_POSTLEVELDISTINCTION on o.T_HR_SALARYLEVEL.T_HR_POSTLEVELDISTINCTION.POSTLEVELID equals c.POSTLEVELID
                       where o.SALARYSOLUTIONID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public new IQueryable<T_HR_SALARYSOLUTION> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYSOLUTION");
            SetFilterWithflow("SALARYSOLUTIONID", "T_HR_SALARYSOLUTION", userID, ref CheckState, ref filterString, ref queryParas);
            if (!string.IsNullOrEmpty(CheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " and ";
                }
                filterString += "CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(CheckState);
            }
            IQueryable<T_HR_SALARYSOLUTION> ents = dal.GetObjects<T_HR_SALARYSOLUTION>().Include("T_HR_SALARYSYSTEM");

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYSOLUTION>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }


        /// <summary>
        /// 指定发薪日期定时提醒的XML
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public void GetSalarySolutionEngineXml(T_HR_SALARYSOLUTION entTemp)
        {
            DateTime dtStart = System.DateTime.Now;
            string strStartTime = "10:00";
            int remindDate = Convert.ToInt32(entTemp.PAYDAY) - Convert.ToInt32(entTemp.PAYALERTDAY);
            if (remindDate <= 0) remindDate = 1;
            dtStart = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, remindDate);
            List<object> objArds = new List<object>();
            objArds.Add(entTemp.OWNERCOMPANYID);
            objArds.Add("HR");
            objArds.Add("T_HR_SALARYSOLUTION");
            objArds.Add(entTemp.SALARYSOLUTIONID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("Month");
            objArds.Add("");
            objArds.Add(entTemp.SALARYSOLUTIONNAME + "于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "配置薪资方案定时生成,将于" + dtStart.ToString("yyyy-MM-dd") + "开始按月自动提醒薪资发放日期");
            objArds.Add("");
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para Name=\"SalarySolutionRemind\" Value=\"" + entTemp.SALARYSOLUTIONID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("basicHttpBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        /// <summary>
        /// 对指定发薪日期定时提醒
        /// </summary>
        /// <param name="CreateUserID"></param>
        /// <returns></returns>
        public void TimingPay(T_HR_SALARYSOLUTION salarysolution)
        {
            string submitName = "";
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                       where a.EMPLOYEEID == salarysolution.OWNERID
                       select a;
            EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
            userMsg.FormID = Guid.NewGuid().ToString();
            userMsg.UserID = salarysolution.CREATEUSERID;
            EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
            List[0] = userMsg;
            if (ents.Count() > 0) submitName = ents.FirstOrDefault().EMPLOYEECNAME;
            Client.ApplicationMsgTrigger(List, "HR", "T_HR_SALARYSOLUTION", Utility.ObjListToXml(salarysolution, "HR", submitName), EngineWS.MsgType.Msg);
        }

        /// <summary>
        /// 引擎更新单据状态专用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var entity = (from c in dal.GetObjects()
                              where c.SALARYSOLUTIONID == EntityKeyValue
                              select c).FirstOrDefault();
                if (entity != null)
                {
                    entity.CHECKSTATE = CheckState;
                    SalarySolutionUpdate(entity);
                    i = 1;
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
    }
}
