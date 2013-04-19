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

namespace SMT.HRM.UI.Active
{
    public interface IContainer
    {
        void SetPos(StateActive Active);
        void  StateActiveSet(string ActiveName);
        void RemoveStateActiveSet();
        void AddStateActive();
        void RuleActiveSet(string RuleName);
    }
}
