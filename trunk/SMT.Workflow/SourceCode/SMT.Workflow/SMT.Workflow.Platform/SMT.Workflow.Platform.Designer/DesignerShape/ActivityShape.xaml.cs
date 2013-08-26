/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：ActivityShape.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/15 13:59:11   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerShape 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
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
using SMT.Workflow.Platform.Designer.DesignerControl;

namespace SMT.Workflow.Platform.Designer.DesignerShape
{
    public partial class ActivityShape : UserControl, IShape
    {
        public ActivityShape()
        {
            InitializeComponent();
        }
        private ElementState state = ElementState.Focus;
        public ElementState State
        {
            get { return state; }
        }
        public void SetFocus()
        {
            if (this.state != ElementState.Focus)
            {
                SetHotspotStyle(Colors.Blue, 1.0);
                HotspotLeft.Visibility = Visibility.Visible;
                HotspotTop.Visibility = Visibility.Visible;
                HotspotRight.Visibility = Visibility.Visible;
                HotspotBottom.Visibility = Visibility.Visible;

                this.state = ElementState.Focus;
            }
        }
        public void SetUnFocus()
        {
            if (this.state != ElementState.UnFocus)
            {
                HotspotLeft.Visibility = Visibility.Collapsed;
                HotspotTop.Visibility = Visibility.Collapsed;
                HotspotRight.Visibility = Visibility.Collapsed;
                HotspotBottom.Visibility = Visibility.Collapsed;

                this.state = ElementState.UnFocus;
            }
        }
        public void SetSelected()
        {
            if (this.state != ElementState.Selected)
            {
                SetHotspotStyle(Colors.Red, 0.5);
                HotspotLeft.Visibility = Visibility.Visible;
                HotspotTop.Visibility = Visibility.Visible;
                HotspotRight.Visibility = Visibility.Visible;
                HotspotBottom.Visibility = Visibility.Visible;

                this.state = ElementState.Selected;
            }
        }
        /// <summary>
        /// 设置热点的外观
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="opacity">透明度</param>
        private void SetHotspotStyle(Color color, double opacity)
        {
            HotspotLeft.Fill = new SolidColorBrush(color);
            HotspotLeft.Opacity = opacity;
            HotspotTop.Fill = new SolidColorBrush(color);
            HotspotTop.Opacity = opacity;
            HotspotRight.Fill = new SolidColorBrush(color);
            HotspotRight.Opacity = opacity;
            HotspotBottom.Fill = new SolidColorBrush(color);
            HotspotBottom.Opacity = opacity;
        }

        public void SetTitle(string title)
        {
            this.txtActivityTitle.Text = title;
        }
        public string GetTitle()
        {
            return this.txtActivityTitle.Text;
        }

        public void Fill(Color color, double opacity)
        {
            this.rectActivity.Fill = new SolidColorBrush(color);
            this.rectActivity.Opacity = opacity;
        }
    }
}
