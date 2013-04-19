// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace SMT.SaaS.FrameworkUI
{
   
    public  class SmtDatePicker : DatePicker
    {
        /// <summary>
        /// Default mouse wheel handler for the DatePicker control.
        /// </summary>
        /// <param name="e">Mouse wheel event args.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            //base.OnMouseWheel(e);
            //if (!e.Handled && this.SelectedDate.HasValue)
            //{
            //    DateTime selectedDate = this.SelectedDate.Value;
            //    DateTime? newDate = DateTimeHelper.AddDays(selectedDate, e.Delta > 0 ? -1 : 1);
            //    if (newDate.HasValue && Calendar.IsValidDateSelection(this._calendar, newDate.Value))
            //    {
            //        this.SelectedDate = newDate;
            //        e.Handled = true;
            //    }
            //}
        }
    }
}
