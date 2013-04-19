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
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Platform.Logging;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Views.Travelmanagement;

namespace SMT.SaaS.OA.UI.UserControls.Travelmanagement
{
    public partial class ReplicationProgram : BaseForm, IEntityEditor
    {
        private SmtOAPersonOfficeClient spo;
        private T_OA_TRAVELSOLUTIONS taavel;

        public ReplicationProgram()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ReplicationProgram_Loaded);
            
        }

        void ReplicationProgram_Loaded(object sender, RoutedEventArgs e)
        {
            dpGrid.Visibility = Visibility.Collapsed;
            InitEvent();
        }

        #region 初始化
        private void InitEvent()
        {
            spo = new SmtOAPersonOfficeClient();
            taavel = new T_OA_TRAVELSOLUTIONS();
            spo.GetTravelSolutionFlowCompleted += new EventHandler<GetTravelSolutionFlowCompletedEventArgs>(spo_GetTravelSolutionFlowCompleted);
            spo.GetCopyTravleSolutionCompleted += new EventHandler<GetCopyTravleSolutionCompletedEventArgs>(spo_GetCopyTravleSolutionCompleted);
            LoadData();
        }

        void spo_GetCopyTravleSolutionCompleted(object sender, GetCopyTravleSolutionCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                if (e.Result != null)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        /// <summary>
        /// 查询出差方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void spo_GetTravelSolutionFlowCompleted(object sender, GetTravelSolutionFlowCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #region 确认
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgSelect.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "复制"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            if (dgSelect.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "复制"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }
            T_OA_TRAVELSOLUTIONS ent = new T_OA_TRAVELSOLUTIONS();
            ent = (dgSelect.SelectedItems[0] as T_OA_TRAVELSOLUTIONS);

            if (ent != null)
            {
                Save(ent);
            }
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void Save(T_OA_TRAVELSOLUTIONS ent)
        {
            taavel.TRAVELSOLUTIONSID = System.Guid.NewGuid().ToString();
            taavel.PROGRAMMENAME = txtSolutionName.Text;
            taavel.ANDFROMTHATDAY = ent.ANDFROMTHATDAY;
            taavel.CUSTOMHALFDAY = ent.CUSTOMHALFDAY;
            taavel.RANGEPOSTLEVEL = ent.RANGEPOSTLEVEL;
            taavel.RANGEDAYS = ent.RANGEDAYS;
            taavel.MAXIMUMRANGEDAYS = ent.MAXIMUMRANGEDAYS;
            taavel.MINIMUMINTERVALDAYS = ent.MINIMUMINTERVALDAYS;
            taavel.INTERVALRATIO = ent.INTERVALRATIO;
            taavel.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            taavel.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            taavel.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            taavel.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            taavel.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            taavel.CREATEUSERNAME = Common.CurrentLoginUserInfo.UserName;
            taavel.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            taavel.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            taavel.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            spo.GetCopyTravleSolutionAsync(taavel, ent.TRAVELSOLUTIONSID);
        }
        #endregion

        #region 获取全部方案
        private void dgSelect_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }
        #endregion

        #region dgSelect_SelectionChanged
        private void dgSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion

        #region DataGrid 数据加载
        private void BindDataGrid(List<T_OA_TRAVELSOLUTIONS> obj, int pageCount)//加载出差报销子表
        {
            GridHelper.HandleDataPageDisplay(dpGrid, pageCount);
            if (obj == null || obj.Count < 1)
            {
                dgSelect.ItemsSource = null;
                return;
            }
            dgSelect.ItemsSource = obj;
        }
        #endregion

        #region 查询、分页LoadData()
        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件

            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值

            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            spo.GetTravelSolutionFlowAsync(0, 100, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
        }
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("新增出差方案");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {

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
            return null;
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
    }
}
