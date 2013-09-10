using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    public class HouseHireAppManagementBll : BaseBll<T_OA_HIREAPP>
    {
        //private SMT_OA_EFModelContext hireAppContext = new SMT_OA_EFModelContext();

        public IQueryable<T_OA_HIREAPP> GetHireAppQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                var ents = from q in dal.GetObjects()
                           where q.CHECKSTATE == checkState
                           select q;
                if (flowInfoList != null)
                {
                    ents = from a in ents.ToList().AsQueryable()
                           join l in flowInfoList on a.HIREAPPID equals l.FormID
                           select a;
                }
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_HIREAPP");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_HIREAPP>(ents, pageIndex, pageSize, ref pageCount);
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-GetHireAppQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        //获取求租申请的详细记录
        public IQueryable<V_HouseHireApp> GetHireAppQueryWithPagingByHouseInfoOrList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                
                var ents = from q in dal.GetObjects()
                           join n in
                               (
                                   from k in dal.GetObjects<T_OA_HOUSELIST>()
                                   join m in dal.GetObjects<T_OA_HOUSEINFO>()
                                   on k.T_OA_HOUSEINFO.HOUSEID equals m.HOUSEID
                                   where m.ISRENT == "0"
                                   orderby k.CREATEDATE descending
                                   select new { m, k }
                               )
                            on q.T_OA_HOUSELIST.HOUSELISTID equals n.k.HOUSELISTID                           
                           select new V_HouseHireApp
                           {
                               houseInfoObj = n.m,
                               houseListObj = n.k,
                               houseAppObj = q,
                               OWNERCOMPANYID = q.OWNERCOMPANYID,
                               OWNERID = q.OWNERID,
                               OWNERPOSTID = q.OWNERPOSTID,
                               OWNERDEPARTMENTID = q.OWNERDEPARTMENTID,
                               CREATEUSERID = q.CREATEUSERID
                           };


                if (flowInfoList != null)
                {
                    ents = from a in ents.ToList().AsQueryable()
                           join l in flowInfoList on a.houseAppObj.HIREAPPID equals l.FormID
                           select a;
                }
                if (!string.IsNullOrEmpty(checkState))
                {
                    ents = ents.Where(p => p.houseAppObj.CHECKSTATE == checkState);
                }
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<V_HouseHireApp>(ents, pageIndex, pageSize, ref pageCount);
                return ents;
            }
            catch(Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-GetHireAppQueryWithPagingByHouseInfoOrList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        public IQueryable<T_OA_HOUSEINFO> GetHireAppHouseInfoQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ent = from q in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                          join l in
                              (
                                from t in dal.GetObjects<T_OA_HOUSEINFOISSUANCE>()
                                join n in
                                    (
                                        from k in dal.GetObjects<T_OA_HOUSELIST>()
                                        join m in dal.GetObjects<T_OA_HOUSEINFO>()
                                        on k.T_OA_HOUSEINFO.HOUSEID equals m.HOUSEID
                                        where m.ISRENT == "0"
                                        orderby k.CREATEDATE descending
                                        select new { m, k }
                                    )
                                on t.ISSUANCEID equals n.k.T_OA_HOUSEINFOISSUANCE.ISSUANCEID
                                where t.CHECKSTATE == "2"
                                select new { n.k, n.m, t }
                              )
                          on q.FORMID equals l.t.ISSUANCEID
                          select new { l.m, q, l.k };

                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_HIREAPP");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ent = ent.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                if (ent.Count() > 0)
                {
                    var ents = from q in ent
                               select q.m;
                    ents = ents.OrderBy(sort);

                    ents = Utility.Pager<T_OA_HOUSEINFO>(ents, pageIndex, pageSize, ref pageCount);
                    return ents;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-GetHireAppHouseInfoQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 租房申请  没有出租完的房子可以申请
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_HouseHireList> GetHireAppHouseInfoListQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {

                var ent = from q in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                          join l in
                              (
                                from t in dal.GetObjects<T_OA_HOUSEINFOISSUANCE>()
                                join n in
                                    (
                                        from k in dal.GetObjects<T_OA_HOUSELIST>()
                                        join m in dal.GetObjects<T_OA_HOUSEINFO>()
                                        on k.T_OA_HOUSEINFO.HOUSEID equals m.HOUSEID
                                        where m.ISRENT == "0"
                                        orderby k.CREATEDATE descending
                                        select new { m, k }
                                    )
                                on t.ISSUANCEID equals n.k.T_OA_HOUSEINFOISSUANCE.ISSUANCEID
                                where t.CHECKSTATE == "2"
                                select new { n.k, n.m, t }
                              )
                          on q.FORMID equals l.t.ISSUANCEID
                          select new V_HouseHireList 
                          { houseInfoObj = l.m, distrbuteObj = q, houselistObj = l.k, houseIssueObj = l.t ,
                            OWNERID = l.m.OWNERID,
                            OWNERCOMPANYID = l.m.OWNERCOMPANYID,
                            OWNERDEPARTMENTID = l.m.OWNERDEPARTMENTID,
                            OWNERPOSTID = l.m.OWNERPOSTID,
                            CREATEUSERID = l.m.CREATEUSERID
                          };

                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                //UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "V_HouseHireList.houseIssueObj");
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_HIREAPP");
                //string terString = "(distrbuteObj.VIEWER =@0 or distrbuteObj.VIEWER =@1 or distrbuteObj.VIEWER =@2 or distrbuteObj.VIEWER =@3)";
                if (!string.IsNullOrEmpty(filterString))
                {
                    ent = ent.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                if (ent.Count() > 0)
                {
                    //var ents = from q in ent
                    //           select q.m;
                    ent = ent.OrderBy(sort);

                    ent = Utility.Pager<V_HouseHireList>(ent, pageIndex, pageSize, ref pageCount);
                    return ent;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-GetHireAppHouseInfoListQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        public IQueryable<T_OA_HIREAPP> GetHireAppById(string hireAppID)
        {
            try
            {
                var ents = from q in dal.GetObjects().Include("T_OA_HOUSELIST")
                           where q.HIREAPPID == hireAppID
                           select q;
                return ents.Count() > 0 ? ents : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-GetHireAppById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        public IQueryable<V_HouseHireList> GetHireAppInfoByHouseListId(string ListID)
        {
            try
            {
                var ents = from q in dal.GetObjects<T_OA_HOUSELIST>().Include("T_OA_HOUSEINFO").Include("T_OA_HOUSEINFOISSUANCE")
                           where q.HOUSELISTID == ListID
                           select new V_HouseHireList { houseInfoObj = q.T_OA_HOUSEINFO, houseIssueObj = q.T_OA_HOUSEINFOISSUANCE, houselistObj = q };
                return ents.Count() > 0 ? ents : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-GetHireAppInfoByHouseListId" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 检查发布的房源信息是否已经被申请完，
        /// 先搜索出 房源信息中 可以入住的人数，再查询针对该房源通过申请的记录数，如果相等 则表示该房源已被申请完
        /// edit by liujx 2010-5-22
        /// </summary>        
        /// <param name="houselist"></param>
        /// <returns></returns>
        public bool IsHired(T_OA_HOUSELIST houselist,string StrUserID)
        {
            try
            {
                string checkState = ((int)CheckStates.Approved).ToString();
                var listhouse = from h in dal.GetObjects<T_OA_HOUSEINFO>()
                                where h.HOUSEID == houselist.T_OA_HOUSEINFO.HOUSEID
                                select h;
                var listapp = from k in dal.GetTable()
                              where k.T_OA_HOUSELIST.HOUSELISTID == houselist.HOUSELISTID && k.CHECKSTATE == checkState
                              select k;
                if (listhouse.Count() > 0)
                {
                    if (listhouse.FirstOrDefault().Number == listapp.Count())
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-IsHired" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        public string AddHireApp(T_OA_HIREAPP hireAppObj)
        {
            string StrReturn = "";
            try
            {
                
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.T_OA_HOUSELIST.HOUSELISTID == hireAppObj.T_OA_HOUSELIST.HOUSELISTID
                    && (s.CHECKSTATE == "2" || s.CHECKSTATE == "0" || s.CHECKSTATE == "1") && s.OWNERID == hireAppObj.OWNERID
                    
                    
                    );
                if (tempEnt != null)
                {
                    //StrReturn = "REPETITION"; //{0}已存在，保存失败！ 
                    StrReturn = "HIREDAPPSUCCESSED";//你已申请该房源信息
                }
                else
                {
                    
                    Utility.RefreshEntity(hireAppObj);
                    //DataContext.AddObject("T_OA_HIREAPP", hireAppObj);
                    //.AddObject("T_OA_WELFAREMASERT", Welfare);
                    //int i = DataContext.SaveChanges();// dal.Add(hireAppObj);
                    int i = dal.Add(hireAppObj);
                    if (i > 0)
                    {
                        //StrReturn = "SAVEFAILED";//保存失败
                        StrReturn = "";
                    }
                    else
                    {
                        StrReturn = "SAVEFAILED";
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-AddHireApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrReturn = "SAVEFAILED";
            }
            return StrReturn;
        }

        public bool UpdateHireApp(T_OA_HIREAPP hireAppObj)
        {
            
            try
            {               
                hireAppObj.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_HIREAPP", "HIREAPPID", hireAppObj.HIREAPPID);
                T_OA_HIREAPP tmp = dal.GetObjectByEntityKey(hireAppObj.EntityKey) as T_OA_HIREAPP;
                //DataContext.ApplyPropertyChanges(hireAppObj.EntityKey.EntitySetName, hireAppObj);
                int i = dal.Update(hireAppObj);
                                
                if (i > 0)
                {
                    if (hireAppObj.ISOK == "1" && hireAppObj.ISBACK =="0") //添加引擎调用
                    {
                        List<object> objArds = new List<object>();
                        T_OA_HIRERECORD record = new T_OA_HIRERECORD();
                        objArds.Add(hireAppObj.HIREAPPID);
                        objArds.Add("OA");
                        objArds.Add("hireAppObj.HIREAPPID");
                        objArds.Add(hireAppObj.HIREAPPID);
                        objArds.Add(DateTime.Now.ToString("yyyy/MM/d"));
                        objArds.Add(DateTime.Now.ToString("HH:mm"));
                        objArds.Add("Month");
                        objArds.Add("");
                        //objArds.Add(entTemp.CNAME + "公司于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",开始检测当前公司所属员工的企业工龄");
                        objArds.Add("");
                        objArds.Add("");
                        objArds.Add(Utility.strEngineFuncWSSite);
                        objArds.Add("EventTriggerProcess");
                        objArds.Add("<Para FuncName=\"GetHireAppToHireRecord\" Name=\"HIREAPPID\" Value=\"" + hireAppObj.HIREAPPID + "\"></Para>");
                        objArds.Add("Г");
                        objArds.Add("CustomBinding");

                        Utility.SendEngineEventTriggerData(objArds);

                    }
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-UpdateHireApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        public bool DeleteHireApp(string[] hireAppID)
        {
            try
            {
                bool result = false;
                var entity = (from ent in dal.GetTable().ToList()
                              where hireAppID.Contains(ent.HIREAPPID)
                              select ent);
                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        dal.DeleteFromContext(h);
                    }
                    int i = dal.SaveContextChanges();
                    result = i > 0 ? true : false;
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源申请HouseHireAppManagementBll-DeleteHireApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
    }
}
