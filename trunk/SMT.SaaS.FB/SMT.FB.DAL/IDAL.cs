using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

using SMT_FB_EFModel;
using System.Reflection;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Data;
using System.Linq.Dynamic;
using SMT.SaaS.BLLCommonServices;
using System.Configuration;
using SMT.Foundation.Core;

namespace SMT.FB.DAL
{

    public interface IDAL : IDisposable
    {
        int Add(EntityObject obj);
        int Delete(EntityObject obj);
        int Update(EntityObject obj);
        void Detach(EntityObject obj);
        void Attach(EntityObject entity);
        void DeleteObject(EntityObject entity);
        int SaveChanges();
        IQueryable<TEntity> GetTable<TEntity>();
        IQueryable<TEntity> QueryTable<TEntity>(QueryExpression queryExpression);
        EntityObject GetEntity(EntityKey key);
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();


    }


}
