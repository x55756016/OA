using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SMT.Workflow.Platform.UI.ProcessBar
{
    public partial class FlowDoubleClickDG : UserControl
    {
        public FlowDoubleClickDG()
        {
            InitializeComponent();
            LoadData();
        }

        #region 数据源绑定 
        List<ModuleMode> _listmodule = null;
        /// <summary>
        /// 模块信息加载
        /// </summary>
        public void LoadData()
        {
            _listmodule = new List<ModuleMode>();
            for (int i = 0; i < 17; i++)
            {
                _listmodule.Add(new ModuleMode()
                {
                    Txt1 = "流程描述流程描述-" + i.ToString(),
                    Txt2 = "审批",
                    Txt3 = "**管理员",
                    Txt4 = DateTime.Now.ToShortDateString()
                });
            }
            MyDataGrid.ItemsSource = _listmodule;
        }
        #endregion

        #region 定义Msg依赖属性
        public static readonly DependencyProperty MsgProperty = DependencyProperty.Register("Msg", typeof(string), typeof(FlowDoubleClickDG), new PropertyMetadata(new PropertyChangedCallback(FlowDoubleClickDG.OnMsgPropertyChanged)));

        private static void OnMsgPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlowDoubleClickDG node = d as FlowDoubleClickDG;
            node.Msg = (string)e.NewValue;
        }

        public string Msg
        {
            get
            {
                return (string)base.GetValue(MsgProperty);
            }
            set
            {
                base.SetValue(MsgProperty, value);
            }
        }
        #endregion

        private void MyDataGrid_RowClicked(object sender, DataGridRowClickedArgs e)
        {
            Msg = "单击事件";
        }

        private void MyDataGrid_RowDoubleClicked(object sender, DataGridRowClickedArgs e)
        {
            Msg = "双击事件";
        }
    }

    public class ModuleMode
    {
        public string Txt1 { get; set; }
        public string Txt2 { get; set; }
        public string Txt3 { get; set; }
        public string Txt4 { get; set; }
    }
}
