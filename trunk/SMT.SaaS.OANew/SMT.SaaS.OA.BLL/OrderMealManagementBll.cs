using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    #region 订餐管理
    public class OrderMealManagementBll:BaseBll<T_OA_ORDERMEAL>
    {
        //private OrderMealManagementDal MealDal = new OrderMealManagementDal();
        public string AddOrderMeal(T_OA_ORDERMEAL MealObj)
        {
            //try
            //{

            //    string StrReturn = "";
            //    var tempEnt = DataContext.T_OA_ORDERMEAL.FirstOrDefault(s => s.ORDERMEALTITLE == MealObj.ORDERMEALTITLE
            //        && s.CONTENT == MealObj.CONTENT && s.TEL == MealObj.TEL && s.CREATEDATE == MealObj.CREATEDATE);
            //    if (tempEnt != null)
            //    {
            //        StrReturn = "REPETITION"; //{0}已存在，保存失败！                    
            //    }

            //    int i = dal.Add(MealObj);


            //    if (!(i > 0))
            //    {
            //        StrReturn = "SAVEFAILED";//保存失败
            //    }
            //    return StrReturn;

                                
            //}
            //catch (Exception ex)
            //{
            //    throw (ex);
            //}
            return "";
        }

        public bool UpdateOrderMeal(T_OA_ORDERMEAL MealObj)
        {

            try
            {
                var entity = from ent in dal.GetTable()
                             where ent.ORDERMEALID == MealObj.ORDERMEALID
                             select ent;

                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();
                    Utility.CloneEntity(MealObj,entitys);
                    //entitys.ORDERMEALTITLE = MealObj.ORDERMEALTITLE;
                    //entitys.CONTENT = MealObj.CONTENT;
                    //entitys.REMARK = MealObj.REMARK;
                    //entitys.ORDERMEALFLAG = MealObj.ORDERMEALFLAG;

                    //entitys.OWNERCOMPANYID = MealObj.OWNERCOMPANYID;
                    //entitys.OWNERDEPARTMENTID = MealObj.OWNERDEPARTMENTID;
                    //entitys.OWNERID = MealObj.OWNERID;
                    //entitys.OWNERNAME = MealObj.OWNERNAME;
                    //entitys.OWNERPOSTID = MealObj.OWNERPOSTID;

                    //entitys.CREATEUSERID = MealObj.CREATEUSERID;
                    //entitys.CREATEUSERNAME = MealObj.CREATEUSERNAME;
                    //entitys.CREATECOMPANYID = MealObj.CREATECOMPANYID;
                    //entitys.CREATEDEPARTMENTID = MealObj.CREATEDEPARTMENTID;
                    //entitys.CREATEPOSTID = MealObj.CREATEPOSTID;                    
                    //entitys.CREATEDATE = MealObj.CREATEDATE;
                                       
                    //entitys.UPDATEUSERID = MealObj.UPDATEUSERID;
                    //entitys.UPDATEUSERNAME = MealObj.UPDATEUSERNAME;
                    //entitys.UPDATEDATE = MealObj.UPDATEDATE;
                    
                    
                    if (dal.Update(entitys) >0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        //批量删除订餐信息
        public bool BatchDeleteOrderMealInfos(string[] ArrOrderMealIDs)
        {
            //try
            //{

            //    foreach (string id in ArrOrderMealIDs)
            //    {
            //        var ents = from e in DataContext.T_OA_ORDERMEAL
            //                   where e.ORDERMEALID == id
            //                   select e;
            //        var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

            //        if (ent != null)
            //        {
            //            DataContext.DeleteObject(ent);
            //        }
            //    }

            //    int i = DataContext.SaveChanges();

            //    return i > 0 ? true : false;
                
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //    throw (ex);
            //}
            return true;
        }

        //判断是否有存在的订餐信息
        
        public bool GetOrderMealInfoByAdd(string StrTitle, string StrDepart, string StrCreatUser, DateTime enddt)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_ORDERMEAL>()
                        //where ent.ORDERMEALTITLE == StrTitle && ent.DEPARTNAME == StrDepart && ent.CREATEDATE == enddt && ent.RESERVATIONER == StrCreatUser 
                        where ent.ORDERMEALTITLE == StrTitle && ent.CREATEDATE == enddt
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
                return false;
                throw (ex);
            }
            //return null;
        }


        public T_OA_ORDERMEAL GetOrderMealInfoById(string OrderMealId)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_ORDERMEAL>()
                        where ent.ORDERMEALID == OrderMealId
                        orderby ent.ORDERMEALID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }

        //获取所有的订餐信息
        //public List<T_OA_ORDERMEAL> GetOrderMealInfos(string StrUserID,string StrState)
        //{
        //    var query = from p in MealDal.GetTable()

        //                select p;
        //    //if (!string.IsNullOrEmpty(StrUserID))
        //    //{
        //    //    query = query.Where(s => s.RESERVATIONER == StrUserID);
        //    //}
        //    if (!string.IsNullOrEmpty(StrState))
        //    {
        //        query = query.Where(s => s.ORDERMEALFLAG == StrState);
        //    }
            
        //    query = query.OrderByDescending(s => s.CREATEDATE);
        //    if (query.Count() > 0)
        //    {

        //        return query.ToList<T_OA_ORDERMEAL>();
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}

        public IQueryable<T_OA_ORDERMEAL> GetOrderMealInfos(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            //try
            //{
            //    var ents = from ent in DataContext.T_OA_ORDERMEAL

            //               select ent;
            //    List<object> queryParas = new List<object>();
            //    queryParas.AddRange(paras);
            //    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_ORDERMEAL");
            //    if (queryParas.Count > 0)
            //    {
            //        if (!string.IsNullOrEmpty(filterString))
            //        {
            //            ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            //        }
            //    }
            //    ents = ents.OrderBy(sort);
            //    ents = Utility.Pager<T_OA_ORDERMEAL>(ents, pageIndex, pageSize, ref pageCount);
            //    return ents;

            //}
            //catch (Exception ex)
            //{
            //    throw (ex);
            //}
            return null;

        }




        /// <summary>
        /// 获取某一员工的订餐信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<T_OA_ORDERMEAL> GetOrderMealInfosByMemberID(string userID)
        {
            try
            {
                var query = from p in dal.GetObjects<T_OA_ORDERMEAL>()

                            select p;
                //if (!string.IsNullOrEmpty(userID))
                //{
                //    query = query.Where(s => s.RESERVATIONER == userID);
                //}

                query = query.OrderByDescending(s => s.CREATEDATE);
                if (query.Count() > 0)
                {

                    return query.ToList<T_OA_ORDERMEAL>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }



        //获取查询的订餐信息
        //public IQueryable<T_OA_ORDERMEAL> GetOrderMealInfosListByTitleContentTimeSearch(string StrTitle, string StrDepartment, DateTime DtStart, DateTime DtEnd, string StrContent, string StrCheckState)
        //{
        //    var q = from ent in MealDal.GetTable()

        //            select ent;

        //    if (!string.IsNullOrEmpty(StrCheckState))
        //    {
        //        q = q.Where(s => s.ORDERMEALFLAG == StrCheckState);
        //    }
        //    if (!string.IsNullOrEmpty(StrTitle))
        //    {
        //        q = q.Where(s => StrTitle.Contains(s.ORDERMEALTITLE));
        //    }
        //    if (!string.IsNullOrEmpty(StrContent))
        //    {
        //        q = q.Where(s => StrContent.Contains(s.CONTENT));
        //    }
            
        //    if (!string.IsNullOrEmpty(StrDepartment))
        //    {
        //        q = q.Where(s => StrDepartment.Contains(s.CREATEDEPARTMENTID));
        //    }
        //    //int aa = DtStart.CompareTo(DtEnd);
        //    if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
        //    {
        //        q = q.Where(s => DtStart < s.CREATEDATE);
        //        q = q.Where(s => DtEnd > s.CREATEDATE);
        //    }
        //    q = q.OrderByDescending(s => s.CREATEDATE);
        //    if (q.Count() > 0)
        //    {
        //        return q;
        //    }
        //    return null;
        //}

        public IQueryable<T_OA_ORDERMEAL> GetOrderMealInfosListByTitleContentTimeSearch(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            //try
            //{
            //    var ents = from ent in DataContext.T_OA_ORDERMEAL

            //               select ent;
            //    List<object> queryParas = new List<object>();
            //    queryParas.AddRange(paras);
            //    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_ORDERMEAL");
            //    if (queryParas.Count > 0)
            //    {
            //        if (!string.IsNullOrEmpty(filterString))
            //        {
            //            ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            //        }
            //    }
            //    ents = ents.OrderBy(sort);
            //    ents = Utility.Pager<T_OA_ORDERMEAL>(ents, pageIndex, pageSize, ref pageCount);
            //    return ents;

            //}
            //catch (Exception ex)
            //{
            //    throw (ex);
            //}
            return null;

        }





    }
    #endregion
}
