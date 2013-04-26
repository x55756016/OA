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
using SMT.SAAS.Platform.WebParts.ViewModels;
using SMT.SAAS.Platform.WebParts.NewsWS;
using SMT.SAAS.Platform.WebParts.Models;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI;
namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class NewsView : UserControl
    {
        public event EventHandler Reset;
        PlatformServicesClient client = null;
        BasicServices services = null;
        #region 构造函数 Constructor
        /// <summary>
        /// 展示新闻界面
        /// </summary>
        public NewsView()
        {
            InitializeComponent();
            services = new BasicServices();
            client = services.PlatformClient;
            //ctrUpload.MaxSize = 512000;
            
            RegisterEvent();
        }

        /// <summary>
        /// 展示新闻界面
        /// </summary>
        public NewsView(NewsViewModel viewModel, ViewState state)
            : this()
        {
            viewModel.LoadDetails();
            GoToState(state);
            LoadImage(viewModel.NEWSID, state);
            if (state == ViewState.ADD)
            {
                txtDeptId.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                txtDeptName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                viewModel.PUTDEPTID = txtDeptId.Text;
                viewModel.PUTDEPTNAME = txtDeptName.Text;
                ViewModel = viewModel;

                return;
            }

            viewModel.OnLoadDetailsCompleted += (obj, args) =>
            {
                ViewModel = viewModel;
            };

            //  LoadImage(viewModel.NEWSID, state);
        }
        /// <summary>
        /// 展示新闻界面
        /// </summary>
        public NewsView(string newsID, ViewState state)
            : this()
        {
            GoToState(state);
            LoadImage(newsID, state);
        }
        #endregion

        #region 私有属性 Private Properties
        private const string SYSTEMNAME = "Platform";
        private const string MODELNAME = "News";
        private NewsViewModel _resetModel = null;
        private NewsViewModel _viewModel = null;
        #endregion

        #region 公有属性 Public Properties
        public NewsViewModel ViewModel
        {
            private get
            {
                return (NewsViewModel)this.DataContext;
            }
            set
            {
                this.DataContext = value;
                txtDeptId.Text = string.Concat(value.PUTDEPTID);
                txtDeptName.Text = string.Concat(value.PUTDEPTNAME);
                foreach (var item in value.DISTRS)
                {
                    txtAuditName.Text = string.Empty;
                    txtAuditName.Text += item.MODELNAMES + ";";
                }
                if (value.NEWSCONTENT.IsNotNull())
                {
                    try
                    {
                        //modify by 安凯航
                        //2011年6月20日

                        //System.IO.Stream stream = new System.IO.MemoryStream(value.NEWSCONTENT);
                        //if (stream.Length > 0)
                        //    rtbContent.LoadDocument(stream, "html");

                        //富文本框使用说明:
                        //加载\读取富文内容
                        //如果保存在数据库中的文档是html格式,直接将文档的字节数组赋值给富文本框的Document属性即可
                        rtbContent.Document = value.NEWSCONTENT;
                        //如果是其他格式(目前支持的格式有:html,htm,pdf,docx,txt),有两种方式
                        //1.使用LoadDocument方法,第二个参数为文档扩展名.
                        // rtbContent.LoadDocument(value.NEWSCONTENT, "html");

                        //2.给富文本框的文档格式化提供者"IProvider"属性赋值.
                        //可以是HtmlFormatProvider\PdfFormatProvider\DocxFormatProvider\XamlFormatProvider\TxtFormatProvider之一.
                        //各自对应一种文档格式.然后富文本框的Document属性即可获得相应格式的字节数组
                        //rtbContent.IProvider = new PdfFormatProvider();
                        //rtbContent.Document = value.NEWSCONTENT;

                        //导出\保存富文本内容:
                        //与读取类似,使用富文本框的Document属性或者ExportDoctment()方法导出html格式字节数组.
                        //也可以设置富文本框的文档格式化提供者"IProvider"属性,然后获取Document的值.
                        //或使用ExportDoctment(string extension)方法导出指定扩展名的文档格式字节数组.
                        //同时提供导出为stream的ExportDoctment方法的重载.                        

                        //end modify.
                    }
                    catch (Exception ex)
                    {

                    }

                }
                value.OnDataChangedCompleted += new EventHandler(value_OnDataChangedCompleted);
            }
        }

        #endregion

        #region 私有方法 Private Methods
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvent()
        {
            this.Loaded += new RoutedEventHandler(NewsView_Loaded);
            client.GetNewsModelByIDCompleted += new EventHandler<GetNewsModelByIDCompletedEventArgs>(client_GetNewsModelByIDCompleted);
            btnLookUpDepartment.Click += new RoutedEventHandler(btnLookUpDepartment_Click);
            btnLookUpPutDepartment.Click += new RoutedEventHandler(btnLookUpPutDepartment_Click);

        }

        void btnLookUpPutDepartment_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    var viewmodel = (this.DataContext as NewsViewModel);
                    viewmodel.PUTDEPTNAME = ent[0].ObjectName;
                    viewmodel.PUTDEPTID = ent[0].ObjectID;
                    txtDeptName.Text = ent[0].ObjectName;
                    //foreach (var item in ent)
                    //{
                    //    viewmodel.PUTDEPTNAME = item.ObjectName;
                    //    viewmodel.PUTDEPTID = item.ObjectID;
                    //    txtDeptName.Text += item.ObjectName + ";";
                    //}
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        void client_GetNewsModelByIDCompleted(object sender, GetNewsModelByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    // this.ViewModel = e.Result;
                }
            }
        }

        /// <summary>
        /// 清空当前View 数据
        /// </summary>
        private void ClearValue()
        {
            _resetModel = this.DataContext as NewsViewModel;

            this.DataContext = new NewsViewModel();
        }
        /// <summary>
        /// 设置界面是否启用
        /// </summary>
        /// <param name="isReadonly">是否为只读，True：只读 False: 可写</param>
        private void SetReadOnly(bool isReadonly)
        {
            var txtboxList = from var in currentEntity.Children
                             where var is TextBox
                             select var;
            foreach (var child in txtboxList)
            {
                (child as TextBox).IsReadOnly = isReadonly;
            }
            var checkBoxList = from var in currentEntity.Children
                               where var is CheckBox
                               select var;
            foreach (var child in checkBoxList)
            {
                (child as CheckBox).IsEnabled = !isReadonly;
            }

            var cmbBoxList = from var in currentEntity.Children
                             where var is ComboBox
                             select var;
            foreach (var child in cmbBoxList)
            {
                (child as ComboBox).IsEnabled = !isReadonly;
            }
        }

        private void SaveImage(string FormeID)
        {
            //ctrUpload.FormID = FormeID;
            //ctrUpload.Save();
        }
        private void LoadImage(string FormeID, ViewState viewstate)
        {
            //ctrUpload.SystemName = SYSTEMNAME;
            //ctrUpload.ModelName = MODELNAME;
            //ctrUpload.FormID = FormeID;
            //ctrUpload.Event_AllFilesFinished += new EventHandler<SaaS.FrameworkUI.FileUpload.FileCountEventArgs>(ctrUpload_Event_AllFilesFinished);
            //if (viewstate == ViewState.ADD)
            //{
            //    ctrUpload.InitBtn(System.Windows.Visibility.Visible, System.Windows.Visibility.Visible);
            //}
            //else if (viewstate == ViewState.UPDATE)
            //{
            //    ctrUpload.Load_fileData(FormeID);
            //    ctrUpload.InitBtn(System.Windows.Visibility.Visible, System.Windows.Visibility.Visible);
            //}

            InitFileLoad(FormeID, viewstate, sMTFileUpload1, true);
           
        }
        #endregion

        #region 公有方法 Public Methods
        /// <summary>
        /// View视图状态转换
        /// </summary>
        /// <param name="state"></param>
        public void GoToState(ViewState state)
        {
            switch (state)
            {
                case ViewState.ADD:
                    {
                        StatePanel.Visibility = Visibility.Collapsed;
                        SavePanel.Visibility = Visibility.Visible;
                        btnSave.Visibility = Visibility.Visible;
                        btnUpdate1.Visibility = Visibility.Collapsed;
                        btnSave.Tag = ViewState.ADD;

                        SetReadOnly(false);
                        txtDeptId.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        txtDeptName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                        break;
                    }
                case ViewState.UPDATE:
                    {
                        StatePanel.Visibility = Visibility.Collapsed;
                        SavePanel.Visibility = Visibility.Visible;
                        btnSave.Visibility = Visibility.Collapsed;
                        btnUpdate1.Visibility = Visibility.Visible;
                        btnSave.Tag = ViewState.UPDATE;
                        SetReadOnly(false);
                        break;
                    }
                case ViewState.DELETE:
                    { break; }
                case ViewState.RESET:
                    {
                        StatePanel.Visibility = Visibility.Visible;
                        SavePanel.Visibility = Visibility.Collapsed;
                        SetReadOnly(true);
                        if (_resetModel.IsNotNull())
                            this.DataContext = _resetModel;
                        break;
                    }
                default:
                    break;
            }
        }
        #endregion

        #region 事件函数 EventHanlder Methods
        private void NewsView_Loaded(object sender, RoutedEventArgs e)
        {
            //SetReadOnly(true);
            var viewmodel = (this.DataContext as NewsViewModel);
            if (viewmodel.IsNotNull())
            {
                try
                {
                    //modify by 安凯航
                    //2011年6月20日

                    //System.IO.Stream stream = new System.IO.MemoryStream(viewmodel.NEWSCONTENT);
                    //if (stream.Length > 0)
                    //    rtbContent.LoadDocument(stream, "html");

                    //使用LoadDocument(byte[] bytes, string extension)方法加载内容

                    rtbContent.Document = viewmodel.NEWSCONTENT;

                    //end modify.
                }
                catch (Exception ex)
                {
                }

            }
        }

        private void value_OnDataChangedCompleted(object sender, EventArgs e)
        {
            var vm = (sender as NewsViewModel);
            if (vm.ISIMAGE)
            {
                // SaveImage(vm.NEWSID);
            }
        }

        /// <summary>
        /// 选择审核人
        /// </summary>
        private void btnLookUpDepartment_Click(object sender, RoutedEventArgs e)
        {
            this.txtAuditName.Text = string.Empty;
            this.txtAuditId.Text = string.Empty;
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    var viewmodel = (this.DataContext as NewsViewModel);
                    viewmodel.DISTRS.Clear();
                    foreach (var item in ent)
                    {
                        viewmodel.VIEWER.Add(item.ObjectID);
                        viewmodel.DISTRS.Add(new DISTR() { VIEWERS = item.ObjectID, MODELNAMES = item.ObjectName });
                        txtAuditName.Text += item.ObjectName + ";";
                    }
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        private void btnADD_Click(object sender, RoutedEventArgs e)
        {
            GoToState(ViewState.ADD);
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            GoToState(ViewState.UPDATE);
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (Reset != null)
                Reset(this, EventArgs.Empty);
            // GoToState(ViewState.RESET);
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var viewmodel = (this.DataContext as NewsViewModel);

            viewmodel.NEWSCONTENT = rtbContent.Document;
        }
        #endregion

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {

        }
        
             /// <summary>
        /// 新上传控件调用
        /// </summary>
        /// <param name="strModelCode">模块编码，一般为表名</param>
        /// <param name="strApplicationID">表单ID</param>
        /// <param name="action">动作</param>
        /// <param name="control">上传控件</param>
        /// <param name="AllowDelete">是否允许删除</param>
        public static void InitFileLoad(string strApplicationID, ViewState action, FileUpLoad.FileControl control, bool AllowDelete)
        {
            SMT.FileUpLoad.Classes.UserConfig uc = new SMT.FileUpLoad.Classes.UserConfig();
            uc.CompanyCode = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            uc.SystemCode = "Platform";
            uc.ModelCode = "News";
            uc.UserID = Common.CurrentLoginUserInfo.EmployeeID;
            uc.ApplicationID = strApplicationID;
            uc.NotShowThumbailChckBox = true;
            //if (action == ViewState.Browse || action == ViewState.Audit)
            //{
            //    uc.NotShowUploadButton = true;
            //    uc.NotShowDeleteButton = true;
            //    uc.NotAllowDelete = true;
            //}
            if (!AllowDelete)
            {
                uc.NotShowDeleteButton = true;
            }
            uc.Multiselect = true;
            uc.Filter = "图片文件(*.png, *.jpg)|*.png;*.jpg";
            //uc.Filter = "图片文件(*.jpg,*.gif,*.bmp)|*.jpg;*.gif;*.bmp";
            uc.MaxConcurrentUploads = 1;
            uc.MaxSize = "20.MB";
            uc.CreateName = Common.CurrentLoginUserInfo.EmployeeName;
            uc.PageSize = 20;
            control.Init(uc);
        }

    }

    /// <summary>
    /// 视图状态
    /// </summary>
    public enum ViewState
    {
        ADD,
        UPDATE,
        DELETE,
        RESET
    }
}
