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
using System.Windows.Navigation;

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form;
using SMT.HRM.UI.Form.Salary;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

using SMT.SaaS.Globalization;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class SalaryArchiveHIS : BasePage,IClient
    {
        private int recordPoint = 0;
        SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient orgClient;
        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;
        private bool sign = false;
        List<object> itemsAll = new List<object>();
        List<string> getItemID = new List<string>();
        List<V_SALARYARCHIVES> list = new List<V_SALARYARCHIVES>();
        private V_SALARYARCHIVES salaryRecord = new V_SALARYARCHIVES();
        private DataGrid DtGriddy;
        private int recordnum = 0;

        private SalaryServiceClient client = new SalaryServiceClient();
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        public SalaryArchiveHIS()
        {
            InitializeComponent();
            InitParas();
            GetEntityLogo("T_HR_SALARYARCHIVEHIS");
            loadbar.Stop();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            EditTitle("SALARY", "SALARYARCHIVE");
        }

        private void InitParas()
        {
            LayoutRoot.Visibility = Visibility.Visible;//

            PARENT.Children.Add(loadbar);
            client.GetSalaryArchivehisWithPagingCompleted += new EventHandler<GetSalaryArchivehisWithPagingCompletedEventArgs>(client_GetSalaryArchivehisWithPagingCompleted);
            client.GetSalaryArchiveHisItemByIDCompleted += new EventHandler<GetSalaryArchiveHisItemByIDCompletedEventArgs>(client_GetSalaryArchiveHisItemByIDCompleted);

            #region   工具栏初试化
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);//临时事件

            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Visible;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;
            #endregion

            this.Loaded += new RoutedEventHandler(Left_Loaded);

            orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
        }

        void Login_HandlerClick(object sender, EventArgs e)
        {
            LayoutRoot.Visibility = Visibility.Visible;
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //Views.Setting.FunctionSetting form = new Views.Setting.FunctionSetting();
            //EntityBrowser browser = new EntityBrowser(form);
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void client_GetSalaryArchiveHisItemByIDCompleted(object sender, GetSalaryArchiveHisItemByIDCompletedEventArgs e)
        {
            List<V_SALARYARCHIVEITEM> its = new List<V_SALARYARCHIVEITEM>();
            List<V_SALARYARCHIVEITEM> items = new List<V_SALARYARCHIVEITEM>();
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        its = e.Result.ToList();
                        for (int i = 0; i < its.Count; i++)
                        {
                            its[i].SUM = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(its[i].SUM);
                        }
                        if (sign)
                        {
                            foreach (var it in its)
                            {
                                DataGridTextColumn txtCol = new DataGridTextColumn();

                                txtCol.Header = it.SALARYITEMNAME;
                                txtCol.Binding = new Binding("SUM");
                                txtCol.Width = DataGridLength.SizeToCells;
                                txtCol.MinWidth = 100;
                                txtCol.MaxWidth = 100;
                                DtGriddy.Columns.Add(txtCol);
                                getItemID.Add(it.SALARYITEMID);
                            }
                            sign = false;
                            items = its;
                        }
                        else
                        {
                            for (int i = 0; i < its.Count; i++)
                            {
                                foreach (var it in its)
                                {
                                    if (it.SALARYITEMID == getItemID[i])
                                    {
                                        items.Add(it);
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
                itemsAll.Add(items);
                ++recordnum;
                if (recordnum < list.Count)
                {
                    for (int i = recordnum; i < list.Count; i++)
                    {
                        try
                        {
                            client.GetSalaryArchiveHisItemByIDAsync(list[recordnum].SALARYARCHIVEID);
                            break;
                        }
                        catch (Exception ex)
                        {
                            ex.Message.ToString();
                        }
                    }
                }
                else
                {
                    int i = 0;
                    foreach (List<V_SALARYARCHIVEITEM> a in itemsAll)
                    {
                        i += a.Count;
                    }
                    if (i == 0) SpSalaryRecord.Children.Add(DtGriddy);
                    else
                    {
                        SpSalaryRecord.Children.Add(DtGriddy);
                        SpSalaryRecord.Loaded += new RoutedEventHandler(SpSalaryRecord_Loaded);
                    }
                }
            }
            catch (Exception exx)
            {
                exx.Message.ToString();
                loadbar.Stop();
            }
        }

        void client_GetSalaryArchivehisWithPagingCompleted(object sender, GetSalaryArchivehisWithPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    for(int i=0;i<list.Count;i++)
                    {
                        try
                        {
                            list[i].SALARYLEVEL = SMT.SaaS.FrameworkUI.Common.Utility.GetConvertResources("SALARYLEVEL", list[i].SALARYLEVEL);
                            list[i].POSTLEVEL = SMT.SaaS.FrameworkUI.Common.Utility.GetConvertResources("POSTLEVEL", list[i].POSTLEVEL);
                        }catch(Exception exc)
                        {
                            exc.Message.ToString();
                        }
                    }
                    dataPager.PageCount = e.pageCount;
                }
                else
                {
                    list = null;
                }
                LoadAutoData();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            loadbar.Stop();
        }


        void SpSalaryRecord_Loaded(object sender, RoutedEventArgs e)
        {
            GetBindContent();
        }

        private void GetBindContent()
        {
            try
            {
                recordnum = 0;
                if (DtGriddy.ItemsSource != null)
                {
                    foreach (object obj in DtGriddy.ItemsSource)
                    {
                        List<V_SALARYARCHIVEITEM> q = (List<V_SALARYARCHIVEITEM>)itemsAll[recordnum];
                        if (q.Count > 0)
                        {
                            for (int i = recordPoint; i < DtGriddy.Columns.Count; i++)
                            {
                                try
                                {
                                    DtGriddy.Columns[i].GetCellContent(obj).DataContext = (itemsAll[recordnum] as List<V_SALARYARCHIVEITEM>)[i - recordPoint];
                                }
                                catch { }
                            }
                        }
                        recordnum++;
                    }
                }
            }
            catch { }

        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (DtGriddy.SelectedItems.Count > 0)
            //    {
            //        T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
            //        //client.FBStatisticsAsync(tmpEnt.EMPLOYEEID, Convert.ToInt32(tmpEnt.SALARYYEAR), Convert.ToInt32(tmpEnt.SALARYMONTH), (object)tmpEnt.EMPLOYEESALARYRECORDID);
            //        Form.Salary.EmployeeSalaryRecordForm form = new SMT.HRM.UI.Form.Salary.EmployeeSalaryRecordForm(FormTypes.Browse, tmpEnt.EMPLOYEESALARYRECORDID);
            //        //form.StandardItemWinForm.ToolBar.IsEnabled =true;
            //        EntityBrowser browser = new EntityBrowser(form);
            //        browser.FormType = FormTypes.Browse;
            //        //browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            //    }
            //    else
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            //    }
            //}
            //catch
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"));
            //}
        }


        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            BindTree();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryArchiveHisData();
        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryArchiveHisData();
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryArchiveHisData();
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region 添加,修改,删除,查询,审核

        void browser_ReloadDataEvent()
        {
            LoadSalaryArchiveHisData();
        }

        private void LoadSalaryArchiveHisData()
        {
            loadbar.Start();
            int pageCount = 0;
            itemsAll.Clear();
            string filter = "";
            int sType = 0;
            string sValue = "";
            list = null;
            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            if (DateTime.Now.Month <=2)
                starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
            else
                starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 2, 1);

            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "0":
                        OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                        sType = 0;
                        sValue = company.COMPANYID;
                        break;
                    case "1":
                        OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
                        sType = 1;
                        sValue = department.DEPARTMENTID;
                        break;
                    case "2":
                        OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
                        sType = 2;
                        sValue = post.POSTID;
                        break;
                }
            }
            else
            {
                loadbar.Stop();
                //if (frist) frist = false;  else  Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "PLEASESELECT");
                return;
            }

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            //DatePicker dpstarttimes = Utility.FindChildControl<DatePicker>(expander, "dpstarttime");
            //DatePicker dpendtimes = Utility.FindChildControl<DatePicker>(expander, "dpendtime");
            NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
            NumericUpDown nuEndYear = Utility.FindChildControl<NumericUpDown>(expander, "NuEndyear");
            NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
            NumericUpDown nuEndmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuEndmounth");

            #region 多月份的过滤 后备代码
            try
            {
                starttimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), 1);
                if (nuYear.Value > nuEndYear.Value)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始年份不能大于结束年份"));
                    loadbar.Stop();
                    return;
                }
                if ((nuYear.Value == nuEndYear.Value) && (nuStartmounth.Value > nuEndmounth.Value))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始月份不能小于结束月份"));
                    loadbar.Stop();
                    return;
                }
                if (nuEndmounth.Value.ToInt32() == 12) endtimes = new DateTime(nuEndYear.Value.ToInt32() + 1, 1, 1); else endtimes = new DateTime(nuEndYear.Value.ToInt32(), nuEndmounth.Value.ToInt32(), 1);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            #endregion

            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                 //filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                paras.Add(txtName.Text.Trim());
            }

            client.GetSalaryArchivehisWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYARCHIVEID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), sType, sValue, userID);
        }

        private void LoadAutoData()
        {
            loadbar.Start();
            recordnum = 0;
            sign = true;
            getItemID.Clear();
            SpSalaryRecord.Children.Clear();
            DtGriddy = new DataGrid();

            #region 设置当前加载DtGriddy样式
            DtGriddy.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DtGriddy.CellStyle = Application.Current.Resources["DataGridCellStyle"] as Style;
            DtGriddy.RowHeaderStyle = Application.Current.Resources["DataGridRowHeaderStyle"] as Style;
            DtGriddy.RowStyle = Application.Current.Resources["DataGridRowStyle"] as Style;
            DtGriddy.ColumnHeaderStyle = Application.Current.Resources["DataGridColumnHeaderStyle"] as Style;
            #endregion

            DtGriddy.AutoGenerateColumns = false;
            DtGriddy.HorizontalAlignment = HorizontalAlignment.Stretch;
            DtGriddy.VerticalAlignment = VerticalAlignment.Stretch;
            DtGriddy.IsReadOnly = true;

            #region  初始化必需项  
            DataGridTextColumn col = new DataGridTextColumn();
            col.Header = Utility.GetResourceStr("SALARYARCHIVEID");
            col.Binding = new Binding("SALARYARCHIVEID");
            col.Visibility = Visibility.Collapsed;
            DtGriddy.Columns.Add(col);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = Utility.GetResourceStr("EMPLOYEENAME");
            col2.Binding = new Binding("EMPLOYEENAME");
            DtGriddy.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = Utility.GetResourceStr("EMPLOYEECODE");
            col3.Binding = new Binding("EMPLOYEECODE");
            DtGriddy.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = Utility.GetResourceStr("SALARYLEVEL");
            col4.Binding = new Binding("SALARYLEVEL");
            DtGriddy.Columns.Add(col4);

            DataGridTextColumn col5 = new DataGridTextColumn();
            col5.Header = Utility.GetResourceStr("POSTLEVEL");
            col5.Binding = new Binding("POSTLEVEL");
            DtGriddy.Columns.Add(col5);

            DataGridTextColumn col8 = new DataGridTextColumn();
            //col8.Binding.Converter = Application.Current.Resources["CustomDateConverter"] as IValueConverter;
            //col8.Binding.ConverterParameter = "DATE";
            col8.Header = Utility.GetResourceStr("CREATEDATE");
            col8.Binding = new Binding("CREATEDATE");
            DtGriddy.Columns.Add(col8);

            //DataGridTextColumn col34 = new DataGridTextColumn();
            //col34.Header = Utility.GetResourceStr("REMARK");
            //col34.Binding = new Binding("REMARK");
            //DtGriddy.Columns.Add(col34);

            //DataGridTextColumn col5 = new DataGridTextColumn(); 
            //col5.Header = Utility.GetResourceStr("CHECKSTATE");
            //col5.Binding = new Binding("CHECKSTATE");   //("{Binding CHECKSTATE,Converter={StaticResource DictionaryConverter},ConverterParameter=CHECKSTATE}");         
            //DtGriddy.Columns.Add(col5);
            #endregion

            DtGriddy.ItemsSource = list;
            recordPoint = DtGriddy.Columns.Count;
            if (list != null)
                client.GetSalaryArchiveHisItemByIDAsync(list[recordnum].SALARYARCHIVEID);
            else
                SpSalaryRecord.Children.Add(DtGriddy);
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    string Result = "";
            //    if (DtGriddy.SelectedItems.Count > 0)
            //    {
            //        V_SALARYARCHIVEITEM tmpEnt = DtGriddy.SelectedItems[0] as V_SALARYARCHIVEITEM;
            //        if (tmpEnt.CHECKSTATE == "0" || tmpEnt.CHECKSTATE == "3")
            //        {
            //            ObservableCollection<string> ids = new ObservableCollection<string>();

            //            foreach (V_SALARYARCHIVEITEM tmp in DtGriddy.SelectedItems)
            //            {
            //                ids.Add(tmp.EMPLOYEESALARYRECORDID);
            //            }
            //            recorddel = DtGriddy.SelectedItems.Count;
            //            ComfirmWindow com = new ComfirmWindow();
            //            com.OnSelectionBoxClosed += (obj, result) =>
            //            {
            //                loadbar.Start();
            //                //ToolBar.btnDelete.IsEnabled = false;
            //                foreach (string id in ids)
            //                    client.EmployeeSalaryRecordItemDeleteAsync(id, (object)id);
            //                //client.EmployeeSalaryRecordDeleteAsync(ids);  
            //            };
            //            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //        }
            //        else
            //        {
            //            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
            //        }
            //    }
            //    else
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            //    }
            //}
            //catch
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"));
            //}

        }

        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGriddy, e.Row, "V_SALARYARCHIVEITEM");
        }

        private void dpstarttime_Loaded(object sender, RoutedEventArgs e)
        {
            //DatePicker dp = (DatePicker)sender;
            //dp.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString();           
        }

        private void dpendtime_Loaded(object sender, RoutedEventArgs e)
        {
            //DatePicker dp = (DatePicker)sender;
            //int MaxDate = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            //dp.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, MaxDate).ToShortDateString();
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            LoadSalaryArchiveHisData();
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
            orgClient.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion


        //#region 树形控件的操作
        ////绑定树
        //private void BindTree()
        //{
        //    orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //}

        //void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allCompanys = e.Result.ToList();
        //        }
        //        BindCompany();
        //        orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}

        //void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allDepartments = e.Result.ToList();
        //        }
        //        BindDepartment();
        //        orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}

        //void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        //{

        //}

        //void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allPositions = e.Result.ToList();
        //        }
        //        BindPosition();
        //    }
        //}

        //private void BindCompany()
        //{
        //    treeOrganization.Items.Clear();
        //    if (allCompanys != null)
        //    {
        //        foreach (OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
        //        {
        //            if (tmpOrg.T_HR_COMPANY2 == null || string.IsNullOrEmpty(tmpOrg.T_HR_COMPANY2.COMPANYID))
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpOrg.CNAME;
        //                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //                item.DataContext = tmpOrg;

        //                //标记为公司
        //                item.Tag = "0";
        //                treeOrganization.Items.Add(item);

        //                AddChildOrgItems(item, tmpOrg.COMPANYID);
        //            }
        //        }
        //    }
        //}

        //private void AddChildOrgItems(TreeViewItem item, string companyID)
        //{
        //    List<OrganizationWS.T_HR_COMPANY> childs = GetChildORG(companyID);
        //    if (childs == null || childs.Count <= 0)
        //        return;

        //    foreach (OrganizationWS.T_HR_COMPANY childOrg in childs)
        //    {
        //        TreeViewItem childItem = new TreeViewItem();
        //        childItem.Header = childOrg.CNAME;
        //        childItem.DataContext = childOrg;
        //        childItem.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //        childItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //        //标记为公司
        //        childItem.Tag = "0";
        //        item.Items.Add(childItem);

        //        AddChildOrgItems(childItem, childOrg.COMPANYID);
        //    }
        //}

        //private List<T_HR_COMPANY> GetChildORG(string companyID)
        //{
        //    List<T_HR_COMPANY> orgs = new List<T_HR_COMPANY>();

        //    foreach (T_HR_COMPANY org in allCompanys)
        //    {
        //        if (org.T_HR_COMPANY2 != null && org.T_HR_COMPANY2.COMPANYID == companyID)
        //            orgs.Add(org);
        //    }
        //    return orgs;
        //}

        //private void BindDepartment()
        //{
        //    if (allDepartments != null)
        //    {
        //        foreach (OrganizationWS.T_HR_DEPARTMENT tmpDep in allDepartments)
        //        {
        //            if (tmpDep.T_HR_COMPANY == null)
        //                continue;

        //            TreeViewItem parentItem = GetParentItem("0", tmpDep.T_HR_COMPANY.COMPANYID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();

        //                item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //                item.DataContext = tmpDep;
        //                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //                //标记为部门
        //                item.Tag = "1";
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //    }
        //}

        //private TreeViewItem GetParentItem(string parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    foreach (TreeViewItem item in treeOrganization.Items)
        //    {
        //        tmpItem = GetParentItemFromChild(item, parentType, parentID);
        //        if (tmpItem != null)
        //        {
        //            break;
        //        }
        //    }
        //    return tmpItem;
        //}

        //private TreeViewItem GetParentItemFromChild(TreeViewItem item, string parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    if (item.Tag != null && item.Tag.ToString() == parentType)
        //    {
        //        switch (parentType)
        //        {
        //            case "0":
        //                OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as OrganizationWS.T_HR_COMPANY;
        //                if (tmpOrg != null)
        //                {
        //                    if (tmpOrg.COMPANYID == parentID)
        //                        return item;
        //                }
        //                break;
        //            case "1":
        //                OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as OrganizationWS.T_HR_DEPARTMENT;
        //                if (tmpDep != null)
        //                {
        //                    if (tmpDep.DEPARTMENTID == parentID)
        //                        return item;
        //                }
        //                break;
        //        }

        //    }
        //    if (item.Items != null && item.Items.Count > 0)
        //    {
        //        foreach (TreeViewItem childitem in item.Items)
        //        {
        //            tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
        //            if (tmpItem != null)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return tmpItem;
        //}

        //private void BindPosition()
        //{
        //    if (allPositions != null)
        //    {
        //        foreach (OrganizationWS.T_HR_POST tmpPosition in allPositions)
        //        {
        //            if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
        //                continue;
        //            TreeViewItem parentItem = GetParentItem("1", tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
        //                item.DataContext = tmpPosition;
        //                item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;


        //                //标记为岗位
        //                item.Tag = "2";
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //    }
        //    //树全部展开
        //    //treeOrganization.ExpandAll();
        //    //if (treeOrganization.Items.Count > 0)
        //    //{
        //    //    TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
        //    //    selectedItem.IsSelected = true;
        //    //}
        //}
        //#endregion

        #region 树形控件的操作  NEW缓存
        //绑定树
        private void BindTree()
        {

            if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfo"))
            {
                // allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                BindCompany();
            }
            else
            {
                orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entTemps = e.Result;
                allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                allCompanys.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allCompanys.Add(item);
                });

                UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> entTemps = e.Result;
                allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                allDepartments.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allDepartments.Add(item);
                });

                UICache.CreateCache("ORGTREESYSDepartmentInfo", allDepartments);

                BindCompany();

                orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            }
        }

        void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        {

        }

        void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("ORGTREESYSPostInfo", allPositions);
                BindPosition();
            }
        }

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
                return;
            }

            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

            foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
            {
                //如果当前公司没有父机构的ID，则为顶级公司
                if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = tmpOrg.CNAME;
                    item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    item.DataContext = tmpOrg;

                    //状态在未生效和撤消中时背景色为红色
                    SolidColorBrush brush = new SolidColorBrush();
                    if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                    {
                        brush.Color = Colors.Red;
                        item.Foreground = brush;
                    }
                    else
                    {
                        brush.Color = Colors.Black;
                        item.Foreground = brush;
                    }
                    //标记为公司
                    item.Tag = OrgTreeItemTypes.Company.ToInt32();
                    treeOrganization.Items.Add(item);
                    TopCompany.Add(tmpOrg);
                }
                else
                {
                    //查询当前公司是否在公司集合内有父公司存在
                    var ent = from c in allCompanys
                              where tmpOrg.FATHERTYPE == "0" && c.COMPANYID == tmpOrg.FATHERID
                              select c;
                    var ent2 = from c in allDepartments
                               where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
                               select c;

                    //如果不存在，则为顶级公司
                    if (ent.Count() == 0 && ent2.Count() == 0)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = tmpOrg;

                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为公司
                        item.Tag = OrgTreeItemTypes.Company.ToInt32();
                        treeOrganization.Items.Add(item);

                        TopCompany.Add(tmpOrg);
                    }
                }
            }
            //开始递归
            foreach (var topComp in TopCompany)
            {
                TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, topComp.COMPANYID);
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
                                                                              where ent.FATHERTYPE == "0"
                                                                              && ent.FATHERID == topComp.COMPANYID
                                                                              select ent).ToList();

                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
                                                                                    where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
                                                                                    select ent).ToList();

                AddOrgNode(lsCompany, lsDepartment, parentItem);
            }
            allPositions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            if (allPositions != null)
            {
                BindPosition();
            }
        }

        private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        {
            //绑定公司的子公司
            foreach (var childCompany in lsCompany)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childCompany.CNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childCompany;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childCompany.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为公司
                item.Tag = OrgTreeItemTypes.Company.ToInt32();
                FatherNode.Items.Add(item);

                if (lsCompany.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
            //绑定公司下的部门
            foreach (var childDepartment in lsDepartment)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                item.DataContext = childDepartment;
                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childDepartment.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为部门
                item.Tag = OrgTreeItemTypes.Department.ToInt32();
                FatherNode.Items.Add(item);

                if (lsDepartment.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
        }

        /// <summary>
        /// 绑定岗位
        /// </summary>
        private void BindPosition()
        {
            if (allPositions != null)
            {
                foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
                {
                    if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                        continue;
                    TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        item.DataContext = tmpPosition;
                        item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpPosition.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为岗位
                        item.Tag = OrgTreeItemTypes.Post.ToInt32();
                        parentItem.Items.Add(item);
                    }
                }
            }
            //树全部展开
            //  treeOrganization.ExpandAll();
            if (treeOrganization.Items.Count > 0)
            {
                TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
                selectedItem.IsSelected = true;
            }
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private TreeViewItem GetParentItem(OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeOrganization.Items)
            {
                tmpItem = GetParentItemFromChild(item, parentType, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            if (item.Tag != null && item.Tag.ToString() == parentType.ToInt32().ToString())
            {
                switch (parentType)
                {
                    case OrgTreeItemTypes.Company:
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case OrgTreeItemTypes.Department:
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        if (tmpDep != null)
                        {
                            if (tmpDep.DEPARTMENTID == parentID)
                                return item;
                        }
                        break;
                }

            }
            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (TreeViewItem childitem in item.Items)
                {
                    tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }


        #endregion

        private void Nuyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Year.ToDouble();
        }

        private void NuStartmounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nustartmonuth = (NumericUpDown)sender;
            nustartmonuth.Value = DateTime.Now.Month.ToDouble();
        }

        private void NuEndmounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuendmonuth = (NumericUpDown)sender;
            nuendmonuth.Value = DateTime.Now.Month.ToDouble();
        }

        private void NuEndyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuEndyear = (NumericUpDown)sender;
            nuEndyear.Value = DateTime.Now.Year.ToDouble();
        }

        private void EditTitle(string parentTitle, string newTitle)
        {
            System.Text.StringBuilder sbtitle = new System.Text.StringBuilder();
            sbtitle.Append(Utility.GetResourceStr(parentTitle));
            sbtitle.Append(">>");
            sbtitle.Append(Utility.GetResourceStr(newTitle));
            ViewTitles.TextTitle.Text = sbtitle.ToString();
        }

    }
}
