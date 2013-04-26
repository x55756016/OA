//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 信息列表子节点
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：高雁 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SMT.SAAS.Platform.Controls.InfoPanel
{
    public partial class InfoIndexItem : UserControl
    {
        private int id;
        public RoutedEventHandler Click;
        public InfoIndexItem()
        {
            this.InitializeComponent();
            this.LayoutRoot.MouseLeftButtonDown += new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonDown);
        }
        public void DeSelectItem()
        {
            VisualStateManager.GoToState(this, "Deselect", false);
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Click != null)
            {
                this.Click.Invoke(this, e);
            }
        }

        public void SelectItem()
        {
            VisualStateManager.GoToState(this, "Select", false);
        }

        // Properties
        public int Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
            }
        }
    }
}
