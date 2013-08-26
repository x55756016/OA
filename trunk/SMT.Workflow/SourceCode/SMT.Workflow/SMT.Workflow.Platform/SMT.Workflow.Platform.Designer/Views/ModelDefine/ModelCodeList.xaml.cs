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
using SMT.Workflow.Platform.Designer.UControls;
using SMT.Workflow.Platform.Designer.PlatformService;
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.Class;
using SMT.Workflow.Platform.Designer.Common;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.Utils;
using Telerik.Windows.Controls;

namespace SMT.Workflow.Platform.Designer.Views.ModelDefine
{
    public partial class ModelCodeList : UserControl
    {
        #region 全局参数定义
        public Flow_ModelDefineClient client = null; //流程模块WCF定义
        public FlowXmlDefineClient clientXml = null; //xmlWCF定义      
        public ObservableCollection<FLOW_MODELDEFINE_T> FlowModelList = new ObservableCollection<FLOW_MODELDEFINE_T>();
        public ObservableCollection<AppSystem> appSystem = new ObservableCollection<AppSystem>();//系统XML定义    
        public ObservableCollection<AppModel> appModel = new ObservableCollection<AppModel>();
        #endregion
        public ModelCodeList()
        {
            InitializeComponent();
            client = new Flow_ModelDefineClient(); //实例化流程模块
            clientXml = new FlowXmlDefineClient(); //实例化xml
            ToolBarMinor.ButtonNew.Click += new RoutedEventHandler(ButtonNew_Click);
            ToolBarMinor.ButtonUpdate.Click += new RoutedEventHandler(ButtonUpdate_Click); //修改
            ToolBarMinor.ButtonDelete.Click += new RoutedEventHandler(ButtonDelete_Click); //删除        
            ToolBarMinor.ButtonSearch.Visibility = System.Windows.Visibility.Collapsed;
            ToolBarMinor.ButtonSave.Visibility = System.Windows.Visibility.Collapsed;
            RegisterServices();
            pBar.Start();
            clientXml.ListSystemAsync();
        }

      
       

        private void ModelCodeListBind()
        {
            int pageCont = 0;
            string strFilter = "";
            if (cbSystemCode.SelectedIndex > 0)
            {
                var systemcode = cbSystemCode.SelectedItem as AppSystem;
                strFilter += " AND SYSTEMCODE='" + systemcode.Name + "'";
            }
            if (txtModelCode.Text.Trim() != string.Empty)
            {
                strFilter += " AND DESCRIPTION like '%" + txtModelCode.Text.Trim() + "%'";
            }
          
            client.GetModelDefineListAsync(strFilter, dataPager1.PageIndex, dataPager1.PageSize, pageCont);
        }
       
        #region 操作完成后事件
        /// <summary>
        /// 注册WCF
        /// </summary>
        private void RegisterServices()
        {
            client.GetModelDefineListCompleted += new EventHandler<GetModelDefineListCompletedEventArgs>(client_GetModelDefineListCompleted);
            clientXml.ListSystemCompleted+=new EventHandler<ListSystemCompletedEventArgs>(clientXml_ListSystemCompleted);
            clientXml.ListModelCompleted += new EventHandler<ListModelCompletedEventArgs>(clientXml_ListModelCompleted);
            client.DeleteModelDefineCompleted += new EventHandler<DeleteModelDefineCompletedEventArgs>(client_DeleteModelDefineCompleted);
        }

        void client_DeleteModelDefineCompleted(object sender, DeleteModelDefineCompletedEventArgs e)
        {
            if (e.Result == "1")
            {
                ModelCodeListBind();
                ComfirmWindow.ConfirmationBox("提示信息", "删除成功!", "确定");

            }
            else if (e.Result == "10")
            {
                ComfirmWindow.ConfirmationBox("提示信息", "已经关联流程不能删除!", "确定");
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "删除失败!", "确定");
            }
            pBar.Stop();
        }

        void clientXml_ListModelCompleted(object sender, ListModelCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                 
                    e.Result.Insert(0, new AppModel() { Name = "0", Description = "请选择......" });
                    appModel = e.Result;
                    ModelCodeListBind();                 
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "系统模块加载错误！", "确定");
            }
        }

     

        void client_GetModelDefineListCompleted(object sender, GetModelDefineListCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                dgModelCode.ItemsSource = e.Result;
                dataPager1.PageCount = e.pageCount;               
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示信息", "数据查询失败，请重新刷新数据！", "确定");
            }
            pBar.Stop();
        }

      
    
     
        //加载所以系统
        void clientXml_ListSystemCompleted(object sender, ListSystemCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    ObservableCollection<string> xmlurl = new ObservableCollection<string>(); //子系统XML路径
                    FlowSystemModel.ListAppSystem = e.Result == null ? null : e.Result.ToList();
                    appSystem.Clear();
                    appSystem.Add(new AppSystem
                    {
                        Description = "请选择......",
                        Name = "0",
                        ObjectFolder = "0"
                    });
                    foreach (var item in FlowSystemModel.ListAppSystem)
                    {
                        appSystem.Add(item);
                        xmlurl.Add(item.ObjectFolder);
                    }
                    if (xmlurl.Count > 0)
                    {
                        clientXml.ListModelAsync(xmlurl);
                    }
                    this.cbSystemCode.ItemsSource = appSystem;
                    this.cbSystemCode.SelectedIndex = 0;
                }               
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBox("提示", ex.Message.ToString(), "确定");
            }
           
        }      
        #endregion

        #region 按钮事件 新增 修改 删除 分页 查询
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            if (appModel.Count > 1 && appSystem.Count > 1)
            {
                ModelCodeEdit edit = new ModelCodeEdit(null, appModel, appSystem);
                edit.SaveCompleted += (obj, ev) =>
                {
                    ModelCodeListBind();
                };
                edit.Show();
            }
            else            
            {
                ComfirmWindow.ConfirmationBox("提示", "数据加载有误！", "确定");
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (appModel.Count > 1 && appSystem.Count > 1)
            {
                if (this.dgModelCode.SelectedItems.Count == 1)
                {
                    FLOW_MODELDEFINE_T define = this.dgModelCode.SelectedItem as FLOW_MODELDEFINE_T;
                    ModelCodeEdit edit = new ModelCodeEdit(define, appModel, appSystem);
                    edit.SaveCompleted += (obj, ev) =>
                    {
                        ModelCodeListBind();
                    };
                    edit.Show();
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示", "选择一条需要修改的记录！", "确定");
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox("提示", "数据加载有误！", "确定");
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {

            if (dgModelCode.SelectedItems.Count > 1)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "只能选择一条需要删除的记录！", "确定");
                return;
            }
            if (dgModelCode.SelectedItems.Count == 1)
            {              
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    pBar.Start();
                    FLOW_MODELDEFINE_T define = (dgModelCode.SelectedItem as FLOW_MODELDEFINE_T);
                    ObservableCollection<string> delete = new ObservableCollection<string>();
                    delete.Add(define.MODELCODE);
                    client.DeleteModelDefineAsync(delete);
                };
                com.SelectionBox("删除确定", "你确定删除选中的记录吗？", ComfirmWindow.titlename, Result);
               
            }
            else
            {               
                ComfirmWindow.ConfirmationBox("提示信息", "请先选择一条需要删除的记录！", "确定");
            }
        }
        /// <summary>
        /// 查询事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dataPager1.PageIndex = 1;
            pBar.Start();
            ModelCodeListBind();
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

                ModelCodeListBind();
                curentIndex = dataPager1.PageIndex;
            }

        }
        #endregion


    }
}
