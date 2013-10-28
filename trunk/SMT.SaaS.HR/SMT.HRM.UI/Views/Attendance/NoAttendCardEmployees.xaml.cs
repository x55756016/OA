/*
 * 文件名：AttendanceSolutionAsign.xaml.cs
 * 作  用：考勤方案应用页
 * 创建人：吴鹏
 * 创建时间：2010年2月23日, 14:26:11
 * 修改人：
 * 修改时间：
 */

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
using System.Windows.Navigation;

using System.Windows.Data;
using System.Collections.ObjectModel;
using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Json;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using SMT.HRM.UI.OutApplyWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;

namespace SMT.HRM.UI.Views.Attendance
{

    public class UserInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public bool IsMember { get; set; }

    }

    public partial class NoAttendCardEmployees : BasePage, IClient
    {
        #region 全局变量
        OutAppliecrecordServiceClient clientAtt = new OutAppliecrecordServiceClient();
        private SMTLoading loadbar = new SMTLoading();
        public  string Checkstate = "0";
        #endregion

        #region 初始化
        public NoAttendCardEmployees()
        {
            InitializeComponent();
            RegisterEvents();
            GetEntityLogo("T_HR_NOATTENDCARDEMPLOYEES");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        
        #endregion

        #region 私有方法

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);
            
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

            clientAtt.GetNoAttendCardEmployeesPagingCompleted += clientAtt_GetNoAttendCardEmployeesPagingCompleted;
            clientAtt.DeleteNoAttendCardEmployeesRdCompleted += clientAtt_DeleteNoAttendCardEmployeesRdCompleted;
            this.Loaded += new RoutedEventHandler(AttendanceSolutionAsign_Loaded);
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_NOATTENDCARDEMPLOYEES", true);
            BindComboxBox();
            BindGrid();
        }

        /// <summary>
        /// 加载审核状态列表
        /// </summary>
        private void BindComboxBox()
        {
            if (toolbar1.cbxCheckState.ItemsSource == null)
            {
                Utility.CbxItemBinder(toolbar1.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            }
        }

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    paras.Add(ent.EMPLOYEECNAME);
                }
            }
            string strAttendanceSolutionID = string.Empty, strAssignedObjectType = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty, strCheckState = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " NOATTENDCARDEMPLOYEESID ";
            CheckInputFilter(ref strAttendanceSolutionID, ref strAssignedObjectType, ref strCheckState);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;
            if (strCheckState == Convert.ToInt32(CheckStates.All).ToString()) strCheckState = "";

            //clientAtt.GetNoAttendCardEmployeesPagingAsync(strOwnerID, strCheckState, strAttendanceSolutionID, strAssignedObjectType, strSortKey, pageIndex, pageSize, pageCount);
            clientAtt.GetNoAttendCardEmployeesPagingAsync(dataPager.PageIndex, dataPager.PageSize, "STARTDATE", filter, paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            loadbar.Start();
        }

        /// <summary>
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strFineType"></param>
        private void CheckInputFilter(ref string strAttendanceSolutionID, ref string strAssignedObjectType, ref string strCheckState)
        {
           

            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                strCheckState = entDic.DICTIONARYVALUE.ToString();
            }
        }
        #endregion

        #region 事件


        #region 查询和删除完成事件
        void clientAtt_DeleteNoAttendCardEmployeesRdCompleted(object sender, DeleteNoAttendCardEmployeesRdCompletedEventArgs e)
        {

            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDANCESOLUTIONASIGNFORM")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            BindGrid();
        }

        void clientAtt_GetNoAttendCardEmployeesPagingCompleted(object sender, GetNoAttendCardEmployeesPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_NOATTENDCARDEMPLOYEES> entlist = e.Result;
                dgAttSolAsignList.ItemsSource = entlist;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
            //throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// 根据审核状态显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;

                Checkstate = entDic.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(entDic.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_NOATTENDCARDEMPLOYEES");
                BindGrid();
            }
        }        
        /// <summary>
        /// 页面加载时，预绑定FormToolBar的状态ComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AttendanceSolutionAsign_Loaded(object sender, RoutedEventArgs e)
        {
            InitPage();
        }




        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void entBrowser_ReloadDataEvent()
        {
            ReLoadGrid();
        }

        /// <summary>
        /// 请假类型表单提交后，重新加载数据到数据列表
        /// </summary>
        private void ReLoadGrid()
        {
            BindGrid();
        }



        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //Uri serviceUri = new Uri("http://localhost:1514/Json_SilverlightWeb/UserService.svc/GetUser");
            //WebClient webClient = new WebClient();
            //webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            //webClient.OpenReadAsync(serviceUri);

            BindGrid();
        }


        //void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        //{
        //    //声明一个UserInfo类型的DataContractJsonSerializer实例
        //    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UserInfo));
            
        //    //获取JSON数据流的信息，并将它转换成为UserInfo实例
        //    UserInfo userInfo = (UserInfo)serializer.ReadObject(e.Result);//e.Result为JSON流数据

        //    //下面显示userInfo中的数据信息
        //    UserList.Items.Add(string.Format("Name:{0}, Address:{1}, Age:{2}, IsMember:{3} ",
        //        userInfo.Name,
        //        userInfo.Address,
        //        userInfo.Age,
        //        userInfo.IsMember));
        //}

        /// <summary>
        /// 首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAttSolAsignList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgAttSolAsignList, e.Row, "T_HR_NOATTENDCARDEMPLOYEES");
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 弹出表单子窗口，以便新增考勤方案应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //Utility.CreateFormFromEngine("862E6FA9-AB18-4D1F-A10E-46ED56EAD481", "SMT.HRM.UI.Form.Attendance.NoAttendCardEmployeesForm", "Audit");
            //return;
            string strAttendanceSolutionAsignID = string.Empty;
            NoAttendCardEmployeesForm formAttendanceSolutionAsign = new NoAttendCardEmployeesForm(FormTypes.New, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.New;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 重新提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionAsignID = string.Empty;

            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_NOATTENDCARDEMPLOYEES ent = dgAttSolAsignList.SelectedItems[0] as T_HR_NOATTENDCARDEMPLOYEES;

            strAttendanceSolutionAsignID = ent.NOATTENDCARDEMPLOYEESID.ToString();
            NoAttendCardEmployeesForm formAttendanceSolutionAsign = new NoAttendCardEmployeesForm(FormTypes.Resubmit, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.Resubmit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出子窗口，以便浏览指定的考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionAsignID = string.Empty;

            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_NOATTENDCARDEMPLOYEES ent = dgAttSolAsignList.SelectedItems[0] as T_HR_NOATTENDCARDEMPLOYEES;

            strAttendanceSolutionAsignID = ent.NOATTENDCARDEMPLOYEESID.ToString();
            NoAttendCardEmployeesForm formAttendanceSolutionAsign = new NoAttendCardEmployeesForm(FormTypes.Browse, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.Browse;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑考勤方案应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionAsignID = string.Empty;

            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_NOATTENDCARDEMPLOYEES ent = dgAttSolAsignList.SelectedItems[0] as T_HR_NOATTENDCARDEMPLOYEES;

            strAttendanceSolutionAsignID = ent.NOATTENDCARDEMPLOYEESID.ToString();
            NoAttendCardEmployeesForm formAttendanceSolutionAsign = new NoAttendCardEmployeesForm(FormTypes.Edit, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.Edit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});

        }

        /// <summary>
        /// 删除指定考勤方案应用记录(物理删除，待定)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
           ObservableCollection<string>  strids = new ObservableCollection<string>();
            foreach (object ovj in dgAttSolAsignList.SelectedItems)
            {
                T_HR_NOATTENDCARDEMPLOYEES ent = ovj as T_HR_NOATTENDCARDEMPLOYEES;               
                if (ent != null)
                {
                    strID = ent.NOATTENDCARDEMPLOYEESID.ToString();
                    if (ent.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETEAUDITERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    strids.Add(strID);
                   }
            }
            ComfirmWindow delComfirm = new ComfirmWindow();
            delComfirm.OnSelectionBoxClosed += (obj, result) =>
            {
                clientAtt.DeleteNoAttendCardEmployeesRdAsync(strids);
            };
            string Result = "";
            delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string strID = string.Empty;
            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_NOATTENDCARDEMPLOYEES entAttSolAsign = dgAttSolAsignList.SelectedItems[0] as T_HR_NOATTENDCARDEMPLOYEES;
            strID = entAttSolAsign.NOATTENDCARDEMPLOYEESID;
            NoAttendCardEmployeesForm formAttSolAsign = new NoAttendCardEmployeesForm(FormTypes.Audit, strID);
            EntityBrowser entBrowser = new EntityBrowser(formAttSolAsign);
            formAttSolAsign.MinWidth = 600;
            formAttSolAsign.MinHeight = 240;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.FormType = FormTypes.Audit;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }


        /// <summary>
        /// 重置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIsNull_Click(object sender, RoutedEventArgs e)
        {
            this.lkEmpName.DataContext = null;

            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);
        }

        /// <summary>
        /// 选择员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                if (ent != null)
                {
                    lkEmpName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }


        void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (dgAttSolAsignList.SelectedItems.Count > 0)
            {
                // T_HR_LEFTOFFICE temp = DtGrid.SelectedItems[0] as T_HR_LEFTOFFICE;
                T_HR_NOATTENDCARDEMPLOYEES tempview = dgAttSolAsignList.SelectedItems[0] as T_HR_NOATTENDCARDEMPLOYEES;
                if (tempview.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        tempview.CHECKSTATE = "0";
                        clientAtt.UpdateNoAttendCardEmployeesRdAsync(tempview);
                    };
                    string Result = "";
                    delComfirm.SelectionBox(Utility.GetResourceStr("确认"), Utility.GetResourceStr("请确认是否取消免打卡员工设置，确认后即刻生效不需要审核"), ComfirmWindow.titlename, Result);
                
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("免打卡人员名单未审核通过，不能取消"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

        }
        #endregion

        #region IClient 成员

        public void ClosedWCFClient()
        {
            clientAtt.DoClose();
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
