using System.Windows.Input;
using System.ComponentModel.Composition;

namespace SMT.SaaS.FrameworkUI.RichNotepad.ActionLink.Contracts
{
    [InheritedExport]
    public interface IPerformCommand : ICommand {}
}
