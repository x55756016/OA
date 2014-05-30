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

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Packaging;
using SMT.SaaS.FrameworkUI.RichNotepad.ActionLink.Contracts;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace SMT.SaaS.FrameworkUI.RichNotepad.InteractiveText
{
    public class InteractiveTextBox : RichTextBox
    {
        private const string PREFIX = "http://desperate/";

        [Import]
        public IDefineCommand _defineCommand { private get; set; }

        [Import]
        public IPerformCommand _performCommand { get; set; }

        public InteractiveTextBox()
            : base()
        {
            try
            {
                var catalog = new PackageCatalog();
                catalog.AddPackage(Package.Current);
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
            catch (Exception exc)
            {
                throw new Exception("The application is missing an IDefineCommand and/or IPerformCommand component. Make sure the application refers to projects that implement these. ", exc);
            }
        }

        public void DisplayActionDefinition()
        {
            if (_defineCommand != null)
            {
                // try get current value of CommandParameter
                string commandParameter = String.Empty;
                XElement root = XElement.Parse(this.Selection.Xaml);
                XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                var linkElement = root.Element(ns + "Paragraph").Element(ns + "Hyperlink");
                if (linkElement != null)
                {
                    XAttribute attr = linkElement.Attribute("CommandParameter");
                    if (attr != null)
                    {
                        commandParameter = attr.Value;
                    }
                    else // delete this once CommandParameter is supported in TextSelection.Xaml
                    {
                        XAttribute attr1 = linkElement.Attribute("NavigateUri"); // abuse alert
                        if (attr1 != null)
                        {
                            commandParameter = attr1.Value.Replace(PREFIX, String.Empty);
                        }
                    }
                }
                _defineCommand.Prompt(this.Selection.Text, commandParameter, ConsumeLinkDefinition);
            }
        }

        public void ToggleReadOnly()
        {
            IsReadOnly = !IsReadOnly;
            this.Xaml = Xaml; // to set commands
        }

        public new string Xaml
        {
            get
            {
                return (this as RichTextBox).Xaml;
            }
            set
            {
                (this as RichTextBox).Xaml = value;
                if (_performCommand != null)
                {
                    SetCommands(Blocks);
                }
            }
        }

        private void SetCommands(BlockCollection blocks)
        {
            var res = from block in blocks
                      from inline in (block as Paragraph).Inlines
                      where inline.GetType() == typeof(InlineUIContainer)
                      select inline;

            foreach (var block in blocks)
            {
                Paragraph p = block as Paragraph;

                foreach (var inline in p.Inlines)
                {
                    Hyperlink hlink = inline as Hyperlink;
                    if (hlink != null)
                    {
                        string uri = hlink.NavigateUri.AbsoluteUri;
                        if (uri.StartsWith(PREFIX))
                        {
                            // We have a CommandParameter
                            hlink.Command = _performCommand;
                            // Delete the following once CommandParameter is supported in TextSelection.Xaml
                            hlink.CommandParameter = uri.Substring(PREFIX.Length);
                        }
                    }
                }
            }
        }

        private void ConsumeLinkDefinition(string content, object linkDefinition)
        {
            Hyperlink hyperlink = new Hyperlink();

            hyperlink.Inlines.Add(content);
            string def = Convert.ToString(linkDefinition);
            if (!String.IsNullOrEmpty(def))
            {
                hyperlink.CommandParameter = def;       // this is useless; it does not show up in the Xaml of a TextSelection
                hyperlink.NavigateUri = new Uri(PREFIX + def);   // abuse alert; no other place to go :(
            }
            this.Selection.Insert(hyperlink);   // this fails if the selection is within another hyperlink
        }

    }
}
