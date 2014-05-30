using System;
using System.ComponentModel.Composition;

namespace SMT.SaaS.FrameworkUI.RichNotepad.ActionLink.Contracts
{
    [InheritedExport]
    public interface IDefineCommand
    {
        void Prompt(string content, string commandParameter, Action<string, object> callback);
    }
}
