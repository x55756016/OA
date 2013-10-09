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

using System.Collections.Generic;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.UserControls
{
    public class VehicleMgt
    {
       
        /// <summary>
        /// 设置工具栏按钮
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static List<ToolbarItem> GetToolBarItems(ref object[,] arr)
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item;
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                item = new ToolbarItem
               {
                   DisplayType = (ToolbarItemDisplayTypes)arr[i, 0],
                   Key = (string)arr[i, 1],
                   Title = Utility.GetResourceStr((string)arr[i, 2]),
                   ImageUrl = (string)arr[i, 3]
               };
                items.Add(item);
            }
            return items;
        }
    }
}
