/****************************************************************
 * 作者：    亢晓方
 * 书写时间：2012/9/6 11:17:30 
 * 内容概要： 
 *  ------------------------------------------------------
 * 修改：    
****************************************************************/
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
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.Utils;
using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer.Views.ModelDefine
{
    public partial class ModelCodeEdit : ChildWindow
    {
        private ObservableCollection<AppModel> appModel;//XML模块代码
        private ObservableCollection<AppSystem> appSystem;//XML系统代码
        public event EventHandler SaveCompleted;
        private FLOW_MODELDEFINE_T entity = null;
        private Flow_ModelDefineClient client = new Flow_ModelDefineClient();
        ObservableCollection<FLOW_MODELDEFINE_FLOWCANCLE> FlowCancelCompanyList = new System.Collections.ObjectModel.ObservableCollection<FLOW_MODELDEFINE_FLOWCANCLE>();
        ObservableCollection<FLOW_MODELDEFINE_FREEFLOW> FreeFlowCompanyList = new System.Collections.ObjectModel.ObservableCollection<FLOW_MODELDEFINE_FREEFLOW>();
        public ModelCodeEdit(FLOW_MODELDEFINE_T define, ObservableCollection<AppModel> modelCodeList, ObservableCollection<AppSystem> systemList)
        {
            InitializeComponent();
            this.entity = define;
            appModel = modelCodeList;
            appSystem = systemList;
            if (define == null)
            {             
                this.cbSystemCode.ItemsSource = systemList;
                this.cbSystemCode.SelectedIndex = 0;
                //this.cbModelCode.ItemsSource = modelCodeList;
                //this.cbModelCode.SelectedIndex = 0;
                this.btnSelect1.Visibility = Visibility.Collapsed;
                this.tbCancel.Visibility = Visibility.Collapsed;
                this.dgCompany1.Visibility = Visibility.Collapsed;
                this.dataPager1.Visibility = Visibility.Collapsed;
                this.btnSelect2.Visibility = Visibility.Collapsed;
                this.tbFree.Visibility = Visibility.Collapsed;
                this.dgCompany2.Visibility = Visibility.Collapsed;
                this.dataPager2.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.cbSystemCode.ItemsSource = systemList;
                //this.cbModelCode.ItemsSource = modelCodeList;
                this.cbSystemCode.Selected<AppSystem>("Name", define.SYSTEMCODE);
                this.cbModelCode.Selected<AppModel>("Name", define.MODELCODE);
                GetAllOwnerCompanyId();
                BindCompanyList1();
                BindCompanyList2();
                this.cbSystemCode.IsEnabled = false;
                this.cbModelCode.IsEnabled = false;
                pBar.Start();
            }       
            InitWcfEvent();
        }

        private void InitWcfEvent()
        {
            client.AddModelDefineCompleted+=(e,arg)=>
                {
                    pBar.Stop();
                    if (arg.Error == null)
                    {
                        if (arg.Result != null)
                        {
                            if (arg.Result == "1")
                            {
                                this.btnSelect1.Visibility = Visibility.Visible;
                                this.tbCancel.Visibility = Visibility.Visible;
                                this.dgCompany1.Visibility = Visibility.Visible;
                                this.dataPager1.Visibility = Visibility.Visible;
                                this.btnSelect2.Visibility = Visibility.Visible;
                                this.tbFree.Visibility = Visibility.Visible;
                                this.dgCompany2.Visibility = Visibility.Visible;
                                this.dataPager2.Visibility = Visibility.Visible;
                                ComfirmWindow.ConfirmationBox("提示信息", "新增成功", "确定");
                                if (this.SaveCompleted != null)
                                {
                                    this.SaveCompleted(this, null);
                                }
                               
                            }
                            else if (arg.Result == "10")
                            {
                                ComfirmWindow.ConfirmationBox("提示信息", "录入的模块定义重复！", "确定");                               
                            }
                            else
                            {
                                ComfirmWindow.ConfirmationBox("提示信息", "新增失败！", "确定");
                            }
                        }
                    }
                 
                };
            client.UpdateModelDefineCompleted += (e, arg) =>
            {
                pBar.Stop();
                if (arg.Error == null)
                {
                    if (arg.Result != null)
                    {
                        if (arg.Result == "1")
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", "修改成功", "确定");
                            if (this.SaveCompleted != null)
                            {
                                this.SaveCompleted(this, null);
                            }

                        }
                        else if (arg.Result == "10")
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", "修改的模块定义重复！", "确定");
                        
                        }
                        else
                        {
                            ComfirmWindow.ConfirmationBox("提示信息", "修改失败！", "确定");
                        }
                    }
                }
               
            };
            client.GetFlowCancleListCompleted += (e, args) =>
                {
                    pBar.Stop();
                    if (args.Error == null)
                    {
                        if (args.Result != null)
                        {
                            dgCompany1.ItemsSource = args.Result;
                            FlowCancelCompanyList = args.Result;
                            dataPager1.PageCount = args.pageCount;
                        }
                    }
                  
                };
            client.GetFreeFlowListCompleted += (e, args) =>
            {
                pBar.Stop();
                if (args.Error == null)
                {
                    if (args.Result != null)
                    {
                        dgCompany2.ItemsSource = args.Result;
                        FreeFlowCompanyList = args.Result;
                        dataPager2.PageCount = args.pageCount;
                    }
                }
             
            };
        }
         
        #region 按钮事件
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtModelCode.Text.Trim() == "")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "模块代码不能为空！", "确定");
                return;
            }       
            if (entity == null)
            {
                entity = new FLOW_MODELDEFINE_T();
                entity.FlowCancelCompanyList = FlowCancelCompanyList;
                entity.FreeFlowCompanyList = FreeFlowCompanyList;
                entity.MODELDEFINEID = Guid.NewGuid().ToString();
                entity.MODELCODE = this.txtModelCode.Text.Trim();
                entity.SYSTEMCODE = (this.cbSystemCode.SelectedItem as AppSystem).Name;
                entity.SYSTEMNAME = (this.cbSystemCode.SelectedItem as AppSystem).Description;
                entity.DESCRIPTION = this.txtModelName.Text.Trim();
                entity.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                entity.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                entity.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                entity.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                entity.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.AddModelDefineAsync(entity);
                //pBar.Start();
            }
            else
            {
                entity.FlowCancelCompanyList = FlowCancelCompanyList;
                entity.FreeFlowCompanyList = FreeFlowCompanyList;
                entity.MODELCODE = this.txtModelCode.Text.Trim();
                entity.SYSTEMCODE = (this.cbSystemCode.SelectedItem as AppSystem).Name;
                entity.DESCRIPTION = this.txtModelName.Text.Trim();            
                entity.EDITUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                entity.EDITUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.UpdateModelDefineAsync(entity);
               // pBar.Start();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #endregion

        #region 下拉事件
        private void cbSYSTEMCODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item =this.cbSystemCode.SelectedItem as AppSystem;
            if (item != null && item.ObjectFolder != null)
            {
                if (appModel != null)
                {
                    //string name = "";
                    //int i = 1;
                    //foreach (var m in appModel)
                    //{
                    //    name +="【"+i.ToString()+"】"+ m.Name + " ====" + (m.ObjectFolder!=null?m.ObjectFolder.ToString():"") + "\r\n";
                    //    i++;
                    //}
                    var models = from ent in appModel
                                 where ent.ObjectFolder == item.ObjectFolder || ent.Name == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cbModelCode.ItemsSource = null;
                        this.cbModelCode.ItemsSource = models;
                        this.cbModelCode.SelectedIndex = 0;
                    }
                }
            }
            if (item != null && item.Name == "0")
            {
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.Name == item.Name
                                 select ent;
                    if (models.Count() > 0)
                    {
                        this.cbModelCode.ItemsSource = null;
                        this.cbModelCode.ItemsSource = models;
                        this.cbModelCode.SelectedIndex = 0;
                    }
                }
            }

        }
        private void cbModelCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = this.cbModelCode.SelectedItem as AppModel;
            if (item != null && this.cbModelCode.SelectedIndex > 0)
            {
                this.txtModelCode.Text = item.Name;
                this.txtModelName.Text = item.Description;
            }
            else
            {
                this.txtModelCode.Text = "";
                this.txtModelName.Text = "";
            }
        }

        #endregion


        #region 选择公司(流程是否允许提单人撒回)
        private void btnSelect1_Click(object sender, RoutedEventArgs e)
        {

            CompanyLookUp up = new CompanyLookUp();
            up.SelectedClick += (obj, ev) =>
            {
                if (up.SelectList != null)
                {
                    foreach (var item in up.SelectList)
                    {
                        var companyObj = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance);
                        string companyid = companyObj.COMPANYID;
                        string companyName = companyObj.CNAME;
                        FLOW_MODELDEFINE_FLOWCANCLE entity = new FLOW_MODELDEFINE_FLOWCANCLE();
                        entity.MODELDEFINEFLOWCANCLEID = Guid.NewGuid().ToString().Replace("-", ""); ;
                        entity.MODELCODE = this.txtModelCode.Text;//模块代码
                        entity.COMPANYNAME = companyName;//允许提单人撒回流程公司名称
                        entity.COMPANYID = companyid;//允许提单人撒回流程公司ID
                        entity.CREATEUSERID = Utility.CurrentUser.OWNERID;//创建人ID
                        entity.CREATEUSERNAME = Utility.CurrentUser.USERNAME;//创建人名
                        entity.CREATECOMPANYID = Utility.CurrentUser.OWNERCOMPANYID;//创建公司ID
                        entity.CREATEDEPARTMENTID = Utility.CurrentUser.OWNERDEPARTMENTID;//创建部门ID
                        entity.CREATEPOSTID = Utility.CurrentUser.OWNERPOSTID;//创建岗位ID
                        entity.CREATEDATE = DateTime.Now;//创建时间

                        var company = from ee in FlowCancelCompanyList
                                      where ee.COMPANYID == companyid
                                      select ee;
                        if (company.FirstOrDefault() == null)
                        {
                            FlowCancelCompanyList.Add(entity);
                        }                     
                    }
                    dgCompany1.ItemsSource = FlowCancelCompanyList;
                }
            };
            up.Show();
            #region
            //object objs = null;
            //if (Application.Current.Resources["CurrentUserID"] != null)
            //{
            //    objs = Application.Current.Resources["CurrentUserID"];
            //    Application.Current.Resources.Remove("CurrentUserID");
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            //if (Application.Current.Resources["CurrentUserID"] == null)
            //{
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            //SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup(Utility.CurrentUser.EMPLOYEEID, "3", "");
            //lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            //lookup.MultiSelected = true;
            //lookup.SelectedClick += (obj, ev) =>
            //{
            //    if (lookup.SelectedObj != null)
            //    {
                  
            //        foreach (var item in lookup.SelectedObj)
            //        {
            //            var companyObj = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance);
            //            string companyid = companyObj.COMPANYID;
            //            string companyName = companyObj.CNAME;
            //            FLOW_MODELDEFINE_FLOWCANCLE entity = new FLOW_MODELDEFINE_FLOWCANCLE();
            //            entity.MODELDEFINEFLOWCANCLEID = Guid.NewGuid().ToString().Replace("-", ""); ;
            //            entity.MODELCODE = this.txtModelCode.Text;//模块代码
            //            entity.COMPANYNAME = companyName;//允许提单人撒回流程公司名称
            //            entity.COMPANYID = companyid;//允许提单人撒回流程公司ID
            //            entity.CREATEUSERID = Utility.CurrentUser.OWNERID;//创建人ID
            //            entity.CREATEUSERNAME = Utility.CurrentUser.USERNAME;//创建人名
            //            entity.CREATECOMPANYID = Utility.CurrentUser.OWNERCOMPANYID;//创建公司ID
            //            entity.CREATEDEPARTMENTID = Utility.CurrentUser.OWNERDEPARTMENTID;//创建部门ID
            //            entity.CREATEPOSTID = Utility.CurrentUser.OWNERPOSTID;//创建岗位ID
            //            entity.CREATEDATE = DateTime.Now;//创建时间

            //            var company = from ee in FlowCancelCompanyList
            //                          where ee.COMPANYID == companyid
            //                          select ee;
            //            if (company.FirstOrDefault() == null)
            //            {
            //                FlowCancelCompanyList.Add(entity);
            //            }
            //            Application.Current.Resources.Remove("CurrentUserID");
            //            if (objs != null)
            //            {
            //                Application.Current.Resources.Add("CurrentUserID", objs);
            //            }
            //        }
            //        dgCompany1.ItemsSource = FlowCancelCompanyList;
            //    }
            //};
            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });


            #endregion
        }
        private void btnDel1_Click(object sender, RoutedEventArgs e)
        {
            string companyid = (sender as Button).Tag.ToString();
            var company = from ee in FlowCancelCompanyList
                          where ee.COMPANYID == companyid
                          select ee;
            if (company.FirstOrDefault() != null)
            {
                client.DeleteFlowCancleAsync(txtModelCode.Text, companyid);
                client.DeleteFlowCancleCompleted += (obj, args) =>
                {
                    if (args.Error == null)
                    {                       
                        FlowCancelCompanyList.Remove(company.FirstOrDefault());
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox("错误信息", args.Error.ToString(), "确定");
                    }
                };

            }

        }
        void BindCompanyList1()
        {
            if (!string.IsNullOrEmpty(allOwnerCompanyId))
            {
                int pageCont = 0;
                string strFilter = "";
                #region 选择条件过滤
                strFilter += " AND MODELCODE='" + this.txtModelCode.Text.Trim() + "' AND COMPANYID in (" + allOwnerCompanyId + ")";
                #endregion

                client.GetFlowCancleListAsync(dataPager1.PageSize, dataPager1.PageIndex, strFilter, "CREATEDATE DESC", pageCont);
            }
        }

        /// <summary>
        /// 当前页引索
        /// </summary>
        int curentIndex = 1;
        /// <summary>
        /// 分页事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>   
        private void dataPager1_Click(object sender, RoutedEventArgs e)
        {
            if (dataPager1.PageIndex > 0 && curentIndex != dataPager1.PageIndex)
            {
                //loadbar.Start();
                BindCompanyList1();
                curentIndex = dataPager1.PageIndex;
            }

        }
        #endregion

        public string allOwnerCompanyId = "";
        public void GetAllOwnerCompanyId()
        {
            allOwnerCompanyId = "";
            #region 加载所有公司
            try
            {
                string path = "silverlightcache\\" + Utility.CurrentUser.EMPLOYEEID + "ID.txt";
                var company = SLCache.GetCache<string>(path, 5);
                if (company != null)
                {
                    allOwnerCompanyId = company;
                    // return company;
                }
                else
                {
                    SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient osc = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
                    osc.GetCompanyViewAsync(Utility.CurrentUser.EMPLOYEEID, "3", "");
                    osc.GetCompanyViewCompleted += (obj, args) =>
                    {
                        if (args.Result != null)
                        {
                            string companylist = "";
                            foreach (var ent in args.Result)
                            {
                                companylist += "'" + ent.COMPANYID + "',";
                            }
                            allOwnerCompanyId = companylist.TrimEnd(',');
                            BindCompanyList1();
                            BindCompanyList2();
                            SLCache.SaveData<string>(companylist.TrimEnd(','), path);
                        }
                    };
                    System.Threading.Thread.Sleep(1000);//等异步完成
                }
            }
            catch (Exception ee)
            {
                ComfirmWindow.ConfirmationBox("错误信息", ee.Message, "确定");
            }
            #endregion
        }
        #region 选择公司(该模块允许[自选流程]的公司)
        private void btnSelect2_Click(object sender, RoutedEventArgs e)
        {

            CompanyLookUp up = new CompanyLookUp();
            up.SelectedClick += (obj, ev) =>
            {
                if (up.SelectList != null)
                {
                    foreach (var item in up.SelectList)
                    {
                        var companyObj = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance);
                        string companyid = companyObj.COMPANYID;
                        string companyName = companyObj.CNAME;
                        FLOW_MODELDEFINE_FREEFLOW entity = new FLOW_MODELDEFINE_FREEFLOW();
                        entity.MODELDEFINEFREEFLOWID = Guid.NewGuid().ToString().Replace("-", ""); ;
                        entity.MODELCODE = this.txtModelCode.Text;//模块代码
                        entity.COMPANYNAME = companyName;//允许[自选流程]的公司名称
                        entity.COMPANYID = companyid;//允许[自选流程]的公司D
                        entity.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;// Utility.CurrentUser.EMPLOYEEID;//创建人ID
                        entity.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;// Utility.CurrentUser.USERNAME;//创建人名
                        entity.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;// Utility.CurrentUser.OWNERCOMPANYID;//创建公司ID
                        entity.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//  Utility.CurrentUser.OWNERDEPARTMENTID;//创建部门ID
                        entity.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;//  Utility.CurrentUser.OWNERPOSTID;//创建岗位ID
                        entity.CREATEDATE = DateTime.Now;//创建时间
                        var company = from ee in FreeFlowCompanyList
                                      where ee.COMPANYID == companyid
                                      select ee;
                        if (company.FirstOrDefault() == null)
                        {
                            FreeFlowCompanyList.Add(entity);
                        }                        
                    }
                    dgCompany2.ItemsSource = FreeFlowCompanyList;
                }
            };
            up.Show();
            #region
            //object objs = null;
            //if (Application.Current.Resources["CurrentUserID"] != null)
            //{
            //    objs = Application.Current.Resources["CurrentUserID"];
            //    Application.Current.Resources.Remove("CurrentUserID");
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            //if (Application.Current.Resources["CurrentUserID"] == null)
            //{
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            //SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup(Utility.CurrentUser.EMPLOYEEID, "3", "");
            //lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            //lookup.MultiSelected = true;
            //lookup.SelectedClick += (obj, ev) =>
            //{
            //    if (lookup.SelectedObj != null)
            //    {                   
            //        foreach (var item in lookup.SelectedObj)
            //        {
            //            var companyObj = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)(item.ObjectInstance);
            //            string companyid = companyObj.COMPANYID;
            //            string companyName = companyObj.CNAME;
            //            FLOW_MODELDEFINE_FREEFLOW entity = new FLOW_MODELDEFINE_FREEFLOW();
            //            entity.MODELDEFINEFREEFLOWID = Guid.NewGuid().ToString().Replace("-", ""); ;
            //            entity.MODELCODE = this.txtModelCode.Text;//模块代码
            //            entity.COMPANYNAME = companyName;//允许[自选流程]的公司名称
            //            entity.COMPANYID = companyid;//允许[自选流程]的公司D
            //            entity.CREATEUSERID = Utility.CurrentUser.OWNERID;//创建人ID
            //            entity.CREATEUSERNAME = Utility.CurrentUser.USERNAME;//创建人名
            //            entity.CREATECOMPANYID = Utility.CurrentUser.OWNERCOMPANYID;//创建公司ID
            //            entity.CREATEDEPARTMENTID = Utility.CurrentUser.OWNERDEPARTMENTID;//创建部门ID
            //            entity.CREATEPOSTID = Utility.CurrentUser.OWNERPOSTID;//创建岗位ID
            //            entity.CREATEDATE = DateTime.Now;//创建时间
            //            FreeFlowCompanyList.Add(entity);

            //            Application.Current.Resources.Remove("CurrentUserID");
            //            if (objs != null)
            //            {
            //                Application.Current.Resources.Add("CurrentUserID", objs);
            //            }
            //        }
            //        dgCompany2.ItemsSource = FreeFlowCompanyList;
            //    }
            //};
            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });


            #endregion
        }
        private void btnDel2_Click(object sender, RoutedEventArgs e)
        {
            string companyid = (sender as Button).Tag.ToString();
            var company = from ee in FreeFlowCompanyList
                          where ee.COMPANYID == companyid
                          select ee;
            if (company.FirstOrDefault() != null)
            {
                client.DeleteFreeFlowAsync(txtModelCode.Text, companyid);
                client.DeleteFreeFlowCompleted += (obj, args) =>
                {
                    if (args.Error == null)
                    {
                        FreeFlowCompanyList.Remove(company.FirstOrDefault());
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox("错误信息", args.Error.ToString(), "确定");
                    }
                };

            }
        }
        void BindCompanyList2()
        {
            if (!string.IsNullOrEmpty(allOwnerCompanyId))
            {
                int pageCont = 0;
                string strFilter = "";
                #region 选择条件过滤
                strFilter += " AND MODELCODE='" + txtModelCode.Text.Trim() + "' AND COMPANYID in (" + allOwnerCompanyId + ")";
                #endregion

                client.GetFreeFlowListAsync(dataPager2.PageSize, dataPager2.PageIndex, strFilter, "CREATEDATE DESC", pageCont);
            }
        }

        /// <summary>
        /// 当前页引索
        /// </summary>
        int curentIndex2 = 1;
        /// <summary>
        /// 分页事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>   
        private void dataPager2_Click(object sender, RoutedEventArgs e)
        {
            if (dataPager2.PageIndex > 0 && curentIndex2 != dataPager2.PageIndex)
            {
                //loadbar.Start();
                BindCompanyList2();
                curentIndex2 = dataPager2.PageIndex;
            }

        }
        #endregion

    }
}

