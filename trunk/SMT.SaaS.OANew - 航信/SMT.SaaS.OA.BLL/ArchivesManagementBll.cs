using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;


namespace SMT.SaaS.OA.BLL
{
    public class ArchivesManagementBll : BaseBll<T_OA_ARCHIVES>
    {
        //private ArchivesManagementDal archivesDal = new ArchivesManagementDal();
        //private SMT_OA_EFModelContext archivesContext = new SMT_OA_EFModelContext();
        /// <summary>
        /// //获取所有的档案信息
        /// </summary>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_ARCHIVES> GetArchives() 
        {            
            var entity = from p in dal.GetTable()
                         orderby p.CREATEDATE descending
                        select p;
            return entity.Count() > 0 ? entity : null;
        }

        //获取所有可以借阅的档案信息

        /// <summary>
        /// 根据档案ID获取档案信息
        /// </summary>
        /// <param name="archivesID">档案ID</param>
        /// <returns>返回结果</returns>
        public T_OA_ARCHIVES GetArchivesById(string archivesID)
        {
            var entity = from p in dal.GetTable()
                         where p.ARCHIVESID == archivesID
                         select p;
            return entity.Count() > 0 ? entity.FirstOrDefault() : null;
        }

        /// <summary>
        /// 获取最新一条档案信息
        /// </summary>
        /// <returns>返回结果</returns>
        public T_OA_ARCHIVES GetLastArchives()
        {
            var entity = from p in dal.GetTable()
                         orderby p.CREATEDATE descending
                         select p;
            return entity.Count() > 0 ? entity.FirstOrDefault() : null;
        }


        
        /// <summary>
        /// 判断是否有存在的档案信息
        /// </summary>
        /// <param name="title">档案标题</param>
        /// <param name="createUserID">创建者</param>
        /// <returns>返回结果</returns>
        public bool IsExistArchives(string title, string createUserID)
        {
            bool IsExist = false;
            var q = from ent in dal.GetTable()
                    where ent.ARCHIVESTITLE == title && ent.OWNERCOMPANYID == createUserID
                    orderby ent.CREATEUSERID
                    select ent;
            if (q.Count() > 0)
            {
                //return q.FirstOrDefault();
                IsExist = true;
            }
            return IsExist;

            //return null;
        }

        
        /// <summary>
        /// //获取查询的档案信息
        /// </summary>
        /// <param name="type">档案类型</param>
        /// <param name="title">档案标题</param>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_ARCHIVES> GetArchivesInfosListBySearch(string type, string title)
        {
            var q = from ent in dal.GetTable()
                    select ent;

            if (!string.IsNullOrEmpty(type))
            {
                q = q.Where(s => type.Contains(s.RECORDTYPE));
            }
            if (!string.IsNullOrEmpty(title))
            {
                q = q.Where(s => title.Contains(s.ARCHIVESTITLE));
            }

            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        //新增档案信息
        public bool AddArchives(T_OA_ARCHIVES archivesInfo)
        {
            try
            {
                
                //ent.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                //        new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);

                if (dal.Add(archivesInfo) == 1)
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
                throw (ex);
            }
        }

        //修改档案信息
        public bool UpdateArchives(T_OA_ARCHIVES archivesInfo)
        {
            try
            {                
                var entity = from q in dal.GetTable()
                             where q.ARCHIVESID == archivesInfo.ARCHIVESID
                             select q;
                if (entity.Count() > 0)
                {

                    var entitys = entity.FirstOrDefault();
                    CloneEntity(archivesInfo, entitys);                    
                    if (dal.Update(entitys) == 1)
                    {
                        return true;
                    }                                     
                }
                return false;
            }
            catch (Exception ex)
            {
                //return false;
                throw (ex);
            }
        }

        //删除档案信息
        public bool DeleteArchives(string[] archivesID,ref string errorMsg)
        {
            bool result = false;
            try
            {
                for (int i = 0; i < archivesID.Length; i++)
                {
                    string archivesId = archivesID[i];
                    if (!string.IsNullOrEmpty(archivesId))
                    {
                        var entity = from q in dal.GetTable()
                                     where q.ARCHIVESID == archivesId
                                     select q;
                        if (entity.Count() > 0)
                        {
                            var entitys = entity.FirstOrDefault();                            
                            var ent = from p in dal.GetObjects<T_OA_LENDARCHIVES>()
                                      where entitys.ARCHIVESID == p.T_OA_ARCHIVES.ARCHIVESID
                                      && !p.ENDDATE.HasValue && (p.CHECKSTATE == "0" || p.CHECKSTATE == "1" || p.CHECKSTATE == "2")
                                      select p;
                            if (ent.Count() == 0)
                            {
                                if (dal.Delete(entitys) == 1)
                                {
                                    result = true;
                                }
                            }
                            else
                            {
                                result = false;
                                errorMsg = "此档案已存在相关借阅请求,不能被删除";
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //return false;
                throw (ex);
            }
        }

        

    }
}
