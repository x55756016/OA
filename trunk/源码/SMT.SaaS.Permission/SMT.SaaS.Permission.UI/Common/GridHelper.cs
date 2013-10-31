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
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;

namespace SMT.SaaS.Permission.UI
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


        public static void HandleDataPageDisplay(GridPager gridpager, int pageCout)
        {
            if (pageCout > 0)
            {
                gridpager.Visibility = Visibility.Visible;
            }
            else
            {
                gridpager.Visibility = Visibility.Collapsed;
            }
            gridpager.PageCount = pageCout;
        }

        public static void SetUnCheckAll(DataGrid grid)
        {
            if (grid == null) return;
            CheckBox chkbox = Utility.FindChildControl<CheckBox>(grid, "SelectAll");
            if (chkbox != null)
            {
                chkbox.IsChecked = false;
            }
        }

        public static ObservableCollection<string> GetSelectItem(DataGrid grid, string chkboxName, Action action)
        {
            if (grid.ItemsSource != null)
            {
                ObservableCollection<string> selectedObj = new ObservableCollection<string>();
                foreach (object obj in grid.ItemsSource)
                {
                    if (grid.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = grid.Columns[0].GetCellContent(obj).FindName(chkboxName) as CheckBox; //cb为
                        if (ckbSelect.IsChecked == true)
                        {
                            selectedObj.Add(ckbSelect.Tag.ToString());
                            if (action == Action.Edit)
                            {
                                break;
                            }
                        }

                    }
                }
                if (selectedObj.Count > 0)
                {
                    return selectedObj;
                }
            }
            return null;
        }



    }
}
