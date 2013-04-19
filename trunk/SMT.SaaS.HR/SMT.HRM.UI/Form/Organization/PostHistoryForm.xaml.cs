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

using SMT.Saas.Tools.OrganizationWS;

namespace SMT.HRM.UI.Form
{
    public partial class PostHistoryForm : BaseForm
    {
        private T_HR_POSTHISTORY departPosition;

        public T_HR_POSTHISTORY Post
        {
            get { return departPosition; }
            set
            {
                departPosition = value;
                this.DataContext = departPosition;
            }
        }

        OrganizationServiceClient client;
        public PostHistoryForm(T_HR_POSTHISTORY posHis,string companyName,string departName)
        {
            InitializeComponent();
            InitControlEvent();
            Post = posHis;
            //公司名称
            txtCpyName.Text = companyName;
            txtDepartName.Text = departName;
            //绑定部门字典
            client.GetPostDictionaryAllAsync();
            this.IsEnabled = false;
        }

        private void InitControlEvent()
        {
            client = new OrganizationServiceClient();
            client.GetPostDictionaryAllCompleted += new EventHandler<GetPostDictionaryAllCompletedEventArgs>(client_GetPostDictionaryAllCompleted);
        }

        void client_GetPostDictionaryAllCompleted(object sender, GetPostDictionaryAllCompletedEventArgs e)
        {
            cbxPosition.ItemsSource = e.Result;
            cbxPosition.DisplayMemberPath = "POSTNAME";
            if (Post != null)
            {
                foreach (var item in cbxPosition.Items)
                {
                    T_HR_POSTDICTIONARY dict = item as T_HR_POSTDICTIONARY;
                    if (dict != null)
                    {
                        if (dict.POSTDICTIONARYID == Post.T_HR_POSTDICTIONARY.POSTDICTIONARYID)
                        {
                            cbxPosition.SelectedItem = item;
                            txtPosCode.Text = dict.POSTCODE;
                            break;
                        }
                    }
                }
            }
        }
    }
}
