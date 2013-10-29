using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Wintellect.Sterling.Database;
using System.Collections.Generic;
using SMT.SaaS.LocalData.Tables;

namespace SMT.SaaS.LocalData.Database
{
    /// <summary>
    /// 本地数据库注册类
    /// </summary>
    public class SMTLacalDB : BaseDatabaseInstance
    {
        /// <summary>
        /// 本地数据库名称
        /// </summary>
        public override string Name
        {
            get
            {
                return "SMT Loacal Databse";
            }
        }

        /// <summary>
        /// 注册表到本地数据库
        /// </summary>
        /// <returns>返回注册的表集合</returns>
        protected override System.Collections.Generic.List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                       {                           
                           CreateTableDefinition<V_ModuleInfo,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_ModuleInfo_DependsOn,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_ModuleInfo_Params,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_CompanyInfo,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_DepartmentInfo,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_PostInfo,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_DictionaryInfo,string>(c => c.DICTIONARYID),
                           CreateTableDefinition<V_UserLogin,string>(c => c.UserName),
                           CreateTableDefinition<LoginUserInfo,string>(c => c.EmployeeID),
                           CreateTableDefinition<UserPost,string>(c => c.EmployeeID),
                           CreateTableDefinition<V_PermissionCheck,string>(c => c.EmployeeID),
                           CreateTableDefinition<V_UserPermUILocal,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_CustomerPermission,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_PermissionValue,string>(c => c.UserModuleID),
                           CreateTableDefinition<V_OrgObject,string>(c => c.UserModuleID)
                       };
        }
    }
}
