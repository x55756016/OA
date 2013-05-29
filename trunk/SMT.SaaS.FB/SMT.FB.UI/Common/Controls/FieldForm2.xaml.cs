using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.FB.UI.Common;
using SMT.SaaS.FrameworkUI.Validator;

namespace SMT.FB.UI
{
    public partial class FieldForm2 : UserControl, IControlAction
	{
		public FieldForm2()
		{
			// 为初始化变量所必需
			InitializeComponent();
            LeftFForm = this.LeftForm;
            RightFForm = this.RightForm;
		}
        public FieldForm LeftFForm { set; get; }
        public FieldForm RightFForm { set; get; }

        #region IControlAction 成员

        public void InitControl(OperationTypes operationType)
        {
            LeftFForm.InitControl(operationType);
            RightFForm.InitControl(operationType);
        }
        public Control FindControl(string name)
        {
            Control c = LeftFForm.FindControl(name);
            if (c == null)
            {
               c =  RightFForm.FindControl(name);
            }
            return c;
        }
        #endregion



        #region IControlAction 成员


        public ValidatorManager ValidatorManager
        {
            get
            {
                return null;
            }
            set
            {
                this.LeftFForm.ValidatorManager = value;
                this.RightFForm.ValidatorManager = value;
            }

        }


        #endregion

        #region IControlAction 成员


        public void InitData(OrderEntity entity)
        {
            return;
        }

        public bool SaveData(Action<IControlAction> SavedCompletedAction)
        {
            return true;
        }

        #endregion
    }
}