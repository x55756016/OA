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

using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Personnel
{
    public partial class CheckItem : BaseForm,IClient
    {
        private PersonnelServiceClient client;

        private ObservableCollection<T_HR_CHECKPOINTSET> Pointset = new ObservableCollection<T_HR_CHECKPOINTSET>();

        public CheckItem()
        {
            InitializeComponent();
            InitiEvent();
        }

        private void InitiEvent()
        {
            client = new PersonnelServiceClient();
            client.GetCheckPointSetByPrimaryIDCompleted += new EventHandler<GetCheckPointSetByPrimaryIDCompletedEventArgs>(client_GetCheckPointSetByPrimaryIDCompleted);
        }

        void client_GetCheckPointSetByPrimaryIDCompleted(object sender, GetCheckPointSetByPrimaryIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                Pointset = e.Result;
                lbxCheckPoint.DataContext = Pointset;
            }
        }

        public void LoadData(T_HR_CHECKPROJECTSET entity)
        {
            //Projectset = entity;
        }
        #region IClient
        public void ClosedWCFClient()
        {
            //  throw new NotImplementedException();
            client.DoClose();
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
