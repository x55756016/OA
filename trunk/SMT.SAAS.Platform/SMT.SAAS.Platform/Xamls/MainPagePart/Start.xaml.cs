using System;
using System.Windows;
using System.Windows.Controls;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class Start : UserControl
    {
        public event EventHandler OnClick;

        public Start()
        {
            InitializeComponent();
        }

        private void BtnMenus_Click(object sender, RoutedEventArgs e)
        {
            if (OnClick != null)
                OnClick(this, EventArgs.Empty);
        }
    }
}
