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

using System.Collections.Generic;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.SaaS.FrameworkUI
{
    public delegate void FormClosedHandler(object sender, FormClosedEventArgs args);
    public interface IClient
    {
        /// <summary>
        /// 窗体关闭完成后触发的事件
        /// </summary>
        //event FormClosedHandler OnFormClosed;

        void ClosedWCFClient();

        bool CheckDataContenxChange();

        void SetOldEntity(object entity);
    }
    public class FormClosedEventArgs : EventArgs
    {
        public object EntityEditor { get; set; }
        public string RefreshedArgs { get; set; }
    }

    //public virtual class FormExtension :ChildWindow, IForm
    //{
    //    /// <summary>
    //    /// 编辑前实体
    //    /// </summary>
    //    public object OldEntity { get; set; }

    //    /// <summary>
    //    /// 编辑后实体
    //    /// </summary>
    //    public object NewEntity { get; set; }


    //    public virtual void FormClosed(SMT.SAAS.Controls.Toolkit.Windows.Window ParentWindow)
    //    {
    //        if (OldEntity != NewEntity)
    //        {
    //            ParentWindow.IsClose = false;
    //            string Result = "";
    //            ComfirmWindow cw = new ComfirmWindow();
    //            cw.OnSelectionBoxClosed += (obj, result) =>
    //            {
    //                ParentWindow.IsClose = false;
    //                this.Close();
    //            };
    //            cw.SelectionBox(Utility.GetResourceStr("CLOSEWINDOW"), Utility.GetResourceStr("CLOSEWINDOWALTER"), ComfirmWindow.titlename, Result);

    //        }
    //    }


    //    #region IForm 成员

    //    public event FormClosedHandler OnFormClosed;

    //    #endregion
    //}
}
