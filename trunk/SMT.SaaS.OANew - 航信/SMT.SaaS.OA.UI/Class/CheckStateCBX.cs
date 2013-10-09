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



namespace SMT.SaaS.OA.UI
{
    public class CheckStateCBX
    {
        public static void SetCheckStateCBX(ComboBox cbx)
        {
            try
            {
                
                CheckStateItems[] cbxItems = { new CheckStateItems("0", Utility.GetResourceStr("DRAFTBOX")), 
                                 new CheckStateItems("2", Utility.GetResourceStr("AUDITING")), 
                                 new CheckStateItems("1", Utility.GetResourceStr("AUDITPASS")), 
                                 new CheckStateItems("3", Utility.GetResourceStr("AUDITINGDEFEATED"))
                               };
                cbx.ItemsSource = cbxItems;
                cbx.DisplayMemberPath = "text";
                cbx.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }        
    }

    public class CheckStateItems
    {
        public string value { get; set; }
        public string text { get; set; }

        public CheckStateItems(string value, string text)
        {
            this.value = value;
            this.text = text;
        }

    }
}
