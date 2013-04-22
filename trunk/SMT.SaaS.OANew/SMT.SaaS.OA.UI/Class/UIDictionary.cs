//-----------------------------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 在baseForm和BasePage中添加按需加载字典的方法.
//               保证在字典加载完之后在执行其他操作.
//               继承Base的页面不能在构造函数和页面Grid控件的loaded事件中写加载数据的方法.
//               否则无法保证字典提前加载.
// 完成日期：2011年7月15日
// 版    本：V1.0
// 作    者：安凯航 
// 最后修改人：安凯航
// 最后修改时间： 2011年7月18日,
// 最后修改内容： 解决字典已存在时无法加载load的问题.
//-----------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.OA.UI.UserControls.Meeting;
using SMT.SaaS.OA.UI.Views.ArchivesManagement;
using SMT.SaaS.OA.UI.Views.BenefitsAdministration;
using SMT.SaaS.OA.UI.Views.Bumf;
using SMT.SaaS.OA.UI.Views.ContractManagement;
using SMT.SaaS.OA.UI.Views.EmployeeSatisfactionSurveys;
using SMT.SaaS.OA.UI.Views.HouseManagement;
using SMT.SaaS.OA.UI.Views.Meeting;
using SMT.SaaS.OA.UI.Views.OrganManagement;
using SMT.SaaS.OA.UI.Views.PersonalOffice;
using SMT.SaaS.OA.UI.Views.Travelmanagement;
using SMT.SaaS.OA.UI.Views.VehicleManagement;
using SMT.SAAS.ClientUtility;

namespace SMT.SaaS.OA.UI
{
    public partial class BaseForm
    {
        public BaseForm()
        {
            currentMain = Application.Current.RootVisual as MainPage;
            CheckResourceConverter();
            base.Loaded += new RoutedEventHandler(BaseForm_Loaded);
        }

        void BaseForm_Loaded(object sender, RoutedEventArgs e)
        {
            DictionaryManager dm = new DictionaryManager();
            if (!UIDictionary.DictOfDict.ContainsKey(this.GetType()))
            {
                if (this.Loaded != null)
                {
                    this.Loaded(this, new RoutedEventArgs());
                }
                return;
            }
            dm.OnDictionaryLoadCompleted += (o, args) =>
            {
                if (this.Loaded != null)
                {
                    this.Loaded(o, new RoutedEventArgs());
                }
            };
            dm.LoadDictionary(UIDictionary.DictOfDict[this.GetType()]);
        }

        private void CheckResourceConverter()
        {

            

            if (Application.Current.Resources["GridHeaderConverter"] == null)
            {
                Application.Current.Resources.Add("GridHeaderConverter", new SMT.SaaS.Globalization.GridHeaderConverter());
            }

            if (Application.Current.Resources["ResourceConveter"] == null)
            {
                Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            }

            if (Application.Current.Resources["DictionaryConverter"] == null)
            {
                Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.OA.UI.DictionaryConverter());
            }

            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new SMT.SaaS.OA.UI.CustomDateConverter());
            }
            if (Application.Current.Resources["StateConvert"] == null)
            {
                Application.Current.Resources.Add("StateConvert", new SMT.SaaS.OA.UI.CheckStateConverter());
            }
            if (Application.Current.Resources["RentConvert"] == null)
            {
                Application.Current.Resources.Add("RentConvert", new SMT.SaaS.OA.UI.RentFlagConverter());
            }
            if (!Application.Current.Resources.Contains("ModuleNameConverter"))
            {
                Application.Current.Resources.Add("ModuleNameConverter", new SMT.SaaS.OA.UI.ModuleNameConverter());
            }
            if (!Application.Current.Resources.Contains("ObjectTypeConverter"))
            {
                Application.Current.Resources.Add("ObjectTypeConverter", new SMT.SaaS.OA.UI.ObjectTypeConverter());
            }
        }

        public new event RoutedEventHandler Loaded;
    }

    public partial class BasePage
    {
        public BasePage()
        {
            SMT.SaaS.FrameworkUI.Validator.ValidatorService.ResourceMgr = SMT.SaaS.Globalization.Localization.ResourceMgr;
            CheckResourceConverter();
            base.Loaded += new RoutedEventHandler(BaseForm_Loaded);
        }

        void BaseForm_Loaded(object sender, RoutedEventArgs e)
        {
            DictionaryManager dm = new DictionaryManager();
            if (!UIDictionary.DictOfDict.ContainsKey(this.GetType()))
            {
                if (this.Loaded != null)
                {
                    this.Loaded(this, new RoutedEventArgs());
                }
                return;
            }
            dm.OnDictionaryLoadCompleted += (o, args) =>
            {
                if (this.Loaded != null)
                {
                    this.Loaded(o, new RoutedEventArgs());
                }
            };
            dm.LoadDictionary(UIDictionary.DictOfDict[this.GetType()]);
        }

        public new event RoutedEventHandler Loaded;
    }

    public static class UIDictionary
    {
        static UIDictionary()
        {
            try
            {
                GetUIDictionary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("1!!" + ex.Message);
            }
        }

        public static Dictionary<Type, List<string>> DictOfDict;

        /// <summary>
        /// 在此处写个子页面需要的字典.
        /// </summary>
        private static void GetUIDictionary()
        {
            //使用List先储存所有字典索引信息,然后放到字典索引信息的字典中.
            //避免key重复的情况.
            List<KeyValuePair<Type, List<string>>> dicts = new List<KeyValuePair<Type, List<string>>>();

            #region 安凯航
            //事项审批From
            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ApprovalForm_add), new List<string> { "TYPEAPPROVAL" }));

            //事项审批From
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ApprovalForm_aud), new List<string> { "TYPEAPPROVAL" }));

            //事项审批From
            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ApprovalForm_upd), new List<string> { "TYPEAPPROVAL" }));

            //事项审批View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmApprovalManagement), new List<string> { "CHECKSTATE" }));

            //事项审批设置
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ApprovalTypeSet), new List<string> { "CHECKSTATE", "TYPEAPPROVAL" }));

            //会议室管理View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingRoomManagementPage), new List<string> { "CHECKSTATE" }));

            //会议室管理Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingRoomChildWindow), new List<string> { "CHECKSTATE" }));

            //会议室申请View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingRoomAppManagement), new List<string> { "CHECKSTATE" }));

            //会议室申请Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingRoomAppForm), new List<string> { "CHECKSTATE" }));

            //会议类型View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingTypeManagement), new List<string> { "CHECKSTATE" }));

            //会议类型View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingTypeForm), new List<string> { "CHECKSTATE" }));

            //会议类型Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingTypeForm), new List<string> { "CHECKSTATE" }));

            //会议类型模板View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingTemplateManagement), new List<string> { "CHECKSTATE" }));

            //会议类型模板Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingTemplateForm), new List<string> { "CHECKSTATE" }));

            //会议申请View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingManagementInfos), new List<string> { "CHECKSTATE" }));

            //会议申请Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MeetingForm), new List<string> { "CHECKSTATE" }));

            //会议主持 
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmceeMeeting), new List<string> { "CHECKSTATE" }));

            //我的会议 
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MyMeetingInfosManagement), new List<string> { "CHECKSTATE" }));

            //公司发文View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CompanySendDocManagement), new List<string> { "CHECKSTATE", "COMPANYDOCGRADE", "COMPANYDOCPRIORITY" }));

            //公司发文Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CompanyDocForm), new List<string> { "CHECKSTATE", "COMPANYDOCGRADE", "COMPANYDOCPRIORITY" }));

            ////公文模板View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(DocTypeTemplateManagement), new List<string> { "CHECKSTATE", "COMPANYDOCGRADE", "COMPANYDOCPRIORITY" }));

            //公文模板From
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(DocTypeTemplateForm), new List<string> { "CHECKSTATE", "COMPANYDOCGRADE", "COMPANYDOCPRIORITY" }));

            //公文缓急View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PriorityManagement), new List<string> { "CHECKSTATE" }));

            //公文缓急From
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(PriorityForm), new List<string> { "CHECKSTATE" }));

            //档案管理View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmArchivesManager), new List<string> { "CHECKSTATE", "RECORDTYPE" }));

            //档案管理Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ArchivesAddForm), new List<string> { "CHECKSTATE", "RECORDTYPE" }));

            //档案借阅View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmArchivesLending), new List<string> { "CHECKSTATE", "RECORDTYPE" }));

            //档案借阅Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ArchivesLendingForm), new List<string> { "CHECKSTATE", "RECORDTYPE" }));

            //档案归还View
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmArchivesReturn), new List<string> { "CHECKSTATE", "RECORDTYPE" }));

            //档案归还Form
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ArchivesReturnForm), new List<string> { "CHECKSTATE", "RECORDTYPE" }));

            //员工调查方案
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeSurveysMaster
), new List<string> { "CHECKSTATE" }));

            //员工调查申请
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeSurveysApp), new List<string> { "CHECKSTATE" }));

            //员工调查发布
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeSurveysDistribute), new List<string> { "CHECKSTATE" }));

            //员工调查参与调查
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(EmployeeSurveying), new List<string> { "CHECKSTATE" }));

            //满意度调查方案
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Satisfaction), new List<string> { "CHECKSTATE" }));

            //满意度调查申请
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SatisfactionApp), new List<string> { "CHECKSTATE" }));

            //满意度调查发布
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SatisfactionDistribute), new List<string> { "CHECKSTATE" }));

            //满意度参与调查
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(Satisfactioning), new List<string> { "CHECKSTATE" }));

            #endregion

            //出差申请
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(BusinessApplicationsForm), new List<string> { "CHECKSTATE", "VICHILESTANDARD", "VICHILELEVEL", "PROVINCE", "CITY" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ConserVationForm), new List<string> { "CONSERVANAME" }));

            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ApprovalTypeList), new List<string> { "POSTLEVEL", "TYPEAPPROVAL" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TravelapplicationPage), new List<string> { "CHECKSTATE", "CITY", "CHECKSTATE" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TravelRequestForm), new List<string> { "CHECKSTATE", "VICHILESTANDARD", "VICHILELEVEL", "PROVINCE", "CITY" }));

            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TravelReimbursementPage), new List<string> { "CHECKSTATE", "CITY", "VICHILESTANDARD", "VICHILELEVEL" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SolutionManagement), new List<string> { "CHECKSTATE", "POSTLEVEL", "VICHILESTANDARD", "VICHILELEVEL" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TravelReimbursementControl), new List<string> { "CHECKSTATE", "POSTLEVEL", "VICHILESTANDARD", "VICHILELEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TravelRequestForm), new List<string> { "CHECKSTATE", "CITY", "VICHILESTANDARD", "VICHILELEVEL", "PROVINCE" }));
            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TravelReimbursementPage), new List<string> { "CHECKSTATE", "CITY", "VICHILESTANDARD", "VICHILELEVEL" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(SolutionManagement), new List<string> { "CHECKSTATE", "POSTLEVEL", "VICHILESTANDARD", "VICHILELEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AreaAllowance), new List<string> { "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AreaAllowanceForm), new List<string> { "POSTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(AreaSortForm), new List<string> { "CHECKSTATE", "CITY", "PROVINCE" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ContractTypeDefinition), new List<string> { "CONTRACTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ContractTypeDefinitionPages), new List<string> { "CONTRACTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ContractTemplatePage), new List<string> { "CONTRACTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ContractTemplates), new List<string> { "CONTRACTLEVEL" }));
            // 合同申请
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ApplicationsForContracts), new List<string> { "CONTRACTLEVEL", "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ApplicationsForContractsPages), new List<string> { "CONTRACTLEVEL", "CHECKSTATE" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ContractPrintingPage), new List<string> { "CONTRACTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ContractPrintUploadControl), new List<string> { "CONTRACTLEVEL" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ViewContractApplicationPage), new List<string> { "CONTRACTLEVEL" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(BenefitsAdministrationPage), new List<string> { "WELFAREPROID" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(BenefitsAdministrationChildWindows), new List<string> { "WELFAREPROID", "POSTLEVELA" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(WelfareProvisionPage), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(WelfareProvisionChildWindows), new List<string> { "WELFAREPROID" }));

            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmVehicleInfoManager), new List<string> { "VEHICLEFLAG" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(VehicleInfo_upd), new List<string> { "VEHICLEFLAG" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(VehicleInfo_add), new List<string> { "VEHICLEFLAG" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmVehicleUseAppManager), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmVehicleDispatchManager), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(VehicleDispatchRecord), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(VehicleDispatchRecord_add), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(VehicleDispatchRecord_upd), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmConserVationManager), new List<string> { "CHECKSTATE" }));
            /*
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ConserVationForm), new List<string> { "CHECKSTATE", "CONSERVANAME" }));
            */
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ConserVationRecord), new List<string> { "CHECKSTATE", "CONSERVANAME" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ConserVationRecord_add), new List<string> { "CHECKSTATE", "CONSERVANAME" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ConserVationRecord_upd), new List<string> { "CHECKSTATE", "CONSERVANAME" }));



            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmMaintenanceAppManager), new List<string> { "CHECKSTATE", "MAINTENANCENAME" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MaintenanceAppForm), new List<string> { "CHECKSTATE", "MAINTENANCENAME" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MaintenanceRecord), new List<string> { "CHECKSTATE", "MAINTENANCENAME" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MaintenanceRecordForm_add), new List<string> { "CHECKSTATE", "MAINTENANCENAME" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MaintenanceRecordForm_upd), new List<string> { "CHECKSTATE", "MAINTENANCENAME" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(CFrmCostRecordManager), new List<string> { "FEESTYPE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmHouseInfoIssurance), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmHouseHireAppManagement), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(HireRecordManagement), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmOrganRegister), new List<string> { "CHECKSTATE" }));
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmLicenseBorrowManagement), new List<string> { "CHECKSTATE" }));
            // 证照归还 
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(FrmLicenseReturnManagement), new List<string> { "LENDSTATE", "CHECKSTATE" }));
            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(MissionReportsPage), new List<string> { "CHECKSTATE" }));

            //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(TravelReportsForm), new List<string> { "CHECKSTATE", "POSTLEVEL", "VICHILESTANDARD", "VICHILELEVEL" }));
            // 证照
            dicts.Add(new KeyValuePair<Type, List<string>>(typeof(ChooseLicenseForm), new List<string> { "LICENSE" }));
          
            DictOfDict = new Dictionary<Type, List<string>>();

            foreach (var item in dicts)
            {
                if (!DictOfDict.ContainsKey(item.Key))
                {
                    DictOfDict.Add(item.Key, item.Value);
                }
            }

        }
    }
}
