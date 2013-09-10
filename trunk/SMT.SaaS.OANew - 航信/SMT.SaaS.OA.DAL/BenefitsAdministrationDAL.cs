using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    //福利标准定义
    public class BenefitsAdministrationDAL : CommDaL<T_OA_WELFAREMASERT>
    { }
    //福利标准明细
    public class BenefitsAdministrationDetails : CommDaL<T_OA_WELFAREDETAIL>
    { }
    //福利发放明细
    public class WelfarePaymentDetails : CommDaL<T_OA_WELFAREDISTRIBUTEDETAIL>
    { }
    //福利发放
    public class WelfareProvision : CommDaL<T_OA_WELFAREDISTRIBUTEMASTER>
    { }
    //福利发放撤销
    public class WelfarePaymentWithdrawal : CommDaL<T_OA_WELFAREDISTRIBUTEUNDO>
    { }
}
