using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI
{
    public partial class S_Images : BaseForm, IClient
    {
        public S_Images()
        {
            // Required to initialize variables
            InitializeComponent();
            IndentifyCodeClass icc = new IndentifyCodeClass();
            icc.CreatImage(icc.CreateIndentifyCode(6), I_mage, 100, 30);
        }

        private void I_mage_MLBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	IndentifyCodeClass icc = new IndentifyCodeClass();
            icc.CreatImage(icc.CreateIndentifyCode(6), I_mage, 100, 30);
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            throw new NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}