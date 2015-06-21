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
using System.Windows.Interactivity;

namespace SMT.SAAS.Behaviors
{
    public class OpenComboBoxBehavior : Behavior<ComboBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.MouseEnter += new MouseEventHandler(AssociatedObject_MouseEnter);
        }
        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseEnter -= new MouseEventHandler(AssociatedObject_MouseEnter);
            base.OnDetaching();
        }
        void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            this.AssociatedObject.IsDropDownOpen = true;
        }
    }
}
