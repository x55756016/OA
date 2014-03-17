using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class LeaveTypeSetForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string LeaveTypeSetID { get; set; }

        public T_HR_LEAVETYPESET entLeaveTypeSet { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化
        public LeaveTypeSetForm(FormTypes formtype, string strLeaveTypeSetID)
        {
            FormType = formtype;
            LeaveTypeSetID = strLeaveTypeSetID;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LeaveTypeSetForm_Loaded);
        }

        void LeaveTypeSetForm_Loaded(object sender, RoutedEventArgs e)
        {
            UnVisibleGridToolControl();
            RegisterEvents();
            InitParas();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            RegEventsByLT();
            RegEventsByFLT();
        }

        /// <summary>
        /// 注册事件(假期标准)
        /// </summary>
        private void RegEventsByLT()
        {
            clientAtt.GetLeaveTypeSetByIDCompleted += new EventHandler<GetLeaveTypeSetByIDCompletedEventArgs>(clientAtt_GetLeaveTypeSetByIDCompleted);
            clientAtt.AddLeaveTypeSetCompleted += new EventHandler<AddLeaveTypeSetCompletedEventArgs>(clientAtt_AddLeaveTypeSetCompleted);
            clientAtt.ModifyLeaveTypeSetCompleted += new EventHandler<ModifyLeaveTypeSetCompletedEventArgs>(clientAtt_ModifyLeaveTypeSetCompleted);
        }

        /// <summary>
        /// 注册事件(带薪假设置)
        /// </summary>
        private void RegEventsByFLT()
        {
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

            clientAtt.GetFreeLeaveDaySetRdListByMultSearchCompleted += new EventHandler<GetFreeLeaveDaySetRdListByMultSearchCompletedEventArgs>(clientAtt_GetFreeLeaveDaySetRdListByMultSearchCompleted);
            clientAtt.RemoveFreeLeaveDaySetCompleted += new EventHandler<RemoveFreeLeaveDaySetCompletedEventArgs>(clientAtt_RemoveFreeLeaveDaySetCompleted);
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitParas()
        {
            if (FormType == FormTypes.New)
            {
                InitForm();
                SetToolBar();
            }
            else
            {
                PARENT.Children.Add(loadbar);
                
                LoadData();
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// 表单初始化
        /// </summary>
        private void InitForm()
        {
            entLeaveTypeSet = new T_HR_LEAVETYPESET();
            entLeaveTypeSet.LEAVETYPESETID = System.Guid.NewGuid().ToString().ToUpper();
            entLeaveTypeSet.ISFREELEAVEDAY = "1";

            //权限控制
            entLeaveTypeSet.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entLeaveTypeSet.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entLeaveTypeSet.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entLeaveTypeSet.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entLeaveTypeSet.CREATEDATE = DateTime.Now;
            entLeaveTypeSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entLeaveTypeSet.UPDATEDATE = System.DateTime.Now;
            entLeaveTypeSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            entLeaveTypeSet.FINETYPE = "0";
            entLeaveTypeSet.MAXDAYS = decimal.Parse("0");
            entLeaveTypeSet.SEXRESTRICT = "2";
            rbNoLimit.IsChecked = true;

            this.DataContext = entLeaveTypeSet;            
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(LeaveTypeSetID))
            {
                return;
            }

            clientAtt.GetLeaveTypeSetByIDAsync(LeaveTypeSetID);
            loadbar.Start();
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("LEAVESETFORM");            
        }

        public string GetStatus()
        {
            return string.Empty;
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

        private void RefreshUI(RefreshedTypes type)
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
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        #region 私有方法

        /// <summary>
        /// 
        /// </summary>
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entLeaveTypeSet"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;

            string strVacName = string.Empty, strLeaveTypeValue = string.Empty, strFineType = string.Empty, strIsFactor = string.Empty, strRemark = string.Empty;
                        
            if (string.IsNullOrEmpty(txtLeaveTypeName.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("VACATIONNAME"), Utility.GetResourceStr("REQUIRED", "VACATIONNAME"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
                strVacName = txtLeaveTypeName.Text;
            }

            if (cbxkLeaveTypeValue.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPEVALUE"), Utility.GetResourceStr("REQUIRED", "LEAVETYPEVALUE"));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkLeaveTypeValue.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINETYPE"), Utility.GetResourceStr("REQUIRED", "FINETYPE"));
                    flag = false;
                    return;
                }

                strLeaveTypeValue = entDic.DICTIONARYVALUE.ToString();
                entLeaveTypeSet.LEAVETYPEVALUE = strLeaveTypeValue;
            }

            if (cbxkFineType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINETYPE"), Utility.GetResourceStr("REQUIRED", "FINETYPE"));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkFineType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINETYPE"), Utility.GetResourceStr("REQUIRED", "FINETYPE"));
                    flag = false;
                    return;
                }

                strFineType = entDic.DICTIONARYVALUE.ToString();
                entLeaveTypeSet.FINETYPE = strFineType;
            }

            entLeaveTypeSet.ENTRYRESTRICT = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbxEntryRestrict.IsChecked.Value == true)
            {
                entLeaveTypeSet.ENTRYRESTRICT = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }
                       

            if (!flag)
            {
                return;
            }

            if (FormType == FormTypes.Edit)
            {
                entLeaveTypeSet.UPDATEDATE = DateTime.Now;
                entLeaveTypeSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;

            try
            {
                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
                if (validators.Count > 0)
                {
                    return false;
                }

                CheckSubmitForm(out flag);

                if (!flag)
                {
                    return false;
                }

                if (FormType == FormTypes.New)
                {
                    //如果最小值是0.5获取到的值是0
                    if (nudMaxDays.Value > 0)
                    {
                        if (entLeaveTypeSet.MAXDAYS == 0)
                        {
                            entLeaveTypeSet.MAXDAYS = (decimal)nudMaxDays.Value;
                        }
                    }
                    clientAtt.AddLeaveTypeSetAsync(entLeaveTypeSet);
                }
                else
                {
                    clientAtt.ModifyLeaveTypeSetAsync(entLeaveTypeSet);
                }

                loadbar.Start();
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            return flag;
        }

        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        private void Cancel()
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
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            string strLeaveTypeSetID = string.Empty, strIsFactor = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " FREELEAVEDAYSETID ";
            strLeaveTypeSetID = LeaveTypeSetID;
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetFreeLeaveDaySetRdListByMultSearchAsync(strOwnerID, strLeaveTypeSetID, strIsFactor, strSortKey, pageIndex, pageSize, pageCount);            
        }

        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            //toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            //toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
            
            toolbar1.IsEnabled = false;
            dataPager.IsEnabled = false;
        }

        /// <summary>
        /// 检查当前的假期类别是否为默认带薪假(即调休假，年假)
        /// </summary>
        private void CheckIsFreeLeave()
        {
            cbxkFineType.IsEnabled = true;            
            cbxkPostLevelRestrict.IsEnabled = true;
            rbWomen.IsEnabled = true;
            rbMan.IsEnabled = true;
            rbNoLimit.IsEnabled = true;

            if (cbxkLeaveTypeValue.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkLeaveTypeValue.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                return;
            }

            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(LeaveTypeValue.AdjustLeave) + 1).ToString() || entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(LeaveTypeValue.AnnualLeave) + 1).ToString())
            {
                cbxkFineType.IsEnabled = false;                
                cbxkPostLevelRestrict.IsEnabled = false;
                rbWomen.IsEnabled = false;
                rbMan.IsEnabled = false;
                rbNoLimit.IsEnabled = false;

                if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(LeaveTypeValue.AnnualLeave) + 1).ToString())
                {
                    toolbar1.IsEnabled = true;
                }

                foreach (object obj in cbxkFineType.Items)
                {
                    T_SYS_DICTIONARY entFineType = obj as T_SYS_DICTIONARY;
                    if (entFineType.DICTIONARYVALUE.ToString() == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString())
                    {
                        cbxkFineType.SelectedItem = obj;                        
                        break;
                    }
                }               
            }
        }
        #endregion

        #region 事件

        #region 假期标准事件

        /// <summary>
        /// 根据主键索引，获得指定的假期记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetLeaveTypeSetByIDCompleted(object sender, GetLeaveTypeSetByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entLeaveTypeSet = e.Result;
                this.DataContext = entLeaveTypeSet;

                cbxkLeaveTypeValue.IsEnabled = false;

                cbxEntryRestrict.IsChecked = false;
                if (entLeaveTypeSet.ENTRYRESTRICT == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                {
                    cbxEntryRestrict.IsChecked = true;
                }

                toolbar1.IsEnabled = false;
                dataPager.IsEnabled = false;
                if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString())
                {
                    toolbar1.IsEnabled = true;
                    dataPager.IsEnabled = true;
                }

                CheckIsFreeLeave();

                BindGrid();
                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 新增假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddLeaveTypeSetCompleted(object sender, AddLeaveTypeSetCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
                    LeaveTypeSetID = entLeaveTypeSet.LEAVETYPESETID;
                    toolbar1.IsEnabled = true;
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }

                FormType = FormTypes.Edit;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
            loadbar.Stop();
        }

        /// <summary>
        /// 更新假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyLeaveTypeSetCompleted(object sender, ModifyLeaveTypeSetCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "LEAVESETFORM")));                    
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
            loadbar.Stop();
        }

        /// <summary>
        /// 控制当前设定的假期标准是否为带薪假
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxkFineType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxkFineType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkFineType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {                
                return;
            }

            entLeaveTypeSet.ISFREELEAVEDAY = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            
            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString())
            {
                entLeaveTypeSet.ISFREELEAVEDAY = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }            
        }

        private void rbWomen_Checked(object sender, RoutedEventArgs e)
        {
            if (entLeaveTypeSet == null)
            {
                return;
            }

            entLeaveTypeSet.SEXRESTRICT = "0";
        }

        private void rbMan_Checked(object sender, RoutedEventArgs e)
        {
            if (entLeaveTypeSet == null)
            {
                return;
            }

            entLeaveTypeSet.SEXRESTRICT = "1";
        }

        private void rbNoLimit_Checked(object sender, RoutedEventArgs e)
        {
            if (entLeaveTypeSet == null)
            {
                return;
            }

            entLeaveTypeSet.SEXRESTRICT = "2";
        }

        private void cbxkLeaveTypeValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckIsFreeLeave();
        }
        #endregion
        
        #region 带薪假设置事件
        /// <summary>
        /// 加载数据列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetFreeLeaveDaySetRdListByMultSearchCompleted(object sender, GetFreeLeaveDaySetRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_FREELEAVEDAYSET> entlist = e.Result;
                dgFreeLeaveDaySetList.ItemsSource = entlist;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 删除指定记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_RemoveFreeLeaveDaySetCompleted(object sender, RemoveFreeLeaveDaySetCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "FREELEAVEDAYSETFORM")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            BindGrid();
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void entBrowser_ReloadDataEvent()
        {
            BindGrid();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }        

        /// <summary>
        /// 弹出表单子窗口，以便新增带薪假设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strFreeLeaveDaySetID = string.Empty;
            FreeLeaveDaySetForm formFreeLeaveDaySet = new FreeLeaveDaySetForm(FormTypes.New, strFreeLeaveDaySetID, entLeaveTypeSet);
            EntityBrowser entBrowser = new EntityBrowser(formFreeLeaveDaySet);
            entBrowser.MinWidth = 600;
            entBrowser.MinHeight = 300;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strFreeLeaveDaySetID = string.Empty;
            if (dgFreeLeaveDaySetList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgFreeLeaveDaySetList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_FREELEAVEDAYSET ent = dgFreeLeaveDaySetList.SelectedItems[0] as T_HR_FREELEAVEDAYSET;
            strFreeLeaveDaySetID = ent.FREELEAVEDAYSETID.ToString();

            FreeLeaveDaySetForm formFreeLeaveDaySet = new FreeLeaveDaySetForm(FormTypes.Browse, strFreeLeaveDaySetID, null);
            EntityBrowser entBrowser = new EntityBrowser(formFreeLeaveDaySet);
            entBrowser.MinWidth = 600;
            entBrowser.MinHeight = 300;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑带薪假设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strFreeLeaveDaySetID = string.Empty;
            if (dgFreeLeaveDaySetList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgFreeLeaveDaySetList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_FREELEAVEDAYSET ent = dgFreeLeaveDaySetList.SelectedItems[0] as T_HR_FREELEAVEDAYSET;
            strFreeLeaveDaySetID = ent.FREELEAVEDAYSETID.ToString();

            FreeLeaveDaySetForm formFreeLeaveDaySet = new FreeLeaveDaySetForm(FormTypes.Edit, strFreeLeaveDaySetID, null);
            EntityBrowser entBrowser = new EntityBrowser(formFreeLeaveDaySet);
            entBrowser.MinWidth = 600;
            entBrowser.MinHeight = 300;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });


        }

        /// <summary>
        /// 删除指定的带薪假设置(物理删除，待定)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgFreeLeaveDaySetList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgFreeLeaveDaySetList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgFreeLeaveDaySetList.SelectedItems)
            {
                T_HR_FREELEAVEDAYSET ent = ovj as T_HR_FREELEAVEDAYSET;

                string Result = "";
                if (ent != null)
                {
                    strID = ent.FREELEAVEDAYSETID.ToString();
                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveFreeLeaveDaySetAsync(strID);
                    };
                    delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }

            }

        }

        #endregion        
        
        #endregion        
    }
}
