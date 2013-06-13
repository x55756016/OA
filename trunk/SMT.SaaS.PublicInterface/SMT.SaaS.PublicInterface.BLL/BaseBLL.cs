using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using SMT_System_EFModel;
using System.Data.Objects;
using SMT.Foundation.Log;
using SMT.SaaS.PublicInterface.DAL;

namespace SMT.SaaS.PublicInterface.BLL
{
    public class BaseBLL<TEntity>: IDisposable where TEntity : class
    {
        public SysRtfDAL dal;

        public BaseBLL()
        {
            if (dal == null)
            {
                dal = new SysRtfDAL();
            }
        }

        public bool Add(TEntity entity)
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

        public bool Delete(TEntity entity)
        {
            try
            {
                dal.Delete(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        public bool Update(TEntity entity)
        {
            try
            {
                //Utility.RefreshEntity(entity as EntityObject);
                int i=dal.Update(entity);
                return i > 0 ? true : false;
                
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IQueryable GetTable()
        {
            return dal.GetTable();
        }
        public ObjectQuery GetObjects()
        {
            return dal.GetObjects();
        }

        public object CustomerQuery(string Sql)
        {
            return dal.CustomerQuery(Sql);
        }

        #region IDisposable 成员

        public void Dispose()
        {
            dal.Dispose();
        }

        #endregion

    }
}
