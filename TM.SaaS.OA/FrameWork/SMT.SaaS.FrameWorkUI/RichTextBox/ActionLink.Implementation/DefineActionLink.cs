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
using System.Xml.Linq;
using System.Linq;

namespace SMT.SaaS.FrameworkUI.RichNotepad.ActionLink.Implementation
{
    public class DefineActionLink : ChildWindow, IDefineCommand
    {
        private TextBox _commandInput = null;

        public DefineActionLink()
            : base()
        {
            Title = "Please enter a command";

            _commandInput = new TextBox();
            _commandInput.Width = 200.0;
            this.Content = _commandInput;

            this.Closing += (s, e) =>
            {
                if (_callback != null)
                {
                    _callback(_content, _commandInput.Text);
                }
            };
        }


        #region IDefineCommand Members

        private Action<string, object> _callback = null;    // to convey the link text and the command definition
        private string _content;
        public void Prompt(string content, string commandParameter, Action<string, object> callback)
        {
            _callback = callback;
            _content = content;     // This simplest example does not support changing the content 
            _commandInput.Text = commandParameter;

            this.Show();
        }

        #endregion
    }
}
