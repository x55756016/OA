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
using SterlingDemoProject.Tables;
using SMT.SaaS.LocalData.Tables;

namespace SMT.SaaS.LocalData.Database
{
    public class SMTLacalDB : BaseDatabaseInstance
    {

        public override string Name
        {
            get { return "SMT Loacal Databse"; }
        }

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
                           CreateTableDefinition<UserPost,string>(c => c.EmployeeID)
                       };
        }
    }
}
