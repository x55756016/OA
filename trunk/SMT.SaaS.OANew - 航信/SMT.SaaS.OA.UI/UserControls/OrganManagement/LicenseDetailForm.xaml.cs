using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class LicenseDetailForm : BaseForm,IClient, IEntityEditor
    {
        private ObservableCollection<T_OA_LICENSEDETAIL> licenseDetail = new ObservableCollection<T_OA_LICENSEDETAIL>();
        private ObservableCollection<T_OA_LICENSEDETAIL> licenseOriginalDetail = new ObservableCollection<T_OA_LICENSEDETAIL>();
        private ObservableCollection<T_OA_LICENSEDETAIL> licenseNewDetail = new ObservableCollection<T_OA_LICENSEDETAIL>();
        //private RegisterType comboSeletedItem;
        private SmtOADocumentAdminClient client;
        private string licenseID;
        //private ObservableCollection<string> param = new ObservableCollection<string>();
        private T_OA_LICENSEMASTER licenseMaster;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        //private bool flagChange = false;    //是否有做修改
        FormTypes formaction;
        public T_OA_LICENSEMASTER LicenseMaster
        {
            get { return licenseMaster; }
            set
            {
                licenseMaster = value;
                this.DataContext = value;
            }
        }

        public ObservableCollection<T_OA_LICENSEDETAIL> LicenseDetail
        {
            get { return licenseDetail; }
            set { licenseDetail = value; }
           
        }

        #region 初始化
        public LicenseDetailForm(FormTypes action,string licenseID)
        {
            formaction = action;
            InitializeComponent();
            this.licenseID = licenseID;
            //InitData();
            InitEvent();
            if (action == FormTypes.Browse)//查看
            {
                this.position.IsEnabled = false;
                this.today.IsEnabled = false;
                comboxType.IsEnabled = false;
                btnAdd.IsEnabled = false;
                
            }
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.GetLicenseListByIdCompleted += new EventHandler<GetLicenseListByIdCompletedEventArgs>(client_GetLicenseListByIdCompleted);
            //client.GetLicenseListCompleted += new EventHandler<GetLicenseListCompletedEventArgs>(client_GetLicenseListCompleted);
            client.UpdateLicenseDetailCompleted += new EventHandler<UpdateLicenseDetailCompletedEventArgs>(client_UpdateLicenseDetailCompleted);
            client.GetLicenseDetailListCompleted += new EventHandler<GetLicenseDetailListCompletedEventArgs>(client_GetLicenseDetailListCompleted);
            Utility.CbxItemBinder(comboxType, "REGISTERTYPE", "0");
            client.GetLicenseListByIdAsync(licenseID);

        }

        void client_GetLicenseListByIdCompleted(object sender, GetLicenseListByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        LicenseMaster = e.Result.ToList()[0];
                        client.GetLicenseDetailListAsync(licenseMaster.LICENSEMASTERID);
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void InitData()
        {
            BindCombo();
        }

        private void BindCombo()
        {
            RegisterType[] Items = { new RegisterType("年检"), new RegisterType("变更"), new RegisterType("注销") };
            comboxType.ItemsSource = Items;
            comboxType.DisplayMemberPath = "registerType";
            comboxType.SelectedIndex = 0;
        }
        #endregion

        #region 绑定DataGird
        private void BindData()
        {
            this.dgDetail.ItemsSource = null;
            this.dgDetail.ItemsSource = LicenseDetail;
            this.dgDetail.Height = licenseDetail.Count * 240;

        }
        #endregion

        #region 完成事件
        private void client_GetLicenseDetailListCompleted(object sender, GetLicenseDetailListCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        foreach (var license in e.Result)
                        {
                            licenseDetail.Add(license);
                            licenseOriginalDetail.Add(license.Clone());
                        }
                        BindData();
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_UpdateLicenseDetailCompleted(object sender, UpdateLicenseDetailCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        //HtmlPage.Window.Alert(e.Result);                        
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //HtmlPage.Window.Alert("证照信息修改成功！");
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "LICENSE"));
                            RefreshUI(refreshType);
                        }
                        else
                        {
                            client.GetLicenseListByIdAsync(licenseID);
                        }
                    }
                }
                else
                {
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        
        #endregion

        #region 按钮事件

        /// <summary>
        /// 新增证照详情
        /// </summary>
        private void LicenseDetailAdd()
        {
            try
            {
                T_OA_LICENSEDETAIL licenseDetailTmp = new T_OA_LICENSEDETAIL();
                licenseDetailTmp.LICENSEDETAILID = Guid.NewGuid().ToString();
                //comboSeletedItem = this.comboxType.SelectedItem as RegisterType;
                licenseDetailTmp.T_OA_LICENSEMASTER = licenseMaster;
                licenseDetailTmp.T_OA_LICENSEMASTER.LICENSEMASTERID = licenseMaster.LICENSEMASTERID;
                //licenseDetailTmp.LICENSEMASTERID = licenseMaster.LICENSEMASTERID;
                licenseDetailTmp.REGISTERTYPE = Utility.GetCbxSelectItemText(comboxType);
                licenseDetailTmp.ADDRESS = licenseMaster.ADDRESS;
                licenseDetailTmp.BUSSINESSAREA = licenseMaster.BUSSINESSAREA;
                licenseDetailTmp.FROMDATE = Convert.ToDateTime(licenseMaster.FROMDATE);
                licenseDetailTmp.TODATE = licenseMaster.TODATE;
                licenseDetailTmp.LICENCENO = licenseMaster.LICENCENO;
                licenseDetailTmp.LEGALPERSON = licenseMaster.LEGALPERSON;
                LicenseDetail.Add(licenseDetailTmp);
                licenseNewDetail.Add(licenseDetailTmp);
                //dgDetail.Height += 240;
                BindData();
            }
            catch (Exception ex)
            {
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        /// <summary>
        /// 新增证照详情
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //EducateHistoryAdd();
            //if (licenseDetail.Count > 0)
            //{
            //    foreach (var detail in licenseDetail)
            //    {
            //        if (Utility.GetCbxSelectItemText(comboxType) == detail.REGISTERTYPE)
            //        {
            //            //HtmlPage.Window.Alert("已存在此类型的信息！");
            //            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ALREADYEXISTED"));
            //            return;
            //        }
            //    }
            //}
            LicenseDetailAdd();
        }

        /// <summary>
        ///  更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
                return;
            }
            List<T_OA_LICENSEDETAIL> removeArr = new List<T_OA_LICENSEDETAIL>();
            for (int i = 0; i < licenseDetail.Count; i++)
            {
                if (i < licenseOriginalDetail.Count)
                {
                    if (licenseDetail[i].ADDRESS != licenseOriginalDetail[i].ADDRESS)
                    {
                        continue;
                    }
                    if (licenseDetail[i].BUSSINESSAREA != licenseOriginalDetail[i].BUSSINESSAREA)
                    {
                        continue;
                    }
                    if (licenseDetail[i].FROMDATE != licenseOriginalDetail[i].FROMDATE)
                    {
                        continue;
                    }
                    if (licenseDetail[i].TODATE != licenseOriginalDetail[i].TODATE)
                    {
                        continue;
                    }
                    if (licenseDetail[i].REGISTERTYPE != licenseOriginalDetail[i].REGISTERTYPE)
                    {
                        continue;
                    }
                    if (licenseDetail[i].REGISTERCHARGE != licenseOriginalDetail[i].REGISTERCHARGE)
                    {
                        continue;
                    }
                    if (licenseDetail[i].REMARK != licenseOriginalDetail[i].REMARK)
                    {
                        continue;
                    }
                    if (licenseDetail[i].LEGALPERSON != licenseOriginalDetail[i].LEGALPERSON)
                    {
                        continue;
                    }
                    if (licenseDetail[i].LICENCENO != licenseOriginalDetail[i].LICENCENO)
                    {
                        continue;
                    }
                    removeArr.Add(licenseDetail[i]);
                }
            }
            if (removeArr.Count > 0)
            {
                foreach (var item in removeArr)
                {
                    licenseDetail.Remove(item);
                }
            }
            licenseOriginalDetail.Clear();
            //更新的
            foreach (var license in licenseDetail)
            {
                T_OA_LICENSEDETAIL licenseDetailTmp = new T_OA_LICENSEDETAIL();
                licenseDetailTmp.LICENSEDETAILID = Guid.NewGuid().ToString();
                licenseDetailTmp.T_OA_LICENSEMASTER = licenseMaster;
                licenseDetailTmp.T_OA_LICENSEMASTER.LICENSEMASTERID = licenseMaster.LICENSEMASTERID;
                //licenseDetailTmp.LICENSEMASTERID = license.LICENSEMASTERID;
                licenseDetailTmp.REGISTERTYPE = license.REGISTERTYPE;
                licenseDetailTmp.REGISTERCHARGE = license.REGISTERCHARGE;
                licenseDetailTmp.ADDRESS = license.ADDRESS;
                licenseDetailTmp.BUSSINESSAREA = license.BUSSINESSAREA;
                licenseDetailTmp.FROMDATE = license.FROMDATE;
                licenseDetailTmp.TODATE = license.TODATE;
                licenseDetailTmp.LICENCENO = license.LICENCENO;
                licenseDetailTmp.LEGALPERSON = license.LEGALPERSON;
                licenseDetailTmp.REMARK = license.REMARK;
                licenseDetailTmp.CREATECOMPANYID = "smt";
                licenseDetailTmp.CREATEDEPARTMENTID = "oa";
                licenseDetailTmp.CREATEPOSTID = "soft";
                licenseDetailTmp.CREATEDATE = DateTime.Now;
                licenseDetailTmp.CREATEUSERID = "admin";
                licenseDetailTmp.UPDATEDATE = DateTime.Now;
                licenseDetailTmp.UPDATEUSERID = "admin";
                licenseOriginalDetail.Add(licenseDetailTmp);
            }            
            licenseMaster.UPDATEDATE = DateTime.Now;
            licenseMaster.UPDATEUSERID = "admin";
            client.UpdateLicenseDetailAsync(licenseOriginalDetail, licenseMaster);
        }

        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EDITTITLE", "LICENSE");
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (formaction == FormTypes.Edit)
            {
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
            }

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

        #region 保存
        private void Save()
        {
            try
            {

                if (Check())
                {
                    RefreshUI(RefreshedTypes.ProgressBar);
                    List<T_OA_LICENSEDETAIL> removeArr = new List<T_OA_LICENSEDETAIL>();
                    for (int i = 0; i < licenseDetail.Count; i++)
                    {
                        if (i < licenseOriginalDetail.Count)
                        {
                            if (licenseDetail[i].ADDRESS != licenseOriginalDetail[i].ADDRESS)
                            {
                                continue;
                            }
                            if (licenseDetail[i].BUSSINESSAREA != licenseOriginalDetail[i].BUSSINESSAREA)
                            {
                                continue;
                            }
                            if (licenseDetail[i].FROMDATE != licenseOriginalDetail[i].FROMDATE)
                            {
                                continue;
                            }
                            if (licenseDetail[i].TODATE != licenseOriginalDetail[i].TODATE)
                            {
                                continue;
                            }
                            if (licenseDetail[i].REGISTERTYPE != licenseOriginalDetail[i].REGISTERTYPE)
                            {
                                continue;
                            }
                            if (licenseDetail[i].REGISTERCHARGE != licenseOriginalDetail[i].REGISTERCHARGE)
                            {
                                continue;
                            }
                            if (licenseDetail[i].REMARK != licenseOriginalDetail[i].REMARK)
                            {
                                continue;
                            }
                            if (licenseDetail[i].LEGALPERSON != licenseOriginalDetail[i].LEGALPERSON)
                            {
                                continue;
                            }
                            if (licenseDetail[i].LICENCENO != licenseOriginalDetail[i].LICENCENO)
                            {
                                continue;
                            } 
                            removeArr.Add(licenseDetail[i]);
                        }
                    }
                    if (removeArr.Count > 0)
                    {
                        foreach (var item in removeArr)
                        {
                            licenseDetail.Remove(item);
                        }
                    }
                    licenseOriginalDetail.Clear();
                    //更新的
                    foreach (var license in licenseDetail)
                    {
                        T_OA_LICENSEDETAIL licenseDetailTmp = new T_OA_LICENSEDETAIL();
                        licenseDetailTmp.LICENSEDETAILID = Guid.NewGuid().ToString();
                        licenseDetailTmp.T_OA_LICENSEMASTER = licenseMaster;
                        licenseDetailTmp.T_OA_LICENSEMASTER.LICENSEMASTERID = licenseMaster.LICENSEMASTERID;
                        licenseDetailTmp.REGISTERTYPE = license.REGISTERTYPE;
                        licenseDetailTmp.REGISTERCHARGE = license.REGISTERCHARGE;
                        licenseDetailTmp.ADDRESS = license.ADDRESS;
                        licenseDetailTmp.BUSSINESSAREA = license.BUSSINESSAREA;
                        licenseDetailTmp.FROMDATE = license.FROMDATE;
                        licenseDetailTmp.TODATE = license.TODATE;
                        licenseDetailTmp.LICENCENO = license.LICENCENO;
                        licenseDetailTmp.LEGALPERSON = license.LEGALPERSON;
                        licenseDetailTmp.REMARK = license.REMARK;  
 
                        licenseDetailTmp.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        licenseDetailTmp.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        licenseDetailTmp.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        licenseDetailTmp.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        licenseDetailTmp.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        licenseDetailTmp.CREATEDATE = DateTime.Now;

                        licenseDetailTmp.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        licenseDetailTmp.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        licenseDetailTmp.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        licenseDetailTmp.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        licenseDetailTmp.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

                        licenseDetailTmp.UPDATEDATE = DateTime.Now;
                        licenseDetailTmp.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        licenseDetailTmp.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                        licenseOriginalDetail.Add(licenseDetailTmp);
                    }
                    licenseMaster.UPDATEDATE = DateTime.Now;
                    licenseMaster.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    licenseMaster.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    client.UpdateLicenseDetailAsync(licenseOriginalDetail, licenseMaster);
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        private void Close()
        {
            //saveType = "1";
            //Save();
            RefreshUI(refreshType);
        }

        //验证
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    //HtmlPage.Window.Alert(h.ErrorMessage);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            return true;
        }
        #endregion

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtBussinessArea.Height = ((Grid)sender).ActualHeight * 0.5;
            dgDetail.Width = ((Grid)sender).ActualWidth * 0.9;
            dgDetail.Height = ((Grid)sender).ActualHeight * 0.99;   
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
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

    public class RegisterType
    {
        public string registerType { set; get; }
        public RegisterType(string registerType)
        {
            this.registerType = registerType;
        }
    }

    /// <summary>
    ///  克隆实体
    /// </summary>
    public static class Clones
    {
        public static T Clone<T>(this T source)
        {
            var dcs = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            using (var ms = new System.IO.MemoryStream())
            {
                dcs.WriteObject(ms, source);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)dcs.ReadObject(ms);
            }
        }
    }
}
