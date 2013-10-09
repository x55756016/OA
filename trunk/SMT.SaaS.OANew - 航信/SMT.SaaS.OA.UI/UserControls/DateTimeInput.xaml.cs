using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class DateTimeInput : BaseForm, IClient
    {
        public DateTimeInput()
        {
            InitializeComponent();
        }

        // 属性缺省
        private DateTime dateTimeValue;
        public DateTime DateTimeValue
        {
            get
            {
                try
                {
                    dateTimeValue = Convert.ToDateTime(dpDate.Text + " " + tpTime.Value.Value.TimeOfDay.ToString());
                    return dateTimeValue;
                }
                catch
                {
                    return new DateTime(1, 1, 1, 0, 0, 0);
                }
            }
            set
            {
                dateTimeValue = value;
                if (value == null)
                {
                    dateTimeValue = System.DateTime.Now;
                }
                dpDate.Text = dateTimeValue.ToShortDateString();
                tpTime.Value = Convert.ToDateTime(dateTimeValue.ToShortTimeString());
            }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            set
            {
                if (value)
                {
                    dpDate.IsEnabled = !value;
                    tpTime.IsEnabled = !value;
                }
            }
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            throw new NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
