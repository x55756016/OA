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
using System.Windows.Data;
namespace SMT.SaaS.OA.UI.Class
{
    public static class DataPageExtension
    {
        public static void BindSource(this　DataPager dataPager, int totalCount, int pageSize)
        {
            List<int> list = new List<int>(totalCount);
            for (int i = 0; i < totalCount * pageSize; i++)
            {
                list.Add(i);
            }
            PagedCollectionView pcv = new PagedCollectionView(list);
            pcv.PageSize = pageSize;
            //dataPager.Source = pcv;
            //dataPager.it = pcv;
            dataPager.DataContext = pcv;
            //int iii = dataPager.PageCount;
        }
    }
}