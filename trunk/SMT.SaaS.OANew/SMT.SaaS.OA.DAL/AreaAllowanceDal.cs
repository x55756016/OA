using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    class AreaAllowanceDal
    {
    }

    //地区津贴
    public class AreaAllowanceManagementtDal : CommDaL<T_OA_AREAALLOWANCE>
    {

    }
    //地区
    public class AreaCityManagementtDal : CommDaL<T_OA_AREACITY>
    {

    }
    //地区差异
    public class AreaDifferenceManagementtDal : CommDaL<T_OA_AREADIFFERENCE>
    {

    }
    //出差解决方案
    public class TrvaleSolutionDal : CommDaL<T_OA_TRAVELSOLUTIONS>
    { 

    }
    //交通工具标准
    public class TravleTransportDal : CommDaL<T_OA_TAKETHESTANDARDTRANSPORT>
    { 

    }
    //飞机路线
    public class TravlePlaneDal : CommDaL<T_OA_CANTAKETHEPLANELINE>
    { 

    }
    //出差方案应用
    public class TravleSolutionSet : CommDaL<T_OA_PROGRAMAPPLICATIONS>
    { 

    }
}
