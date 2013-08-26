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
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.PlatformService;

using SMT.SaaS.FrameworkUI;

namespace SMT.Workflow.Platform.Designer.Views.Message
{
    public partial class DefaultList : UserControl
    {
        PlatformService.DefaultMessageClient client = new PlatformService.DefaultMessageClient();
        PlatformService.FlowXmlDefineClient clientXml = new PlatformService.FlowXmlDefineClient();
        private ObservableCollection<AppModel> appModel = new ObservableCollection<AppModel>();//子系统XML定义
        private ObservableCollection<AppSystem> appSystem = new ObservableCollection<AppSystem>();//系统XML定义
        private ObservableCollection<string> xmlurl = new ObservableCollection<string>(); //子系统XML路径
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件  
        private int count = 0;
        public DefaultList()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            //调试用，发布时间删除
            //System.Windows.Controls.Window.Parent = windowParent;
            //System.Windows.Controls.Window.TaskBar = new StackPanel();
            //System.Windows.Controls.Window.Wrapper = this;
            //System.Windows.Controls.Window.IsShowtitle = false;
            //调试用，发布时间删除
            this.btnInit.Visibility = Visibility.Collapsed;
            this.btnInit.Click += new RoutedEventHandler(btnInit_Click);
            this.btnAdd.Click += new RoutedEventHandler(btnAdd_Click);
            this.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            this.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            this.btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
            loadbar.Start();
            clientXml.ListSystemAsync();
            SearchDefaultList();
            InitWCF();
        }

        void InitWCF()
        {
            client.InitMessageCompleted += (e, args) =>
            {
                if (args.Error == null)
                {
                    if (args.Result == "1")
                    {
                        this.btnInit.Visibility = Visibility.Collapsed;
                        SearchDefaultList();
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "数据初始化错误" + args.Result, "确定");
                    }
                }
            };
            client.GetMessageCompleted += (e, args) =>
            {
                if (args.Error == null)
                {
                    if (count == 0 && args.Result.Count < 1)
                    {
                        this.btnInit.Visibility = Visibility.Visible;
                    }
                    count++;
                    dgDefaultMessage.ItemsSource = args.Result;
                    dataPager1.PageCount = args.pageCount;
                    loadbar.Stop();
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "数据查询出错", "确定");
                }
            };
            clientXml.ListSystemCompleted += (e, args) =>
                {
                    if (args.Result != null)
                    {
                        appSystem = args.Result;
                        foreach (var item in appSystem)
                        {
                            xmlurl.Add(item.ObjectFolder);
                        }
                        clientXml.ListModelAsync(xmlurl);
                        appSystem.Insert(0, new AppSystem() { Name = "0", Description = "请选择......" });
                        cbSystemCode.ItemsSource = appSystem;
                        cbSystemCode.SelectedIndex = 0;
                    }
                };
            clientXml.ListModelCompleted += (e, args) =>
                {
                    if (args.Result != null)
                    {
                        appModel = args.Result;
                        appModel.Insert(0, new AppModel() { Name = "0", Description = "请选择......" });
                        cbModelCode.ItemsSource = appModel;
                        cbModelCode.SelectedIndex = 0;
                        loadbar.Stop();

                    }
                };
            client.DeleteMessageCompleted += (e, args) =>
            {
                if (args.Error == null)
                {
                    if (args.Result == "1")
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "删除成功！", "确定");
                        SearchDefaultList();
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox("提示信息", "删除失败！", "确定");
                    }
                }
            };

        }
        void SearchDefaultList()
        {
            int pageCont = 0;
            string strFilter = "";
            var systemcode = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.AppSystem;
            var modelCode = cbModelCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.AppModel;          
            #region 选择条件过滤
            if (systemcode != null && modelCode != null)
            {
                if (systemcode.Name != "0" && modelCode.Name != "0")
                {
                    strFilter += " AND SYSTEMCODE='" + systemcode.Name + "' AND MODELCODE='" + modelCode.Name + "'";
                }
                if (systemcode.Name != "0" && modelCode.Name == "0")
                {
                    strFilter += " AND SYSTEMCODE='" + systemcode.Name + "'";
                }
                if (systemcode.Name == "0" && modelCode.Name != "0")
                {
                    strFilter += " AND MODELCODE='" + modelCode.Name + "'";
                }
            }
            #endregion

            client.GetMessageAsync(dataPager1.PageIndex, dataPager1.PageSize, strFilter, "CREATEDATE DESC", pageCont);
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
                loadbar.Start();
                SearchDefaultList();
                curentIndex = dataPager1.PageIndex;
            }

        }
     
        void btnInit_Click(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            client.InitMessageAsync();
        }

        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            dataPager1.PageIndex = 1;
            SearchDefaultList();
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDefaultMessage.SelectedItems.Count == 1)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    T_WF_DEFAULTMESSAGE ent = dgDefaultMessage.SelectedItem as T_WF_DEFAULTMESSAGE;
                    client.DeleteMessageAsync(ent.MESSAGEID);
                };
                com.SelectionBox("删除确定", "你确定删除选中信息吗？", ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要删除的记录", "确定");
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            PlatformService.T_WF_DEFAULTMESSAGE entity = dgDefaultMessage.SelectedItem as PlatformService.T_WF_DEFAULTMESSAGE;
            if (entity != null)
            {
                EditDefaultMessage edit = new EditDefaultMessage(entity);
                edit.SaveCompleted += (obj, ev) =>
                {//保存成功，重新绑定
                    SearchDefaultList();
                };
                edit.appSystem = appSystem;
                edit.appModel = appModel;
                edit.cobMODELCODE.ItemsSource = this.cbModelCode.ItemsSource;
                edit.cobMODELCODE.SelectedIndex = 0;
                edit.cobSYSTEMCODE.ItemsSource = this.cbSystemCode.ItemsSource;
                edit.cobSYSTEMCODE.SelectedIndex = 0;
                for (int i = 0; i < edit.cobSYSTEMCODE.Items.Count(); i++)
                {
                    if ((edit.cobSYSTEMCODE.Items[i] as AppSystem).Name == entity.SYSTEMCODE)
                    {
                        edit.cobSYSTEMCODE.SelectedIndex = i;
                    }
                }
                for (int i = 0; i < edit.cobMODELCODE.Items.Count(); i++)
                {
                    if ((edit.cobMODELCODE.Items[i] as AppModel).Name == entity.MODELCODE)
                    {
                        edit.cobMODELCODE.SelectedIndex = i;
                    }
                }
                edit.Show();
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择一条记录进行修改！", "确定");
            }
        }

        void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            EditDefaultMessage edit = new EditDefaultMessage(null);
            edit.SaveCompleted += (obj, ev) =>
            {//保存成功，重新绑定
                this.btnInit.Visibility = Visibility.Collapsed;
                SearchDefaultList();
            };
            edit.appSystem = appSystem;
            edit.appModel = appModel;
            edit.cobSYSTEMCODE.ItemsSource = this.cbSystemCode.ItemsSource;
            edit.cobSYSTEMCODE.SelectedIndex = 0;
            edit.cobMODELCODE.ItemsSource = this.cbModelCode.ItemsSource;
            edit.cobMODELCODE.SelectedIndex = 0;           
            edit.Show();
        }

        private void cbSystemCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbSystemCode.SelectedItem as SMT.Workflow.Platform.Designer.PlatformService.AppSystem;
            if (item != null && item.ObjectFolder != null)
            {//ent.Name=="0" ==请选择......
                if (appModel != null)
                {
                    var models = from ent in appModel
                                 where ent.ObjectFolder == item.ObjectFolder || ent.Name == "0"
                                 select ent;
                    if (models.Count() > 0)
                    {
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
                        this.cbModelCode.ItemsSource = models;
                        this.cbModelCode.SelectedIndex = 0;
                    }
                }
            }
        }

    }
}
