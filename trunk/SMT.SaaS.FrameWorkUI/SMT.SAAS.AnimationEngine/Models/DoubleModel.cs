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

namespace SMT.SAAS.AnimationEngine.Model
{
    public class DoubleModel : LinearModel<Double>, IModel
    {
        #region IModel 成员

        public IModelType ModelType
        {
            get
            {
                return IModelType.Double;
            }
            
        }

        #endregion
    }
}
