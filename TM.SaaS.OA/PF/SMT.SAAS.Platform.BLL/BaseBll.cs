using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Reflection;
using System.Data.Objects.DataClasses;
using System.Configuration;
using SMT.SAAS.Platform.DAL;

namespace SMT.SAAS.Platform.BLL
{
    public class BaseBll<TEntity> : IDisposable where TEntity : class
    {
        public static string qualifiedEntitySetName = ConfigurationManager.AppSettings["DBContextName"] + ".";

        public CommonDAL<TEntity> dal;
        public BaseBll()
        {
            if (dal == null)
            {
                dal = new CommonDAL<TEntity>();
               // dal.LogNewDal(typeof(TEntity).Name);

            }
        }

        public bool Add(TEntity entity)
        {
            try
            {
               
                bool flag = dal.Add(entity);
                return flag;
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

        public void Update(TEntity entity)
        {
            try
            {
                dal.Update(entity);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IQueryable<TEntity> GetTable()
        {
            //return ((ObjectQuery<TEntity>)dal.GetTable(typeof(TEntity).Name)).AsQueryable();
            return dal.GetTable<TEntity>();
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
