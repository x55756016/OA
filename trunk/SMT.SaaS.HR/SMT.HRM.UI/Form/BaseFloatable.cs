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

namespace SMT.HRM.UI
{
    public class BaseFloatable : System.Windows.Controls.Window
    {
        private MainPage currentMain;
        public BaseFloatable()
        {
            Grid grid = Application.Current.RootVisual as Grid;
            if (grid != null && grid.Children.Count > 0)
            {
                currentMain = grid.Children[0] as MainPage;
            }           
        }
        public delegate void refreshGridView();

        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
    }
}
