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
    public partial class CheckPointSetForm : BaseFloatable, IClient
    {
        public T_HR_CHECKPROJECTSET ProjectSet { get; set; }

        public FormTypes FormType { get; set; }

        private int currentSumPoint = 0;
        private int currentPointMaxScore = 0;

        private T_HR_CHECKPOINTSET pointSet;

        public T_HR_CHECKPOINTSET PointSet
        {
            get { return pointSet; }
            set
            {
                pointSet = value;
                this.DataContext = value;
            }
        }
        private string projectID;
        private ObservableCollection<T_HR_CHECKPOINTLEVELSET> pointLevel = new ObservableCollection<T_HR_CHECKPOINTLEVELSET>();

        public ObservableCollection<T_HR_CHECKPOINTLEVELSET> PointLevel
        {
            get { return pointLevel; }
            set { pointLevel = value; }
        }

        PersonnelServiceClient client;

        public CheckPointSetForm(FormTypes type, T_HR_CHECKPROJECTSET project, string strID)
        {
            InitializeComponent();
            FormType = type;
            this.projectID = strID;
            ProjectSet = project;
            InitParas(strID);
        }

        private void InitParas(string strID)
        {
            client = new PersonnelServiceClient();

            client.GetCheckPointByEmployeeTypeSumCompleted += new EventHandler<GetCheckPointByEmployeeTypeSumCompletedEventArgs>(client_GetCheckPointByEmployeeTypeSumCompleted);
            client.GetCheckPointAvailableCompleted += new EventHandler<GetCheckPointAvailableCompletedEventArgs>(client_GetCheckPointAvailableCompleted);
            client.GetCheckPointSetByIDCompleted += new EventHandler<GetCheckPointSetByIDCompletedEventArgs>(client_GetCheckPointSetByIDCompleted);
            client.CheckPointSetAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CheckPointSetAddCompleted);
            client.CheckPointSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CheckPointSetUpdateCompleted);

            client.GetCheckPointLevelSetByPrimaryIDCompleted += new EventHandler<GetCheckPointLevelSetByPrimaryIDCompletedEventArgs>(client_GetCheckPointLevelSetByPrimaryIDCompleted);
            this.Loaded += new RoutedEventHandler(CheckPointSetForm_Loaded);

        }

        void CheckPointSetForm_Loaded(object sender, RoutedEventArgs e)
        {

            if (FormType == FormTypes.New)
            {
                PointSet = new T_HR_CHECKPOINTSET();
                pointSet.CHECKPOINTSETID = Guid.NewGuid().ToString();
                pointSet.T_HR_CHECKPROJECTSET = ProjectSet;
                pointSet.CHECKEMPLOYEETYPE = "0";
                CheckPointLevelSetAdd();
            }
            else
            {

                client.GetCheckPointSetByIDAsync(projectID);
            }
        }

        void client_GetCheckPointByEmployeeTypeSumCompleted(object sender, GetCheckPointByEmployeeTypeSumCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                currentSumPoint = e.Result;
                if (FormTypes.Edit == FormType)
                {
                    currentSumPoint -= Convert.ToInt32(PointSet.CHECKPOINTSCORE);
                }
            }
        }

        void client_GetCheckPointAvailableCompleted(object sender, GetCheckPointAvailableCompletedEventArgs e)
        {

            if (e.Error != null && e.Error.Message != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (FormTypes.New == FormType)
                {
                    NudScore.Maximum = e.Result;
                    NudScore.Value = e.Result;
                    currentPointMaxScore = e.Result;
                }
                else
                {
                    //如果为修改，可用分加上原来的分数
                    NudScore.Maximum = e.Result + Convert.ToInt32(PointSet.CHECKPOINTSCORE);
                    currentPointMaxScore = e.Result + Convert.ToInt32(PointSet.CHECKPOINTSCORE);
                }
            }
        }

        void client_GetCheckPointSetByIDCompleted(object sender, GetCheckPointSetByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                PointSet = e.Result;
                //获取等级
                client.GetCheckPointLevelSetByPrimaryIDAsync(PointSet.CHECKPOINTSETID);
            }
        }

        void client_GetCheckPointLevelSetByPrimaryIDCompleted(object sender, GetCheckPointLevelSetByPrimaryIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                PointLevel = e.Result;
                this.DtGrid.ItemsSource = null;
                this.DtGrid.ItemsSource = PointLevel;
            }
        }

        private void CheckPointLevelSetAdd()
        {
            string strLevel = "SABCD";
            for (int i = 0; i < strLevel.Length; i++)
            {
                T_HR_CHECKPOINTLEVELSET pointLevel = new T_HR_CHECKPOINTLEVELSET();
                pointLevel.POINTLEVEID = Guid.NewGuid().ToString();
                pointLevel.T_HR_CHECKPOINTSET = PointSet;
                pointLevel.POINTLEVEL = strLevel[i].ToString();
                pointLevel.POINTSCORE = 0;
                PointLevel.Add(pointLevel);
            }
            this.DtGrid.ItemsSource = null;
            this.DtGrid.ItemsSource = PointLevel;

            NumericUpDownScopeSet();
        }

        private void NumericUpDownScopeSet()
        {
            //为DataGrid设置范围
            NumericUpDown txtEmpName = Utility.FindChildControl<NumericUpDown>(DtGrid, "pointLevelScore");

            foreach (object obj in DtGrid.ItemsSource)
            {
                int i = 0;
                if (DtGrid.Columns[1].GetCellContent(obj) != null)
                {
                    NumericUpDown nud1 = DtGrid.Columns[1].GetCellContent(obj).FindName("pointLevelScore") as NumericUpDown;
                    if (nud1 != null)
                    {
                        nud1.Maximum = currentPointMaxScore - i;
                    }
                }
                i++;
            }
        }

        #region 保存
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                // RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            //判断分数是否超过100分
            if (currentSumPoint + Convert.ToInt32(NudScore.Value) > 100)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CHECKSUMPOINT", "CHECKEMPLOYEETYPE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("CHECKSUMPOINT", "CHECKEMPLOYEETYPE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            //List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            //if (validators.Count > 0)
            //{
            //   // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(validators.Count.ToString() + " invalid validators"));
            //    return;
            //}

            if (FormType == FormTypes.New)
            {
                PointSet.CREATEDATE = DateTime.Now;
                PointSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.CheckPointSetAddAsync(PointSet, PointLevel);
            }
            else
            {
                PointSet.UPDATEDATE = DateTime.Now;
                PointSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.CheckPointSetUpdateAsync(PointSet, PointLevel);
            }
        }

        void client_CheckPointSetAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ADDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                this.ReloadData();
            }
            this.Close();
        }

        void client_CheckPointSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EDITERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                this.ReloadData();
            }
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            this.Close();
        }
        #endregion

        private void cbxEmployeeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PointSet != null)
            {
                //获取可用分数
                client.GetCheckPointAvailableAsync(PointSet.CHECKEMPLOYEETYPE, ProjectSet.CHECKPROJECTID);
                //获取当用员工类型的总分数
                client.GetCheckPointByEmployeeTypeSumAsync(PointSet.CHECKEMPLOYEETYPE);
            }
        }

        private void pointLevelScore_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
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

