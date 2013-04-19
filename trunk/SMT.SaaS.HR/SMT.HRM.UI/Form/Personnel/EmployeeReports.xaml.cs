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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.IO;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Collections.ObjectModel;

using SMT.Saas.Tools.SalaryWS;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeReports : System.Windows.Controls.Window, IClient
    {
        #region 定义变量
        private PersonnelServiceClient personClient = new PersonnelServiceClient();
        private SalaryServiceClient client = new SalaryServiceClient(); 
        SMTLoading loadbar = new SMTLoading();
        string sType = "", sValue = "";
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        public int iLandYear = DateTime.Now.Year;
        public int iLandMonthStart = DateTime.Now.Month;
        public int iLandMonthEnd = DateTime.Now.Month ;
        private string ReportsType = "";//报表类型 默认为员工报表 1 员工统计报表 
        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<V_EmployeeBasicInfo> ListInfos;
        #endregion
        public EmployeeReports()
        {
            InitializeComponent();
            this.TitleContent = Utility.GetResourceStr("员工报表信息");
            initEvents();
            this.Loaded += new RoutedEventHandler(EmployeeReports_Loaded);
        }

        public EmployeeReports(string StrType)
        {
            InitializeComponent();
            ReportsType = StrType;
            this.TitleContent = Utility.GetResourceStr("员工统计报表");
            this.dg.Visibility = Visibility.Collapsed;//隐藏dg

            initEvents();
            this.Loaded += new RoutedEventHandler(EmployeeReports_Loaded);
        }

        void EmployeeReports_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime dtLandStart = new DateTime();
            DateTime dtLandEnd = new DateTime();
            DateTime.TryParse(iLandYear + "-" + iLandMonthStart + "-1", out dtLandStart);

            dtLandEnd = dtLandStart.AddMonths(1);
            if (iLandMonthStart < iLandMonthEnd)
            {
                DateTime.TryParse(iLandYear + "-" + iLandMonthEnd + "-1", out dtLandEnd);
                dtLandEnd = dtLandEnd.AddMonths(1);
            }

            nuYear.Value = DateTime.Now.Year;
            nuMonth.Value = DateTime.Now.Month;
            LoadData();
            //loadbar.Start();
            //loadbar.Stop();
            //if (string.IsNullOrEmpty(ReportsType))
            //{
            //    LoadData();
            //}
        }
        void initEvents()
        {
            PARENT.Children.Add(loadbar);
            //loadbar.Stop();
            T_HR_COMPANY companyInit = new T_HR_COMPANY();
            companyInit.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            companyInit.CNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            sType = "Company";
            sValue = companyInit.COMPANYID;
            
            lkSelectObj.DataContext = companyInit;
            lkSelectObj.DisplayMemberPath = "CNAME";
            StrCompanyIDsList.Add(sValue);
            //personClient.GetEmployeesIntimeCompleted += new EventHandler<GetEmployeesIntimeCompletedEventArgs>(personClient_GetEmployeesIntimeCompleted);
            //personClient.ExportEmployeesIntimeCompleted += new EventHandler<ExportEmployeesIntimeCompletedEventArgs>(personClient_ExportEmployeesIntimeCompleted);
            personClient.GetEmployeeBasicInfosByCompanyCompleted += new EventHandler<GetEmployeeBasicInfosByCompanyCompletedEventArgs>(personClient_GetEmployeeBasicInfosByCompanyCompleted);
            personClient.ExportEmployeeBasicInfosByCompanyReportsCompleted += new EventHandler<ExportEmployeeBasicInfosByCompanyReportsCompletedEventArgs>(personClient_ExportEmployeeBasicInfosByCompanyReportsCompleted);
            personClient.ExportEmployeeTructReportsCompleted += new EventHandler<ExportEmployeeTructReportsCompletedEventArgs>(personClient_ExportEmployeeTructReportsCompleted);
            personClient.ExportEmployeeCollectsReportsCompleted += new EventHandler<ExportEmployeeCollectsReportsCompletedEventArgs>(personClient_ExportEmployeeCollectsReportsCompleted);
            personClient.ExportEmployeeBasicInfosNoGetReportsCompleted += new EventHandler<ExportEmployeeBasicInfosNoGetReportsCompletedEventArgs>(personClient_ExportEmployeeBasicInfosNoGetReportsCompleted);
            client.ExportEmployeePensionReportsCompleted += new EventHandler<ExportEmployeePensionReportsCompletedEventArgs>(client_ExportEmployeePensionReportsCompleted);
        }

        void client_ExportEmployeePensionReportsCompleted(object sender, ExportEmployeePensionReportsCompletedEventArgs e)
        {
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();
        }

        void personClient_ExportEmployeeBasicInfosNoGetReportsCompleted(object sender, ExportEmployeeBasicInfosNoGetReportsCompletedEventArgs e)
        {
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();
        }

        void personClient_ExportEmployeeCollectsReportsCompleted(object sender, ExportEmployeeCollectsReportsCompletedEventArgs e)
        {
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();
        }

        void personClient_ExportEmployeeTructReportsCompleted(object sender, ExportEmployeeTructReportsCompletedEventArgs e)
        {
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();
        }

        void personClient_ExportEmployeeBasicInfosByCompanyReportsCompleted(object sender, ExportEmployeeBasicInfosByCompanyReportsCompletedEventArgs e)
        {
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();
        }

        void personClient_GetEmployeeBasicInfosByCompanyCompleted(object sender, GetEmployeeBasicInfosByCompanyCompletedEventArgs e)
        {
            
            if (e.Error == null)
            {
                dg.ItemsSource = e.Result;
                if (e.Result !=null)
                {
                    ListInfos = new ObservableCollection<V_EmployeeBasicInfo>(e.Result.ToList());
                    //ListInfos.Concat<V_EmployeeBasicInfo>( e.Result.ToList());// as ObservableCollection<V_EmployeeBasicInfo>;
                }
            }
            loadbar.Stop();
        }

        void LoadData()
        {
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();


            if (StrCompanyIDsList.Count() > 0)
            {
                StrCompanyIDsList.ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";
                        filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                        paras.Add(item);
                    }
                    else
                    {
                        filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                        paras.Add(item);
                    }
                });
            }
            DateTime dtLandStart = new DateTime();
            DateTime dtLandEnd = new DateTime();

            DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1", out dtLandStart);
            DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value + "-1", out dtLandEnd);
            dtLandEnd = dtLandEnd.AddMonths(1);            
                                  
            loadbar.Start();
            if (string.IsNullOrEmpty(ReportsType))
            {
                personClient.GetEmployeeBasicInfosByCompanyAsync(StrCompanyIDsList, "COMPANYNAME,DEPARTMENTNAME", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, dtLandStart, dtLandEnd);
            }
            else
            {
                loadbar.Stop();
            }
            //switch (ReportsType)
            //{
            //    case "1":
            //        break;
            //    case "2":
            //        //personClient.ExportEmployeeTructReportsAsync(StrCompanyIDsList, "CREATEDATE", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sValue, System.DateTime.Now);
            //        personClient.ExportEmployeeTructReportsAsync(StrCompanyIDsList, "CREATEDATE", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sValue, dtLandStart, dtLandEnd);
            //        break;
            //    case "3":
            //        personClient.ExportEmployeeCollectsReportsAsync("", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sValue, dtLandStart, dtLandEnd);
            //        break;
            //    default:
            //        personClient.GetEmployeeBasicInfosByCompanyAsync(StrCompanyIDsList, "COMPANYNAME,DEPARTMENTNAME", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, dtLandStart, dtLandEnd);
            //        //personClient.ExportEmployeeBasicInfosByCompanyReportsAsync(StrCompanyIDsList, "CREATEDATE", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType);
            //        break;
            //}
            
        }

        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            int rowNumber = e.Row.GetIndex() + 1;
            TextBlock Tnumber = dg.Columns[0].GetCellContent(e.Row).FindName("Tnumber") as TextBlock;
            Tnumber.Text = rowNumber.ToString();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            // ispaging = false;
            
            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            
            dialog.FilterIndex = 1;

            result = dialog.ShowDialog();
            if (result.Value == true)
            {
                string filter = "";
                System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

                if (StrCompanyIDsList.Count() > 0)
                {
                    StrCompanyIDsList.ForEach(item =>
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " or ";
                            filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                            paras.Add(item);
                        }
                        else
                        {
                            filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                            paras.Add(item);
                        }
                    });
                }
                DateTime dtLandStart = new DateTime();
                DateTime dtLandEnd = new DateTime();

                DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1", out dtLandStart);
                DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1", out dtLandEnd);
                dtLandEnd = dtLandEnd.AddMonths(1);
                
                    
                    //if (string.IsNullOrEmpty(filter))
                    //{
                    //    filter += " CREATEDATE >= @" + paras.Count().ToString();
                    //    paras.Add(dtLandStart);
                    //}
                    //else
                    //{
                    //    filter += " and CREATEDATE >= @" + paras.Count().ToString();
                    //    paras.Add(dtLandStart);
                    //}

                    //filter += " and CREATEDATE < @" + paras.Count().ToString();
                    //paras.Add(dtLandEnd);
                

                loadbar.Start();
                switch (ReportsType)
                {
                    case "1":
                        break;
                    case "2":
                        personClient.ExportEmployeeTructReportsAsync(StrCompanyIDsList, "CREATEDATE",filter,paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sValue,dtLandStart,dtLandEnd);
                        break;
                    case "3":
                        personClient.ExportEmployeeCollectsReportsAsync("", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sValue, dtLandStart,dtLandEnd);
                        break;
                    case "4":
                        sValue = StrCompanyIDsList.FirstOrDefault();
                        client.ExportEmployeePensionReportsAsync("COMPANYNAME", filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID,sValue, dtLandStart);
                        break;
                    default:
                        //personClient.GetEmployeeBasicInfosByCompanyAsync(dataPager.PageIndex, dataPager.PageSize, "POSTNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, sValue);
                        //personClient.ExportEmployeeBasicInfosByCompanyReportsAsync(StrCompanyIDsList, "CREATEDATE",filter, paras,  SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType);
                        if (ListInfos != null)
                        {
                            personClient.ExportEmployeeBasicInfosNoGetReportsAsync(ListInfos,dtLandStart,StrCompanyIDsList);
                        }
                        else
                        { 
                            //personClient.ExportEmployeeBasicInfosByCompanyReportsAsync(
                        }
                        break;
                }
                
                //personClient.ExportEmployeesIntimeAsync(dataPager.PageIndex, dataPager.PageSize, "DepartmentName",
                //    filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        #region  完成事件
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void personClient_ExportEmployeesIntimeCompleted(object sender, ExportEmployeesIntimeCompletedEventArgs e)
        {
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();

        }
        /// <summary>
        /// 获取实时信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void personClient_GetEmployeesIntimeCompleted(object sender, GetEmployeesIntimeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                dg.ItemsSource = e.Result;
            }
            loadbar.Stop();
        }
        #endregion

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {

            string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            string perm = (((int)FormTypes.Browse) + 1).ToString();   //zl 3.1
            //string entity = "MonthlyBudgetAnalysis";
            //OrganizationLookup ogzLookup = new OrganizationLookup(userID, perm, entity);
            OrganizationLookup ogzLookup = new OrganizationLookup();
            ogzLookup.MultiSelected = true;
            ogzLookup.SelectedObjType = OrgTreeItemTypes.Company;
            //ogzLookup.ShowMessageForSelectOrganization();

            ogzLookup.SelectedClick += (o, ev) =>
            {
                StrCompanyIDsList.Clear();
                List<ExtOrgObj> ent = ogzLookup.SelectedObj as List<ExtOrgObj>;
                if (ent.Count() == 0)
                {
                    return;
                }
                List<ExtOrgObj> entall = new List<ExtOrgObj>();
                if (ent != null && ent.Count > 0)
                {
                    //issuanceExtOrgObj = ent;
                    string StrCompanyName = "";
                    foreach (var h in ent)
                    {
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)//公司
                        {
                            StrCompanyIDsList.Add(h.ObjectID);
                            //先添加总公司
                            ExtOrgObj obj2 = new ExtOrgObj();
                            obj2.ObjectID = h.ObjectID;
                            obj2.ObjectName = h.ObjectName;
                            obj2.ObjectType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
                            entall.Add(obj2);
                            lkSelectObj.DataContext = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)h.ObjectInstance;
                            lkSelectObj.DisplayMemberPath = "CNAME";
                            StrCompanyName += h.ObjectName + ",";                            

                        }
                    }
                    if (!string.IsNullOrEmpty(StrCompanyName))
                    {
                        StrCompanyName = StrCompanyName.Substring(0,StrCompanyName.Length-1);
                    }
                    ToolTipService.SetToolTip(lkSelectObj.TxtLookUp,StrCompanyName);

                }
            };

            ogzLookup.Show<string>(DialogMode.ApplicationModal, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, userID);
                
            
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region Iclient接口

        public void ClosedWCFClient()
        {
            personClient.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion
    }
}

