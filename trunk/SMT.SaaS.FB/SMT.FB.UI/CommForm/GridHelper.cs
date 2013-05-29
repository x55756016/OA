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

namespace SMT.FB.UI.CommForm
{
    public class GridHelper
    {
        public static void HandleAllCheckBoxClick(DataGrid grid, string cbxName, bool isSelectedAll)
        {
            if (grid.ItemsSource != null)
            {
                if (isSelectedAll)//全选
                {
                    foreach (object obj in grid.ItemsSource)
                    {
                        if (grid.Columns[0].GetCellContent(obj) != null)
                        {
                            CheckBox cb1 = grid.Columns[0].GetCellContent(obj).FindName(cbxName) as CheckBox; //cb为
                            if (cb1 != null)
                            {
                                cb1.IsChecked = true;
                            }
                        }
                        grid.SelectedItems.Add(obj);

                    }
                }
                else//取消
                {
                    foreach (object obj in grid.ItemsSource)
                    {
                        if (grid.Columns[0].GetCellContent(obj) != null)
                        {
                            CheckBox cb2 = grid.Columns[0].GetCellContent(obj).FindName(cbxName) as CheckBox;
                            if (cb2 != null)
                            {
                                cb2.IsChecked = false;
                            }
                        }
                        grid.SelectedItems.Remove(obj);
                    }
                }
            }
        }
    }
}
