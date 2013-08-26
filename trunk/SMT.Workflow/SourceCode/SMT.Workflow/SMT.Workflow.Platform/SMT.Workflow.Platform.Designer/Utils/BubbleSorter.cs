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

namespace SMT.Workflow.Platform.Designer.Utils
{
    public static class BubbleSorter
    {
        public delegate bool Compare<T>(T t1, T t2);

        public static void Sort<T>(T[] items, Compare<T> compare)
        {
            for (int i = 0; i < items.Length; i++)
            {
                for (int j = i + 1; j < items.Length; j++)
                {
                    if (!compare(items[i], items[j]))
                    {
                        var temp = items[i];
                        items[i] = items[j];
                        items[j] = (T)temp;
                    }
                    //compare < 则顺序,> 则逆序 
                }
            }
        }

        public static void Sort<T>(List<T> list, Compare<T> compare)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (!compare(list[i], list[j]))
                    {
                        var temp = list[i];
                        list[i] = list[j];
                        list[j] = (T)temp;
                    }
                    //compare < 则顺序,> 则逆序 
                }
            }
        }
    }
}
