/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： LINQ数据访问接口
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data;

namespace SMT.Foundation.Core
{
    public interface IDAL
    {
        int Add(object obj);

        int Update(object obj);

        int Delete(object obj);

        int SaveChanges();

        object GetObjectByEntityKey(EntityKey entityKey);

        IQueryable<TEntity> GetTable<TEntity>();

        ObjectContext GetDataContext();

         /// <summary>
         /// 返回ObjectQuery对象
         /// </summary>
         /// <typeparam name="TEntity"></typeparam>
         /// <returns></returns>
         ObjectQuery<TEntity> GetObjects<TEntity>();

         void BeginTransaction();

         /// <summary>
         /// 确认事务处理
         /// </summary>
         void CommitTransaction();
         /// <summary>
         /// 回滚事务处理
         /// </summary>
         void RollbackTransaction();
    }
}
