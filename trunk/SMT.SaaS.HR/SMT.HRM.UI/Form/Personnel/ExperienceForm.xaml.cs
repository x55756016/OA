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
    public partial class ExperienceForm : BaseForm,IClient
    {
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        private T_HR_RESUME resume;

        public T_HR_RESUME Resume
        {
            get { return resume; }
            set { resume = value; }
        }

        //待删除工作记录列表
        private ObservableCollection<T_HR_EXPERIENCE> delableExp = new ObservableCollection<T_HR_EXPERIENCE>();

        public ObservableCollection<T_HR_EXPERIENCE> DelableExp
        {
            get { return delableExp; }
            set { delableExp = value; }
        }

        //数据表里有的工作记录列表
        private ObservableCollection<T_HR_EXPERIENCE> usableExp = new ObservableCollection<T_HR_EXPERIENCE>();

        public ObservableCollection<T_HR_EXPERIENCE> UsableExp
        {
            get { return usableExp; }
            set { usableExp = value; }
        }

        private ObservableCollection<T_HR_EXPERIENCE> experienceList = new ObservableCollection<T_HR_EXPERIENCE>();

        public ObservableCollection<T_HR_EXPERIENCE> ExperienceList
        {
            get { return experienceList; }
            set { experienceList = value; }
        }

        PersonnelServiceClient client;
        public ExperienceForm()
        {
            InitializeComponent();
            InitControlEvent();
        }

        public void LoadData(FormTypes type, string resumeID,T_HR_RESUME hrResume)
        {
            formType = type;
            Resume = hrResume;
            if(formType ==FormTypes.Browse)
            {
                this.IsEnabled =false;
            }
            if (formType == FormTypes.New)
            {
                //判断是否已添加
                if (ExperienceList.Count>0)
                {
                    DataGridBinder();
                }
                else
                {
                    ExperienceAdd();
                }
            }
            else
            {
                if (ExperienceList!=null&&ExperienceList.Count > 0)
                {
                    DataGridBinder();
                }
                else
                {
                    client.GetExperienceAllAsync(resumeID);
                }
            }
        }

        //DataGrid绑定
        private void DataGridBinder()
        {
            this.DtGrid.ItemsSource = null;
            this.DtGrid.ItemsSource = ExperienceList;
        }

        private void ExperienceAdd()
        {
            if (ExperienceList == null)
            {
                ExperienceList = new ObservableCollection<T_HR_EXPERIENCE>();
            }
            T_HR_EXPERIENCE experience = new T_HR_EXPERIENCE();
            experience.EXPERIENCEID = Guid.NewGuid().ToString();
            experience.CREATEDATE = DateTime.Now;
            experience.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            experience.T_HR_RESUME = Resume;
            ExperienceList.Add(experience);
            DataGridBinder();
        }

        private void InitControlEvent()
        {
            client = new PersonnelServiceClient();
            client.GetExperienceAllCompleted += new EventHandler<GetExperienceAllCompletedEventArgs>(client_GetExperienceAllCompleted);
        }

        void client_GetExperienceAllCompleted(object sender, GetExperienceAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                ExperienceList = e.Result ;
                UsableExp = e.Result;
                DataGridBinder();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ExperienceAdd();
            
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                T_HR_EXPERIENCE experience = DtGrid.SelectedItem as T_HR_EXPERIENCE;
                foreach (var exp in UsableExp)
                {
                    if (exp.EXPERIENCEID == experience.EXPERIENCEID)
                    {
                        DelableExp.Add(experience);
                        UsableExp.Remove(experience);
                        break;
                    }
                }
                ExperienceList.Remove(experience);
            }
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
