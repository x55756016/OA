using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class DepartmentDictionaryBLL : BaseBll<T_HR_DEPARTMENTDICTIONARY>, IOperate
    {
        /// <summary>
        /// 获取所有的部门子典
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_DEPARTMENTDICTIONARY> GetDepartmentDictionaryAll()
        {
            //2012-6-26过滤了审核通过的部门字典才显示
            var ent = from a in dal.GetObjects()
                      where a.CHECKSTATE =="2"
                      select a;
            return ent;
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的公司信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_DEPARTMENTDICTIONARY> DepartmentDictionaryPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string strCheckstate)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_DEPARTMENTDICTIONARY");
            if (strCheckstate != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {

                if (strCheckstate != Convert.ToInt32(CheckStates.All).ToString())
                {
                    if (queryParas.Count() > 0)
                    {
                        filterString += " AND ";
                    }

                    filterString += "CHECKSTATE==@" + queryParas.Count().ToString();
                    queryParas.Add(strCheckstate);
                }
            }
            else
            {
                SetFilterWithflow("DEPARTMENTDICTIONARYID", "T_HR_DEPARTMENTDICTIONARY", userID, ref strCheckstate, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }
            IQueryable<T_HR_DEPARTMENTDICTIONARY> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_DEPARTMENTDICTIONARY>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 根据部门字典ID获取部门字典信息
        /// </summary>
        /// <param name="id">部门字典ID</param>
        /// <returns>返回当前部门字典ID的部门子典信息</returns>
        public T_HR_DEPARTMENTDICTIONARY GetDepartmentDictionaryById(string depid)
        {
            var q = from ent in dal.GetObjects()
                    where ent.DEPARTMENTDICTIONARYID == depid
                    select ent;
            return q.Count() > 0 ? q.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据部门字典ID删除部门字典信息
        /// </summary>
        /// <param name="id">部门字典ID</param>
        /// <returns></returns>
        public int DepartmentDictionaryDelete(string[] ids, ref string strMsg)
        {
            try
            {
                foreach (var id in ids)
                {
                    var entity = dal.GetObjects().FirstOrDefault(s => s.DEPARTMENTDICTIONARYID == id);
                    var postdic = dal.GetObjects<T_HR_POSTDICTIONARY>().Where(p => p.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID == id);
                    if (postdic.Count() > 0)
                    {
                        strMsg = "DEPARTMENTDICTIONARYUSED";
                        return 0;
                    }
                    else
                    {
                        var depart = dal.GetObjects<T_HR_DEPARTMENT>().Where(d => d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID == id);
                        if (depart.Count() > 0)
                        {
                            strMsg = "DEPARTMENTDICTIONARYUSED";
                            return 0;
                        }
                        else
                        {
                            var departhis = dal.GetObjects<T_HR_DEPARTMENTHISTORY>().Where(h => h.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID == id);
                            if (departhis.Count() > 0)
                            {
                                strMsg = "DEPARTMENTDICTIONARYUSED";
                                return 0;
                            }
                        }
                    }
                    if (entity != null)
                    {
                        // DataContext.DeleteObject(entity);
                        dal.DeleteFromContext(entity);
                    }
                }
                //return DataContext.SaveChanges();
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                // throw new Exception("DEPARTMENTDICTIONARYUSED");
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentDictionaryDelete:" + ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 添加部门字典
        /// </summary>
        /// <param name="entity"></param>
        public void DepartmentDictionaryAdd(T_HR_DEPARTMENTDICTIONARY entity, ref string strMsg)
        {
            try
            {
                var ent = dal.GetObjects().FirstOrDefault(s => (s.DEPARTMENTCODE == entity.DEPARTMENTCODE
                    || s.DEPARTMENTNAME == entity.DEPARTMENTNAME) && s.DEPARTMENTTYPE == entity.DEPARTMENTTYPE);
                if (ent != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                dal.Add(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentDictionaryAdd:" + ex.Message);
            }
        }
        /// <summary>
        /// 修改部门字典信息
        /// </summary>
        /// <param name="entity">被修改的部门字典实体</param>
        public void DepartmentDictionaryUpdate(T_HR_DEPARTMENTDICTIONARY entity, ref string strMsg)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => (s.DEPARTMENTCODE == entity.DEPARTMENTCODE
                || s.DEPARTMENTNAME == entity.DEPARTMENTNAME) && s.DEPARTMENTTYPE == entity.DEPARTMENTTYPE && s.DEPARTMENTDICTIONARYID != entity.DEPARTMENTDICTIONARYID);
                if (temp != null)
                {
                    //  throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                #region back
                //var users = from ent in dal.GetObjects()
                //            where ent.DEPARTMENTDICTIONARYID == entity.DEPARTMENTDICTIONARYID
                //            select ent;

                //if (users.Count() > 0)
                //{
                //    var user = users.FirstOrDefault();
                //    Utility.CloneEntity(entity, user);
                //    dal.Update(user);
                //}
                #endregion
                //string a = "";
                //foreach (var prop in entity.GetType().GetProperties())
                //{
                //    var attr = prop.GetCustomAttributes(typeof(System.Data.Objects.DataClasses.EdmScalarPropertyAttribute), false).FirstOrDefault()
                //        as System.Data.Objects.DataClasses.EdmScalarPropertyAttribute;
                //    if (attr != null && attr.EntityKeyProperty)
                //    {
                //        a = prop.Name;
                //        string value = prop.GetValue(entity, null).ToString();
                //        break;
                //    }
                //}
                entity.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.DEPARTMENTDICTIONARYID);
                int i = dal.Update(entity);
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "DepartmentDictionaryUpdate CheckState:" + entity.CHECKSTATE + ",UpdateResult:" + i.ToString());
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentDictionaryUpdate:" + ex.Message);
                //  throw (ex);
            }
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
                var department = (from c in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>()
                                  where c.DEPARTMENTDICTIONARYID == EntityKeyValue
                                  select c).FirstOrDefault();
                if (department != null)
                {
                    department.CHECKSTATE = CheckState;
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        if (department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            department.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                        }
                        else
                        {
                            department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                        }
                    }
                    if (CheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        if (department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                        }
                        else
                        {
                            department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        }
                    }
                    this.DepartmentDictionaryUpdate(department, ref strMsg);
                    if (string.IsNullOrEmpty(strMsg))
                    {
                        i = 1;
                    }
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
