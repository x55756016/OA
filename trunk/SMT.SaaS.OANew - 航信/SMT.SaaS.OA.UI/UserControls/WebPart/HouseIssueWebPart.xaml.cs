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
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Windows.Browser;
using System.Windows.Data;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.FlowWFService;
using SMT.SaaS.OA.UI.Views.HouseManagement;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PersonnelWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class HouseIssueWebPart : BaseForm,IClient,IEntityEditor
    {
        
        private HouseInfoChooseForm addFrm;
        private List<T_OA_HOUSEINFO> houseInfoList;
        //private List<T_OA_HOUSEINFO> originHouseInfoList;
        private List<string> houseID;
        private T_OA_HOUSEINFO houseObj;
        private T_OA_HOUSEINFOISSUANCE issuanceObj;
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        //private List<T_OA_HOUSELIST> originHouseList;
        private List<T_OA_HOUSELIST> houseList;
        private List<T_OA_DISTRIBUTEUSER> distributeList;
        private string issuanceID = "";
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private List<ExtOrgObj> issuanceExtOrgObj;
        private ObservableCollection<T_OA_DISTRIBUTEUSER> distributeLists;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private string checkstate = "";
        private bool submitflag=false;

        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<string> StrDepartmentIDsList = new ObservableCollection<string>();  //获取部门ID
        private ObservableCollection<string> StrPositionIDsList = new ObservableCollection<string>();  //获取岗位ID
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID

        private List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST>();
        //public T_OA_HOUSEINFOISSUANCE IssuanceObj
        //{
        //    get { return issuanceObj; }
        //    set
        //    {
        //        this.DataContext = value;
        //        issuanceObj = value;
        //    }
        //}
        public T_OA_HOUSEINFO InfoObj
        {
            get { return houseObj; }
            set
            {
                this.DataContext = value;
                houseObj = value;
            }
        }

        private Action action;
        private SmtOACommonAdminClient client;
        private ObservableCollection<T_OA_HOUSELIST> houseLists;


        #region 初始化
        public HouseIssueWebPart(string issuanceID)
        {
            this.issuanceID = issuanceID;            
            InitializeComponent();
            InitEvent();            
            InitData();
        }

        

        private void InitEvent()
        {            
            houseInfoList = new List<T_OA_HOUSEINFO>();
            houseID = new List<string>();
            houseObj = new T_OA_HOUSEINFO();
            houseLists = new ObservableCollection<T_OA_HOUSELIST>();
            distributeLists = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            distributeList = new List<T_OA_DISTRIBUTEUSER>();
            issuanceExtOrgObj = new List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
            client = new SmtOACommonAdminClient();
            
            client.GetIssuanceListByIdCompleted += new EventHandler<GetIssuanceListByIdCompletedEventArgs>(client_GetIssuanceListByIdCompleted);
            client.GetIssuanceHouseInfoListCompleted += new EventHandler<GetIssuanceHouseInfoListCompletedEventArgs>(client_GetIssuanceHouseInfoListCompleted);
            client.GetIssuanceHouseListCompleted += new EventHandler<GetIssuanceHouseListCompletedEventArgs>(client_GetIssuanceHouseListCompleted);
            client.GetDistributeUserListCompleted += new EventHandler<GetDistributeUserListCompletedEventArgs>(client_GetDistributeUserListCompleted);
            personclient.GetEmployeeDetailByParasCompleted += new EventHandler<GetEmployeeDetailByParasCompletedEventArgs>(personclient_GetEmployeeDetailByParasCompleted);
        }        

        private void InitData()
        {
            client.GetIssuanceListByIdAsync(issuanceID);
        }

        void personclient_GetEmployeeDetailByParasCompleted(object sender, GetEmployeeDetailByParasCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    vemployeeObj.Clear();
                    StrStaffList.Clear();//清空员工ID集合 否则会逐条记录添加
                    StrDepartmentIDsList.Clear();
                    StrCompanyIDsList.Clear();
                    StrPositionIDsList.Clear();
                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();
                        //vemployeeObj.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEECNAME
                        BindDataMember();
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        
        private void BindDataMember()
        {

            if (vemployeeObj == null || vemployeeObj.Count < 1)
            {
                dgmember.ItemsSource = null;

                return;
            }
            else
            {
                dgmember.ItemsSource = vemployeeObj;
            }

        }
        #endregion

        #region 完成事件
        private void client_GetDistributeUserListCompleted(object sender, GetDistributeUserListCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        distributeList = e.Result.ToList();
                        foreach (var h in distributeList)
                        {
                            object obj = new object();
                            SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj extOrgObj = new SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj();
                            if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company).ToString())
                            {
                                StrCompanyIDsList.Add(h.VIEWER);
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department).ToString())
                            {
                                StrDepartmentIDsList.Add(h.VIEWER);
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post).ToString())
                            {
                                StrPositionIDsList.Add(h.VIEWER);
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel).ToString())
                            {
                                StrStaffList.Add(h.VIEWER);
                            }
                            
                            
                        }
                        personclient.GetEmployeeDetailByParasAsync(StrCompanyIDsList, StrDepartmentIDsList, StrPositionIDsList, StrStaffList);
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }   

        private void client_GetIssuanceListByIdCompleted(object sender, GetIssuanceListByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        issuanceObj = new T_OA_HOUSEINFOISSUANCE();
                        issuanceObj = e.Result.ToList()[0];
                        this.tblIssuecontent.Text = issuanceObj.CONTENT.ToString();
                        this.issuetitle.Text = issuanceObj.ISSUANCETITLE;
                        
                        client.GetIssuanceHouseInfoListAsync(issuanceObj.ISSUANCEID);
                        client.GetIssuanceHouseListAsync(issuanceObj.ISSUANCEID);
                        client.GetDistributeUserListAsync(issuanceObj.ISSUANCEID);
                        
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }


        

        


        private void client_GetIssuanceHouseInfoListCompleted(object sender, GetIssuanceHouseInfoListCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        houseInfoList = e.Result.ToList();
                        //originHouseInfoList = e.Result.ToList();
                        BindData(houseInfoList);
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }

        }

        private void client_GetIssuanceHouseListCompleted(object sender, GetIssuanceHouseListCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        houseList = e.Result.ToList();
                        //originHouseList = e.Result.ToList();
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        

        #endregion

        #region 绑定数据
        private void BindData(List<T_OA_HOUSEINFO> houseobj)
        {
            if (houseobj.Count() > 0)
            {
                InfoObj = houseobj.FirstOrDefault();
            }
            
        }
        #endregion

        #region 按钮事件

        

        
        private void addFrm_Closed(object sender, EventArgs e)
        {
            if (addFrm.houseInfoList != null && addFrm.houseInfoList.Count > 0)
            {
                foreach (var h in addFrm.houseInfoList)
                {
                    var entity = from q in houseInfoList
                                 where h.HOUSEID == q.HOUSEID
                                 select q;
                    if (entity.Count() == 0)
                    {
                        houseInfoList.Add(h);
                    }
                }
                //BindData();
            }
        }

        

        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VIEWHOUSEISSUENOTICE");
            
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            RefreshUI(RefreshedTypes.Close);
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
            return ToolbarItems;
        }       

        private List<ToolbarItem> CreateFormNewButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem            
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            

            return items;
        }

        private List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            

            return items;
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

        #region 保存、添加、删除
        private void Save()
        {
            
        }

        private void Close()
        {
            RefreshUI(refreshType);
        }

        

        
        private void browser_ReloadDataEvent()
        {
            if (addFrm.houseInfoList != null && addFrm.houseInfoList.Count > 0)
            {
                foreach (var h in addFrm.houseInfoList)
                {
                    var entity = from q in houseInfoList
                                 where h.HOUSEID == q.HOUSEID
                                 select q;
                    if (entity.Count() == 0)
                    {
                        houseInfoList.Add(h);
                    }
                }
                //BindData();
            }
        }  

        
        
        
        private IssuanceObjectType GetObjectType(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj)
        {
            if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)
            {
                return IssuanceObjectType.Company;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)
            {
                return IssuanceObjectType.Department;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post)
            {
                return IssuanceObjectType.Post;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
            {
                return IssuanceObjectType.Employee;
            }
            return IssuanceObjectType.Company;
        }
        #endregion

        #region 流程
        public void SumbitAudit()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }

        



        private void Cancel()
        {
            RefreshUI(refreshType);
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            RefreshUI(refreshType);
        }



        private void dgmember_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST StaffV = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST)e.Row.DataContext;

            int index = e.Row.GetIndex();
            var cell = dgmember.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();


        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            personclient.DoClose();
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
