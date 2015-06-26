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
using System.Windows.Navigation;
using SMT.FileUpLoad;

namespace SMT.FileUpLoad
{
    public partial class Test : Page
    {
        public Test()
        {
            InitializeComponent();
            FileList.Init("LM", "CUSTOME", "Fuyicheng", "d027ed66-48c5-46cb-b24d-866b17704728");
        }


        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

    }
}
