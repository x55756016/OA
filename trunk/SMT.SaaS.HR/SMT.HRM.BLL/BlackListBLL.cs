using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class BlackListBLL : BaseBll<T_HR_BLACKLIST>
    {
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
        public IQueryable<T_HR_BLACKLIST> GetBlackListPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_BLACKLIST");
            IQueryable<T_HR_BLACKLIST> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_BLACKLIST>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 新增黑名单
        /// </summary>
        /// <param name="entity"></param>
        public string BlackListAdd(T_HR_BLACKLIST entity)
        {
            string strMsg = string.Empty;
            try
            {

                var ents = from a in dal.GetObjects()
                           where a.IDCARDNUMBER == entity.IDCARDNUMBER
                           select a;
                if (ents.Count() <= 0)
                {
                   
                    if (dal.Add(entity) > 0)
                    {
                        strMsg = "SAVESUCCESSED";
                    }
                }
                else
                {
                    strMsg = "IDCARDNUMBERREPETITION";
                }
            }
            catch (Exception ex)
            {
               // strMsg = ex.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " BlackListAdd:" + ex.Message);
            }
            return strMsg;
        }
        /// <summary>
        /// 更新黑名单
        /// </summary>
        /// <param name="entity">黑名单实体</param>
        public void BlackListUpdate(T_HR_BLACKLIST entity,ref string strMsg)
        {
            
            try
            {
                var tmps = from b in dal.GetObjects()
                           where b.IDCARDNUMBER == entity.IDCARDNUMBER && b.BLACKLISTID != entity.BLACKLISTID
                           select b;
                if(tmps.Count()>0)
                {
                  // throw new Exception("IDCARDNUMBERREPETITION");
                    strMsg = "IDCARDNUMBERREPETITION";
                    return;
                }
                //var ents = from a in dal.GetObjects()
                //           where a.BLACKLISTID == entity.BLACKLISTID
                //           select a;
                //if (ents.Count() > 0)
                //{
                //    var ent = ents.FirstOrDefault();
                //    ent.IDCARDNUMBER = entity.IDCARDNUMBER;
                //    ent.NAME = entity.NAME;
                //    ent.REASON = entity.REASON;
                //    ent.BEGINDATE = entity.BEGINDATE;
                //    ent.UPDATEDATE = entity.UPDATEDATE;
                //    ent.UPDATEUSERID = entity.UPDATEUSERID;
                //    dal.Update(ent);
                //}
                entity.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_BLACKLIST", "BLACKLISTID", entity.BLACKLISTID);
                dal.Update(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " BlackListAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 根据黑名单ID查询实体
        /// </summary>
        /// <param name="blackListID">黑名单ID</param>
        /// <returns>返回黑名单实体</returns>
        public T_HR_BLACKLIST GetBlackListByID(string blackListID)
        {
            var ents = from a in dal.GetObjects()
                       where a.BLACKLISTID == blackListID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据黑名单ID查询实体
        /// </summary>
        /// <param name="blackListID">黑名单ID</param>
        /// <returns>返回黑名单实体</returns>
        public bool CheckBlackListByName(string name)
        {
            var ents = from a in dal.GetObjects()
                       where a.NAME == name
                       select a;
            return ents.Count() > 0 ? true : false;
        }
        /// <summary>
        /// 删除黑名单记录，可同时删除多行记录
        /// </summary>
        /// <param name="blackLists">黑名单ID数组</param>
        /// <returns></returns>
        public int BlackListDelete(string[] blackLists)
        {
          
            try
            {
               
                foreach (string id in blackLists)
                {
                    var ents = from e in dal.GetObjects()
                               where e.BLACKLISTID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                       
                    }
                }
                return dal.SaveContextChanges();
            }
            catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " BlackListAdd:" + ex.Message);
                throw ex;
            }
           // return DataContext.SaveChanges();
           
        }
        /// <summary>
        /// 根据身份证号码查询
        /// </summary>
        /// <param name="strIDCard">身份证号码</param>
        /// <returns>如果有则返回实体，否则就返回NULL</returns>
        public T_HR_BLACKLIST GetBlackListByIDCard(string strIDCard)
        {
            return dal.GetObjects().FirstOrDefault(s => s.IDCARDNUMBER == strIDCard);
        }
    }
}
