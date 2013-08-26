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
    public static class DesignerUtils
    {
        public static int GetIdentityNo(int[] arySource)
        {
            if (arySource.Length == 0) return 1;

            BubbleSorter.Sort(arySource,
                (int n1, int n2) => { return n1 < n2; });

            for (int i = 0; i < arySource.Length; i++)
            {
                if (arySource[i] != i + 1) return i + 1;
            }

            return arySource.Length + 1;
        }

        public static int GetIdentityNo(List<int> list)
        {
            if (list.Count == 0) return 1;

            BubbleSorter.Sort(list,
                (int n1, int n2) => { return n1 < n2; });

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != i + 1) return i + 1;
            }

            return list.Count + 1;
        }
    }
}
