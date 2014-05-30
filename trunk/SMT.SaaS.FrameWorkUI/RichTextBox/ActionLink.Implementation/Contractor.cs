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

using SMT.SaaS.FrameworkUI.RichNotepad.ActionLink.Contracts;

namespace SMT.SaaS.FrameworkUI.RichNotepad.ActionLink.Implementation
{
    public class Contractor : IPerformCommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            MessageBox.Show(Convert.ToString(parameter), "Contractor executes ...", MessageBoxButton.OK);
        }

        #endregion
    }
}
