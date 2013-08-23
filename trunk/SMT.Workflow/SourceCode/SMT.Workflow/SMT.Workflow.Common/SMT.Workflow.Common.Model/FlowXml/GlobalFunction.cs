using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.Common.Model.FlowXml
{
    public static class GlobalFunction
    {
        public static string CvtString(this object obj)
        {
            if (obj == null) return "";
            try
            {
                return obj.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
