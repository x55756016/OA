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
using SMT.HRM.UI.Views.Organization;
using System.Collections.Generic;
using SMT.HRM.UI.Form.Organization;
using SMT.HRM.UI.Form;
using SMT.HRM.UI.Form.Personnel;
using SMT.HRM.UI.Views.Personnel;

namespace SMT.HRM.UI
{
    public partial class UIDictionary
    {
        private static void GetUIDictionary_WRY()
        {

            //部门字典
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(DepartmentDictionary), new List<string> { "COMPANYTYPE", "CHECKSTATE", "DEPARTMENTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(DepartmentDictionaryForms), new List<string> { "COMPANYTYPE", "DEPARTMENTLEVEL", "CHECKSTATE" }));
            //岗位字典
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PostDictionary), new List<string> { "COMPANYTYPE", "CHECKSTATE", "COMPANYLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PostDictionaryForms), new List<string> { "COMPANYTYPE", "CHECKSTATE" }));
            //公司信息
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Company), new List<string> { "COMPANYTYPE", "CHECKSTATE", "EDITSTATE", "COMPANYLEVEL", "PROVINCE", "CITY", "COUNTYTYPE" }));
            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CompanyForm), new List<string> { "COUNTYTYPE", "COMPANYLEVEL", "CHECKSTATE", "COMPANYTYPE", "OWNERPROVINCE" }));
            //部门信息
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Department), new List<string> { "COMPANYTYPE", "CHECKSTATE", "DEPARTMENTLEVEL", "EDITSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(DepartmentForm), new List<string> { "COMPANYTYPE", "CHECKSTATE" }));
            //岗位信息
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Post), new List<string> { "COMPANYTYPE", "CHECKSTATE", "EDITSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PostForm), new List<string> { "COMPANYTYPE", "CHECKSTATE", "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CompanyForm), new List<string> { "COMPANYTYPE", "CHECKSTATE", "COUNTYTYPE", "COMPANYLEVEL", "PROVINCE", "CITY", }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CompanyTree), new List<string> { "CHECKSTATE" }));
            //员工应聘简历
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(BaseinfoForm), new List<string> { "CHECKSTATE", "SEX", "NATION", "MARRIAGE", "PROVINCE", "POLITICS", "TOPEDUCATION" }));
            //员工个人档案
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Resume), new List<string> { "POLITICS" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Employee), new List<string> { "CHECKSTATE", "SEX", "EMPLOYEESTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeForm), new List<string> { "POSTCHANGCATEGORY","POSTLEVEL","CHECKSTATE", "SEX", "NATION", "BLOODTYPE", "IDTYPE", "SECONDLANGUAGE", "SECONDLANGUAGEDEGREE", "EMPLOYEESTATE", "PROFESSIONALTITLE", "TOPEDUCATION", "POLITICS", "MARRIAGE", "PROVINCE", "RESIDENCETYPE", "REGRESIDENCETYPE", "ATTENDANCETYPE", "CARDTYPE", "WORKDAYTYPE","OFFESTTYPE","SVALID"  }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeInfoRealTime), new List<string> { "CHECKSTATE", "SEX", "NATION", "BLOODTYPE", "IDTYPE", "SECONDLANGUAGE", "SECONDLANGUAGEDEGREE", "EMPLOYEESTATE", "PROFESSIONALTITLE", "TOPEDUCATION", "POLITICS", "MARRIAGE", "PROVINCE", "RESIDENCETYPE", "REGRESIDENCETYPE", "ATTENDANCETYPE", "CARDTYPE", "WORKDAYTYPE" }));
            //员工劳动合同
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeContract), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeContractForm), new List<string> { "CHECKSTATE" }));
            //员工商业保险
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeInsurance), new List<string> { "CHECKSTATE", "INSURANCECATEGORY" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeInsuranceForm), new List<string> { "CHECKSTATE", "INSURANCECATEGORY" }));
            //员工黑名单
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(BlackList), new List<string> { "CHECKSTATE", "EDITSTATE", "BLACKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(BlackListForm2), new List<string> { "CHECKSTATE", "BLACKSTATE" }));
            //员工入职
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeEntry), new List<string> { "CHECKSTATE", "SEX", "NATION", "BLOODTYPE", "IDTYPE", "SECONDLANGUAGE", "SECONDLANGUAGEDEGREE", "EMPLOYEESTATE", "PROFESSIONALTITLE", "TOPEDUCATION", "POLITICS", "MARRIAGE", "PROVINCE", "RESIDENCETYPE", "REGRESIDENCETYPE", "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeEntryForm), new List<string> { "CHECKSTATE", "POSTLEVEL" }));
            //员工转正
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeCheck), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeCheckForm), new List<string> { "CHECKSTATE" }));
            //员工异动
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeePostChange), new List<string> { "CHECKSTATE", "POSTCHANGCATEGORY", "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeePostChangeForm), new List<string> { "CHECKSTATE", "POSTCHANGCATEGORY", "POSTLEVEL" }));
            //员工离职
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(LeftOffice), new List<string> { "CHECKSTATE", "LEFTOFFICECATEGORY" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(LeftOfficeForm), new List<string> { "CHECKSTATE", "LEFTOFFICECATEGORY", "POSTLEVEL" }));
            //离职确认
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(LeftOfficeConfirm), new List<string> { "CHECKSTATE", "LEFTOFFICECATEGORY" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(LeftOfficeConfirmForm), new List<string> { "CHECKSTATE", "LEFTOFFICECATEGORY" }));
            //员工社保档案
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PensionMaster), new List<string> { "CHECKSTATE", "ISVALID" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PensionMasterForm), new List<string> { "CHECKSTATE", "ISVALID" }));
            //社保缴交记录
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PensionDetail), new List<string> { "CHECKSTATE", "COUNTYTYPE", "PROVINCE", "CITY" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PensionDetailForm), new List<string> { "CHECKSTATE" }));
            //社保导入设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ImportSetMaster), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ImportSetMasterForm), new List<string> { "CHECKSTATE", "CITY" }));
            //社保缴交提醒
            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PensionAlarmSet), new List<string> { "CHECKSTATE" }));
            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PensionAlarmSetForm), new List<string> { "CHECKSTATE" }));
        }
    }
}
