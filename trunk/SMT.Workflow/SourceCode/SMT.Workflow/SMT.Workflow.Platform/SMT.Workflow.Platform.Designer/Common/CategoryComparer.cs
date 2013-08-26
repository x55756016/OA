using System;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using SMT.Workflow.Platform.Designer.PlatformService;

namespace SMT.Workflow.Platform.Designer.Common
{
    public class CategoryComparer : IEqualityComparer<FLOW_FLOWCATEGORY>
    {
        public bool Equals(FLOW_FLOWCATEGORY x, FLOW_FLOWCATEGORY y)
        {
            if (x.FLOWCATEGORYID != null && y.FLOWCATEGORYID != null)
            {
                return x.FLOWCATEGORYID.Equals(y.FLOWCATEGORYID);
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(FLOW_FLOWCATEGORY obj)
        {
            if (obj != null && obj.FLOWCATEGORYID!=null)
            {
                return obj.FLOWCATEGORYID.GetHashCode();
            }
            else
            {
                return 0;
            }
        }
    }
}
