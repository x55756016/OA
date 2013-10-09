using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;

namespace SMT.SaaS.OA.UI.Form.EmployeeSurveys
{
    public partial class SelectSchemeCW
    {
        private SmtOAPersonOfficeClient client =new SmtOAPersonOfficeClient();
        SMTLoading loadbar = new SMTLoading();

        public SelectSchemeCW()
        {
            InitializeComponent();
            QueryData();
            client.GetEmployeeSurveysCompleted += new EventHandler<GetEmployeeSurveysCompletedEventArgs>(client_GetEmployeeSurveysCompleted);
        }

        void client_GetEmployeeSurveysCompleted(object sender, GetEmployeeSurveysCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            ObservableCollection<T_OA_REQUIREMASTER> dataList = e.Result;
            if (dataList != null && dataList.Count > 0)
            {
                BindDateGrid(dataList);
            }
            else
            {
                BindDateGrid(null);
            }
        }

        /// <summary>
        /// 绑定数据到DataGrid
        /// </summary>
        private void BindDateGrid(ObservableCollection<T_OA_REQUIREMASTER> dataList)
        {
            if (dataList != null && dataList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dgMain.ItemsSource = pcv;
            }
            else
            {
                dataPager.DataContext = null;
                dgMain.ItemsSource = null;
            }
        }

        private void QueryData()
        {
            DateTime dtStart = new DateTime();
            DateTime dtEnd = new DateTime();
            int pageCount = 0;
            string filter = "";    //查询过滤条件

            ObservableCollection<object> paras = new ObservableCollection<object>();   //参数值            

            // 开始时间
            if (!string.IsNullOrEmpty(this.dateStart.Text.Trim()))
            {
                dtStart = Convert.ToDateTime(this.dateStart.Text);
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CREATEDATE  >= @" + paras.Count();
                paras.Add(dtStart);
            }

            // 结束时间
            if (!string.IsNullOrEmpty(this.dateEnd.Text.Trim()))
            {
                dtEnd = Convert.ToDateTime(this.dateEnd.Text);
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CREATEDATE <= @" + paras.Count();
                paras.Add(dtEnd);
            }
            if (!string.IsNullOrEmpty(this.dateStart.Text.Trim()) && !string.IsNullOrEmpty(this.dateEnd.Text.Trim()))
            {
                if (dtStart > dtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    return;
                }
            }
            loadbar.Start();
            client.GetEmployeeSurveysAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID, "2");
        }

        private void dataPager_Click(object sender, RoutedEventArgs e)
        {
            this.QueryData();
        }



         public T_OA_REQUIREMASTER SelectedOneObj
        {
            get
            {
                T_OA_REQUIREMASTER master = new T_OA_REQUIREMASTER();
                if (dgMain.SelectedItems.Count == 1)
                {
                    master = dgMain.SelectedItem as T_OA_REQUIREMASTER;
                    return master;
                }
                else
                {
                   ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARING"), "警告", "请选择一条数据", MessageIcon.Information);
                    return null;
                }
            }
        }

        public event EventHandler SelectedClick;

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = string.Empty;

           if (this.SelectedClick != null)
            {
                this.SelectedClick(this, null);
            }

            if (dgMain.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARING"), "警告", "请选择一条数据", MessageIcon.Information);
            }
            else
                this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            this.QueryData();
        }

    }
}

