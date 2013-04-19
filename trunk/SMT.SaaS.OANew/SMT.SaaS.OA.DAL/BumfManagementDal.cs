using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    class BumfManagementDal
    {
    }

    //公文等级
    public class BumfGradeManagementDal : CommDaL<T_OA_GRADED>
    { 

    }

    //公文缓急
    public class BumfPrioritiesManagementDal : CommDaL<T_OA_PRIORITIES>
    { 

    }
    //文档类型
    public class BumfDocTypeManagementDal : CommDaL<T_OA_SENDDOCTYPE>
    { 

    }

    //公司发文
    public class BumfCompanySendDocManagementDal : CommDaL<T_OA_SENDDOC>
    {
    }
    //公司发文模板
    public class BumfDocTypeTemplateManagementDal : CommDaL<T_OA_SENDDOCTEMPLATE>
    { 

    }

    //公司文档发布
    public class BumfSendDocToDistrbuteManagementDal : CommDaL<T_OA_DISTRIBUTEUSER>
    {
        //private ObjectContext edm = null;
        //private System.Data.Common.DbTransaction tran = null;
        //public void BeginTransaction()
        //{
        //    edm = this.GetDataContext();
        //    if (edm.Connection.State == System.Data.ConnectionState.Closed)
        //    {
        //        edm.Connection.Open();
        //    }

        //    tran = edm.Connection.BeginTransaction();
        //}
        //public void CommitTransaction()
        //{
        //    tran.Commit();
        //}
        //public void RollbackTransaction()
        //{
        //    tran.Rollback();
        //}

    }


}
