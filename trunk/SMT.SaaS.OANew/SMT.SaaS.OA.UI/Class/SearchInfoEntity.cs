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

namespace SMT.SaaS.OA.UI.Class
{
    public class SearchInfoEntity
    {
        private string compareString;
        public string CompareString
        {
            get { return compareString; }
            set { compareString = value; }
        }

        private string valueString;
        public string ValueString
        {
            get { return valueString; }
            set { valueString = value; }
        }
    }
}
