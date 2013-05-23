using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Dynamic;
using System.Reflection;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    class BumfManagementBll
    {
    }

    #region 文档缓急管理
    //文档缓急管理
    public class BumfPrioritiesManagementBll : BaseBll<T_OA_PRIORITIES>
    {

        
        public string AddPrioritiesInfo(T_OA_PRIORITIES PrioritiesObj)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.PRIORITIES == PrioritiesObj.PRIORITIES);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                }

                int i = dal.Add(PrioritiesObj);


                if (!(i > 0))
                {
                    StrReturn = "SAVEFAILED";//保存失败
                }
                return StrReturn;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-AddPrioritiesInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "";
            }
        }
        

        public bool UpdatePrioritiesInfo(T_OA_PRIORITIES PrioritiesObj)
        {

            try
            {
                var users = from ent in dal.GetTable()
                            where ent.PRIORITIESID == PrioritiesObj.PRIORITIESID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    Utility.CloneEntity(PrioritiesObj, user);
                    int i=dal.Update(user);
                    if (i > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }

                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdatePrioritiesInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //批量删除密级信息
        public bool BatchDeletePrioritiesInfos(string[] ArrOrderMealIDs)
        {
            try
            {

                foreach (string id in ArrOrderMealIDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.PRIORITIESID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    dal.DeleteFromContext(ent);
                    
                    
                }
                int i = dal.SaveContextChanges();
                if (i > 0)
                    return true;

                return false;
                
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchDeletePrioritiesInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //判断是否有存在的密级名称


        public bool GetPrioritiesInfoByAdd(string StrName)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_PRIORITIES>()
                        where ent.PRIORITIES == StrName
                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetPrioritiesInfoByAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
            //return null;
        }


        public T_OA_PRIORITIES GetPrioritiesInfoById(string PrioritiesId)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_PRIORITIES>()
                        where ent.PRIORITIESID == PrioritiesId
                        orderby ent.PRIORITIESID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetPrioritiesInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        //获取所有的密级信息
        public List<T_OA_PRIORITIES> GetPrioritiesInfos()
        {
            try
            {
                var query = from p in dal.GetObjects()
                            orderby p.CREATEDATE descending
                            select p;



                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_PRIORITIES>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetPrioritiesInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }
        //返回缓急类型
        public List<string> GetProritityNameInfos()
        {
            try
            {
                var query = from p in dal.GetTable()
                            orderby p.CREATEDATE descending
                            select p.PRIORITIES;

                return query.Count() > 0 ? query.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetProritityNameInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }


        //获取查询的密级信息
        public IQueryable<T_OA_PRIORITIES> GetPrioritiesInfosListByTypeCompanyDepartmentSearch(string StrTitle, string StrDepartment, DateTime DtStart, DateTime DtEnd, string StrCompany, string StrPosition)
        {
            try
            {
                var q = from ent in dal.GetObjects()

                        select ent;


                if (!string.IsNullOrEmpty(StrTitle))
                {
                    q = q.Where(s => StrTitle.Contains(s.PRIORITIES));
                }
                if (!string.IsNullOrEmpty(StrDepartment))
                {
                    q = q.Where(s => StrDepartment.Contains(s.CREATEDEPARTMENTID));
                }
                if (!string.IsNullOrEmpty(StrCompany))
                {
                    q = q.Where(s => StrCompany.Contains(s.CREATECOMPANYID));
                }

                if (!string.IsNullOrEmpty(StrPosition))
                {
                    q = q.Where(s => StrPosition.Contains(s.CREATEPOSTID));
                }
                //int aa = DtStart.CompareTo(DtEnd);
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.CREATEDATE);
                    q = q.Where(s => DtEnd > s.CREATEDATE);
                }

                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetPrioritiesInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        
        public IQueryable<T_OA_PRIORITIES> GetPrioritiesInfosListByTypeCompanyDepartmentSearch(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in dal.GetObjects()

                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_PRIORITIES");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_PRIORITIES>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetPrioritiesInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }




    }
    #endregion

    #region 公文密级
    //公文等级
    public class BumfGradeManagementBll : BaseBll<T_OA_GRADED>
    {

        
        public string AddGradeInfo(T_OA_GRADED GradeObj)
        {
            try
            {

                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.GRADED == GradeObj.GRADED);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                }

                int i = dal.Add(GradeObj);


                if (!(i > 0))
                {
                    StrReturn = "SAVEFAILED";//保存失败
                }
                return StrReturn;


                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-AddGradeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "";
                
            }
        }

        public bool UpdateGradeInfo(T_OA_GRADED GradeObj)
        {

            try
            {
                var users = from ent in dal.GetTable()
                            where ent.GRADEDID == GradeObj.GRADEDID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    //Utility.CloneEntity(GradeObj, user);
                    user.GRADED = GradeObj.GRADED;
                    user.UPDATEDATE = GradeObj.UPDATEDATE;
                    user.UPDATEUSERID = GradeObj.UPDATEUSERID;
                    user.UPDATEUSERNAME = GradeObj.UPDATEUSERNAME;
                    int i = dal.Update(user);
                    if (i > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }

                return false;

            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdateGradeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //批量删除密级信息
        public bool BatchDeleteGradeInfos(string[] ArrOrderMealIDs)
        {
            try
            {
                foreach (string id in ArrOrderMealIDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.GRADEDID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                        
                    }
                }
                int i = dal.SaveContextChanges();
                if (i > 0)
                    return true;

                return false;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchDeleteGradeInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //判断是否有存在的密级名称
        

        public bool GetGradeInfoByAdd(string StrName)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_GRADED>()
                        where ent.GRADED == StrName
                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetGradeInfoByAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }

            //return null;
        }


        public T_OA_GRADED GetGradeInfoById(string GradeId)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.GRADEDID == GradeId
                        orderby ent.GRADEDID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetGradeInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        //获取所有的密级信息
        public List<T_OA_GRADED> GetGradeInfos()
        {
            try
            {
                var query = from p in dal.GetObjects<T_OA_GRADED>()
                            orderby p.CREATEDATE descending
                            select p;


                query = query.OrderByDescending(s => s.CREATEDATE);
                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_GRADED>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetGradeInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }
        
        //返回级别信息
        public List<string> GetGradeNameInfos()
        {
            try
            {
                var query = from p in dal.GetTable()
                            orderby p.CREATEDATE descending
                            select p.GRADED;

                return query.Count() > 0 ? query.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetGradeNameInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }


        //获取查询的密级信息
        public IQueryable<T_OA_GRADED> GetGradeInfosListByTypeCompanyDepartmentSearch(string StrTitle, string StrDepartment, DateTime DtStart, DateTime DtEnd, string StrCompany, string StrPosition)
        {
            try
            {
                var q = from ent in dal.GetObjects()

                        select ent;


                if (!string.IsNullOrEmpty(StrTitle))
                {
                    q = q.Where(s => StrTitle.Contains(s.GRADED));
                }
                if (!string.IsNullOrEmpty(StrDepartment))
                {
                    q = q.Where(s => StrDepartment.Contains(s.CREATEDEPARTMENTID));
                }
                if (!string.IsNullOrEmpty(StrCompany))
                {
                    q = q.Where(s => StrCompany.Contains(s.CREATECOMPANYID));
                }

                if (!string.IsNullOrEmpty(StrPosition))
                {
                    q = q.Where(s => StrPosition.Contains(s.CREATEPOSTID));
                }
                //int aa = DtStart.CompareTo(DtEnd);
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.CREATEDATE);
                    q = q.Where(s => DtEnd > s.CREATEDATE);
                }

                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetGradeInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }



        


        public IQueryable<T_OA_GRADED> GetGradeInfosListByTypeCompanyDepartmentSearch(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in dal.GetObjects()

                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_GRADED");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_GRADED>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetGradeInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }


    }
    #endregion

    #region 公文类型
    //文档类型
    public class BumfDocTypeManagementBll : BaseBll<T_OA_SENDDOCTYPE>
    {
        /// <summary>
        /// 添加公文类型
        /// </summary>
        /// <param name="DocTypeObj">公文类型实体</param>
        /// <returns>返回操作的字符串</returns>
        public string AddDocTypeInfo(T_OA_SENDDOCTYPE DocTypeObj)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.SENDDOCTYPE == DocTypeObj.SENDDOCTYPE);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                }
                else
                {
                    int i = dal.Add(DocTypeObj);

                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;

                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-AddDocTypeInfo"+ System.DateTime.Now.ToString() +" "+ex.ToString());
                return "ERROR";
            }
        }
        /// <summary>
        /// 更新公文类型
        /// </summary>
        /// <param name="DocTypeObj">公文类型实体</param>
        /// <returns>是、否</returns>
        public bool UpdateDocTypeInfo(T_OA_SENDDOCTYPE DocTypeObj)
        {

            try
            {

                var users = from ent in dal.GetObjects<T_OA_SENDDOCTYPE>()
                            where ent.SENDDOCTYPEID == DocTypeObj.SENDDOCTYPEID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    if(DocTypeObj.EntityKey == null)
                        DocTypeObj.EntityKey = user.EntityKey;
                    Utility.CloneEntity(DocTypeObj, user);
                    int i = dal.Update(user);
                    if (i > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
                return false;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdateDocTypeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
               
            }
        }

        //批量删除文档类型信息
        /// <summary>
        /// 批量删除公文类型
        /// </summary>
        /// <param name="ArrOrderMealIDs">公文类型ID集合</param>
        /// <returns>操纵的字符串</returns>
        public string BatchDeleteDocTypeInfos(string[] ArrOrderMealIDs)
        {
            try
            {
                string StrReturn = "";
                foreach (string id in ArrOrderMealIDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.SENDDOCTYPEID == id
                               select e;
                    
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        var enttemplate = from p in dal.GetObjects<T_OA_SENDDOCTEMPLATE>()
                                      where p.SENDDOCTYPE == ents.FirstOrDefault().SENDDOCTYPE
                                              select p;
                        var entsends = from m in dal.GetObjects<T_OA_SENDDOC>().Include("T_OA_SENDDOCTYPE")
                                       where m.T_OA_SENDDOCTYPE.SENDDOCTYPE == ents.FirstOrDefault().SENDDOCTYPE
                                       select m;

                        if (enttemplate.Count() > 0 || entsends.Count()>0)
                        {
                            StrReturn = "PLEASEDELETETEMPLATE";//请先删除对应的模板信息
                            break;
                        }
                        else
                        {
                            dal.DeleteFromContext(ent);
                            int i = dal.SaveContextChanges();
                            if (!(i > 0))
                                StrReturn = "ERROR";
                        }
                    }
                }
              
                return StrReturn;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchDeleteDocTypeInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "ERROR";
                
            }
        }

        //判断是否有存在的文档类型
        
        /// <summary>
        /// 判断是否存在相同的公文类型
        /// </summary>
        /// <param name="StrName"></param>
        /// <returns></returns>
        public bool GetDocTypeInfoByAdd(string StrName)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_SENDDOCTYPE>()
                        where ent.SENDDOCTYPE == StrName
                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeInfoByAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
          
            }
            //return null;
        }

        /// <summary>
        /// 获取公文类型实体
        /// </summary>
        /// <param name="DocTypeId">公文类型ID</param>
        /// <returns></returns>
        public T_OA_SENDDOCTYPE GetDocTypeInfoById(string DocTypeId)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.SENDDOCTYPEID == DocTypeId
                        orderby ent.SENDDOCTYPEID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
          
            }
        }

        //获取所有的文档类型信息
        /// <summary>
        /// 获取所有的公文类型集合
        /// </summary>
        /// <returns></returns>
        public List<T_OA_SENDDOCTYPE> GetDocTypeInfos()
        {
            try
            {
                var query = from p in dal.GetObjects()
                            orderby p.CREATEDATE descending
                            select p;


                query = query.OrderByDescending(s => s.CREATEDATE);
                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_SENDDOCTYPE>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;

            }

        }
        /// <summary>
        /// 通过权限过滤获取公文类型信息集合
        /// </summary>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">sql字符串</param>
        /// <param name="paras">传递的参数</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<T_OA_SENDDOCTYPE> GetDocTypeInfosListByTypeCompanyDepartmentSearch(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {

                var ents = from ent in dal.GetObjects()
                        select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OABUMFDOCTYPE");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_SENDDOCTYPE>(ents, pageIndex, pageSize, ref pageCount);
                return ents == null ? null:ents.ToList();
                
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
 
            }

        }
        //返回级别类型
        /// <summary>
        /// 返回公文类型的类型名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetDocTypeNameInfos()
        {
            try
            {
                var query = from p in dal.GetTable()
                            orderby p.CREATEDATE descending
                            select p.SENDDOCTYPE;

                return query.Count() > 0 ? query.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeNameInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
             
            }
        }

        
        //获取查询的文档类型信息
        /// <summary>
        /// 返回公文类型集合  （没权限过滤）
        /// </summary>
        /// <param name="StrTitle"></param>
        /// <param name="StrDepartment"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <param name="StrCompany"></param>
        /// <param name="StrPosition"></param>
        /// <returns></returns>
        public IQueryable<T_OA_SENDDOCTYPE> GetDocTypeInfosListByTypeCompanyDepartmentSearch(string StrTitle, string StrDepartment, DateTime DtStart, DateTime DtEnd, string StrCompany,string StrPosition)
        {
            try
            {
                var q = from ent in dal.GetObjects()

                        select ent;


                if (!string.IsNullOrEmpty(StrTitle))
                {
                    q = q.Where(s => StrTitle.Contains(s.SENDDOCTYPE));
                }
                if (!string.IsNullOrEmpty(StrDepartment))
                {
                    q = q.Where(s => StrDepartment.Contains(s.CREATEDEPARTMENTID));
                }
                if (!string.IsNullOrEmpty(StrCompany))
                {
                    q = q.Where(s => StrCompany.Contains(s.CREATECOMPANYID));
                }

                if (!string.IsNullOrEmpty(StrPosition))
                {
                    q = q.Where(s => StrPosition.Contains(s.CREATEPOSTID));
                }
                //int aa = DtStart.CompareTo(DtEnd);
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.CREATEDATE);
                    q = q.Where(s => DtEnd > s.CREATEDATE);
                }
                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
      
            }
        }

        
    }
    #endregion

    #region 公司发文类型模板
    //公司发文模板
    public class BumfDocTypeTemplateManagementBll : BaseBll<T_OA_SENDDOCTEMPLATE>
    {
        private BumfDocTypeTemplateManagementDal DocTypeTemplateDal = new BumfDocTypeTemplateManagementDal();
        /// <summary>
        /// 添加公文模板实体
        /// </summary>
        /// <param name="DocTypeTemplateObj">公文模板实体</param>
        /// <returns>返回字符串</returns>
        public string AddDocTypeTemplateInfo(T_OA_SENDDOCTEMPLATE DocTypeTemplateObj)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.PRIORITIES == DocTypeTemplateObj.PRIORITIES
                    && s.SENDDOCTITLE == DocTypeTemplateObj.SENDDOCTITLE && s.SENDDOCTYPE == DocTypeTemplateObj.SENDDOCTYPE
                    && s.TEMPLATENAME == DocTypeTemplateObj.TEMPLATENAME);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                }
                else
                {
                    int i = dal.Add(DocTypeTemplateObj);


                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;


            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-AddDocTypeTemplateInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "SAVEFAILED";
               
            }
        }
        /// <summary>
        /// 更新公文模板  2010-7-29
        /// </summary>
        /// <param name="DocTypeTemplateObj"></param>
        /// <param name="StrResult"></param>
        /// <returns></returns>
        public bool UpdateDocTypeTemplateInfo(T_OA_SENDDOCTEMPLATE DocTypeTemplateObj,ref string StrResult)
        {

            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.SENDDOCTITLE == DocTypeTemplateObj.SENDDOCTITLE && ent.SENDDOCTYPE == DocTypeTemplateObj.SENDDOCTYPE &&
                        ent.GRADED == DocTypeTemplateObj.GRADED &&  ent.PRIORITIES == DocTypeTemplateObj.PRIORITIES && ent.CREATECOMPANYID == DocTypeTemplateObj.CREATECOMPANYID &&
                        ent.CREATEDEPARTMENTID == DocTypeTemplateObj.CREATEDEPARTMENTID && ent.CREATEPOSTID == DocTypeTemplateObj.CREATEPOSTID &&
                        ent.TEMPLATENAME == DocTypeTemplateObj.TEMPLATENAME
                        select ent;
                if (q.Count() > 1)
                {
                    StrResult = "REPETITION";
                    return false;
                }
                var entity = from ent in dal.GetTable()
                             where ent.SENDDOCTEMPLATEID == DocTypeTemplateObj.SENDDOCTEMPLATEID
                             select ent;

                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();                    
                    entitys.TEMPLATENAME = DocTypeTemplateObj.TEMPLATENAME;
                    entitys.SENDDOCTYPE = DocTypeTemplateObj.SENDDOCTYPE;
                    entitys.PRIORITIES = DocTypeTemplateObj.PRIORITIES;
                    entitys.GRADED = DocTypeTemplateObj.GRADED;
                    entitys.CONTENT = DocTypeTemplateObj.CONTENT;
                    entitys.SENDDOCTITLE = DocTypeTemplateObj.SENDDOCTITLE;
                    
                    //entitys.CREATECOMPANYID = DocTypeTemplateObj.CREATECOMPANYID;
                    //entitys.CREATEDATE = DocTypeTemplateObj.CREATEDATE;
                    //entitys.CREATEDEPARTMENTID = DocTypeTemplateObj.CREATEDEPARTMENTID;
                    //entitys.CREATEPOSTID = DocTypeTemplateObj.CREATEPOSTID;
                    //entitys.CREATEUSERID = DocTypeTemplateObj.CREATEUSERID;
                    entitys.UPDATEDATE = DocTypeTemplateObj.UPDATEDATE;
                    entitys.UPDATEUSERNAME = DocTypeTemplateObj.UPDATEUSERNAME;
                    entitys.UPDATEUSERID = DocTypeTemplateObj.UPDATEUSERID;

                    //DataContext.ap
                    if (dal.Update(entitys) >0 )
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdateDocTypeTemplateInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
           
            }
        }

        //批量删除文档类型信息
        /// <summary>
        /// 根据公文模板ID集合删除公文模板
        /// </summary>
        /// <param name="ArrOrderMealIDs">公文模板ID集合</param>
        /// <returns></returns>
        public bool BatchDeleteDocTypeTemplateInfos(string[] ArrOrderMealIDs)
        {
            try
            {

                foreach (string id in ArrOrderMealIDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.SENDDOCTEMPLATEID == id
                               select e;
                    
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        //DataContext.DeleteObject(ent);
                        dal.DeleteFromContext(ent);
                        
                    }
                }
                int i = dal.SaveContextChanges();
                if (i > 0)
                    return true;
               
                return false ;

            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchDeleteDocTypeTemplateInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
       
            }
        }

        //判断是否有存在的公文类型模板
        /// <summary>
        /// 是否存在相同的公文类型模板
        /// </summary>
        /// <param name="StrTitle">模板名称</param>
        /// <param name="StrType">公文类型</param>
        /// <param name="StrGrade">级别</param>
        /// <param name="StrProritity">缓急程度</param>
        /// <param name="StrCompanyID">公司ID</param>
        /// <param name="StrDepartmentID">部门ID</param>
        /// <param name="StrPositionID">岗位ID</param>
        /// <returns></returns>
        public bool GetDocTypeTemplateInfoByAdd(string StrTitle, string StrType, string StrGrade, string StrProritity, string StrCompanyID, string StrDepartmentID, string StrPositionID)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in DocTypeTemplateDal.GetTable()
                        where ent.SENDDOCTITLE == StrTitle && ent.SENDDOCTYPE == StrType && ent.GRADED == StrGrade &&
                        ent.PRIORITIES == StrProritity && ent.CREATECOMPANYID == StrCompanyID &&
                        ent.CREATEDEPARTMENTID == StrDepartmentID && ent.CREATEPOSTID == StrPositionID
                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeTemplateInfoByAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
               
            }
            //return null;
        }

        /// <summary>
        /// 根据公文模板ID获取公文模板信息
        /// </summary>
        /// <param name="DocTypeTemplateId">模板ID</param>
        /// <returns></returns>
        public T_OA_SENDDOCTEMPLATE GetDocTypeTemplateInfoById(string DocTypeTemplateId)
        {
            try
            {
                var q = from ent in dal.GetTable()
                        where ent.SENDDOCTEMPLATEID == DocTypeTemplateId
                        orderby ent.SENDDOCTEMPLATEID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeTemplateInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
      
            }
        }

        //获取所有的某一文档类型的所有模板信息
        /// <summary>
        /// 获取某一类型的所有公文模板
        /// </summary>
        /// <param name="StrDocType">公文模板</param>
        /// <returns></returns>
        public List<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateNameInfosByDocType(string StrDocType)
        {
            try
            {
                var query = from p in dal.GetTable()
                            where p.SENDDOCTYPE == StrDocType
                            orderby p.CREATEDATE descending
                            select p;


                query = query.OrderByDescending(s => s.CREATEDATE);
                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_SENDDOCTEMPLATE>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeTemplateNameInfosByDocType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
               
            }

        }

        

        //获取所有的文档类型信息
        /// <summary>
        /// 获取所有的公文模板信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateInfos()
        {
            try
            {
                var query = from p in dal.GetTable()
                            orderby p.CREATEDATE descending
                            select p;

                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_SENDDOCTEMPLATE>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeTemplateInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }



        //获取查询的文档类型信息
        /// <summary>
        /// 获取所有公文模板信息
        /// </summary>
        /// <param name="StrTitle"></param>
        /// <param name="StrDepartment"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <param name="StrCompany"></param>
        /// <param name="StrPosition"></param>
        /// <returns></returns>
        public IQueryable<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateInfosListByTypeCompanyDepartmentSearch(string StrTitle, string StrDepartment, DateTime DtStart, DateTime DtEnd, string StrCompany, string StrPosition)
        {
            try
            {
                var q = from ent in dal.GetTable()

                        select ent;


                if (!string.IsNullOrEmpty(StrTitle))
                {
                    q = q.Where(s => StrTitle.Contains(s.SENDDOCTYPE));
                }
                if (!string.IsNullOrEmpty(StrDepartment))
                {
                    q = q.Where(s => StrDepartment.Contains(s.CREATEDEPARTMENTID));
                }
                if (!string.IsNullOrEmpty(StrCompany))
                {
                    q = q.Where(s => StrCompany.Contains(s.CREATECOMPANYID));
                }

                if (!string.IsNullOrEmpty(StrPosition))
                {
                    q = q.Where(s => StrPosition.Contains(s.CREATEPOSTID));
                }
                //int aa = DtStart.CompareTo(DtEnd);
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.CREATEDATE);
                    q = q.Where(s => DtEnd > s.CREATEDATE);
                }
                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeTemplateInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }


        


        /// <summary>
        /// 通过权限过滤获取公文模板信息集合
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateInfosListByTypeCompanyDepartmentSearch(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in dal.GetObjects()

                        select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OABUMFDOCTEMPLATE");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_SENDDOCTEMPLATE>(ents, pageIndex, pageSize, ref pageCount);
                return ents;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocTypeTemplateInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;

            }

        }



    }
    #endregion

    #region 公司发文
    //公司发文
    public class BumfCompanySendDocManagementBll : BaseBll<T_OA_SENDDOC>
    {
        /// <summary>
        /// 添加公文信息
        /// </summary>
        /// <param name="SendDocObj">公文实体</param>
        /// <returns>返回字符串  成功为空</returns>
        public string AddSendDocInfo(T_OA_SENDDOC SendDocObj)
        {
            try
            {
                
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.PRIORITIES == SendDocObj.PRIORITIES
                    && s.GRADED == SendDocObj.GRADED && s.T_OA_SENDDOCTYPE.SENDDOCTYPE == SendDocObj.T_OA_SENDDOCTYPE.SENDDOCTYPE
                    //&& s.GRADED == SendDocObj.GRADED && s.T_OA_SENDDOCTYPEReference.EntityKey == SendDocObj.T_OA_SENDDOCTYPEReference.EntityKey
                    && s.DEPARTID == SendDocObj.DEPARTID && s.CREATECOMPANYID == SendDocObj.CREATECOMPANYID
                    && s.NUM == SendDocObj.NUM 
                    && s.SENDDOCTITLE == SendDocObj.SENDDOCTITLE  && s.CREATEDEPARTMENTID == SendDocObj.CREATEDEPARTMENTID);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                }
                else
                {
                    SendDocObj.CREATEDATE = System.DateTime.Now;
                    Utility.RefreshEntity(SendDocObj);
                    bool i = Add(SendDocObj);
                    string Record=string.Empty;
                    if (i)
                    {
                        Tracer.Debug("公司发文保存成功 ID:"+SendDocObj.SENDDOCID);
                    }
                         
                    if (i == false || !(string.IsNullOrEmpty(Record)))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;

            }
            catch (Exception ex)
            {
                Tracer.Debug("T_OA_SENDDOC Add"+ System.DateTime.Now.ToString()+" "+ex.ToString());
                return "SAVEFAILED";//保存失败
                
            }
        }

        public void BeginTransaction()
        {
            dal.BeginTransaction();
        }
        public void CommitTransaction()
        {
            dal.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            dal.RollbackTransaction();
        }
        /// <summary>
        /// 修改公文
        /// </summary>
        /// <param name="SendDocObj">公文实体</param>
        /// <param name="StrResult">返回的字符串</param>
        /// <returns></returns>
        public bool UpdateSendDocInfo(T_OA_SENDDOC SendDocObj,ref string StrResult)
        {

            try
            {    
                var tempEnt = dal.GetObjects().Where(s => s.PRIORITIES == SendDocObj.PRIORITIES
                && s.GRADED == SendDocObj.GRADED && s.T_OA_SENDDOCTYPE.SENDDOCTYPE == SendDocObj.T_OA_SENDDOCTYPE.SENDDOCTYPE
                && s.NUM == SendDocObj.NUM
                && s.DEPARTID == SendDocObj.DEPARTID && s.CREATECOMPANYID == SendDocObj.CREATECOMPANYID
                && s.SENDDOCTITLE == SendDocObj.SENDDOCTITLE && s.CREATEDEPARTMENTID == SendDocObj.CREATEDEPARTMENTID);
                if (tempEnt.Count() > 1)
                {
                    StrResult = "REPETITION"; //{0}已存在，保存失败！    
                    return false;
                }


                var users = from ent in dal.GetObjects().Include("T_OA_SENDDOCTYPE")//.T_OA_SENDDOC
                            where ent.SENDDOCID == SendDocObj.SENDDOCID
                            select ent;
                    
                if (users.Count() > 0)
                {
                    SendDocObj.UPDATEDATE = System.DateTime.Now;
                    var user = users.FirstOrDefault();
                    if (SendDocObj.EntityKey == null)
                    {
                        SendDocObj.EntityKey = user.EntityKey;
                    }
                    if (SendDocObj.CONTENT == null || SendDocObj.CONTENT.Length ==0)
                    {
                        SendDocObj.CONTENT = user.CONTENT;
                    }
                        
                    SendDocObj.CREATEDATE = user.CREATEDATE;
                    
                    int i = dal.Update(SendDocObj);
                    //int i = dal.SaveContextChanges();
                    string Record = SaveMyRecord(SendDocObj);//添加到我的单据中
                    if (i > 0 && string.IsNullOrEmpty(Record))
                    {
                        return true;
                    }
                    else
                    {
                        StrResult = "ERROR";
                        return false;
                    }


                }

                return false;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdateSendDocInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }


        /// <summary>
        /// 修改公文
        /// </summary>
        /// <param name="SendDocObj">公文实体</param>
        /// <param name="StrResult">返回的字符串</param>
        /// <returns></returns>
        public bool UpdateSendDocInfoForDistrbute(T_OA_SENDDOC SendDocObj, ref string StrResult)
        {

            try
            {
                SendDocObj.UPDATEDATE = System.DateTime.Now;
                //var entDoc = from ent in dal.GetObjects<T_OA_SENDDOC>()
                //          where ent.SENDDOCID == SendDocObj.SENDDOCID
                //          select ent;
                //entDoc.FirstOrDefault().PUBLISHDATE = SendDocObj.PUBLISHDATE;
                //entDoc.FirstOrDefault().ISDISTRIBUTE = SendDocObj.ISDISTRIBUTE;
                //entDoc.FirstOrDefault().UPDATEDATE = System.DateTime.Now;
                int i = dal.Update(SendDocObj);
                //int i = dal.SaveContextChanges();
                //string Record = SaveMyRecord(SendDocObj);//添加到我的单据中
                if (i > 0 )
                {
                    return true;
                }
                else
                {
                    StrResult = "ERROR";
                    return false;
                }


            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文发布后BumfManagementBll-UpdateSendDocInfoForDistrbute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;

            }
        }

        //批量删除发文信息
        /// <summary>
        /// 根据公文ID集合删除公文
        /// </summary>
        /// <param name="ArrSendDocIDs">公文ID集合</param>
        /// <returns></returns>
        public bool BatchDeleteSendDocInfos(string[] ArrSendDocIDs)
        {
            try
            {
                PublicInterfaceWS.PublicServiceClient PublicClient = new PublicInterfaceWS.PublicServiceClient();
                dal.BeginTransaction();
                foreach (string id in ArrSendDocIDs)
                {
                    var ents = from e in dal.GetTable()// DataContext.T_OA_SENDDOC
                               where e.SENDDOCID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        //DataContext.DeleteObject(ent);
                        //dal.DataContext.DeleteObject(ent);
                        dal.DeleteFromContext(ent);
                        string Record=DeleteMyRecord(ent);
                        if (!string.IsNullOrEmpty(Record))
                        {
                            return false;
                        }

                    }
                }

                int i = dal.SaveContextChanges();
                if (i > 0)
                {
                    //删除富文本框
                    foreach (string id in ArrSendDocIDs)
                    {
                        PublicClient.DeleteContent(id);
                    }
                    dal.CommitTransaction();
                    //PublicClient.DeleteContent();
                    return true;
                }
                dal.RollbackTransaction();
                return  false;

                                
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("T_OA_SENDDOC BatchDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
             
            }
        }

        //判断是否有存在的发文信息

        /// <summary>
        /// 判断是否存在相同的公文信息
        /// </summary>
        /// <param name="StrTitle">公文标题</param>
        /// <param name="StrGrade">公文级别</param>
        /// <param name="StrPrioritity">公文缓急</param>
        /// <param name="StrType">公文类型</param>
        /// <param name="SendDepart">发文部门ID</param>
        /// <param name="StrCompanyID">公司ID</param>
        /// <param name="StrDepartmentID">部门ID</param>
        /// <param name="StrPositionID">岗位ID</param>
        /// <returns></returns>
        public bool GetSendDocInfoByAdd(string StrTitle,string StrGrade,string StrPrioritity,string StrType,string SendDepart,string StrCompanyID,string StrDepartmentID,string StrPositionID)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_SENDDOC>()
                        where ent.SENDDOCTITLE == StrTitle && ent.T_OA_SENDDOCTYPE.SENDDOCTYPEID == StrType && ent.PRIORITIES == StrPrioritity &&
                        ent.GRADED == StrGrade && ent.CREATECOMPANYID == StrCompanyID && ent.DEPARTID == SendDepart &&
                        ent.CREATEDEPARTMENTID == StrDepartmentID && ent.CREATEPOSTID == StrPositionID
                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdateSendDocInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
            //return null;
        }

        /// <summary>
        /// 根据公文ID获取公文信息
        /// </summary>
        /// <param name="SendDocId">公文ID</param>
        /// <returns></returns>
        public T_OA_SENDDOC GetSendDocInfoById(string SendDocId)
        {
            try
            {
                var q = from ent in dal.GetObjects().Include("T_OA_SENDDOCTYPE")
                        where ent.SENDDOCID == SendDocId
                        
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("T_OA_SENDDOC GetSendDoc" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        //获取所有的发文信息
        /// <summary>
        /// 获取公文信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_SENDDOC> GetSendDocInfos()
        {
            try
            {
                var query = from p in dal.GetObjects<T_OA_SENDDOC>()
                            orderby p.CREATEDATE descending
                            select p;


                query = query.OrderBy(s => s.CREATEDATE);
                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_SENDDOC>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("T_OA_SENDDOC GetSendDocInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        //获取所有的已发布的公文信息
        /// <summary>
        /// takecount 取前几条数据
        /// </summary>
        /// <param name="takecount"></param>
        /// <returns></returns>
        public List<T_OA_SENDDOC> GetDistrbutedInfos(int takecount)
        {
            try
            {
                var query = from p in dal.GetObjects<T_OA_SENDDOC>()
                            where p.ISDISTRIBUTE == "1"
                            orderby p.CREATEDATE descending
                            select p;


                if (query.Count() > 0)
                {

                    return query.Take(takecount).ToList<T_OA_SENDDOC>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("T_OA_SENDDOC GetDistrbutedInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            
            }

        }

        //已发布的公文文档
        /// <summary>
        /// 获取已发布的公文信息
        /// </summary>
        /// <param name="StrTitle"></param>
        /// <param name="StrContent"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <param name="StrGrade"></param>
        /// <param name="StrProritity"></param>
        /// <param name="StrDocType"></param>
        /// <returns></returns>
        public List<T_OA_SENDDOC> GetDistributedSendDocInfos(string StrTitle, string StrContent, DateTime DtStart, DateTime DtEnd, string StrGrade, string StrProritity, string StrDocType)
        {
            try
            {
                var query = from p in dal.GetObjects<T_OA_SENDDOC>()
                            where p.ISDISTRIBUTE == "1"

                            select p;

                if (!string.IsNullOrEmpty(StrTitle))
                {
                    query = query.Where(s => StrTitle.Contains(s.SENDDOCTITLE));
                }

                if (!string.IsNullOrEmpty(StrGrade))
                {
                    query = query.Where(s => s.GRADED == StrGrade);
                }
                if (!string.IsNullOrEmpty(StrProritity))
                {
                    query = query.Where(s => s.PRIORITIES == StrProritity);
                }
                if (!string.IsNullOrEmpty(StrDocType))
                {
                    query = query.Where(s => s.T_OA_SENDDOCTYPE.SENDDOCTYPEID == StrDocType);
                }

                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    query = query.Where(s => DtStart < s.CREATEDATE);
                    query = query.Where(s => DtEnd > s.CREATEDATE);
                }
                query = query.OrderByDescending(s => s.UPDATEDATE);

                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_SENDDOC>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("T_OA_SENDDOC GetDistributedSendDocInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        //已归档的公文文档
        /// <summary>
        /// 获取已归档的公文文档
        /// </summary>
        /// <param name="StrTitle"></param>
        /// <param name="StrContent"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <param name="StrGrade"></param>
        /// <param name="StrProritity"></param>
        /// <param name="StrDocType"></param>
        /// <returns></returns>
        public List<T_OA_SENDDOC> GetSavedSendDocInfos(string StrTitle, string StrContent, DateTime DtStart, DateTime DtEnd, string StrGrade, string StrProritity, string StrDocType)
        {
            try
            {
                var query = from p in dal.GetObjects<T_OA_SENDDOC>()
                            where p.ISSAVE == "1"
                            select p;

                if (!string.IsNullOrEmpty(StrTitle))
                {
                    query = query.Where(s => StrTitle.Contains(s.SENDDOCTITLE));
                }


                if (!string.IsNullOrEmpty(StrGrade))
                {
                    query = query.Where(s => s.GRADED == StrGrade);
                }
                if (!string.IsNullOrEmpty(StrProritity))
                {
                    query = query.Where(s => s.PRIORITIES == StrProritity);
                }
                if (!string.IsNullOrEmpty(StrDocType))
                {
                    query = query.Where(s => s.T_OA_SENDDOCTYPE.SENDDOCTYPEID == StrDocType);
                }

                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    query = query.Where(s => DtStart < s.CREATEDATE);
                    query = query.Where(s => DtEnd > s.CREATEDATE);
                }
                query = query.OrderByDescending(s => s.UPDATEDATE);

                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_SENDDOC>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("T_OA_SENDDOC GetSavedSendDocInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
               
            }

        }
        /// <summary>
        /// 获取公文文档信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_BumfCompanySendDoc> GetSendDocInfosListByTypeCompanyDepartmentSearch(int pageIndex, int pageSize, 
            string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, 
            string checkState, string userID)
        {
            try
            {
                //SMT_OA_EFModel.SMT_OA_EFModelContext context = dal.lbc.GetDataContext() as SMT_OA_EFModel.SMT_OA_EFModelContext;

                

                var ents = (from a in dal.GetObjects().Include("T_OA_SENDDOCTYPE")
                            join b in dal.GetObjects<T_OA_SENDDOCTYPE>() on a.T_OA_SENDDOCTYPE.SENDDOCTYPE equals b.SENDDOCTYPE
                            select new V_BumfCompanySendDoc
                            {
                                senddoc=a,
                                OACompanySendDoc = a,doctype=b,
                                OWNERCOMPANYID=a.OWNERCOMPANYID,
                                OWNERID = a.OWNERID,
                                OWNERPOSTID = a.OWNERPOSTID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                CREATEUSERID = a.CREATEUSERID                                
                            });
                
                if (ents.Count() > 0)
                {                        
                    if (flowInfoList != null)
                    {
                            
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.OACompanySendDoc.SENDDOCID equals l.FormID
                                select new V_BumfCompanySendDoc
                                { 
                                    OACompanySendDoc = a.OACompanySendDoc,
                                    doctype=a.doctype, flowApp = l ,
                                    CREATEDATE = (DateTime)a.OACompanySendDoc.CREATEDATE,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID

                                });
                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.OACompanySendDoc.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    //DateTime dt222 = new DateTime();
                    //dt222 = System.DateTime.Now;
                    if (checkState != CheckStates.WaittingApproval.ToString())//如果是待审核，不需权限控制
                    {
                        UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_SENDDOC");
                    }
                    //DateTime dt3 = new DateTime();
                    //dt3 = System.DateTime.Now;
                    //TimeSpan ddd = dt3 - dt222;
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        //ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                        ents = ents.Where(filterString, queryParas.ToArray());
                    }
                    //DateTime dt33 = new DateTime();
                    //dt33 = System.DateTime.Now;
                    //ents = from ent in ents
                    //         orderby ent.OACompanySendDoc.CREATEDATE descending
                    //         select ent;
                    //ents = Utility.DataSorting<V_BumfCompanySendDoc>(ents, "CREATEDATE", "DESC");
                    //DateTime dt22 = new DateTime();
                    //dt22 = System.DateTime.Now;
                    //TimeSpan ddt1 = dt22 - dt33;
                    
                    ////ents = ents.OrderBy(sort);
                    
                    //DateTime dt = new DateTime();
                    //dt = System.DateTime.Now;
                    //ents = Utility.Pager<V_BumfCompanySendDoc>(ents, pageIndex, pageSize, ref pageCount);
                    ents = ents.OrderBy(sort);//.OrderByDescending(sd => sd.OACompanySendDoc.ISDISTRIBUTE);
                    ents = Utility.Pager<V_BumfCompanySendDoc>(ents, pageIndex, pageSize, ref pageCount);
                    //DateTime dt2 = new DateTime();
                    //dt2 = System.DateTime.Now;
                    //TimeSpan ddt = dt2 - dt;
                    return ents.Count() >0 ? ents.ToList():null;
                }
                                
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetSendDocInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }
        //获取我的公文文档
        /// <summary>
        /// 获取我的公文信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState"></param>
        /// <param name="userID"></param>
        /// <param name="postID"></param>
        /// <param name="companyID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public IQueryable<V_BumfCompanySendDoc> GetMySendDocInfosList(int pageIndex, int pageSize, string sort, 
            string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, 
            string userID, string postID, string companyID, string departmentID)
        {
            try
            {
                //SMT_OA_EFModel.SMT_OA_EFModelContext context = dal.lbc.GetDataContext() as SMT_OA_EFModel.SMT_OA_EFModelContext;
                
                

                var ents = (from a in dal.GetObjects().Include("T_OA_SENDDOCTYPE")
                            join b in dal.GetObjects<T_OA_SENDDOCTYPE>() on a.T_OA_SENDDOCTYPE.SENDDOCTYPE equals b.SENDDOCTYPE
                            join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on a.SENDDOCID equals c.FORMID
                            //where a.ISDISTRIBUTE == "1" && (c.VIEWER == userID || c.VIEWER == postID || c.VIEWER == departmentID || c.VIEWER == companyID)
                            select new V_BumfCompanySendDoc 
                            { 
                                OACompanySendDoc = a, doctype = b, distrbuteuser = c,
                                OWNERCOMPANYID = a.OWNERCOMPANYID,OWNERID = a.OWNERID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,OWNERPOSTID = a.OWNERPOSTID
                            });
                if (ents.Count() > 0)
                {

                        
                    ents = ents.Where(s => (s.OACompanySendDoc.ISDISTRIBUTE == "1") && 
                        (s.distrbuteuser.VIEWER == userID 
                        || s.distrbuteuser.VIEWER == postID 
                        || s.distrbuteuser.VIEWER == departmentID 
                        || s.distrbuteuser.VIEWER == companyID));
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    //UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAMYCOMPANYDOC");  个人没有权限验证2010-7-22

                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }

                    //ents = ents.OrderBy(sort);
                    ents = ents.OrderByDescending(item=>item.OACompanySendDoc.CREATEDATE);
                    ents = Utility.Pager<V_BumfCompanySendDoc>(ents, pageIndex, pageSize, ref pageCount);
                    return ents;
                }

                
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetMySendDocInfosList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }
        /// <summary>
        /// 查找公司发文信息，现用于打开待办里的公司发文发布信息
        /// </summary>
        /// <param name="docId"></param>
        /// <returns></returns>
        public V_BumfCompanySendDoc GetBumfDocInfo(string docId)
        {
            try
            {
                var entBumf = (from a in dal.GetObjects().Include("T_OA_SENDDOCTYPE")
                            join b in dal.GetObjects<T_OA_SENDDOCTYPE>() on a.T_OA_SENDDOCTYPE.SENDDOCTYPE equals b.SENDDOCTYPE
                            //join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on a.SENDDOCID equals c.FORMID
                            //where a.ISDISTRIBUTE == "1" && (c.VIEWER == userID || c.VIEWER == postID || c.VIEWER == departmentID || c.VIEWER == companyID)
                            where a.SENDDOCID==docId && a.CHECKSTATE=="2"
                            select new V_BumfCompanySendDoc
                            {
                                OACompanySendDoc = a,
                                doctype = b,
                                distrbuteuser = null,
                                OWNERCOMPANYID = a.OWNERCOMPANYID,
                                OWNERID = a.OWNERID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                OWNERPOSTID = a.OWNERPOSTID
                            });
                if (entBumf.Count() > 0)
                {
                    //var dtbtBumf = entBumf.FirstOrDefault().senddoc;
                    //CloseDotask(dtbtBumf.SENDDOCID,dtbtBumf.OWNERID);
                    return entBumf.FirstOrDefault();
                }
                else
                {
                    Tracer.Debug("BumfManagementBll-GetBumfDocInfo:未找到相应公司发文或状态不是审核通过");
                    return null;
                }
            }
            catch(Exception ex)
            {
                Tracer.Debug("BumfManagementBll-GetBumfDocInfo:" + ex.ToString());
                return null;
            }
        }
        //获取查询的发文信息
        /// <summary>
        /// 获取查询的公文信息
        /// </summary>
        /// <param name="StrTitle"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <returns></returns>
        public List<T_OA_SENDDOC> GetSendDocInfosListByTypeCompanyDepartmentSearch(string StrTitle, DateTime DtStart, DateTime DtEnd)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_SENDDOC>()

                        select ent;
                               
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.CREATEDATE);
                    q = q.Where(s => DtEnd > s.CREATEDATE);
                }
                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q.ToList<T_OA_SENDDOC>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetSendDocInfosListByTypeCompanyDepartmentSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }
        /// <summary>
        /// 获取用户在设定的权限范围内的 公文编号信息2011-3-11
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public List<V_CompanyDocNum> GetCompanyDocNumsBuUserID(string userID, string sort, string filterString)
        {
            try
            {
                //获取审核通过的公文
                var ents = from ent in dal.GetObjects<T_OA_SENDDOC>()
                           where ent.CHECKSTATE == "2"
                           select new V_CompanyDocNum {
                               CHECKSTATE = ent.CHECKSTATE,
                               NUM = ent.NUM,
                               CREATEDATE = (DateTime)ent.CREATEDATE,
                               OWNERCOMPANYID = ent.OWNERCOMPANYID,
                               OWNERID = ent.OWNERID,
                               OWNERPOSTID = ent.OWNERPOSTID,
                               OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID,
                               CREATEUSERID = ent.CREATEUSERID
                           };


                if (ents.Count() > 0)
                {
                    
                    List<object> queryParas = new List<object>();
                   
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_SENDDOC");
                    
                   
                    if (!string.IsNullOrEmpty(filterString))
                    {                        
                        ents = ents.Where(filterString, queryParas.ToArray());
                    }
                    
                    ents = ents.OrderBy(sort);
                    
                    return ents.Count() > 0 ? ents.ToList() : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetCompanyDocNumsBuUserID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;

            }

        }

        /// <summary>
        /// 修改实体审核状态
        /// </summary>
        /// <param name="strEntityName">实体名</param>
        /// <param name="EntityKeyName">主键名</param>
        /// <param name="EntityKeyValue">主键值</param>
        /// <param name="CheckState">审核状态</param>
        public int UpdateCheckStateBumfEngine(string sendDocId, string strCheckState)
        {
            dal.BeginTransaction();
            Tracer.Debug("引擎开始更新公司发文：公司发文id：" + sendDocId + " 审核状态：" + strCheckState);
            try
            {
                string strMsg = "";//string.Empty;
                var Master = GetSendDocInfoById(sendDocId);
                Master.CHECKSTATE = strCheckState;

                if (string.IsNullOrEmpty(strMsg))//预算没有错误才执行改变表单状态的操作
                {
                    int i = Update(Master);
                    if (i > 0)
                    {
                        dal.CommitTransaction();
                        Tracer.Debug("引擎更新公司发文状态成功：公司发文id：" + sendDocId + " 审核状态：" + strCheckState);
                        //add by luojie 先添加以查看是否有重现更改状态不成功的情况，若影响性能可删除
                        var nowEnt=GetSendDocInfoById(sendDocId);
                        string NowCheckState = nowEnt.CHECKSTATE;

                        if (Master.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                        {
                            //开始生成一个发布单
                            AddSendDocRecord(nowEnt);
                        }
                        else
                        {
                            Tracer.Debug("引擎状态更新完成：" + System.DateTime.Now.ToString());
                        }
                    }
                    else
                    {
                        Tracer.Debug("引擎更新公司发文状态失败：公司发文id：" + sendDocId + " 审核状态：" + strCheckState);
                        dal.RollbackTransaction();
                    }
                }
                else
                {
                    Tracer.Debug("引擎更新公司发文状态失败：" + sendDocId + System.DateTime.Now.ToString() + " " + "预算操作失败。");
                    throw new Exception(strMsg);
                }
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("引擎更新公司发文状态失败：" + sendDocId + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw ex;
            }
            return 1;
        }

        /// <summary>
        /// 审核通过后添加我的单据，以提醒发布
        /// </summary>
        /// <param name="entDoc"></param>
        /// <returns></returns>
        private int AddSendDocRecord(T_OA_SENDDOC entDoc)
        {
            try
            {
                Tracer.Debug("公司发文 开始生成待办以提醒发布");
                using (BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient ewClient
                    = new BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient())
                {
                    string strXML = EmployeeSurveyAppBll.ObjListToXml(entDoc, "OA");
                    BLLCommonServices.EngineConfigWS.CustomUserMsg[] userMsg=new BLLCommonServices.EngineConfigWS.CustomUserMsg[1];
                    userMsg[0]=new BLLCommonServices.EngineConfigWS.CustomUserMsg()
                    {
                        FormID=entDoc.SENDDOCID,
                        UserID=entDoc.OWNERID
                    };
                    ewClient.ApplicationEngineTrigger(userMsg, "OA", "T_OA_SENDDOC",entDoc.OWNERCOMPANYID, strXML, BLLCommonServices.EngineConfigWS.MsgType.Task);
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("BumfManagementBll-AddSendDocRecord:" + ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// 关闭公司发文审核通过后的待办
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="receiveUser"></param>
        public static void CloseDotask(string docId,string receiveUser)
        {
            Tracer.Debug("公司发文 开始关闭用于提醒发布的待办");
            using (BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient ewClient
                = new BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient())
            {
                string[] senddocArray = new string[1];
                senddocArray[0] = docId;
                //ewClient.CloseDoTask(senddocArray, "T_OA_SENDDOC", receiveUser);
                ewClient.TaskDeleteALL("OA", docId, receiveUser);
            }
        }

        private string SetXmlData(string sendDocID)
        {
            StringBuilder xmlData = new StringBuilder();
            if (!string.IsNullOrEmpty(sendDocID))
            {
                xmlData.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                xmlData.Append("<System>");
                xmlData.Append("<AssemblyName>SMT.SaaS.OA.UI</AssemblyName>"
                    + "<PublicClass>SMT.SaaS.OA.UI.Utility</PublicClass>"
                    + "<ProcessName>CreateFormFromEngine</ProcessName>"
                    + "<PageParameter>SMT.SaaS.OA.UI.UserControls.AddDistrbuteForm</PageParameter>"
                    + "<ApplicationOrder>" + sendDocID + "</ApplicationOrder>"
                    + "<FormTypes>Edit</FormTypes></System>");
            }
            return xmlData.ToString() ;
        }
    }
    #endregion    

    #region 公文发布
    //公司发文
    public class BumfCompanyDocDistrbuteManagementBll : BaseBll<T_OA_DISTRIBUTEUSER>
    {
        /// <summary>
        /// 添加公文发布
        /// </summary>
        /// <param name="DistrbuteObj">发布实体</param>
        /// <returns>成功为  真  失败为假</returns>
        public bool AddDocDistrbuteInfo(T_OA_DISTRIBUTEUSER DistrbuteObj)
        {
            try
            {
                int i = dal.Add(DistrbuteObj);
                if (i == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-AddDocDistrbuteInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //批量添加公文发布
        /// <summary>
        /// 批量发布公文
        /// </summary>
        /// <param name="DistrbuteList"></param>
        /// <returns></returns>
        public bool BatchAddDocDistrbuteInfo(T_OA_DISTRIBUTEUSER[] DistrbuteList)
        {
            try
            {
                string StrReturn = "";
                if (DistrbuteList.Count() > 0)
                {
                    
                    foreach (T_OA_DISTRIBUTEUSER obj in DistrbuteList)
                    {
                        var tempEnt = dal.GetObjects().FirstOrDefault(s => s.FORMID == obj.FORMID && s.MODELNAME == obj.MODELNAME
                                           && s.VIEWER == obj.VIEWER && s.VIEWTYPE == obj.VIEWTYPE && s.CREATEUSERID == obj.CREATEUSERID);
                        if (tempEnt != null)
                        {
                            //StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                            continue;
                        }
                        else
                        {
                            //dal.DataContext.AddObject("T_OA_DISTRIBUTEUSER", obj);
                            dal.AddToContext(obj);
                            
                        }

                        //暂不考虑  会使用数据引擎来处理

                        //StrFormID = obj.FORMID;
                        //T_OA_SENDDOC SenddocT = new T_OA_SENDDOC();
                        //BumfCompanySendDocManagementBll aa = new BumfCompanySendDocManagementBll();
                        //SenddocT=aa.GetSendDocInfoById(StrFormID);


                        //if (SenddocT.ISSAVE == "1") //已归档
                        //{
                        //    //using (string aaaa)
                        //    //{
                        //    //    aaaa.Clone();
                        //    //}
                        //}
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        
                        return true;
                    }
                        
                    


                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchAddDocDistrbuteInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }


        //批量添加公文发布
        /// <summary>
        /// 批量发布公文，并发送RTX、邮件、消息提醒
        /// </summary>
        /// <param name="DistrbuteList"></param>
        /// <param name="employeeids"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public bool BatchAddDocDistrbuteInfoOnlyDoc(T_OA_DISTRIBUTEUSER[] DistrbuteList, List<string> employeeids, T_OA_SENDDOC doc)
        {
            try
            {
                string StrReturn = "";
                if (DistrbuteList.Count() > 0)
                {
                    int IntK = 0;
                    foreach (T_OA_DISTRIBUTEUSER obj in DistrbuteList)
                    {
                        IntK++;
                        var tempEnt = dal.GetObjects().FirstOrDefault(s => s.FORMID == obj.FORMID && s.MODELNAME == obj.MODELNAME
                                           && s.VIEWER == obj.VIEWER && s.VIEWTYPE == obj.VIEWTYPE && s.CREATEUSERID == obj.CREATEUSERID);
                        if (tempEnt != null)
                        {
                            //StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                            continue;
                        }
                        else
                        {
                            //dal.DataContext.AddObject("T_OA_DISTRIBUTEUSER", obj);
                            dal.AddToContext(obj);
                            Tracer.Debug("发布公司发文" + System.DateTime.Now.ToString() + "公文ID "+doc.SENDDOCID+"公文标题：" + doc.SENDDOCTITLE +"组织为："+obj.VIEWER+" # " +obj.VIEWTYPE);
                        }
                        Tracer.Debug("发布公司发文公文标题：" + doc.SENDDOCTITLE+"组织架构为"+ IntK.ToString());
                        //暂不考虑  会使用数据引擎来处理

                        //StrFormID = obj.FORMID;
                        //T_OA_SENDDOC SenddocT = new T_OA_SENDDOC();
                        //BumfCompanySendDocManagementBll aa = new BumfCompanySendDocManagementBll();
                        //SenddocT=aa.GetSendDocInfoById(StrFormID);


                        //if (SenddocT.ISSAVE == "1") //已归档
                        //{
                        //    //using (string aaaa)
                        //    //{
                        //    //    aaaa.Clone();
                        //    //}
                        //}
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient EngineClient = new SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient();
                        SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[] List = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[employeeids.Count];
                        string submitName = string.Empty;
                        submitName = doc.SENDDOCTITLE;
                        if (employeeids.Count > 0)
                        {
                            
                            for (int k = 0; k < employeeids.Count; k++)
                            {
                                SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg userMsg = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg();
                                userMsg.FormID = doc.SENDDOCID;
                                userMsg.UserID = employeeids[k];                                
                                List[k] = userMsg;
                                string bb = Utility.ObjListToXml(doc, "OA", submitName);                                
                            }
                            EngineClient.ApplicationMsgTrigger(List, "OA", "T_OA_SENDDOC", Utility.ObjListToXml(doc, "OA", submitName), SMT.SaaS.BLLCommonServices.EngineConfigWS.MsgType.Msg);
                            Tracer.Debug("发布公司发文公文标题：" + doc.SENDDOCTITLE + "共发布了" + employeeids.Count.ToString() +" 人");
                        }
                        BumfCompanySendDocManagementBll docbll = new BumfCompanySendDocManagementBll();
                        string result = "";
                        doc.ISDISTRIBUTE = "1";
                        bool Isbool= docbll.UpdateSendDocInfo(doc,ref result);
                        return Isbool;
                    }




                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchAddDocDistrbuteInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }


        //批量添加公文发布
        /// <summary>
        /// 批量发布公文，并发送RTX、邮件、消息提醒
        /// </summary>
        /// <param name="DistrbuteList"></param>
        /// <param name="employeeids"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public bool BatchAddDocDistrbuteInfoOnlyDocForNew(List<T_OA_DISTRIBUTEUSER> DistrbuteList, List<string> LstCompanyIDs, List<string> LstDepartmentIDs, List<string> LstPostIDs, List<string> employeeids, T_OA_SENDDOC doc)
        {
            bool Isbool = false;
            dal.BeginTransaction();
            try
            {
                string StrReturn = "";
                if (DistrbuteList.Count() > 0)
                {
                    int IntK = 0;
                    //发布公文的时候关闭待办
                    BumfCompanySendDocManagementBll.CloseDotask(doc.SENDDOCID, doc.OWNERID);
                    foreach (T_OA_DISTRIBUTEUSER obj in DistrbuteList)
                    {
                        IntK++;
                        var tempEnt = dal.GetObjects().FirstOrDefault(s => s.FORMID == obj.FORMID && s.MODELNAME == obj.MODELNAME
                                           && s.VIEWER == obj.VIEWER && s.VIEWTYPE == obj.VIEWTYPE && s.CREATEUSERID == obj.CREATEUSERID);
                        if (tempEnt != null)
                        {
                            //StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
                            continue;
                        }
                        else
                        {
                            //dal.DataContext.AddObject("T_OA_DISTRIBUTEUSER", obj);
                            dal.AddToContext(obj);
                            Tracer.Debug("发布公司发文" + System.DateTime.Now.ToString() + "公文ID " + doc.SENDDOCID + "公文标题：" + doc.SENDDOCTITLE + "组织为：" + obj.VIEWER + " # " + obj.VIEWTYPE);
                        }

                    }
                    Tracer.Debug("发布公司发文公文标题：" + doc.SENDDOCTITLE + "共添加记录数为：" + IntK.ToString());
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        BumfCompanySendDocManagementBll docbll = new BumfCompanySendDocManagementBll();
                        string result = "";
                        doc.ISDISTRIBUTE = "1";
                        Isbool = docbll.UpdateSendDocInfoForDistrbute(doc, ref result);
                        if (!Isbool)
                        {
                            SMT.Foundation.Log.Tracer.Debug("发布完更新公文时出错");
                            dal.RollbackTransaction();
                        }
                        else
                        {
                            dal.CommitTransaction();
                        }
                        //发布邮件或RTX放置后期处理，并不影响程序更新
                        SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient PersonnelClient = new BLLCommonServices.PersonnelWS.PersonnelServiceClient();

                        string[] ListEmployees = PersonnelClient.GetEmployeeIDsWithParas(LstCompanyIDs.ToArray(), false, LstDepartmentIDs.ToArray(), false, LstPostIDs.ToArray());

                        ListEmployees = ListEmployees.Distinct().ToArray();
                        SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient EngineClient = new SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient();

                        SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[] List = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[ListEmployees.Length];
                        string submitName = string.Empty;
                        submitName = doc.SENDDOCTITLE;
                        int SendCount = 0;

                        
                        if (ListEmployees.Length > 0)
                        {

                            if (ListEmployees.Length > 0)
                            {
                                if (ListEmployees.Length > 200)
                                {
                                    int IntForTimes = ListEmployees.Length / 200 + (ListEmployees.Length % 200 == 0 ? 0 : 1);//200的倍数
                                    for (int g = 0; g < IntForTimes; g++)
                                    {
                                        SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[] List1 = null;
                                        if (g == IntForTimes - 1 && ListEmployees.Length % 200 != 0)
                                        {
                                            List1 = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[ListEmployees.Length % 200];
                                        }
                                        else
                                        {
                                            List1 = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[200];
                                        }
                                        
                                        for (int k = g* 200; k < ((g+1) * 200); k++)
                                        {
                                            SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg userMsg = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg();
                                            userMsg.FormID = doc.SENDDOCID;
                                            if (k < ListEmployees.Length)
                                            {
                                                userMsg.UserID = ListEmployees[k];
                                                List1[k % 200] = userMsg;
                                                string bb = Utility.ObjListToXml(doc, "OA", submitName);
                                                SMT.Foundation.Log.Tracer.Debug("员工ID:"+ userMsg.UserID);
                                                
                                            }
                                            else
                                            {
                                                break;
                                            }
                                            
                                        }
                                        EngineClient.ApplicationMsgTrigger(List1, "OA", "T_OA_SENDDOC", Utility.ObjListToXml(doc, "OA", submitName), SMT.SaaS.BLLCommonServices.EngineConfigWS.MsgType.Msg);
                                        
                                        

                                    }

                                }
                                else
                                {
                                    //不足200的按员工数发
                                    for (int k = 0; k < ListEmployees.Length; k++)
                                    {
                                        SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg userMsg = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg();
                                        userMsg.FormID = doc.SENDDOCID;
                                        userMsg.UserID = ListEmployees[k];
                                        List[k] = userMsg;
                                        string bb = Utility.ObjListToXml(doc, "OA", submitName);
                                        SendCount = SendCount + 1;                                        
                                    }
                                    EngineClient.ApplicationMsgTrigger(List, "OA", "T_OA_SENDDOC", Utility.ObjListToXml(doc, "OA", submitName), SMT.SaaS.BLLCommonServices.EngineConfigWS.MsgType.Msg);
                                    Tracer.Debug("发布公司发文公文标题：" + doc.SENDDOCTITLE + "共发布了" + SendCount.ToString() + " 人");
                                }
                                
                                
                                    
                            }
                            
                            if (employeeids != null)
                            {
                                SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[] List1 = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[employeeids.Count()];
                                for (int k = 0; k < employeeids.Count; k++)
                                {
                                    SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg userMsg = new SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg();
                                    userMsg.FormID = doc.SENDDOCID;
                                    userMsg.UserID = employeeids[k];
                                    List1[k] = userMsg;
                                    string bb = Utility.ObjListToXml(doc, "OA", submitName);
                                    SendCount = SendCount + 1;
                                }
                                //Tracer.Debug("发布公司发文公文标题：" + doc.SENDDOCTITLE + "共发布了" + employeeids.Count.ToString() + " 人");
                                EngineClient.ApplicationMsgTrigger(List1, "OA", "T_OA_SENDDOC", Utility.ObjListToXml(doc, "OA", submitName), SMT.SaaS.BLLCommonServices.EngineConfigWS.MsgType.Msg);
                            }


                        }
                        
                    }
                    else
                    {
                        SMT.Foundation.Log.Tracer.Debug("保存发布对象出现错误,数量为：" + DistrbuteList.Count().ToString());
                    }

                }
                else
                {
                    dal.RollbackTransaction();
                    SMT.Foundation.Log.Tracer.Debug("发布对象为空");
                }
                //return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchAddDocDistrbuteInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                dal.RollbackTransaction();
                //Tracer.Debug("公司发文BumfManagementBll-BatchAddDocDistrbuteInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //return false;


                

            }
            return Isbool;
        }

        
        /// <summary>
        /// 更改发布信息
        /// </summary>
        /// <param name="DistrbuteObj"></param>
        /// <returns></returns>
        public bool UpdateDocDistrbuteInfo(T_OA_DISTRIBUTEUSER DistrbuteObj)
        {

            try
            {
                var entity = from ent in dal.GetTable()
                             where ent.DISTRIBUTEUSERID == DistrbuteObj.DISTRIBUTEUSERID
                             select ent;

                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();

                    entitys.MODELNAME = DistrbuteObj.MODELNAME;
                    entitys.FORMID = DistrbuteObj.FORMID;
                    entitys.VIEWTYPE = DistrbuteObj.VIEWTYPE;
                    entitys.VIEWER = DistrbuteObj.VIEWER;

                    entitys.OWNERCOMPANYID = DistrbuteObj.OWNERCOMPANYID;
                    entitys.OWNERDEPARTMENTID = DistrbuteObj.OWNERDEPARTMENTID;
                    entitys.OWNERID = DistrbuteObj.OWNERID;
                    entitys.OWNERNAME = DistrbuteObj.OWNERNAME;
                    entitys.OWNERPOSTID = DistrbuteObj.OWNERPOSTID;
                    entitys.CREATEUSERID = DistrbuteObj.CREATEUSERID;
                    
                    entitys.CREATECOMPANYID = DistrbuteObj.CREATECOMPANYID;
                    entitys.CREATEDATE = DistrbuteObj.CREATEDATE;
                    entitys.CREATEDEPARTMENTID = DistrbuteObj.CREATEDEPARTMENTID;
                    entitys.CREATEPOSTID = DistrbuteObj.CREATEPOSTID;                    
                    entitys.UPDATEDATE = DistrbuteObj.UPDATEDATE;
                    entitys.UPDATEUSERID = DistrbuteObj.UPDATEUSERID;


                    if (dal.Update(entitys) >0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdateDocDistrbuteInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }
        /// <summary>
        /// 修改某一表单的 发布对象  2010-7-30 liujx
        /// </summary>
        /// <param name="DistrbuteObj">发布对象集合</param>
        /// <param name="EntityID">表单ID</param>
        /// <returns></returns>
        public bool UpdateDocDistrbuteInfoByBatch(List<T_OA_DISTRIBUTEUSER> DistrbuteObj,string EntityID)
        {

            try
            {
                
                //先删除distributelist
                var entdis = dal.GetObjects().Where(s => s.FORMID == EntityID);
                
                if (entdis != null)
                {
                    foreach (var h in entdis)
                    {
                        //DataContext.DeleteObject(h);                        
                        dal.DeleteFromContext(h);
                    }
                    
                }
                //再插入distributelist
                foreach (var h in DistrbuteObj)
                {
                    //DataContext.AddObject("T_OA_DISTRIBUTEUSER", h);
                    dal.AddToContext(h);
                    

                }
                int i = dal.SaveContextChanges();
                if (i > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-UpdateDocDistrbuteInfoByBatch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //批量删除发文信息
        /// <summary>
        /// 批量删除发布信息
        /// </summary>
        /// <param name="ArrOrderMealIDs"></param>
        /// <returns></returns>
        public bool BatchDeleteDocDistrbuteInfos(string[] ArrOrderMealIDs)
        {
            try
            {
                var entitys = from ent in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                              where ArrOrderMealIDs.Contains(ent.DISTRIBUTEUSERID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        dal.Delete(obj);
                    }

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-BatchDeleteDocDistrbuteInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        

        /// <summary>
        /// 根据发布ID获取发布信息
        /// </summary>
        /// <param name="DistrbuteId"></param>
        /// <returns></returns>
        public T_OA_DISTRIBUTEUSER GetDocDistrbuteInfoById(string DistrbuteId)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.DISTRIBUTEUSERID == DistrbuteId
                        orderby ent.DISTRIBUTEUSERID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocDistrbuteInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        //获取所有的文档发布信息
        public List<T_OA_DISTRIBUTEUSER> GetDocDistrbuteInfos(string FormID)
        {
            try
            {
                var query = from p in dal.GetObjects()
                            where p.FORMID == FormID
                            orderby p.CREATEDATE descending
                            select p;

                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_DISTRIBUTEUSER>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocDistrbuteInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        //返回发布对象为员工类型的员工ID集合
        public List<string> GetDocDistrbuteUserInfosByFormID(string FormID)
        {
            try
            {
                var query = from p in dal.GetObjects()
                            where p.FORMID == FormID && p.VIEWTYPE =="3"
                            orderby p.CREATEDATE descending
                            select p.VIEWER;

                if (query.Count() > 0)
                {
                    return query.ToList<string>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("公司发文BumfManagementBll-GetDocDistrbuteUserInfosByFormID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        
                
    }

    #endregion

}
