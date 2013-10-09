using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SetSolutionForms : BaseForm,IEntityEditor
    {
        SmtOAPersonOfficeClient client = new SmtOAPersonOfficeClient();
        private ObservableCollection<string> companyids = new ObservableCollection<string>();
        List<T_OA_PROGRAMAPPLICATIONS> ListSet = new List<T_OA_PROGRAMAPPLICATIONS>();
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private List<ExtOrgObj> issuanceExtOrgObj;
        string StrSolution = "";
        bool IsEdit = false; //是否是修改
        public SetSolutionForms(string SolutionID)
        {
            InitializeComponent();
            StrSolution = SolutionID;
            client.AddTravleSolutionSetCompleted += new EventHandler<AddTravleSolutionSetCompletedEventArgs>(client_AddTravleSolutionSetCompleted);
            client.UpdateTravleSolutionSetCompleted += new EventHandler<UpdateTravleSolutionSetCompletedEventArgs>(client_UpdateTravleSolutionSetCompleted);
            client.GetTravleSolutionSetBySolutionIDCompleted += new EventHandler<GetTravleSolutionSetBySolutionIDCompletedEventArgs>(client_GetTravleSolutionSetBySolutionIDCompleted);
            client.GetTravleSolutionSetBySolutionIDAsync(SolutionID);
            issuanceExtOrgObj = new List<ExtOrgObj>();
        }

        void client_UpdateTravleSolutionSetCompleted(object sender, UpdateTravleSolutionSetCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                RefreshUI(refreshType);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        void client_AddTravleSolutionSetCompleted(object sender, AddTravleSolutionSetCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                RefreshUI(refreshType);

                if (refreshType == RefreshedTypes.LeftMenu)
                    IsEdit = true;
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        void client_GetTravleSolutionSetBySolutionIDCompleted(object sender, GetTravleSolutionSetBySolutionIDCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    
                    if (e.Result.Count() >0)
                    {
                        IsEdit = true;//z设置为修改
                        issuanceExtOrgObj.Clear();
                        ListSet = e.Result.ToList();
                        foreach (var h in ListSet)
                        {
                            object obj = new object();
                            SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj extOrgObj = new SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj();
                                                        
                            T_HR_COMPANY tmp = new T_HR_COMPANY();
                            tmp.COMPANYID = h.COMPANYID;
                            tmp.CNAME = Utility.GetCompanyName(tmp.COMPANYID);
                            obj = tmp;
                            
                            
                            extOrgObj.ObjectInstance = obj;
                            issuanceExtOrgObj.Add(extOrgObj);
                        }
                        BindData();
                    }
                }

            }
        }

        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button delBtn = sender as Button;
            ExtOrgObj Org = new ExtOrgObj();
            Org = delBtn.Tag as ExtOrgObj;
            issuanceExtOrgObj.Remove(Org);
            BindData();
            
        }

        private void DgOrganize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Save()
        {
            companyids.Clear();
            foreach (var h in issuanceExtOrgObj)
            {
                companyids.Add(h.ObjectID);
            }
            if (IsEdit)
            {
                client.UpdateTravleSolutionSetAsync(StrSolution, companyids, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.UserPosts[0].PostID, Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                client.AddTravleSolutionSetAsync(StrSolution, companyids, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.UserPosts[0].PostID, Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID, Common.CurrentLoginUserInfo.EmployeeID);
            }
        }
        #region IEntityEditor 成员

        public string GetTitle()
        {
            string StrReturn = "方案设置";
            

            return StrReturn;

        }

        public string GetStatus()
        {
            return "";
        }


        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0"://保存
                    refreshType = RefreshedTypes.LeftMenu;
                    Save();
                    break;
                case "1"://保存并关闭
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;                
                //case "3"://选择公司
                //    AddOrganizeObj();
                //    break;
                    
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            //ToolbarItem item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "3",
            //    //Title = Utility.GetResourceStr("MEETINGMEMBER"),
            //    Title ="选择公司",
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addTeam.png"

            //};
            //items.Add(item);

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);
            

            return items;
        }

        private void Close()
        {
            RefreshUI(refreshType);
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

        #region 选择公司
        private void AddOrganizeObj()
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    if (issuanceExtOrgObj.Count() > 0)
                    {
                        foreach (var q in ent)
                        {
                            var ents = from entorg in issuanceExtOrgObj
                                       where entorg.ObjectID == q.ObjectID
                                       select entorg;
                            if (ents.Count()== 0)
                                issuanceExtOrgObj.Add(q);
                        }
                    }
                    else
                    {
                        issuanceExtOrgObj = ent;
                    }
                    BindData();
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }
        private void BindData()
        {
            DgOrganize.ItemsSource = null;
            if (issuanceExtOrgObj == null || issuanceExtOrgObj.Count < 1)
            {
                DgOrganize.ItemsSource = null;
                return;
            }
            else
            {
                DgOrganize.ItemsSource = issuanceExtOrgObj;
            }

        }
        
        #endregion

        private void DgOrganize_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            ExtOrgObj Organize = (ExtOrgObj)e.Row.DataContext;
            Button DelBtn = DgOrganize.Columns[2].GetCellContent(e.Row).FindName("btnDelete") as Button;
            DelBtn.Tag = Organize;
        }

        private void btnAddCompany_Click(object sender, RoutedEventArgs e)
        {
            AddOrganizeObj();
        }

        private void PostsObject_FindClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
