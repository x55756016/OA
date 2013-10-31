using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_System_EFModel;
using SMT.SaaS.Permission.DAL;

namespace SMT.SaaS.Permission.BLL
{
    public class FileUploadManagementBll : BaseBll<T_SYS_FILEUPLOAD>
    {
        //获取附件记录
        public IQueryable<T_SYS_FILEUPLOAD> Get_ParentID(string parentID)
        {
            var q = from ent in dal.GetTable()
                    where ent.FORMID == parentID
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        //获取附件记录
        public IQueryable<T_SYS_FILEUPLOAD> GetCalendarListByUserID(string userID)
        {
            var q = from ent in dal.GetTable()
                    where ent.FORMID == userID
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        public new bool Add(T_SYS_FILEUPLOAD entity)
        {
            try
            {
                int i = dal.Add(entity);
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
                throw (ex);
            }
        }
        //删除数据库记录
        public int Del(string id)
        {
            int i = 0;
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.FILEUPLOADID == id
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                   i= dal.Delete(entity);                    
                }
                return i;
            }
            catch (Exception ex)
            {
                return i;
                throw (ex);
            }
        }
        //批量删除数据库记录
        public int DelTB(string[] ids)
        {
            int i = 0;
            try
            {
                foreach (string id in ids)
                {
                    var ents = from e in GetTable()
                               where e.FILEUPLOADID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    if (ent != null)
                    {
                        if (this.Delete(ent)) i = 0; ;
                    }
                }
                return i;
            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
        }
        //根据父id 删除 数据库附件记录
        public int DelTB_ParentID(string[] parentIDs)
        {
            int i = 0;
            try
            {
                foreach (string parentID in parentIDs)
                {
                    var ents = from e in GetTable()
                               where e.FORMID == parentID
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    if (ent != null)
                        if (base.Delete(ent)) i=1;
                }
                return i;
            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
        }
        public new void Update(T_SYS_FILEUPLOAD entity)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.FILEUPLOADID == entity.FILEUPLOADID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.CREATEDATE = entity.CREATEDATE;
                    user.COMPANYID = entity.COMPANYID;
                    user.CREATEUSERID = entity.CREATEUSERID;
                    user.UPDATEDATE = entity.UPDATEDATE;
                    user.UPDATEUSERID = entity.UPDATEUSERID;
                    user.FILENAME = entity.FILENAME;
                    user.FORMID = entity.FORMID;
                    user.MODELNAME = entity.MODELNAME;
                    dal.Update(user);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}