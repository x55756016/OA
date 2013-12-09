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

using SMT.Saas.Tools.SalaryWS;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.AuditControl;

using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalaryArchiveForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public T_HR_SALARYARCHIVE SalaryArchive { get; set; }
        public T_HR_SALARYSTANDARD salaryStand { get; set; }
        public SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST EmployeePost { get; set; }
        public List<T_HR_SALARYLEVEL> SalaryLevel { get; set; }
        private List<V_SALARYARCHIVEITEM> archiveItemsList { get; set; }
        private bool canSubmit = false;//能否提交审核
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient clientperson = new PersonnelServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private T_HR_SALARYARCHIVEHIS history = new T_HR_SALARYARCHIVEHIS();
        private string strArchiveID { get; set; }
        private int intSalaryLevel;
        private DateTime changeDate { get; set; }
        private string PostLevel { get; set; }
        private decimal? oldSalary { get; set; }
        private decimal? oldsalaryLevel { get; set; }
        private decimal? oldpostlevel { get; set; }

        private string createCompanyID { get; set; }//传给流程的公司ID
        private string createPostID { get; set; } //传给流程的岗位ID
        private string createDepartmentID { get; set; }//传给流程的部门ID
        private bool isNew;//true 表示从快捷方式新建单据
        public T_HR_CUSTOMGUERDONARCHIVE CustomArchive { get; set; }//个人活动经费

        public SalaryArchiveForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            isNew = true;
            strArchiveID = string.Empty;
            this.Loaded += new RoutedEventHandler(SalaryArchiveForm_Loaded);
            lkBalancePost.TxtLookUp.Text = "跨机构发薪时使用";
        }
        public SalaryArchiveForm(FormTypes type, string archiveID)
        {
            InitializeComponent();
            FormType = type;
            strArchiveID = archiveID;
            this.Loaded += new RoutedEventHandler(SalaryArchiveForm_Loaded);
        }

        void SalaryArchiveForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            if (FormType == FormTypes.Audit || FormType == FormTypes.Browse)
            {
                EnableControl();
            }
            if (FormType == FormTypes.New)
            {
                cbxSalaryLevel.IsEnabled = false;
            }
            if (string.IsNullOrEmpty(strArchiveID))
            {
                SalaryArchive = new T_HR_SALARYARCHIVE();
                SalaryArchive.SALARYARCHIVEID = Guid.NewGuid().ToString();
                SalaryArchive.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalaryArchive.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                SalaryArchive.CHECKSTATE = Convert.ToInt16(CheckStates.UnSubmit).ToString();
                SalaryArchive.OTHERSUBJOIN = System.DateTime.Now.Year;//生效年份
                SalaryArchive.OTHERADDDEDUCT = System.DateTime.Now.Month;//生效月份
                this.DataContext = SalaryArchive;
                SetToolBar();
            }
            else
            {
                client.GetSalaryArchiveMasterByIDAsync(strArchiveID);
                lkEmployee.IsEnabled = false;
                txtSalaryStandard.IsEnabled = false;
                TxtRemark.IsEnabled = true;
                strArchiveID = strArchiveID;
                // LoadCustomArchive(strArchiveID);

                //获取个人活动经费
                client.GetCustomGuerdonArchiveByArchiveIDAsync(strArchiveID);
                client.GetCustomGuerdonArchiveByArchiveIDCompleted += (o, ee) =>
                {
                    if (ee.Result != null && ee.Result.Count != 0)
                    {
                        CustomArchive = ee.Result.FirstOrDefault();
                        if (ee.Result.FirstOrDefault().SUM != null)
                        {
                            txtSum.Text = ee.Result.FirstOrDefault().SUM.ToString();
                        }
                        else
                        {
                            txtSum.Text = "0";
                        }
                        if (ee.Result.FirstOrDefault().REMARK != null)
                        {
                            txtSumRemark.Text = ee.Result.FirstOrDefault().REMARK.ToString();
                        }

                    }
                };
            }
        }

        private void EnableControl()
        {
            lkSalarySolution.IsEnabled = false;
            lkEmployee.IsEnabled = false;
            txtSalaryStandard.IsEnabled = false;
            // cbIsEnabled.IsEnabled = false;
            txtMonth.IsEnabled = false;
            txtYear.IsEnabled = false;
            cbxPostLevel.IsEnabled = false;
            cbxSalaryLevel.IsEnabled = false;
            TxtRemark.IsReadOnly = true;
            ToolBar.IsEnabled = false;
            DtGridDetail.IsEnabled = false;
            txtSum.IsReadOnly = true;
            txtSumRemark.IsReadOnly = true;
        }

        private void InitParas()
        {
            client.SalaryArchiveHisItemsAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryArchiveHisItemsAddCompleted);
            client.GetSalaryArchiveMasterByIDCompleted += new EventHandler<GetSalaryArchiveMasterByIDCompletedEventArgs>(client_GetSalaryArchiveMasterByIDCompleted);
            client.GetSalaryArchiveItemPagingCompleted += new EventHandler<GetSalaryArchiveItemPagingCompletedEventArgs>(client_GetSalaryArchiveItemPagingCompleted);
            client.SalaryArchiveItemAddCompleted += new EventHandler<SalaryArchiveItemAddCompletedEventArgs>(client_SalaryArchiveItemAddCompleted);
            //client.SalaryArchiveItemDeleteCompleted += new EventHandler<SalaryArchiveItemDeleteCompletedEventArgs>(client_SalaryArchiveItemDeleteCompleted);
            client.SalaryArchiveUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryArchiveUpdateCompleted);
            // client.GetCustomGuerdonArchiveWithPagingCompleted += new EventHandler<GetCustomGuerdonArchiveWithPagingCompletedEventArgs>(client_GetCustomGuerdonArchiveWithPagingCompleted);
            client.CreateSalaryArchiveCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CreateSalaryArchiveCompleted);
            client.GetStandardByEmployeeIDAndSolutionIDAndSalarylevelCompleted += new EventHandler<GetStandardByEmployeeIDAndSolutionIDAndSalarylevelCompletedEventArgs>(client_GetStandardByEmployeeIDAndSolutionIDAndSalarylevelCompleted);
            clientperson.GetLastChangeByEmployeeIDCompleted += new EventHandler<GetLastChangeByEmployeeIDCompletedEventArgs>(clientperson_GetLastChangeByEmployeeIDCompleted);
            client.GetOldFixSalaryCompleted += new EventHandler<GetOldFixSalaryCompletedEventArgs>(client_GetOldFixSalaryCompleted);
            client.GetFixSalaryCompleted += new EventHandler<GetFixSalaryCompletedEventArgs>(client_GetFixSalaryCompleted);

            #region  初始化按钮
            // ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            // ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            //  ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //  ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);

            //ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            //ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;

            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            #endregion

        }



        void client_GetFixSalaryCompleted(object sender, GetFixSalaryCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (oldSalary != null)
                {
                    decimal salarynew = e.Result;
                    SalaryArchive.BALANCE = (salarynew - oldSalary);
                    txtbBalance.Text = SalaryArchive.BALANCE.ToString();
                }


            }
        }

        void client_GetOldFixSalaryCompleted(object sender, GetOldFixSalaryCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    List<string> tmp = e.Result.ToList();
                    //入职新公司不取原来的薪资档案信息
                    string nowCompany = (lkSalaryCompany.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
                    if (tmp.Count > 0 && nowCompany == tmp[3])
                    {
                        bool hasOldLevle = true;
                        oldSalary = Convert.ToDecimal(tmp[0]);
                        if (!string.IsNullOrEmpty(tmp[1]))
                        {
                            oldsalaryLevel = Convert.ToDecimal(tmp[1]);

                            SalaryArchive.OLDSALARYLEVEL = oldsalaryLevel;
                        }
                        else
                        {
                            hasOldLevle = false;
                        }
                        if (!string.IsNullOrEmpty(tmp[2]))
                        {
                            oldpostlevel = Convert.ToDecimal(tmp[2]);
                            SalaryArchive.OLDPOSTLEVEL = oldpostlevel;
                        }
                        else
                        {
                            hasOldLevle = false;
                        }
                        if (hasOldLevle == true)
                        {
                            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxOldSalaryLevel.Items)
                            {
                                if (item.DICTIONARYVALUE == oldsalaryLevel)
                                {
                                    cbxOldSalaryLevel.SelectedItem = item;
                                    break;
                                }
                            }
                            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxOldPostLevel.Items)
                            {
                                if (item.DICTIONARYVALUE == oldpostlevel)
                                {
                                    cbxOldPostLevel.SelectedItem = item;
                                    break;
                                }
                            }
                        }

                    }
                }

            }
        }

        void client_SalaryArchiveUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        void client_SalaryArchiveHisItemsAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void clientperson_GetLastChangeByEmployeeIDCompleted(object sender, GetLastChangeByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    changeDate = Convert.ToDateTime(e.Result.CHANGEDATE);
                    //外部异动 去异动时间不足半年
                    if (changeDate.AddMonths(6) > DateTime.Now && e.Result.POSTCHANGCATEGORY == "1")
                    {
                        SalaryArchive.REMARK = "跨机构异动不足半年不能调薪";
                    }
                }
            }
        }

        void client_GetStandardByEmployeeIDAndSolutionIDAndSalarylevelCompleted(object sender, GetStandardByEmployeeIDAndSolutionIDAndSalarylevelCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //if (e.UserState.ToString() == "New")
                //{
                if (e.Result != null)
                {
                    SalaryArchive.T_HR_SALARYSTANDARD = new T_HR_SALARYSTANDARD();
                    SalaryArchive.T_HR_SALARYSTANDARD.SALARYSTANDARDID = e.Result.SALARYSTANDARDID;
                    SalaryArchive.T_HR_SALARYSTANDARD.SALARYSTANDARDNAME = e.Result.SALARYSTANDARDNAME;
                    txtSalaryStandard.Text = e.Result.SALARYSTANDARDNAME;
                    //  txtbBalance.Text = (e.Result.BASESALARY - oldSalary).ToString();
                }
                else
                {
                    txtSalaryStandard.Text = "";
                }
                //}
                //else
                //{
                //    oldSalary = e.Result.BASESALARY;
                //}
            }
        }
        void client_SalaryArchiveItemAddCompleted(object sender, SalaryArchiveItemAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {

                if (e.Result == 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }
                // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYARCHIVEITEM"));
            }
            RefreshUI(RefreshedTypes.All);
            LoadData();
        }

        //void client_SalaryArchiveItemDeleteCompleted(object sender, SalaryArchiveItemDeleteCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYITEM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYITEM"));
        //    }
        //    RefreshUI(RefreshedTypes.All);
        //    LoadData();
        //}

        void client_GetSalaryArchiveItemPagingCompleted(object sender, GetSalaryArchiveItemPagingCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                // ObservableCollection<V_SALARYARCHIVEITEM> its = new ObservableCollection<V_SALARYARCHIVEITEM>();

                List<V_SALARYARCHIVEITEM> its = new List<V_SALARYARCHIVEITEM>();
                if (e.Result != null)
                {
                    its = e.Result.OrderBy(m => m.ORDERNUMBER).ToList();
                    archiveItemsList = new List<V_SALARYARCHIVEITEM>();
                    foreach (var it in its)
                    {
                        V_SALARYARCHIVEITEM item = new V_SALARYARCHIVEITEM();
                        item.SALARYARCHIVEITEM = it.SALARYARCHIVEITEM;
                        item.SUM = it.SUM;
                        item.SALARYITEMNAME = it.SALARYITEMNAME;
                        item.REMARK = it.REMARK;
                        archiveItemsList.Add(item);
                    }
                    try
                    {
                        for (int i = 0; i < its.Count; i++)
                        {
                            if (its[i].SUM != null)
                                its[i].SUM = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(its[i].SUM);
                        }
                    }
                    catch { }
                    for (int i = 0; i < its.Count; )
                    {
                        if (its[i].SALARYITEMNAME == "应发小计" || its[i].SALARYITEMNAME == "实发工资")
                        {
                            its.Remove(its[i]);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    DtGrid.ItemsSource = its;
                    dataPager.PageCount = e.pageCount;
                }
            }
        }

        void client_CreateSalaryArchiveCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //个人活动能够经费操作
                decimal? DeSum = 0;
                try
                {
                    DeSum = decimal.Parse(txtSum.Text.Trim());
                }
                catch (Exception)
                {
                    DeSum = 0;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "个人活动经费输入错误，请重新输入", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                T_HR_CUSTOMGUERDONARCHIVE custom = new T_HR_CUSTOMGUERDONARCHIVE
                {

                    //SUM = decimal.Parse(txtSum.Text.Trim()),
                    SUM = DeSum,
                    REMARK = txtSumRemark.Text.Trim()
                };
                if (CustomArchive != null)
                {
                    custom.CUSTOMGUERDONARCHIVEID = CustomArchive.CUSTOMGUERDONARCHIVEID;
                }
                else
                {
                    custom.CUSTOMGUERDONARCHIVEID = Guid.NewGuid().ToString();
                }
                custom.T_HR_SALARYARCHIVE = new T_HR_SALARYARCHIVE();
                custom.T_HR_SALARYARCHIVE.SALARYARCHIVEID = SalaryArchive.SALARYARCHIVEID;
                client.CustomGuerdonArchiveUpdateAsync(custom);

                FormTypes temp = FormType;
                FormType = FormTypes.Edit;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                if (temp != FormTypes.New)
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                else
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                canSubmit = true;
                RefreshUI(RefreshedTypes.AuditInfo);
                LoadData();
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_GetSalaryArchiveMasterByIDCompleted(object sender, GetSalaryArchiveMasterByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                SalaryArchive = e.Result.archive;
                this.DataContext = SalaryArchive;

                //显示员工名
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = new SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE();
                ent.EMPLOYEEID = SalaryArchive.EMPLOYEEID;
                ent.EMPLOYEECODE = SalaryArchive.EMPLOYEECODE;
                ent.EMPLOYEECNAME = SalaryArchive.EMPLOYEENAME;
                lkEmployee.DataContext = ent;
                string fullName = string.Empty;
                if (string.IsNullOrEmpty(e.Result.PostName))
                {
                    fullName = SalaryArchive.EMPLOYEENAME;
                }
                else
                {
                    fullName = SalaryArchive.EMPLOYEENAME + " - " + e.Result.PostName + " - " + GetFullOrgName(e.Result.DepartmentID);
                }
                lkEmployee.TxtLookUp.Text = fullName;

                //显示薪资方案名
                T_HR_SALARYSOLUTION tmp = new T_HR_SALARYSOLUTION();
                tmp.SALARYSOLUTIONNAME = SalaryArchive.SALARYSOLUTIONNAME;
                tmp.SALARYSOLUTIONID = SalaryArchive.SALARYSOLUTIONID;
                lkSalarySolution.DataContext = tmp;

                //发薪机构
                var companys = Application.Current.Resources["SYS_CompanyInfo"] as List<OrganizationWS.T_HR_COMPANY>;
                if (companys != null)
                {
                    OrganizationWS.T_HR_COMPANY company = companys.Where(s => s.COMPANYID == SalaryArchive.PAYCOMPANY).FirstOrDefault();
                    lkSalaryCompany.DataContext = company;
                }

                //考勤机构
                if (!string.IsNullOrWhiteSpace(SalaryArchive.ATTENDANCEORGNAME))
                {
                    lkAttendanceCompany.TxtLookUp.Text = SalaryArchive.ATTENDANCEORGNAME;
                }

                //结算岗位
                if (!string.IsNullOrWhiteSpace(SalaryArchive.BALANCEPOSTNAME))
                {
                    lkBalancePost.TxtLookUp.Text = SalaryArchive.BALANCEPOSTNAME;
                }

                //薪资标准
                salaryStand = new T_HR_SALARYSTANDARD();
                salaryStand.SALARYSTANDARDID = e.Result.standerID;
                salaryStand.SALARYSTANDARDNAME = e.Result.standerName;
                SalaryArchive.T_HR_SALARYSTANDARD = salaryStand;

                //传给流程的参数
                createCompanyID = e.Result.CompanyID;
                createDepartmentID = e.Result.DepartmentID;
                createPostID = e.Result.PostID;

                //加载薪资项
                LoadData();

                //重新提交把审核状态修改为未提交
                if (FormType == FormTypes.Resubmit)
                {
                    SalaryArchive.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    SalaryArchive.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                }
                //禁用控件
                if (SalaryArchive.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    EnableControl();
                }
                if (SalaryArchive.POSTLEVEL != null)
                {
                    BindPostLevel(SalaryArchive.POSTLEVEL);
                    PostLevel = SalaryArchive.POSTLEVEL.ToString();
                }

                if (SalaryArchive.OLDSALARYLEVEL != null)
                {
                    oldsalaryLevel = SalaryArchive.OLDSALARYLEVEL;
                    oldpostlevel = SalaryArchive.OLDPOSTLEVEL == null ? 0 : SalaryArchive.OLDPOSTLEVEL;
                    foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxOldSalaryLevel.Items)
                    {
                        if (item.DICTIONARYVALUE == oldsalaryLevel)
                        {
                            cbxOldSalaryLevel.SelectedItem = item;
                            break;
                        }
                    }
                    foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxOldPostLevel.Items)
                    {
                        if (item.DICTIONARYVALUE == oldpostlevel)
                        {
                            cbxOldPostLevel.SelectedItem = item;
                            break;
                        }
                    }

                }
                if (SalaryArchive.BALANCE != null)
                {
                    txtbBalance.Text = SalaryArchive.BALANCE.ToString();
                }

                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }
        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_SALARYARCHIVE", SalaryArchive.OWNERID,
                    SalaryArchive.OWNERPOSTID, SalaryArchive.OWNERDEPARTMENTID, SalaryArchive.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_SALARYARCHIVE", SalaryArchive.OWNERID,
                    SalaryArchive.OWNERPOSTID, SalaryArchive.OWNERDEPARTMENTID, SalaryArchive.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYARCHIVE");
        }
        public string GetStatus()
        {
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }

        }
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "",
                Tooltip = ""
            };
            items.Add(item);

            return items;
        }
        //public List<ToolbarItem> GetToolBarItems()
        //{
        //    List<ToolbarItem> items = new List<ToolbarItem>();

        //    ToolbarItem item = new ToolbarItem
        //    {
        //        DisplayType = ToolbarItemDisplayTypes.Image,
        //        Key = "0",
        //        Title = Utility.GetResourceStr("SAVE"),
        //        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
        //    };

        //    items.Add(item);

        //    item = new ToolbarItem
        //    {
        //        DisplayType = ToolbarItemDisplayTypes.Image,
        //        Key = "1",
        //        Title = Utility.GetResourceStr("SAVEANDCLOSE"),
        //        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_Close.png"
        //    };

        //    items.Add(item);
        //    if (FormType == FormTypes.Browse) items.Clear();
        //    return items;
        //}

        public List<ToolbarItem> GetToolBarItems()
        {
            if (isNew)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion
        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_SALARYARCHIVE Info)
        {
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY oldPostLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYVALUE == Info.OLDPOSTLEVEL).FirstOrDefault();

            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = cbxPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY salaryLevelDict = cbxSalaryLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY oldSalaryLevelDict = cbxOldSalaryLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY payCompany = (lkSalaryCompany.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY);


            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "POSTLEVEL", postLevelDict == null ? "" : postLevelDict.DICTIONARYVALUE.ToString(), postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "SALARYLEVEL", salaryLevelDict == null ? "" : salaryLevelDict.DICTIONARYVALUE.ToString(), salaryLevelDict == null ? "" : salaryLevelDict.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "OLDSALARYLEVEL", oldSalaryLevelDict == null ? "" : oldSalaryLevelDict.DICTIONARYVALUE.ToString(), oldSalaryLevelDict == null ? "" : oldSalaryLevelDict.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "OLDPOSTLEVEL", oldPostLevelDict == null ? "" : oldPostLevelDict.DICTIONARYVALUE.ToString(), oldPostLevelDict == null ? "" : oldPostLevelDict.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "SALARYSOLUTIONID", Info.SALARYSOLUTIONID, Info.SALARYSOLUTIONNAME));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "PAYCOMPANY", Info.PAYCOMPANY, payCompany == null ? "" : payCompany.CNAME));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "SALARYSTANDARDID", Info.T_HR_SALARYSTANDARD.SALARYSTANDARDID, txtSalaryStandard.Text));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "SALARYSOLUTIONNAME", Info.SALARYSOLUTIONID, Info.SALARYSOLUTIONNAME));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "SALARYSTANDARDNAME", Info.T_HR_SALARYSTANDARD.SALARYSTANDARDID, txtSalaryStandard.Text));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "AVAILABLEDATE", Info.OTHERSUBJOIN.ToString() + Utility.GetResourceStr("YEAR") + Info.OTHERADDDEDUCT.ToString() + Utility.GetResourceStr("MONTH"), ""));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "lkAttendanceCompany", lkAttendanceCompany.TxtLookUp.Text, lkAttendanceCompany.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "lkBalancePost", lkBalancePost.TxtLookUp.Text, lkBalancePost.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "FUNDS", txtSum.Text, txtSum.Text));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "FUNDSREMARK", txtSumRemark.Text, txtSumRemark.Text));
            AutoList.Add(basedata("T_HR_SALARYARCHIVE", "EMPLOYEENAME", lkEmployee.TxtLookUp.Text, lkEmployee.TxtLookUp.Text));
            foreach (var v in archiveItemsList)
            {
                AutoList.Add(basedataForChild("V_SALARYARCHIVEITEM", "REMARK", v.REMARK, "", v.SALARYARCHIVEITEM));
            }
            string a = mx.TableToXml(Info, archiveItemsList, StrSource, AutoList);

            return a;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        private AutoDictionary basedataForChild(string TableName, string Name, string Value, string Text, string keyValue)
        {
            string[] strlist = new string[5];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            strlist[4] = keyValue;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryChiledList(strlist);//这里需要传递5个参数过去，keyvalue就是该表的主键ID
            return ad;
        }
        #endregion
        #region IAudit
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != Post.OWNERID)
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            if (FormType == FormTypes.Resubmit && canSubmit == false)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            try
            {
                EntityBrowser browser = this.FindParentByType<EntityBrowser>();
                browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
                // Utility.SetAuditEntity(entity, "T_HR_SALARYARCHIVE", SalaryArchive.SALARYARCHIVEID);
                if (!string.IsNullOrEmpty(SalaryArchive.EMPLOYEENAME))
                {
                    Dictionary<string, string> para = new Dictionary<string, string>();
                    para.Add("EMPLOYEECNAME", SalaryArchive.EMPLOYEENAME);
                    para.Add("EMPLOYEEID", SalaryArchive.EMPLOYEEID);
                    para.Add("POSTLEVEL", PostLevel);
                    para.Add("EFFECTIVETIME", SalaryArchive.OTHERSUBJOIN == null ? "" : (SalaryArchive.OTHERSUBJOIN.ToString() + "年" + SalaryArchive.OTHERADDDEDUCT.ToString() + "月"));

                    entity.SystemCode = "HR";
                    string strXmlObjectSource = string.Empty;
                    //strXmlObjectSource = Utility.ObjListToXml<T_HR_SALARYARCHIVE>(SalaryArchive, "HR");
                    //Utility.SetAuditEntity(entity, "T_HR_SALARYARCHIVE", SalaryArchive.SALARYARCHIVEID, strXmlObjectSource, SalaryArchive.EMPLOYEEID);
                    //   strXmlObjectSource = Utility.ObjListToXml<T_HR_SALARYARCHIVE>(SalaryArchive, para, "HR", para2, null);
                    if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                        strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, SalaryArchive);

                    Dictionary<string, string> paraIDs = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(SalaryArchive.PAYCOMPANY)
                        && SalaryArchive.PAYCOMPANY != SalaryArchive.OWNERCOMPANYID)
                    {
                        paraIDs.Add("CreateUserID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                        // paraIDs.Add("CreatePostID", SalaryArchive.OWNERPOSTID);
                        paraIDs.Add("CreatePostID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                        //  paraIDs.Add("CreateDepartmentID", SalaryArchive.OWNERDEPARTMENTID);
                        paraIDs.Add("CreateDepartmentID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                        paraIDs.Add("CreateCompanyID", SalaryArchive.PAYCOMPANY);
                    }
                    else
                    {
                        paraIDs.Add("CreateUserID", SalaryArchive.EMPLOYEEID);
                        // paraIDs.Add("CreatePostID", SalaryArchive.OWNERPOSTID);
                        paraIDs.Add("CreatePostID", createPostID);
                        //  paraIDs.Add("CreateDepartmentID", SalaryArchive.OWNERDEPARTMENTID);
                        paraIDs.Add("CreateDepartmentID", createDepartmentID);
                        paraIDs.Add("CreateCompanyID", SalaryArchive.OWNERCOMPANYID);
                    }
                    if (SalaryArchive.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        Utility.SetAuditEntity(entity, "T_HR_SALARYARCHIVE", SalaryArchive.SALARYARCHIVEID, strXmlObjectSource, paraIDs);
                    }
                    else
                    {
                        Utility.SetAuditEntity(entity, "T_HR_SALARYARCHIVE", SalaryArchive.SALARYARCHIVEID, strXmlObjectSource);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            //RefreshUI(RefreshedTypes.ProgressBar);
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    SalaryArchive.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (SalaryArchive.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            SalaryArchive.CHECKSTATE = state;
            if (UserState.ToString() == "Audit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else if (UserState.ToString() == "Submit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            #region
            //client.SalaryArchiveUpdateAsync(SalaryArchive);
            //if (state == Utility.GetCheckState(CheckStates.Approved))
            //{
            //    //处理审核通过的逻辑处理
            //    // client.GetSalarySolutionByIDAsync(SalaryArchive.SALARYSOLUTIONID);
            //    //插入历史
            //    try
            //    {
            //        history.SALARYARCHIVEID = SalaryArchive.SALARYARCHIVEID;
            //        history.SALARYSOLUTIONID = SalaryArchive.SALARYSOLUTIONID;
            //        history.SALARYSOLUTIONNAME = SalaryArchive.SALARYSOLUTIONNAME;
            //        history.SALARYSTANDARDID = salaryStand.SALARYSTANDARDID;
            //        history.EMPLOYEEID = SalaryArchive.EMPLOYEEID;
            //        history.EMPLOYEECODE = SalaryArchive.EMPLOYEECODE;
            //        history.EMPLOYEENAME = SalaryArchive.EMPLOYEENAME;
            //        history.OWNERCOMPANYID = SalaryArchive.OWNERCOMPANYID;
            //        history.OWNERDEPARTMENTID = SalaryArchive.OWNERDEPARTMENTID;
            //        history.OWNERPOSTID = SalaryArchive.OWNERPOSTID;
            //        history.OWNERID = SalaryArchive.OWNERID;
            //        history.CREATEUSERID = SalaryArchive.CREATEUSERID;
            //        history.CREATEDATE = System.DateTime.Now;
            //        client.SalaryArchiveHisAddAsync(history);
            //        client.SalaryArchiveHisItemsAddAsync(history.SALARYARCHIVEID, history.SALARYSTANDARDID, history.CREATEUSERID);
            //    }
            //    catch { }
            //}
            //Utility.UpdateCheckState("T_HR_SALARYARCHIVE", "SALARYARCHIVEID", SalaryArchive.SALARYARCHIVEID, args);
            #endregion
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (SalaryArchive != null)
                state = SalaryArchive.CHECKSTATE;
            return state;
        }
        #endregion
        public bool Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            else
            {

                if (string.IsNullOrEmpty(lkSalarySolution.TxtLookUp.Text.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYSOLUTION"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (string.IsNullOrEmpty(lkEmployee.TxtLookUp.Text.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEECNAME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (string.IsNullOrEmpty(lkSalaryCompany.TxtLookUp.Text.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "发薪机构"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (string.IsNullOrEmpty(lkAttendanceCompany.TxtLookUp.Text.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "考勤机构"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (cbxSalaryLevel.SelectedIndex < 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYLEVEL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (string.IsNullOrEmpty(txtSalaryStandard.Text.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("该薪资方案没有对应的薪资标准"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee = lkEmployee.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                SalaryArchive.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalaryArchive.CHECKSTATE = Convert.ToInt16(CheckStates.UnSubmit).ToString();
                SalaryArchive.EMPLOYEEID = employee.EMPLOYEEID;
                SalaryArchive.EMPLOYEENAME = employee.EMPLOYEECNAME;
                //SalaryArchive.EDITSTATE = cbIsEnabled.IsChecked == true ? "1" : "0";
                SalaryArchive.OWNERCOMPANYID = (lkSalaryCompany.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
                SalaryArchive.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                SalaryArchive.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;

                SalaryArchive.SALARYLEVEL = Convert.ToDecimal(intSalaryLevel);
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST tmpempost = new Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST();
                tmpempost.EMPLOYEEPOSTID = Guid.NewGuid().ToString();

                tmpempost.SALARYLEVEL = (cbxSalaryLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE;
                //if (FormType == FormTypes.New)
                //    clientperson.EmployeeSamePostSalaryLevelUpdateAsync(SalaryArchive.EMPLOYEEID, Convert.ToDecimal(tmpempost.SALARYLEVEL), "CHANGE");
                //else
                //    clientperson.EmployeeSamePostSalaryLevelUpdateAsync(SalaryArchive.EMPLOYEEID, Convert.ToDecimal(tmpempost.SALARYLEVEL), "EDIT");

                if (FormType == FormTypes.New)
                {
                    //薪资档案异动，新建薪资档案
                    client.CreateSalaryArchiveAsync(4, SalaryArchive.EMPLOYEEID, SalaryArchive, false);
                }
                else
                {
                    client.CreateSalaryArchiveAsync(4, SalaryArchive.EMPLOYEEID, SalaryArchive, true);
                }


            }
            return true;
        }
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        /// <summary>
        /// 选择员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmployee_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.MultiSelected = false;
            lookup.SelectedClick += (obj, ev) =>
            {
                //SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null)
                {
                    ExtOrgObj employeeInfo = ent.FirstOrDefault();
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST tmp = (ent[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).T_HR_EMPLOYEEPOST.FirstOrDefault();
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employeeTmp = ent[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    lkEmployee.DataContext = employeeTmp;
                    clientperson.GetLastChangeByEmployeeIDAsync((ent[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).EMPLOYEEID);
                    //SalaryArchive.OWNERCOMPANYID = employeeTmp.OWNERCOMPANYID;
                    //SalaryArchive.OWNERDEPARTMENTID = employeeTmp.OWNERDEPARTMENTID;
                    SalaryArchive.OWNERID = employeeTmp.EMPLOYEEID;
                    //SalaryArchive.OWNERPOSTID = employeeTmp.OWNERPOSTID;
                    SalaryArchive.EMPLOYEEPOSTID = tmp.EMPLOYEEPOSTID;//保存薪资档案所属人的岗位关联ID
                    PostLevel = tmp.POSTLEVEL.ToString();
                    BindPostLevel(tmp.POSTLEVEL);
                    BindStand();

                    //岗位
                    ExtOrgObj post = (ExtOrgObj)employeeInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;
                    createPostID = postid;

                    //部门
                    ExtOrgObj dept = (ExtOrgObj)post.ParentObject;
                    string deptId = dept.ObjectID;
                    createDepartmentID = deptId;

                    //公司
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    createCompanyID = corpid;

                    string fullName = string.Empty;
                    if (string.IsNullOrEmpty(postName))
                    {
                        fullName = employeeTmp.EMPLOYEECNAME;
                    }
                    else
                    {
                        string orgName = GetFullOrgName(deptId);
                        if (string.IsNullOrEmpty(orgName))
                        {
                            fullName = employeeTmp.EMPLOYEECNAME + " - " + postName + " - " + corp.CNAME;
                        }
                        else
                        {
                            fullName = employeeTmp.EMPLOYEECNAME + " - " + postName + " - " + GetFullOrgName(deptId);

                        }
                    }
                    lkEmployee.TxtLookUp.Text = fullName;
                    if (lkSalarySolution.DataContext as T_HR_SALARYSOLUTION != null)
                    {
                        cbxSalaryLevel.IsEnabled = true;
                    }
                    oldsalaryLevel = tmp.SALARYLEVEL;
                    oldpostlevel = tmp.POSTLEVEL;
                    SalaryArchive.POSTLEVEL = tmp.POSTLEVEL;
                    client.GetOldFixSalaryAsync(employeeTmp.EMPLOYEEID);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        //选择薪资方案
        private void lkSalarySolution_FindClick(object sender, EventArgs e)
        {

            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("SALARYSOLUTIONNAME", "SALARYSOLUTIONNAME");
            cols.Add("BANKNAME", "BANKNAME");
            cols.Add("BANKACCOUNTNO", "BANKACCOUNTNO");

            //LookupForm lookup = new LookupForm(EntityNames.SalarySolution,
            //    typeof(List<T_HR_SALARYSOLUTION>), cols);
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            filter += " CHECKSTATE=@" + paras.Count().ToString() + "";
            paras.Add("2");
            LookupForm lookup = new LookupForm(EntityNames.SalarySolution, typeof(List<T_HR_SALARYSOLUTION>), cols, filter, paras);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SALARYSOLUTION ent = lookup.SelectedObj as T_HR_SALARYSOLUTION;
                if (ent != null)
                {
                    lkSalarySolution.DataContext = ent;
                    SalaryArchive.SALARYSOLUTIONID = ent.SALARYSOLUTIONID;
                    SalaryArchive.SALARYSOLUTIONNAME = ent.SALARYSOLUTIONNAME;
                    if (lkEmployee.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE != null)
                    {
                        cbxSalaryLevel.IsEnabled = true;
                    }
                    BindStand();
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #region 档案薪资项目
        //private void btnNew_Click(object sender, RoutedEventArgs e)
        //{
        //    string filter = "";
        //    ObservableCollection<object> paras = new ObservableCollection<object>();
        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("SALARYITEMNAME", "SALARYITEMNAME");
        //    cols.Add("GUERDONSUM", "GUERDONSUM");
        //    filter += "CREATECOMPANYID==@" + paras.Count().ToString();
        //    paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
        //    LookupForm lookup = new LookupForm(EntityNames.SalaryItemSet,
        //        typeof(List<T_HR_SALARYITEM>), cols, filter, paras);
        //    lookup.TitleContent = Utility.GetResourceStr("SALARYITEM");
        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_SALARYITEM ent = lookup.SelectedObj as T_HR_SALARYITEM;
        //        if (ent != null)
        //        {
        //            T_HR_SALARYARCHIVEITEM item = new T_HR_SALARYARCHIVEITEM();
        //            item.SALARYARCHIVEITEM = Guid.NewGuid().ToString();
        //            item.SUM = ent.GUERDONSUM.ToString();
        //            item.SALARYITEMID = ent.SALARYITEMID;
        //            item.T_HR_SALARYARCHIVE = new T_HR_SALARYARCHIVE();
        //            item.T_HR_SALARYARCHIVE.SALARYARCHIVEID = SalaryArchive.SALARYARCHIVEID;
        //            // item.SALARYSTANDARDID = SalaryArchive.T_HR_SALARYSTANDARD.SALARYSTANDARDID;
        //            item.CREATEDATE = System.DateTime.Now;
        //            item.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        //            client.SalaryArchiveItemAddAsync(item);
        //        }
        //    };

        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //}
        //private void btnDelete_Click(object sender, RoutedEventArgs e)
        //{

        //    ObservableCollection<string> ids = new ObservableCollection<string>();
        //    if (DtGrid.SelectedItems.Count > 0)
        //    {
        //        for (int i = 0; i < DtGrid.SelectedItems.Count; i++)
        //        {
        //            ids.Add((DtGrid.SelectedItems[0] as V_SALARYARCHIVEITEM).SALARYARCHIVEITEM);
        //        }
        //        client.SalaryArchiveItemDeleteAsync(ids);
        //    }
        //}
        //void btnEdit_Click(object sender, RoutedEventArgs e)
        //{
        //    if (DtGrid.SelectedItems.Count > 0)
        //    {
        //        V_SALARYARCHIVEITEM tmp = DtGrid.SelectedItems[0] as V_SALARYARCHIVEITEM;
        //        SMT.HRM.UI.Form.Salary.SalaryArchiveItemForm form = new SalaryArchiveItemForm(FormTypes.Edit, tmp.SALARYARCHIVEITEM);
        //        EntityBrowser browser = new EntityBrowser(form);
        //        browser.MinWidth = 400;
        //        browser.MinHeight = 240;
        //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
        //            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
        //        return;
        //    }
        //}

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (!string.IsNullOrEmpty(SalaryArchive.SALARYARCHIVEID))
            {
                filter = "SALARYARCHIVEID==@" + paras.Count.ToString();
                paras.Add(SalaryArchive.SALARYARCHIVEID);

            }
            client.GetSalaryArchiveItemPagingAsync(dataPager.PageIndex, dataPager.PageSize, "ORDERNUMBER", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }
        #endregion
        # region  自定义薪资档案
        private void GridPagerDetail_Click(object sender, RoutedEventArgs e)
        {
            // LoadCustomArchive(strArchiveID);
        }

        //void LoadCustomArchive(string salaryArchiveID)
        //{
        //    int pageCount = 0;
        //    string filter = "";
        //    System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();


        //    filter += " SALARYARCHIVEID==@" + paras.Count().ToString();
        //    paras.Add(salaryArchiveID);

        //    client.GetCustomGuerdonArchiveWithPagingAsync(dataPagerDetail.PageIndex, dataPagerDetail.PageSize, " GUERDONNAME", filter, paras, pageCount);
        //}
        //void client_GetCustomGuerdonArchiveWithPagingCompleted(object sender, GetCustomGuerdonArchiveWithPagingCompletedEventArgs e)
        //{
        //    List<V_CUSTOMGUERDONARCHIVE> list = new List<V_CUSTOMGUERDONARCHIVE>();
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            list = e.Result.ToList();
        //        }

        //        DtGridDetail.ItemsSource = list;
        //        dataPagerDetail.PageCount = e.pageCount;
        //    }

        //}
        #endregion

        /// <summary>
        /// 显示标准名
        /// </summary>
        void BindStand()
        {
            if (!string.IsNullOrEmpty(lkEmployee.TxtLookUp.Text) && !string.IsNullOrEmpty(lkSalarySolution.TxtLookUp.Text))
            {
                string employeeID = (lkEmployee.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).EMPLOYEEID;
                string solutionID = (lkSalarySolution.DataContext as T_HR_SALARYSOLUTION).SALARYSOLUTIONID;
                //通过员工ID 和 方案ID 获取标准名
                client.GetStandardByEmployeeIDAndSolutionIDAndSalarylevelAsync(employeeID, solutionID, intSalaryLevel, "New");

            }
        }
        /// <summary>
        /// 显示岗位级别
        /// </summary>
        /// <param name="postlevel"></param>
        void BindPostLevel(decimal? postlevel)
        {

            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxPostLevel.Items)
            {
                if (item.DICTIONARYVALUE == postlevel)
                {
                    cbxPostLevel.SelectedItem = item;
                    break;
                }
            }
        }
        /// <summary>
        /// 显示薪资级别
        /// </summary>
        void BindSalarylevel()
        {
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxSalaryLevel.Items)
            {
                if (item.DICTIONARYVALUE == Convert.ToDecimal(intSalaryLevel))
                {
                    cbxSalaryLevel.SelectedItem = item;
                    break;
                }
            }
        }
        private void cbxSalaryLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            intSalaryLevel = (cbxSalaryLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToInt32();
            BindStand();
            if (string.IsNullOrEmpty(PostLevel)) return;
            client.GetFixSalaryAsync(Convert.ToDecimal(PostLevel), intSalaryLevel.ToString(), (lkSalarySolution.DataContext as T_HR_SALARYSOLUTION).SALARYSOLUTIONID);

        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        /// <summary>
        /// 选择发薪机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkSalaryCompany_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Company;
            lookup.MultiSelected = false;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = ent[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                    lkSalaryCompany.DataContext = company;
                    SalaryArchive.PAYCOMPANY = company.COMPANYID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        /// <summary>
        ///岗位的全称
        /// </summary>
        /// <param name="dep"></param>
        /// <returns></returns>
        public string GetFullOrgName(string depID)
        {
            string orgName = string.Empty;
            try
            {
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = allDepartments.Where(s => s.DEPARTMENTID == depID).FirstOrDefault();
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();

                string fatherType = "0";
                string fatherID = "";
                bool hasFather = false;

                if (department != null)
                {
                    orgName += department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                    {
                        fatherType = department.FATHERTYPE;
                        fatherID = department.FATHERID;
                        hasFather = true;
                    }
                    else
                    {
                        hasFather = false;
                    }
                }

                while (hasFather)
                {
                    if (fatherType == "1" && !string.IsNullOrEmpty(fatherID))
                    {
                        department = (from de in allDepartments
                                      where de.DEPARTMENTID == fatherID
                                      select de).FirstOrDefault();
                        if (department != null)
                        {
                            orgName += " - " + department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                            if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                            {
                                fatherID = department.FATHERID;
                                fatherType = department.FATHERTYPE;
                            }
                            else
                            {
                                hasFather = false;
                            }
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else if (fatherType == "0" && !string.IsNullOrEmpty(fatherID))
                    {
                        company = (from com in allCompanys
                                   where com.COMPANYID == fatherID
                                   select com).FirstOrDefault();

                        if (company != null)
                        {
                            orgName += " - " + company.CNAME;
                            hasFather = false;
                        }
                        else
                        {
                            hasFather = false;
                        }

                    }
                    else
                    {
                        hasFather = false;
                    }

                }
            }
            catch (Exception ex)
            {
                //Utility.Log(ex.ToString());
            }
            return orgName;
        }

        /// <summary>
        /// 设定考勤机构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkAttendanceCompany_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Company;
            lookup.MultiSelected = false;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = ent[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                    lkAttendanceCompany.TxtLookUp.Text = company.CNAME;
                    SalaryArchive.ATTENDANCEORGID = company.COMPANYID;
                    SalaryArchive.ATTENDANCEORGNAME = company.CNAME;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 设定计算薪资的岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkBalancePost_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Post;
            lookup.MultiSelected = false;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_POST post = ent[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                    lkBalancePost.DataContext = post;
                    string strPostInfoName = string.Empty;
                    if (post != null)
                    {
                        if (post.T_HR_POSTDICTIONARY != null)
                        {
                            strPostInfoName = post.T_HR_POSTDICTIONARY.POSTNAME;
                        }

                        if (post.T_HR_DEPARTMENT != null)
                        {
                            if (post.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY != null)
                            {
                                strPostInfoName = strPostInfoName + " - " + post.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + " - " + post.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;

                                if (post.T_HR_DEPARTMENT.T_HR_COMPANY != null)
                                {
                                    strPostInfoName = strPostInfoName + " - " + post.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                                }
                            }
                        }
                    }
                    lkBalancePost.TxtLookUp.Text = strPostInfoName;
                    SalaryArchive.BALANCEPOSTID = post.POSTID;
                    SalaryArchive.BALANCEPOSTNAME = strPostInfoName;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}
