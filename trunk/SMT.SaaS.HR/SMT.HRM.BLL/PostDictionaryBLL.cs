using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class PostDictionaryBLL : BaseBll<T_HR_POSTDICTIONARY>, IOperate
    {
        /// <summary>
        /// 获取所有的部门子典
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_POSTDICTIONARY> GetPostDictionaryAll()
        {
            var ent = from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY")
                      select a;
            return ent;
        }

        /// <summary>
        /// 根据部门子典ID获取岗位字典
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_POSTDICTIONARY> GetPostDictionaryByDepartmentDictionayID(string departmentDictioanryID)
        {
            var ent = from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY")
                      where a.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID == departmentDictioanryID
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
        public IQueryable<T_HR_POSTDICTIONARY> PostDictionaryPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string strCheckstate)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_POSTDICTIONARY");
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
                SetFilterWithflow("POSTDICTIONARYID", "T_HR_POSTDICTIONARY", userID, ref strCheckstate, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }
            IQueryable<T_HR_POSTDICTIONARY> ents = dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_POSTDICTIONARY>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 删除岗位字典信息
        /// </summary>
        /// <param name="id">岗位字典ID</param>
        /// <returns></returns>
        public int PostDictionaryDelete(string[] ids, ref string strMsg)
        {
            try
            {
                foreach (var id in ids)
                {
                    var entitys = dal.GetObjects().FirstOrDefault(s => s.POSTDICTIONARYID == id);
                    var post = dal.GetObjects<T_HR_POST>().Where(p => p.T_HR_POSTDICTIONARY.POSTDICTIONARYID == id);
                    if (post.Count() > 0)
                    {
                        strMsg = "POSTDICTIOANRYUSED";
                        return 0;
                    }
                    else
                    {
                        var posthis = dal.GetObjects<T_HR_POSTHISTORY>().Where(h => h.T_HR_POSTDICTIONARY.POSTDICTIONARYID == id);
                        if (posthis.Count() > 0)
                        {
                            strMsg = "POSTDICTIOANRYUSED";
                            return 0;
                        }
                    }
                    if (entitys != null)
                    {
                        dal.DeleteFromContext(entitys);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                //  throw (ex);
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " PostDictionaryDelete:" + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 根据岗位字典ID获取岗位字典信息
        /// </summary>
        /// <param name="id">岗位字典ID</param>
        /// <returns>返回岗位字典列表</returns>
        public T_HR_POSTDICTIONARY GetPostDictionaryById(string id)
        {
            var q = from ent in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY")
                    where ent.POSTDICTIONARYID == id
                    select ent;
            return q.Count() > 0 ? q.FirstOrDefault() : null;
        }

        /// <summary>
        /// 添加岗位字典信息
        /// </summary>
        /// <param name="entity">岗位字典实体</param>
        public void PostDictionaryAdd(T_HR_POSTDICTIONARY entity, ref string strMsg)
        {
            try
            {
                var ent = dal.GetObjects().FirstOrDefault(s => (s.POSTCODE == entity.POSTCODE
                    || s.POSTNAME == entity.POSTNAME) && s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID == entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                if (ent != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                ent = new T_HR_POSTDICTIONARY();
                Utility.CloneEntity<T_HR_POSTDICTIONARY>(entity, ent);
                if (entity.T_HR_DEPARTMENTDICTIONARY != null)
                {
                    ent.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                }
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " PostDictionaryAdd:" + ex.Message);
            }
        }

        /// <summary>
        /// 修改岗位字典信息
        /// </summary>
        /// <param name="entity">岗位字典实体</param>
        public void PostDictionaryUpdate(T_HR_POSTDICTIONARY entity, ref string strMsg)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => (s.POSTCODE == entity.POSTCODE
                || s.POSTNAME == entity.POSTNAME) && s.POSTDICTIONARYID != entity.POSTDICTIONARYID && s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID == entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                if (temp != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                #region
                //var users = from ent in dal.GetObjects()
                //            where ent.POSTDICTIONARYID == entity.POSTDICTIONARYID
                //            select ent;

                //if (users.Count() > 0)
                //{
                //    var user = users.FirstOrDefault();
                //    Utility.CloneEntity(entity, user);
                //    if (entity.T_HR_DEPARTMENTDICTIONARY != null)
                //    {
                //        user.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                //                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                //    }
                //    dal.Update(user);
                //}
                #endregion

                entity.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.POSTDICTIONARYID);
                if (entity.T_HR_DEPARTMENTDICTIONARY != null)
                {
                    entity.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                    entity.T_HR_DEPARTMENTDICTIONARY.EntityKey =
                          new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                }
                int i = dal.Update(entity);
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PostDictionaryUpdate CheckState:" + entity.CHECKSTATE + ",UpdateResult:" + i.ToString());
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " PostDictionaryUpdate:" + ex.Message);
                throw (ex);
            }
        }

         public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var postdic = (from c in dal.GetObjects<T_HR_POSTDICTIONARY>()
                            where c.POSTDICTIONARYID == EntityKeyValue
                            select c).FirstOrDefault();
                if (postdic != null)
                {
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        postdic.EDITSTATE = ((int)EditStates.Actived).ToString();
                    }
                    if (CheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        postdic.EDITSTATE = ((int)EditStates.UnActived).ToString();
                    }
                    postdic.CHECKSTATE = CheckState;
                    postdic.UPDATEDATE = System.DateTime.Now;
                    i = dal.Update(postdic);
                    if (i==1)
                    {
                        SMT.Foundation.Log.Tracer.Debug("更新岗位字典审核状态成功："+postdic.POSTDICTIONARYID);
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
