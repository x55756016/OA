using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form.Organization;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Organization
{
    public partial class RelationPost : BasePage
    {
        public string PostID { get; set; }
        public T_HR_POST Post { get; set; }

        SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient client;
        public RelationPost()
        {
            InitializeComponent();
            InitEvent();
            GetEntityLogo("T_HR_RELATIONPOST");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //获取参数
            PostID = Utility.GetUrlParamenter(e.Uri.OriginalString, "PostID");
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_RELATIONPOST", false);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            //Post赋值
            client.GetPostByIdAsync(PostID);
            LoadData();
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            client = new OrganizationServiceClient();
            client.RelationPostPagingCompleted += new EventHandler<RelationPostPagingCompletedEventArgs>(client_RelationPostPagingCompleted);
            client.RelationPostDeleteCompleted += new EventHandler<RelationPostDeleteCompletedEventArgs>(client_RelationPostDeleteCompleted);
            client.GetPostByIdCompleted += new EventHandler<GetPostByIdCompletedEventArgs>(client_GetPostByIdCompleted);

            this.DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
        }

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            filter += "T_HR_POST.POSTID==@" + paras.Count().ToString();
            paras.Add(PostID);

            //TextBox txtEmpName = Utility.FindChildControl<TextBox Style="{StaticResource TextBoxStyle}">(expander, "txtEmpName");
            //if (!string.IsNullOrEmpty(txtEmpName.Text))
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "CNAME==@" + paras.Count().ToString();
            //    paras.Add(txtEmpName.Text.Trim());
            //}

            client.RelationPostPagingAsync(dataPager.PageIndex, dataPager.PageSize, "T_HR_POST.POSTID", filter, paras, pageCount);
            loadbar.Stop();
            
        }

        void client_RelationPostPagingCompleted(object sender, RelationPostPagingCompletedEventArgs e)
        {
            List<V_RELATIONPOST> list = new List<V_RELATIONPOST>();
            if (e.Result != null)
            {
                list = e.Result.ToList();
            }
            DtGrid.ItemsSource = list;

            dataPager.PageCount = e.pageCount;
            
        }

        void client_GetPostByIdCompleted(object sender, GetPostByIdCompletedEventArgs e)
        {
            Post = e.Result;
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加,修改,删除
        public V_RELATIONPOST SelectRelationPost { get; set; }
        void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectRelationPost = grid.SelectedItems[0] as V_RELATIONPOST;
            }
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            RelationPostForm form = new RelationPostForm(FormTypes.New, "", Post);
            //form.ParentLayoutRoot = SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot;
            form.ReloadDataEvent += new BaseFloatable.refreshGridView(from_ReloadDataEvent);
            form.ShowDialog();
        }

        void from_ReloadDataEvent()
        {
            LoadData();
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectRelationPost != null)
            {
                RelationPostForm form = new RelationPostForm(FormTypes.Edit, SelectRelationPost.RelationPostID, Post);
                //form.ParentLayoutRoot = SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot;
                form.ReloadDataEvent += new BaseFloatable.refreshGridView(from_ReloadDataEvent);
                form.ShowDialog();
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ComfirmBox deleComfir = new ComfirmBox();
            deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
            deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK_Click);
            deleComfir.Show();
        }

        void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> ids = new ObservableCollection<string>();
            foreach (var id in DtGrid.SelectedItems)
            {
                ids.Add(((V_RELATIONPOST)id).RelationPostID);
            }
            client.RelationPostDeleteAsync(ids);
        }

        void client_RelationPostDeleteCompleted(object sender, RelationPostDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "RELATIONPOST"));
            }
            LoadData();
        }

        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_RELATIONPOST");
        }
    }
}
