using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    //合同类型定义
    public class ContractManagementDAL : CommDaL<T_OA_CONTRACTTYPE>
    {
    }
    //合同模板
    public class ContractTemplate : CommDaL<T_OA_CONTRACTTEMPLATE>
    { 
    }
    //合同申请
    public class ApplicationsForContracts : CommDaL<T_OA_CONTRACTAPP>
    {
       
    }
    //合同打印
    public class ContractPrintingDal : CommDaL<T_OA_CONTRACTPRINT>
    {

    }
    //合同查看申请
    public class ContractViewapplicationsDal : CommDaL<T_OA_CONTRACTVIEW>
    {

    }
}
