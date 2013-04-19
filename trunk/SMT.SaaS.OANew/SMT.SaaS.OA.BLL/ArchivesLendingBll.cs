using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;


namespace SMT.SaaS.OA.BLL
{
    public class ArchivesLendingBll  :  BaseBll<T_OA_LENDARCHIVES>
    {
        
        //private SMT_OA_EFModelContext archivesContext = new SMT_OA_EFModelContext();
        /// <summary>
        /// 获取所有可以借阅的档案信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_ARCHIVES> GetArchivesCanBorrow()
        {
            List<T_OA_ARCHIVES> list = new List<T_OA_ARCHIVES>();
            var entity = (from p in dal.GetObjects()
                         where !p.ENDDATE.HasValue
                         && (p.CHECKSTATE == ((int)CheckStates.Approving).ToString() || p.CHECKSTATE == ((int)CheckStates.Approved).ToString())                         
                         orderby p.CREATEDATE descending
                         select new {archiveID =p.T_OA_ARCHIVES.ARCHIVESID }).Distinct();
            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_LENDARCHIVES");
            foreach (var h in entity)
            {
                var entitys = from q in dal.GetObjects<T_OA_ARCHIVES>()
                              where h.archiveID != q.ARCHIVESID
                              select q;
                
                List<T_OA_ARCHIVES> tmpList = entitys.ToList();
                if (tmpList != null && tmpList.Count > 0)
                {
                    for (int i = 0; i < tmpList.Count(); i++)
                    {
                        list.Add(tmpList[i]);
                    }
                }
                   
            }
            return list;
            //return entity.Count() > 0 ? entity : null;
        }

        /// <summary>
        /// 根据条件查询所有可以借阅的档案信息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<T_OA_ARCHIVES> GetArchivesCanBorrowByCondition(string title,string type)
        {
            var ent = from p in dal.GetObjects<T_OA_ARCHIVES>()
                      where p.SOURCEFLAG == "1"
                      select p;
            if (!string.IsNullOrEmpty(title))
            {
                //ent = ent.Where(s => s.ARCHIVESTITLE.Contains(title));
                ent = ent.Where(s => title.Contains(s.ARCHIVESTITLE));
            }
            if (!string.IsNullOrEmpty(type))
            {
                ent = ent.Where(s => type.Contains(s.RECORDTYPE));
            }
            var entity = (from p in dal.GetObjects()
                          where p.ENDDATE == null
                          && p.CHECKSTATE == "1"                  
                          orderby p.CREATEDATE descending
                          select new { archiveID = p.T_OA_ARCHIVES.ARCHIVESID }).Distinct();
            var list = from c in ent.ToList()
                        where !(from o in entity.ToList()
                        select o.archiveID)
                        .Contains(c.ARCHIVESID)
                        select c;
            return list.Count() > 0 ? list.ToList() : null;
        }

        /// <summary>
        /// 获取用户借阅记录
        /// </summary>
        /// 
        /// <returns></returns>       
        public IQueryable<T_OA_LENDARCHIVES> GetArchivesLendingInfoQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState,string userID)
        {
            try
            {
                
                
                var ents = from q in dal.GetObjects().Include("T_OA_ARCHIVES")
                            where !q.ENDDATE.HasValue
                            select q;
                if (ents.Count() > 0)
                {                        
                    if (flowInfoList != null)
                    {
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.LENDARCHIVESID equals l.FormID
                                select a);
                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OALENDARCHIVES");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<T_OA_LENDARCHIVES>(ents, pageIndex, pageSize, ref pageCount);
                    return ents;
                }
                return null;
                
                
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IQueryable<T_OA_LENDARCHIVES> GetArchivesLendingInfoById(string lendingID)
        {
            
            
            var ents = from q in dal.GetObjects().Include("T_OA_ARCHIVES")
                        where q.LENDARCHIVESID == lendingID
                        select q;
            if (ents.Count() > 0)
            {
                return ents;
            }
            
            return null;
        }


        /// <summary>
        /// 新增档案借阅信息
        /// </summary>
        /// <param name="lendingArchives"></param>
        /// <returns></returns>
        public bool AddArchivesLending(T_OA_LENDARCHIVES lendingArchives)
        {
            try
            {               
                
                //lendingArchives.T_OA_ARCHIVES = dal.GetObjectByEntityKey(lendingArchives.T_OA_ARCHIVES.EntityKey) as T_OA_ARCHIVES;
                //archivesContext.AddObject("T_OA_LENDARCHIVES", lendingArchives);
                Utility.RefreshEntity(lendingArchives);    //add by zl 3.10
                return Add(lendingArchives);
                //string Record = SaveMyRecord(SendDocObj);
                
            }
            catch (Exception ex)
            {
                //return false;
                throw (ex);
            }
        }

        /// <summary>
        /// 更新档案借阅信息
        /// </summary>
        /// <param name="lendingArchives"></param>
        /// <returns></returns>
        public bool UpdateArchivesLending(T_OA_LENDARCHIVES lendingArchives)
        {           
            try
            {
                
                
                T_OA_LENDARCHIVES entity = dal.GetObjectByEntityKey(lendingArchives.EntityKey) as T_OA_LENDARCHIVES;
                entity.T_OA_ARCHIVES = dal.GetObjectByEntityKey(lendingArchives.T_OA_ARCHIVES.EntityKey) as T_OA_ARCHIVES;
                //archivesContext.ApplyPropertyChanges(lendingArchives.EntityKey.EntitySetName, lendingArchives);
                int i = dal.Update(lendingArchives);
                if (i > 0)
                {
                    return true;

                }
                return false;
            }
            catch (Exception ex)
            {
                //return result;
                throw (ex);                
            }
        }

        /// <summary>
        /// 删除档案借阅信息
        /// </summary>
        /// <param name="lendingArchivesID"></param>
        public bool DeleteArchivesLening(string[] lendingArchivesID)
        {
            try
            {                
                bool result = false;
                var entity = (from ent in dal.GetTable().ToList()
                              where lendingArchivesID.Contains(ent.LENDARCHIVESID)
                              && ent.CHECKSTATE != ((int)CheckStates.Approved).ToString() && ent.CHECKSTATE != ((int)CheckStates.Approving).ToString()
                              select ent);
                if (entity.Count() > 0)
                {
                    //var entitys = entity.FirstOrDefault();
                    //if (dal.Delete(entitys) == 1)
                    //{
                    //    result = true;
                    //}
                    foreach (var h in entity)
                    {
                        dal.DeleteFromContext(h);                        
                    }
                    int i = dal.SaveContextChanges();
                    return i > 0 ? true : false;
                }

                
                return result;
            }
            catch (Exception ex)
            {
                //return false;
                throw (ex);
            }
        }

        /// <summary>
        /// 查询档案信息是否能被更新
        /// </summary>
        /// <param name="lendingArchives"></param>
        /// <returns></returns>
        public bool IsArchivesCanUpdate(T_OA_LENDARCHIVES lendingArchives)
        {
            var entity = from q in dal.GetObjects()
                         where q.LENDARCHIVESID == lendingArchives.LENDARCHIVESID
                         && q.CHECKSTATE != "1" && q.CHECKSTATE != "2"
                         select q;
            if (entity.Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 查询档案能否被查看
        /// </summary>
        /// <param name="archivesID"></param>
        /// <returns></returns>
        public bool IsArchivesCanBrowser(string archivesID)
        {
            var entity = from q in dal.GetObjects()
                         where q.LENDARCHIVESID == archivesID && !q.ENDDATE.HasValue
                         && q.CHECKSTATE == "1"
                         select q;
            if (entity.Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据员工ID返回，该员工借的东西没有归还的记录
        /// 目前只有 档案借阅、证照借阅  0 档案借阅   1 证照借阅  20100722 liujx
        /// </summary>
        /// <param name="StrEmployeeID"></param>
        /// <returns></returns>
        public List<string> GetEmployeeNotReturnData(string StrEmployeeID)
        {
            List<string> StrList = new List<string>();
            var archives = from a in dal.GetObjects().Include("T_OA_ARCHIVES")
                           where !a.ENDDATE.HasValue && a.CHECKSTATE == "2" && a.OWNERID == StrEmployeeID
                           select a;
            var licens = from q in dal.GetObjects<T_OA_LICENSEUSER>().Include("T_OA_LICENSEMASTER")
                         where q.HASRETURN == "0" && q.CHECKSTATE =="2" && q.OWNERID == StrEmployeeID
                         select q;
            
            if (archives.Count() > 0)
            {
                foreach (var archive in archives.ToList())
                {
                    StrList.Add("0,"+ archive.T_OA_ARCHIVES.ARCHIVESTITLE);
                }
            }
            if (licens.Count() > 0)
            {
                foreach (var q in licens.ToList())
                {
                    StrList.Add("1,"+ q.T_OA_LICENSEMASTER.LICENSENAME);
                }
            }
            return StrList;
        }
    }
}
