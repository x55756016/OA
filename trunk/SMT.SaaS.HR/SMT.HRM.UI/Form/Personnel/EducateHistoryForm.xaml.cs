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
    public partial class EducateHistoryForm : BaseForm
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

        //数据库里有的教育记录实体
        private ObservableCollection<T_HR_EDUCATEHISTORY> usableEdu = new ObservableCollection<T_HR_EDUCATEHISTORY>();

        public ObservableCollection<T_HR_EDUCATEHISTORY> UsableEdu
        {
            get { return usableEdu; }
            set { usableEdu = value; }
        }

        //待删除的教育记录实体
        private ObservableCollection<T_HR_EDUCATEHISTORY> delableEdu = new ObservableCollection<T_HR_EDUCATEHISTORY>();

        public ObservableCollection<T_HR_EDUCATEHISTORY> DelableEdu
        {
            get { return delableEdu; }
            set { delableEdu = value; }
        }

        private ObservableCollection<T_HR_EDUCATEHISTORY> educateHistory = new ObservableCollection<T_HR_EDUCATEHISTORY>();

        public ObservableCollection<T_HR_EDUCATEHISTORY> EducateHistory
        {
            get { return educateHistory; }
            set { educateHistory = value; }
        }

        PersonnelServiceClient client;
        public EducateHistoryForm()
        {
            InitializeComponent();
            InitControlEvent();
        }

        public void LoadData(FormTypes type, string resumeID,T_HR_RESUME hrResume)
        {
            formType = type;
            Resume = hrResume;
            if(formType == FormTypes.Browse)
            {
                this.IsEnabled = false;
                this.btnAdd.Visibility = Visibility.Collapsed;
                
            }
            if (formType == FormTypes.New)
            {
                //判断是否已添加
                if (EducateHistory.Count > 0)
                {
                    DataGridBinder();
                }
                else
                {
                    //新添加行
                    EducateHistoryAdd();
                }
            }
            else
            {
                //判断是否已添加
                if (EducateHistory!=null&&EducateHistory.Count > 0)
                {
                    DataGridBinder();
                }
                else
                {
                    client.GetEducateHistoryAllAsync(resumeID);
                }
            }
        }

        //DataGrid绑定
        private void DataGridBinder()
        {
            this.DtEduGrid.ItemsSource = null;
            this.DtEduGrid.ItemsSource = EducateHistory;
        }

        private void EducateHistoryAdd()
        {
            if (EducateHistory == null)
            {
                EducateHistory = new ObservableCollection<T_HR_EDUCATEHISTORY>();
            }
            T_HR_EDUCATEHISTORY educate = new T_HR_EDUCATEHISTORY();
            educate.EDUCATEHISTORYID = Guid.NewGuid().ToString();
            educate.CREATEDATE = DateTime.Now;
            educate.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            educate.T_HR_RESUME = Resume;
            EducateHistory.Add(educate);
            DataGridBinder();
        }

        private void InitControlEvent()
        {
            client = new PersonnelServiceClient();
            client.GetEducateHistoryAllCompleted += new EventHandler<GetEducateHistoryAllCompletedEventArgs>(client_GetEducateHistoryAllCompleted);
        }

        void client_GetEducateHistoryAllCompleted(object sender, GetEducateHistoryAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                EducateHistory = e.Result;
                UsableEdu = e.Result;
                DataGridBinder();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            EducateHistoryAdd();
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (DtEduGrid.SelectedItem != null)
            {
                T_HR_EDUCATEHISTORY educate = DtEduGrid.SelectedItem as T_HR_EDUCATEHISTORY;
                //
                foreach (var ent in usableEdu)
                {
                    if (educate.EDUCATEHISTORYID == ent.EDUCATEHISTORYID)
                    {
                        DelableEdu.Add(educate);
                        usableEdu.Remove(educate);
                        break;
                    }
                }
                EducateHistory.Remove(educate);
            }
        }
    }
}
