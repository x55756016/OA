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

namespace SMT.Workflow.Platform.Designer.Views.FlowDesign
{
    public partial class FlowPane : UserControl
    {
        private ObservableCollection<FlowDesigns> _itemFlow = new ObservableCollection<FlowDesigns>();
        private double _initialZoom = 1.0;
        public FlowPane()
        {
            InitializeComponent();
            _zoomcontrol.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_zoomcontrol_PropertyChanged);
        }

        void _zoomcontrol_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetZoom(_zoomcontrol.Zoom);
        }

        public void SetZoom(double zoomFactor)
        {
            if (this.zoomTransform != null)
            {
                this.zoomTransform.ScaleX = zoomFactor;
                this.zoomTransform.ScaleY = zoomFactor;
                this.TLayoutRoot.Width = this.TLayoutRoot.ActualWidth * this.zoomTransform.ScaleX;
                this.TLayoutRoot.Height = this.TLayoutRoot.ActualHeight * this.zoomTransform.ScaleY;
            }
            else
            {
                this._initialZoom = zoomFactor;
            }
        }

        private void ImgButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
    public class FlowDesigns
    {
        public string ID { get; set; } //ID
        public string Name { get; set; } //流程名称
        public int ParentID { get; set; } // 流程菜单级别
        public string PARENTNAME = string.Empty; //流程菜单级别名称
        public string Txml; //xml
    }
}
