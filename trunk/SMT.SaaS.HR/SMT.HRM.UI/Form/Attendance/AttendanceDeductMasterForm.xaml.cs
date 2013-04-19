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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttendanceDeductMasterForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string AttendanceDeductMasterID { get; set; }

        public T_HR_ATTENDANCEDEDUCTMASTER entAttendanceDeductMaster { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;

        private decimal dmaxTimes = 999999999999;
        #endregion

        #region 初始化
        public AttendanceDeductMasterForm(FormTypes formtype, string strAttendanceDeductMasterID)
        {
            FormType = formtype;
            AttendanceDeductMasterID = strAttendanceDeductMasterID;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AttendanceDeductMasterForm_Loaded);
        }

        void AttendanceDeductMasterForm_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
            UnVisibleGridToolControl();
        }        
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ATTENDANCEUNUSUALDEDUCT");
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
        /// 设置显示的按钮
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
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            RegEventsToMaster();

            RegEventsToDetail();
        }

        /// <summary>
        /// 主表事件注册
        /// </summary>
        private void RegEventsToMaster()
        {
            clientAtt.GetAttendanceDeductMasterByIDCompleted += new EventHandler<GetAttendanceDeductMasterByIDCompletedEventArgs>(clientAtt_GetAttendanceDeductMasterByIDCompleted);
            clientAtt.AddAttendanceDeductMasterCompleted += new EventHandler<AddAttendanceDeductMasterCompletedEventArgs>(clientAtt_AddAttendanceDeductMasterCompleted);
            clientAtt.ModifyAttendanceDeductMasterCompleted += new EventHandler<ModifyAttendanceDeductMasterCompletedEventArgs>(clientAtt_ModifyAttendanceDeductMasterCompleted);
        }

        /// <summary>
        /// 明细表事件注册
        /// </summary>
        private void RegEventsToDetail()
        {
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

            clientAtt.GetAttendanceDeductDetailRdListByMultSearchCompleted += new EventHandler<GetAttendanceDeductDetailRdListByMultSearchCompletedEventArgs>(clientAtt_GetAttendanceDeductDetailRdListByMultSearchCompleted);
            clientAtt.RemoveAttendanceDeductDetailCompleted += new EventHandler<RemoveAttendanceDeductDetailCompletedEventArgs>(clientAtt_RemoveAttendanceDeductDetailCompleted);
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
                LoadData();
                cbxkAttType.IsEnabled = false;
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
            entAttendanceDeductMaster = new T_HR_ATTENDANCEDEDUCTMASTER();
            entAttendanceDeductMaster.DEDUCTMASTERID = System.Guid.NewGuid().ToString().ToUpper();

            //权限控制
            entAttendanceDeductMaster.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entAttendanceDeductMaster.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entAttendanceDeductMaster.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entAttendanceDeductMaster.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entAttendanceDeductMaster.CREATEDATE = DateTime.Now;
            entAttendanceDeductMaster.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entAttendanceDeductMaster.UPDATEDATE = System.DateTime.Now;
            entAttendanceDeductMaster.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            entAttendanceDeductMaster.ATTENDABNORMALTYPE = "0";
            entAttendanceDeductMaster.ISACCUMULATING = "0";
            entAttendanceDeductMaster.ISPERFECTATTENDANCEFACTOR = "0";
            entAttendanceDeductMaster.FINETYPE = "0";

            this.DataContext = entAttendanceDeductMaster;
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(AttendanceDeductMasterID))
            {
                return;
            }

            clientAtt.GetAttendanceDeductMasterByIDAsync(AttendanceDeductMasterID);
        }

        private void BindDeductDetail()
        {
            string strDeductMasterID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strDeductMasterID = entAttendanceDeductMaster.DEDUCTMASTERID;
            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " HIGHESTTIMES ";
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetAttendanceDeductDetailRdListByMultSearchAsync(strOwnerID, strDeductMasterID, strSortKey, pageIndex, pageSize, pageCount);
        }

        

        /// <summary>
        /// 加载考勤异常扣款方式下拉菜单
        /// </summary>
        private void BindFineType()
        {
            string strCategory = "ATTEXFINETYPE";
            string strFatherID = ((T_SYS_DICTIONARY)cbxkAttType.SelectedItem).DICTIONARYID.ToString();
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == strCategory && d.T_SYS_DICTIONARY2.DICTIONARYID == strFatherID
                       orderby d.DICTIONARYVALUE
                       select d;
            cbxkFineType.ItemsSource = objs.ToList();
            cbxkFineType.DisplayMemberPath = "DICTIONARYNAME";

            SetSelectItemByFineType();
        }

        private void SetSelectItemByFineType()
        {
            if (cbxkFineType.Items.Count > 0)
            {
                foreach (var item in cbxkFineType.Items)
                {
                    T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DICTIONARYVALUE.ToString() == entAttendanceDeductMaster.FINETYPE)
                        {
                            cbxkFineType.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entAttendanceDeductMaster"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;

            if (cbxkAttType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ATTENDANCETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ATTENDANCETYPE")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkAttType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ATTENDANCETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ATTENDANCETYPE")));
                    flag = false;
                    return;
                }

                flag = true;
            }

            if (cbxkFineType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("FINETYPE")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkFineType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("FINETYPE")));
                    flag = false;
                    return;
                }

                entAttendanceDeductMaster.FINETYPE = entDic.DICTIONARYVALUE.ToString();
                flag = true;
            }

            entAttendanceDeductMaster.ISACCUMULATING = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbxIsAccumulating.IsChecked == true)
            {
                entAttendanceDeductMaster.ISACCUMULATING = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();

            }

            entAttendanceDeductMaster.ISPERFECTATTENDANCEFACTOR = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbxIsPerfectAttendanceFactor.IsChecked == true)
            {
                entAttendanceDeductMaster.ISPERFECTATTENDANCEFACTOR = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();                
            }         
            
            
            if (!flag)
            {
                return;
            }

            if (FormType == FormTypes.Edit)
            {
                entAttendanceDeductMaster.UPDATEDATE = DateTime.Now;
                entAttendanceDeductMaster.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
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
                    clientAtt.AddAttendanceDeductMasterAsync(entAttendanceDeductMaster);
                }
                else
                {
                    clientAtt.ModifyAttendanceDeductMasterAsync(entAttendanceDeductMaster);
                }
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
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void entBrowser_ReloadDataEvent()
        {
            BindDeductDetail();
        } 
        #endregion

        #region 事件
        /// <summary>
        /// 根据主键索引，获得指定的假期记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceDeductMasterByIDCompleted(object sender, GetAttendanceDeductMasterByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entAttendanceDeductMaster = e.Result;
                this.DataContext = entAttendanceDeductMaster;

                if (entAttendanceDeductMaster != null)
                {
                    cbxIsAccumulating.IsChecked = false;
                    if (entAttendanceDeductMaster.ISACCUMULATING == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                    {
                        cbxIsAccumulating.IsChecked = true;
                    }

                    cbxIsPerfectAttendanceFactor.IsChecked = false;
                    if (entAttendanceDeductMaster.ISPERFECTATTENDANCEFACTOR == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                    {
                        cbxIsPerfectAttendanceFactor.IsChecked = true;
                    }                    

                    BindDeductDetail();

                    toolbar1.IsEnabled = true;  //设置按钮可用，以便对主表关联明细表数据进行管理
                }
                
                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 新增假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddAttendanceDeductMasterCompleted(object sender, AddAttendanceDeductMasterCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (string.IsNullOrEmpty(e.Result))
                {
                    return;
                }

                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
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
        }

        /// <summary>
        /// 更新假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyAttendanceDeductMasterCompleted(object sender, ModifyAttendanceDeductMasterCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (string.IsNullOrEmpty(e.Result))
                {
                    return;
                }

                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDANCEUNUSUALDEDUCT")));
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceDeductDetailRdListByMultSearchCompleted(object sender, GetAttendanceDeductDetailRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_ATTENDANCEDEDUCTDETAIL> entlist = e.Result;
                dgDeductDetailList.ItemsSource = entlist;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_RemoveAttendanceDeductDetailCompleted(object sender, RemoveAttendanceDeductDetailCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDANCEDEDUCTDETAILFORM")));
            }

            BindDeductDetail();
        }

        /// <summary>
        /// 根据异常考勤类型，切换绑定的扣款方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxkAttType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindFineType();
        }

        /// <summary>
        /// 根据选定的扣款方式，给予对应的ToolTips(需在T_SYS_Dictionary表对应字典的Remark字段填入内容支持)显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void cbxkFineType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var si = cbxkFineType.SelectedItem as T_SYS_DICTIONARY;
        //    string strToolTip = string.Empty;
        //    if(si == null)
        //    {
        //        return;
        //    }

        //    if(string.IsNullOrWhiteSpace(si.REMARK))
        //    {
        //        strToolTip = si.DICTIONARYNAME;
        //    }
        //    else
        //    {
        //        strToolTip = si.REMARK;
        //    }

        //    ToolTipService.SetToolTip(cbxkFineType, strToolTip);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindDeductDetail();
        }

        /// <summary>
        /// 弹出表单子窗口，以便新增考勤异常扣款定义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strDeductDetailID = string.Empty;
            AttendanceDeductDetailForm formDetail = new AttendanceDeductDetailForm(FormTypes.New, strDeductDetailID, entAttendanceDeductMaster);
            EntityBrowser entBrowser = new EntityBrowser(formDetail);
            entBrowser.MinWidth = 720;
            entBrowser.MinHeight = 300;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindDeductDetail();
        }

        /// <summary>
        /// 弹出子窗口，以便浏览考勤异常扣款定义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strDeductDetailID = string.Empty;
            if (dgDeductDetailList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgDeductDetailList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCEDEDUCTDETAIL ent = dgDeductDetailList.SelectedItems[0] as T_HR_ATTENDANCEDEDUCTDETAIL;

            strDeductDetailID = ent.DEDUCTDETAILID.ToString();
            AttendanceDeductDetailForm formDetail = new AttendanceDeductDetailForm(FormTypes.Browse, strDeductDetailID, entAttendanceDeductMaster);
            EntityBrowser entBrowser = new EntityBrowser(formDetail);
            entBrowser.MinWidth = 700;
            entBrowser.MinHeight = 300;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑考勤异常扣款定义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strDeductDetailID = string.Empty;
            if (dgDeductDetailList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgDeductDetailList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCEDEDUCTDETAIL ent = dgDeductDetailList.SelectedItems[0] as T_HR_ATTENDANCEDEDUCTDETAIL;

            strDeductDetailID = ent.DEDUCTDETAILID.ToString();
            AttendanceDeductDetailForm formDetail = new AttendanceDeductDetailForm(FormTypes.Edit, strDeductDetailID, entAttendanceDeductMaster);
            EntityBrowser entBrowser = new EntityBrowser(formDetail);
            entBrowser.MinWidth = 700;
            entBrowser.MinHeight = 300;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 删除指定假期记录(物理删除，待定)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgDeductDetailList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgDeductDetailList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgDeductDetailList.SelectedItems)
            {
                T_HR_ATTENDANCEDEDUCTDETAIL ent = ovj as T_HR_ATTENDANCEDEDUCTDETAIL;
                string Result = "";
                if (ent != null)
                {
                    strID = ent.DEDUCTDETAILID.ToString();
                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveAttendanceDeductDetailAsync(strID);
                    };
                    delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
            }
        }

        #endregion
    }
}
