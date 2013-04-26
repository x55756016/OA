using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace SMT.SAAS.Platform.Controls.InfoPanel
{
    public class Info
    {
        public string Titel { get; set; }
        public string Uri { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public string InfoID { get; set; }
        public object DataContext { get; set; }
        public BitmapImage ImageSource { get; set; }
    }
}
