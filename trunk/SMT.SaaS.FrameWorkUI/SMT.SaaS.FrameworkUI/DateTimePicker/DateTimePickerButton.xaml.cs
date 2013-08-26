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
using System.Windows.Media.Imaging;

namespace SMT.SaaS.FrameworkUI
{
    public partial class DateTimePickerButton : Button
    {
        public DateTimePickerButton()
        {
            InitializeComponent();
            
        }
        public Image Image
        {
            get { return image; }
        }

        /// <summary>
        /// 新增新的用户控件
        /// </summary>
        /// <param name="_img">图标地址</param>
        /// <param name="_name">显示文本</param>
        /// <returns>当前图片按钮</returns>
        public Button AddButtonAction(string _img, string _name)
        {  
            Image.Source = new BitmapImage(new Uri(_img, UriKind.Relative));
            Image.Tag = _name;
            ToolTipService.SetToolTip(Image, _name);
            //TextBlock.Text = _name;
            return this;
        }
    }
}
