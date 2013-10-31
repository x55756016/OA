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

namespace SMT.SaaS.Permission.UI
{
    public class ServiceResult
    {
        private string resultMessage;

        public string ResultMessage
        {
            get { return resultMessage; }
            set { resultMessage = value; }
        }
    }
}
