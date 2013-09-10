using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using SMT.Foundation.Log;


namespace SMT.SaaS.OA.BLL
{
    public class TravleSolutionBLL : BaseBll<T_OA_TRAVELSOLUTIONS>
    {
        #region 出差方案
        /// <summary>
        /// 添加解决方案
        /// 先添加解决方案然后再添加交通工具、飞机线路
        /// 必须使用事务处理
        /// </summary>
        /// <param name="EntObj">解决方案实体</param>
        /// <param name="ListTransport">交通工具标准列表</param>
        /// <param name="ListPlane">飞机线路列表</param>
        /// <returns></returns>
        public string AddTravleSolution(T_OA_TRAVELSOLUTIONS EntObj, List<T_OA_TAKETHESTANDARDTRANSPORT> ListTransport, List<string> companyids)
        {
            string StrReturn = "";
            try
            {
                dal.BeginTransaction();
                //添加出差方案
                int k = dal.Add(EntObj);
                EntObj.CREATEDATE = DateTime.Now;
                EntObj.CREATEDATE = DateTime.Now;

                ListTransport.ForEach(item =>
                {
                    item.CREATEDATE = System.DateTime.Now;
                    //EntObj.T_OA_TAKETHESTANDARDTRANSPORT.Add(item);
                    item.T_OA_TRAVELSOLUTIONS = null;
                    item.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                            new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", EntObj.TRAVELSOLUTIONSID);

                    Utility.RefreshEntity(item);
                    dal.AddToContext(item);

                });

                //int k = dal.Add(EntObj);
                if (companyids.Count() > 0)
                {
                    for (int i = 0; i < companyids.Count(); i++)
                    {
                        string id = companyids[i];
                        var ents = from a in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                                   where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == EntObj.TRAVELSOLUTIONSID && a.COMPANYID == id
                                   select a;
                        var solution = from b in dal.GetObjects<T_OA_TRAVELSOLUTIONS>()
                                       where b.TRAVELSOLUTIONSID == EntObj.TRAVELSOLUTIONSID
                                       select b;
                        if (!(ents.Count() > 0))
                        {
                            T_OA_PROGRAMAPPLICATIONS ent = new T_OA_PROGRAMAPPLICATIONS();


                            ent.PROGRAMAPPLICATIONSID = System.Guid.NewGuid().ToString();
                            ent.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                            new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", EntObj.TRAVELSOLUTIONSID);

                            ent.COMPANYID = id;
                            ent.CREATEUSERID = EntObj.CREATEUSERID;
                            ent.OWNERCOMPANYID = EntObj.OWNERCOMPANYID;
                            ent.OWNERDEPARTMENTID = EntObj.OWNERDEPARTMENTID;
                            ent.OWNERPOSTID = EntObj.OWNERPOSTID;
                            ent.CREATEDATE = System.DateTime.Now;
                            dal.AddToContext(ent);
                        }

                    }
                }

                int n = dal.SaveContextChanges();
                StrReturn = (k > 0 && n > 0) ? "" : "ERROR";

                dal.CommitTransaction();

            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("出差方案TravleSolutionBLL-AddTravleSolution" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrReturn = "ERROR";
            }
            return StrReturn;
        }

        /// <summary>
        /// 删除出差方案
        /// 先删除出差乘坐工具、飞机线路
        /// </summary>
        /// <param name="SolutionID">方案ID</param>
        /// <returns></returns>
        public bool DeleteTravleSolution(string SolutionID)
        {
            try
            {
                dal.BeginTransaction();
                var entitys = (from ent in dal.GetObjects<T_OA_TRAVELSOLUTIONS>()
                               where ent.TRAVELSOLUTIONSID == SolutionID
                               select ent);
                //获取交通工具标准
                var entstandard = from ent in dal.GetObjects<T_OA_TAKETHESTANDARDTRANSPORT>().Include("T_OA_TRAVELSOLUTIONS")
                                  where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                                  select ent;
                //方案设置
                var apps = from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                           where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                           select ent;

                //删除补贴
                if (entitys.Count() > 0)
                {
                    foreach (var a in entitys)
                    {
                        //删除地区补贴
                        var EntPlane = from ent in dal.GetObjects<T_OA_AREAALLOWANCE>().Include("T_OA_TRAVELSOLUTIONS")
                                       where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == a.TRAVELSOLUTIONSID
                                       select ent;

                        foreach (var k in EntPlane)
                        {
                            dal.DeleteFromContext(k);
                        }
                    }
                }

                //删除交通工具
                if (entstandard.Count() > 0)
                {
                    entstandard.ToList().ForEach(item =>
                        {
                            dal.DeleteFromContext(item);
                        }
                        );
                }
                //删除方案设置
                if (apps.Count() > 0)
                {
                    apps.ToList().ForEach(item =>
                    {
                        dal.DeleteFromContext(item);
                    }
                        );
                }
                dal.SaveContextChanges();
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.DeleteFromContext(entity);
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        dal.CommitTransaction();
                        return true;
                    }
                    else
                    {
                        dal.RollbackTransaction();
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-DeleteTravleSolution" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        /// <summary>
        /// 修改出差方案  
        /// </summary>
        /// <param name="EntTravle"></param>
        /// <param name="TransportObj">交通工具列表</param>
        /// <param name="PlaneObj">飞机路线列表</param>
        /// <param name="IsChange">如果为假 则表示交通工具、飞机路线没做改动 为真则做了改动</param>
        /// <returns></returns>
        public int UpdateTravleSolutionInfo(T_OA_TRAVELSOLUTIONS EntTravle, List<T_OA_TAKETHESTANDARDTRANSPORT> TransportObj, List<string> companyids, bool IsChange)
        {
            try
            {
                bool IsAdd = false;//是否添加动作
                BeginTransaction();
                int result = 0;
                EntTravle.UPDATEDATE = DateTime.Now;
                var users = from ent in dal.GetObjects<T_OA_TRAVELSOLUTIONS>()
                            where ent.TRAVELSOLUTIONSID == EntTravle.TRAVELSOLUTIONSID
                            select ent;
                if (EntTravle.EntityKey == null)
                {
                    if (users.Count() > 0)
                    {
                        var user = users.FirstOrDefault();
                        EntTravle.EntityKey = user.EntityKey;
                    }
                }
                if (!IsChange)
                {

                    int i = dal.Update(EntTravle);
                    if (i > 0)
                    {
                        CommitTransaction();
                        return i;
                    }
                    else
                    {
                        RollbackTransaction();
                        return -1;
                    }
                }
                else
                {
                    int ed = Update(EntTravle);
                    if (ed > 0)
                    {
                        IsAdd = true;
                    }
                    var standards = from ent in dal.GetObjects<T_OA_TAKETHESTANDARDTRANSPORT>().Include("T_OA_TRAVELSOLUTIONS")
                                    where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == EntTravle.TRAVELSOLUTIONSID
                                    select ent;
                    var EntObjs = from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                                  where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == EntTravle.TRAVELSOLUTIONSID
                                  select ent;
                    List<string> AllCompanyids = new List<string>();
                    List<string> AllDeleteCompanyids = new List<string>();//删除的公司ID
                    //删除不存在的  公司ID
                    if (EntObjs != null)
                    {
                        if (EntObjs.Count() > 0)
                        {
                            EntObjs.ToList().ForEach(item =>
                            {
                                var ents = from id in companyids
                                           where id == item.COMPANYID
                                           select id;
                                if (ents != null)
                                {
                                    if (!(ents.Count() > 0))
                                    {
                                        AllDeleteCompanyids.Add(item.COMPANYID);//添加删除的公司ID
                                        dal.DeleteFromContext(item);
                                    }
                                }
                            });
                        }
                    }

                    //删除被删除或修改的数据
                    if (TransportObj.Count() > 0)
                    {
                        TransportObj.ForEach(item =>
                        {
                            if (item.ENDPOSTLEVEL == "" && item.TYPEOFTRAVELTOOLS == "" && item.TAKETHETOOLLEVEL == "")
                            {
                                dal.DeleteFromContext(item);
                            }
                        });
                    }

                    int m = dal.SaveContextChanges();

                    if (TransportObj.Count() > 0)
                    {
                        TransportObj.ForEach(item =>
                            {
                                //三个都不为空则表示不是被删除的对象
                                if (item.TAKETHETOOLLEVEL != "" && item.TYPEOFTRAVELTOOLS != "" && item.ENDPOSTLEVEL != "")
                                {
                                    if (item.T_OA_TRAVELSOLUTIONS != null)
                                    {
                                        if (item.T_OA_TRAVELSOLUTIONS.EntityKey == null)
                                            item.T_OA_TRAVELSOLUTIONS.EntityKey = EntTravle.EntityKey;
                                    }
                                    Utility.RefreshEntity(item);
                                    dal.AddToContext(item);
                                    IsAdd = true;
                                }
                            }
                            );
                    }

                    if (companyids.Count() > 0)
                    {
                        for (int i = 0; i < companyids.Count(); i++)
                        {
                            string id = companyids[i];
                            //同出差方案下，增加原记录里没有的城市
                            var ents = from a in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                                       where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == EntTravle.TRAVELSOLUTIONSID && a.COMPANYID == id
                                       select a;
                            if (!(ents.Count() > 0))
                            {
                                T_OA_PROGRAMAPPLICATIONS ent = new T_OA_PROGRAMAPPLICATIONS();
                                ent.PROGRAMAPPLICATIONSID = System.Guid.NewGuid().ToString();
                                ent.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                                new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", EntTravle.TRAVELSOLUTIONSID);
                                ent.COMPANYID = id;
                                ent.OWNERCOMPANYID = EntTravle.OWNERCOMPANYID;
                                ent.OWNERDEPARTMENTID = EntTravle.OWNERDEPARTMENTID;
                                ent.OWNERPOSTID = EntTravle.OWNERPOSTID;
                                ent.CREATEUSERID = EntTravle.CREATEUSERID;
                                ent.CREATEDATE = System.DateTime.Now;

                                Utility.RefreshEntity(ent);
                                dal.AddToContext(ent);
                                IsAdd = true;
                            }
                        }
                        //同出差方案下，删除已经不需要的城市
                        //var delEnts = from a in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>()
                        //              where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == EntTravle.TRAVELSOLUTIONSID && !companyids.Contains(a.COMPANYID)
                        //              select a;
                        //var delEnts1 = from a in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONID")
                        //              where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == EntTravle.TRAVELSOLUTIONSID
                        //              select a;
                        //var delEnts2 = from a in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>()
                        //              where !companyids.Contains(a.COMPANYID)
                        //              select a;
                        //if (delEnts.Count() > 0)
                        //{
                        //    foreach (var de in delEnts)
                        //    {
                        //        dal.DeleteFromContext(de);
                        //    }
                        //    IsAdd = true;
                        //}
                    }
                    if (IsAdd)
                    {
                        result = dal.SaveContextChanges();
                        if (result > 0 || m > 0)
                        {
                            dal.CommitTransaction();
                            result = result == 0 ? m : result;
                        }
                        else
                        {
                            dal.RollbackTransaction();
                        }
                    }
                    else
                    {
                        //只删除了数据  给没有添加数据
                        if (m > 0)
                        {
                            result = m;//将结果传递出来
                            dal.CommitTransaction();
                        }
                        else
                        {
                            dal.RollbackTransaction();
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-UpdateTravleSolutionInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                RollbackTransaction();
                return -1;
            }
        }

        public IQueryable<T_OA_TRAVELSOLUTIONS> GetTravelSolutionFlow(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_OA_TRAVELSOLUTIONS>()
                           select ent;

                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_TRAVELSOLUTIONS");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_TRAVELSOLUTIONS>(ents, pageIndex, pageSize, ref pageCount);
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetTravelSolutionFlow" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }

        public void GetVechileStandardAndPlaneLine(string SolutionID, ref List<T_OA_CANTAKETHEPLANELINE> ListPlaneList, ref List<T_OA_TAKETHESTANDARDTRANSPORT> ListVechile)
        {
            try
            {
                var EntVechile = from ent in dal.GetObjects<T_OA_TAKETHESTANDARDTRANSPORT>().Include("T_OA_TRAVELSOLUTIONS")
                                 where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                                 select ent;
                var EntPlane = from ent in dal.GetObjects<T_OA_CANTAKETHEPLANELINE>().Include("T_OA_TRAVELSOLUTIONS")
                               where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                               select ent;
                ListPlaneList = (EntPlane == null) ? null : EntPlane.ToList();
                ListVechile = (EntVechile == null) ? null : EntVechile.ToList();

            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetVechileStandardAndPlaneLine" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        /// <summary>
        /// 根据方案ID查询对应的城市分类
        /// </summary>
        /// <param name="SolutionID">方案ID</param>
        public List<T_OA_AREACITY> GetQueryPlanCity(string SolutionID, ref List<T_OA_AREADIFFERENCE> ListAREADIFFERENCE)
        {
            try
            {
                //先根据方案ID查询出地区分类(例如一类、二类)
                var EntVechile = from ent in dal.GetObjects<T_OA_AREADIFFERENCE>().Include("T_OA_TRAVELSOLUTIONS")
                                 where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                                 orderby ent.AREAINDEX ascending
                                 select ent;

                if (EntVechile.Count() > 0)//再根据地区分类ID查询出对应的城市
                {
                    var EntPlane = from ent in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                                   where ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID == EntVechile.FirstOrDefault().AREADIFFERENCEID
                                   select ent;

                    ListAREADIFFERENCE = (EntVechile == null) ? null : EntVechile.ToList();
                    return EntPlane == null ? null : EntPlane.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetQueryPlanCity" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        /// <summary>
        /// 查询对应的补贴
        /// </summary>
        /// <param name="SolutionID">方案ID</param>
        public List<T_OA_AREAALLOWANCE> GetQueryProgramSubsidies(string SolutionID)
        {
            try
            {
                var EntPlane = from ent in dal.GetObjects<T_OA_AREAALLOWANCE>().Include("T_OA_TRAVELSOLUTIONS")
                               where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                               select ent;

                if (EntPlane.Count() > 0)
                {
                    return EntPlane.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetQueryProgramSubsidies" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        /// <summary>
        /// 添加解决方案设置
        /// </summary>
        /// <param name="appObj"></param>
        /// <param name="StrState">“” 表示添加  edit表示修改</param>
        /// <param name="SolutionID">方案ID</param>
        /// <returns></returns>
        public string AddSolutionSet(List<T_OA_PROGRAMAPPLICATIONS> appObj, string StrState, string SolutionID)
        {
            string StrReturn = "";
            try
            {
                if (string.IsNullOrEmpty(StrState))
                {
                    appObj.ForEach(
                        item =>
                        {
                            dal.AddToContext(item);
                        }
                        );
                    int i = dal.SaveContextChanges();
                    StrReturn = i > 0 ? "" : "ERROR";
                }
                else
                {
                    StrReturn = UpdateSolutionSet(appObj, SolutionID);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-AddSolutionSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrReturn = "ERROR";
            }
            return StrReturn;
        }

        /// <summary>
        /// 修改出差方案设置
        /// </summary>
        /// <param name="ListApp">出差方案应用列表</param>
        /// <param name="StrSolutionID">出差方案ID</param>
        /// <returns></returns>
        private string UpdateSolutionSet(List<T_OA_PROGRAMAPPLICATIONS> ListApp, string StrSolutionID)
        {
            string StrReturn = "";
            try
            {
                dal.BeginTransaction();
                var ents = from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                           where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == StrSolutionID
                           select ent;
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item =>
                    {

                        bool istrue = DeleteSolutionSetInfo(item.PROGRAMAPPLICATIONSID);
                        if (!istrue)
                        {
                            StrReturn = "ERROR";
                            //break;
                        }


                    });
                }
                ListApp.ForEach(
                        item =>
                        {
                            dal.AddToContext(item);
                        }
                        );
                int i = dal.SaveContextChanges();
                if (i > 0)
                {
                    dal.CommitTransaction();

                }
                else
                {
                    dal.RollbackTransaction();
                    StrReturn = "ERROR";
                }
                //StrReturn = i > 0 ? "" : "ERROR";

            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-UpdateSolutionSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                dal.RollbackTransaction();
                StrReturn = "ERROR";
            }

            return StrReturn;
        }

        private bool DeleteSolutionSetInfo(string AppSolutionID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>()
                               where ent.PROGRAMAPPLICATIONSID == AppSolutionID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-DeleteSolutionSetInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        /// <summary>
        /// 根据公司ID获取对应的解决方案
        /// </summary>
        /// <param name="Companyid">公司ID</param>
        /// <param name="PlaneObj">飞机线路列表</param>
        /// <param name="StandardObj">交通工具标准</param>
        /// <returns></returns>
        public T_OA_TRAVELSOLUTIONS GetTravelSolutionByCompanyID(string Companyid, ref List<T_OA_CANTAKETHEPLANELINE> PlaneObj, ref List<T_OA_TAKETHESTANDARDTRANSPORT> StandardObj)
        {
            try
            {
                var sets = from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                           where ent.COMPANYID == Companyid
                           select ent;
                if (sets.Count() > 0)
                {
                    T_OA_TRAVELSOLUTIONS solution = sets.FirstOrDefault().T_OA_TRAVELSOLUTIONS;
                    var standards = from ent in dal.GetObjects<T_OA_TAKETHESTANDARDTRANSPORT>().Include("T_OA_TRAVELSOLUTIONS")
                                    where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == solution.TRAVELSOLUTIONSID
                                    select ent;
                    var Planes = from ent in dal.GetObjects<T_OA_CANTAKETHEPLANELINE>().Include("T_OA_TRAVELSOLUTIONS")
                                 where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == solution.TRAVELSOLUTIONSID
                                 select ent;
                    PlaneObj = Planes == null ? null : Planes.ToList();
                    StandardObj = standards == null ? null : standards.ToList();
                    return solution;
                }


                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetTravelSolutionByCompanyID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public T_OA_AREAALLOWANCE GetTravleAreaAllowanceByCompanyID(string postvalue, string cityvalue)
        {
            //var differences = from ent in dal.GetObjects<T_OA_AREACITY>()
            //                  where ent.CITY == 
            try
            {
                var citys = from ent in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                            where ent.CITY == cityvalue
                            select ent;
                if (citys.Count() > 0)
                {
                    var allowances = from ent in dal.GetObjects<T_OA_AREAALLOWANCE>().Include("T_OA_AREADIFFERENCE")
                                     where ent.POSTLEVEL == postvalue && ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID == citys.FirstOrDefault().T_OA_AREADIFFERENCE.AREADIFFERENCEID
                                     select ent;
                    return allowances == null ? null : allowances.FirstOrDefault();
                }


                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetTravleAreaAllowanceByCompanyID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public List<T_OA_AREAALLOWANCE> GetTravleAreaAllowanceByPostValue(string postvalue, string solutionId, ref List<T_OA_AREACITY> citys)
        {
            try
            {
                var differences = from ent in dal.GetObjects<T_OA_AREAALLOWANCE>().Include("T_OA_TRAVELSOLUTIONS")
                                  where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == solutionId
                                  select ent;

                if (differences.Count() > 0)
                {
                    var ents = from ent in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                               select ent;
                    if (ents.Count() > 0)
                    {
                        citys = ents.ToList();
                    }
                    if (citys.Count() > 0)
                    {
                        var allowances = from ent in dal.GetObjects<T_OA_AREAALLOWANCE>().Include("T_OA_AREADIFFERENCE").Include("T_OA_TRAVELSOLUTIONS")
                                         where ent.POSTLEVEL == postvalue
                                         select ent;

                        return allowances == null ? null : allowances.ToList();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetTravleAreaAllowanceByPostValue" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        #endregion
        /// <summary>
        /// 复制出差方案
        /// </summary>
        /// <param name="EntObj">方案实体</param>
        /// <param name="OldtravleSolutionId">旧方案ID</param>
        /// <param name="companyids">公司ID</param>
        /// <returns></returns>
        public string GetCopyTravleSolution(T_OA_TRAVELSOLUTIONS EntObj, string OldtravleSolutionId)
        {
            string StrReturn = string.Empty;
            try
            {
                dal.BeginTransaction();

                //添加方案信息
                int i = dal.Add(EntObj);

                if (i > 0)
                {
                    //根据出差方案ID查询出交通工具标准
                    var ents = from a in dal.GetObjects<T_OA_TAKETHESTANDARDTRANSPORT>().Include("T_OA_TRAVELSOLUTIONS")
                               where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == OldtravleSolutionId
                               select a;

                    if (ents.Count() > 0)
                    {
                        foreach (T_OA_TAKETHESTANDARDTRANSPORT obj in ents)
                        {
                            T_OA_TAKETHESTANDARDTRANSPORT tent = new T_OA_TAKETHESTANDARDTRANSPORT();

                            tent.TAKETHESTANDARDTRANSPORTID = System.Guid.NewGuid().ToString();
                            tent.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                            new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", EntObj.TRAVELSOLUTIONSID);

                            tent.ENDPOSTLEVEL = obj.ENDPOSTLEVEL;
                            tent.TYPEOFTRAVELTOOLS = obj.TYPEOFTRAVELTOOLS;
                            tent.TAKETHETOOLLEVEL = obj.TAKETHETOOLLEVEL;
                            tent.OWNERCOMPANYID = EntObj.OWNERCOMPANYID;
                            tent.OWNERDEPARTMENTID = EntObj.OWNERDEPARTMENTID;
                            tent.OWNERPOSTID = EntObj.OWNERPOSTID;
                            tent.CREATEUSERID = EntObj.CREATEUSERID;
                            tent.CREATEDATE = System.DateTime.Now;
                            dal.AddToContext(tent);
                        }

                        //将出差方案应用到当前公司
                        T_OA_PROGRAMAPPLICATIONS pent = new T_OA_PROGRAMAPPLICATIONS();

                        pent.PROGRAMAPPLICATIONSID = System.Guid.NewGuid().ToString();
                        pent.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                        new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", EntObj.TRAVELSOLUTIONSID);
                        pent.COMPANYID = EntObj.OWNERCOMPANYID;
                        pent.CREATEUSERID = EntObj.CREATEUSERID;
                        pent.OWNERCOMPANYID = EntObj.CREATECOMPANYID;
                        pent.OWNERDEPARTMENTID = EntObj.CREATEDEPARTMENTID;
                        pent.OWNERPOSTID = EntObj.CREATEPOSTID;
                        pent.CREATEDATE = System.DateTime.Now;

                        dal.Add(pent);
                    }

                    //根据出差方案ID查询城市分类
                    var area = from a in dal.GetObjects<T_OA_AREADIFFERENCE>().Include("T_OA_TRAVELSOLUTIONS")
                               where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == OldtravleSolutionId
                               select a;

                    if (area.Count() > 0)
                    {
                        foreach (T_OA_AREADIFFERENCE obj in area)
                        {
                            //    T_OA_AREADIFFERENCE aread = new T_OA_AREADIFFERENCE();

                            //    aread.AREADIFFERENCEID = System.Guid.NewGuid().ToString();//城市分类ID
                            //    aread.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                            //    new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", EntObj.TRAVELSOLUTIONSID);//方案ID

                            //    aread.AREACATEGORY = obj.AREACATEGORY;//城市分类名
                            //    aread.AREAINDEX = obj.AREAINDEX;//分类序号
                            //    aread.OWNERCOMPANYID = EntObj.OWNERCOMPANYID;//所属公司
                            //    aread.OWNERDEPARTMENTID = EntObj.OWNERDEPARTMENTID;//所属部门
                            //    aread.OWNERPOSTID = EntObj.OWNERPOSTID;//所属岗位
                            //    aread.OWNERID = EntObj.OWNERID;//所属人ID
                            //    aread.CREATEUSERID = EntObj.CREATEUSERID;//创建人
                            //    aread.CREATEUSERNAME = EntObj.CREATEUSERNAME;//创建人姓名
                            //    aread.CREATECOMPANYID = EntObj.CREATECOMPANYID;//创建公司
                            //    aread.CREATEDEPARTMENTID = EntObj.CREATEDEPARTMENTID;//创建部门
                            //    aread.CREATEPOSTID = EntObj.CREATEPOSTID;//创建岗位
                            //    aread.CREATEDATE = System.DateTime.Now;//创建时间

                            //    int add = dal.Add(aread);

                            //if (add > 0)
                            //{
                            //根据城市分类ID、方案ID查询补贴标准
                            var areace = from a in dal.GetObjects<T_OA_AREAALLOWANCE>().Include("T_OA_AREADIFFERENCE").Include("T_OA_TRAVELSOLUTIONS")
                                         where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == obj.AREADIFFERENCEID
                                         && a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == OldtravleSolutionId
                                         select a;

                            foreach (T_OA_AREAALLOWANCE obje in areace)
                            {
                                T_OA_AREAALLOWANCE areadce = new T_OA_AREAALLOWANCE();

                                areadce.AREAALLOWANCEID = System.Guid.NewGuid().ToString();//分类补贴ID
                                areadce.T_OA_AREADIFFERENCEReference.EntityKey =
                                new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_AREADIFFERENCE", "AREADIFFERENCEID", obj.AREADIFFERENCEID);//分类ID
                                areadce.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                                new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", EntObj.TRAVELSOLUTIONSID);//方案ID
                                areadce.POSTLEVEL = obje.POSTLEVEL;//岗位等级
                                areadce.ACCOMMODATION = obje.ACCOMMODATION;//住宿补贴
                                areadce.TRANSPORTATIONSUBSIDIES = obje.TRANSPORTATIONSUBSIDIES;//交通伙食补贴
                                areadce.OWNERCOMPANYID = EntObj.OWNERCOMPANYID;//所属公司
                                areadce.OWNERDEPARTMENTID = EntObj.OWNERDEPARTMENTID;//所属部门
                                areadce.OWNERPOSTID = EntObj.OWNERPOSTID;//所属岗位
                                areadce.OWNERID = EntObj.OWNERID;//所属人ID
                                areadce.CREATEUSERID = EntObj.CREATEUSERID;//创建人
                                areadce.CREATEDATE = System.DateTime.Now;//创建时间

                                dal.AddToContext(areadce);
                            }

                            ////根据城市分类ID查询分类城市
                            //var areacity = from a in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                            //               where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == obj.AREADIFFERENCEID
                            //               select a;

                            //foreach (T_OA_AREACITY objc in areacity)
                            //{
                            //    T_OA_AREACITY arctiy = new T_OA_AREACITY();

                            //    arctiy.AREACITYID = System.Guid.NewGuid().ToString();//分类城市ID
                            //    arctiy.T_OA_AREADIFFERENCEReference.EntityKey =
                            //    new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_AREADIFFERENCE", "AREADIFFERENCEID", aread.AREADIFFERENCEID);//分类ID

                            //    arctiy.CITY = objc.CITY;//所在地城市
                            //    arctiy.CREATEUSERID = EntObj.CREATEUSERID;//创建人
                            //    arctiy.CREATEDATE = System.DateTime.Now;//创建时间

                            //    dal.AddToContext(arctiy);
                            //}
                        }
                        //}
                    }
                    int n = dal.SaveContextChanges();
                    if (i > 0 && n > 0)
                    {
                        dal.CommitTransaction();
                    }
                    else
                    {
                        StrReturn = "ERROR";
                        dal.RollbackTransaction();
                    }
                }
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("出差方案TravleSolutionBLL-AddTravleSolution" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrReturn = "ERROR";
            }
            return StrReturn;
        }

        #region 飞机路线

        #endregion

        #region 出差方案应用
        /// <summary>
        /// 添加出差方案应用
        /// </summary>
        /// <param name="SolutionID"></param>
        /// <param name="companyids"></param>
        /// <param name="ownercompanyid"></param>
        /// <param name="ownerpostid"></param>
        /// <param name="ownerdepartmentid"></param>
        /// <returns></returns>
        public string AddTravleSolutionSet(string SolutionID, List<string> companyids, string ownercompanyid, string ownerpostid, string ownerdepartmentid, string userid)
        {
            string StrRerurn = "";
            try
            {
                for (int i = 0; i < companyids.Count(); i++)
                {
                    string id = companyids[i];
                    var ents = from a in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                               where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID && a.COMPANYID == id
                               select a;
                    var solution = from b in dal.GetObjects<T_OA_TRAVELSOLUTIONS>()
                                   where b.TRAVELSOLUTIONSID == SolutionID
                                   select b;
                    if (!(ents.Count() > 0))
                    {
                        T_OA_PROGRAMAPPLICATIONS ent = new T_OA_PROGRAMAPPLICATIONS();

                        //ent.T_OA_TRAVELSOLUTIONSReference.EntityKey.EntityKeyValues[0].Value = SolutionID;
                        ent.PROGRAMAPPLICATIONSID = System.Guid.NewGuid().ToString();
                        ent.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                        new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", SolutionID);
                        //ent.T_OA_TRAVELSOLUTIONS = solution.FirstOrDefault();
                        //ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID = SolutionID;
                        ent.COMPANYID = id;
                        ent.CREATEUSERID = userid;
                        ent.OWNERCOMPANYID = ownercompanyid;
                        ent.OWNERDEPARTMENTID = ownerdepartmentid;
                        ent.OWNERPOSTID = ownerpostid;
                        ent.CREATEDATE = System.DateTime.Now;
                        dal.AddToContext(ent);
                    }

                }
                StrRerurn = dal.SaveContextChanges() > 0 ? "" : "ERROR";

            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-AddTravleSolutionSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrRerurn = "ERROR";

            }
            return StrRerurn;
        }
        /// <summary>
        /// 出差方案应用修改
        /// </summary>
        /// <param name="SolutionID"></param>
        /// <param name="companyids"></param>
        /// <param name="ownercompanyid"></param>
        /// <param name="ownerpostid"></param>
        /// <param name="ownerdepartmentid"></param>
        /// <returns></returns>
        public string UpdateTravleSolutionSet(string SolutionID, List<string> companyids, string ownercompanyid, string ownerpostid, string ownerdepartmentid, string userid)
        {
            string StrRerurn = "";
            try
            {
                dal.BeginTransaction();
                var EntObjs = from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                              where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                              select ent;
                if (EntObjs.Count() > 0)
                {
                    foreach (var obj in EntObjs)
                    {
                        dal.DeleteFromContext(obj);
                    }
                }
                dal.SaveContextChanges();
                for (int i = 0; i < companyids.Count(); i++)
                {
                    string id = companyids[i];
                    var ents = from a in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                               where a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID && a.COMPANYID == id
                               select a;
                    if (!(ents.Count() > 0))
                    {
                        T_OA_PROGRAMAPPLICATIONS ent = new T_OA_PROGRAMAPPLICATIONS();
                        ent.PROGRAMAPPLICATIONSID = System.Guid.NewGuid().ToString();
                        ent.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                        new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", SolutionID);

                        ent.COMPANYID = id;
                        ent.OWNERCOMPANYID = ownercompanyid;
                        ent.OWNERDEPARTMENTID = ownerdepartmentid;
                        ent.OWNERPOSTID = ownerpostid;
                        ent.CREATEUSERID = userid;
                        ent.CREATEDATE = System.DateTime.Now;
                        dal.AddToContext(ent);
                    }

                }
                StrRerurn = dal.SaveContextChanges() > 0 ? "" : "ERROR";
                if (StrRerurn == "")
                {
                    dal.CommitTransaction();
                }
                else
                {
                    dal.RollbackTransaction();
                }

            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-UpdateTravleSolutionSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrRerurn = "ERROR";
                dal.RollbackTransaction();

            }
            return StrRerurn;
        }

        public IQueryable<T_OA_PROGRAMAPPLICATIONS> GetTravleSolutionSetBySolutionID(string SolutionID)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                           where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                           select ent;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差方案TravleSolutionBLL-GetTravleSolutionSetBySolutionID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }





        #endregion

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="SolutionID"></param>
        ///// <param name="CompanyID"></param>
        //public void GetQueryTravelAllowance(string SolutionID, string CompanyID)
        //{
        //    try
        //    {
        //        var EntVechile = from ent in dal.GetObjects<T_OA_TAKETHESTANDARDTRANSPORT>().Include("T_OA_TRAVELSOLUTIONS")
        //                         where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
        //                         select ent;
        //        var EntPlane = from ent in dal.GetObjects<T_OA_CANTAKETHEPLANELINE>().Include("T_OA_TRAVELSOLUTIONS")
        //                       where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
        //                       select ent;
        //        ListPlaneList = (EntPlane == null) ? null : EntPlane.ToList();
        //        ListVechile = (EntVechile == null) ? null : EntVechile.ToList();



        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("出差方案TravleSolutionBLL-GetVechileStandardAndPlaneLine" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //        throw (ex);
        //    }
        //}
    }
}
