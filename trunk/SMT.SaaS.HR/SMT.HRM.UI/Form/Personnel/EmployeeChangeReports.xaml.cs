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

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeChangeReports : System.Windows.Controls.Window, IClient
    {
        #region 定义变量
        private PersonnelServiceClient personClient = new PersonnelServiceClient();
        SMTLoading loadbar = new SMTLoading();
        string sType = "", sValue = "";
        // bool ispaging = true;

        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        public int iLandYear = DateTime.Now.Year;
        public int iLandMonthStart = DateTime.Now.Month;
        public int iLandMonthEnd = DateTime.Now.Month;
        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<V_EmployeeChangeInfos> ListInfos;
        #endregion
        public EmployeeChangeReports()
        {
            InitializeComponent();
            this.TitleContent = Utility.GetResourceStr("员工异动信息");
            initEvents();
            this.Loaded += new RoutedEventHandler(EmployeeChangeReports_Loaded);
        }

        void EmployeeChangeReports_Loaded(object sender, RoutedEventArgs e)
        {
            nuYear.Value = DateTime.Now.Year;
            nuMonth.Value = DateTime.Now.Month;
            
            LoadData();
        }

        void initEvents()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            T_HR_COMPANY companyInit = new T_HR_COMPANY();
            companyInit.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            companyInit.CNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            sType = "Company";
            sValue = companyInit.COMPANYID;
            StrCompanyIDsList.Add(sValue);
            lkSelectObj.DisplayMemberPath = "CNAME";
            lkSelectObj.DataContext = companyInit;
            personClient.EmployeePostReportInfosCompleted += new EventHandler<EmployeePostReportInfosCompletedEventArgs>(personClient_EmployeePostReportInfosCompleted);
            personClient.ExportEmployeePostReportInfosCompleted += new EventHandler<ExportEmployeePostReportInfosCompletedEventArgs>(personClient_ExportEmployeePostReportInfosCompleted);
            personClient.ExportEmployeePostChangeNoqueryReportCompleted += new EventHandler<ExportEmployeePostChangeNoqueryReportCompletedEventArgs>(personClient_ExportEmployeePostChangeNoqueryReportCompleted);
            //personClient.GetEmployeesIntimeCompleted += new EventHandler<GetEmployeesIntimeCompletedEventArgs>(personClient_GetEmployeesIntimeCompleted);
            //personClient.ExportEmployeesIntimeCompleted += new EventHandler<ExportEmployeesIntimeCompletedEventArgs>(personClient_ExportEmployeesIntimeCompleted);
        }

        void personClient_ExportEmployeePostChangeNoqueryReportCompleted(object sender, ExportEmployeePostChangeNoqueryReportCompletedEventArgs e)
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

        void personClient_ExportEmployeePostReportInfosCompleted(object sender, ExportEmployeePostReportInfosCompletedEventArgs e)
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

        void personClient_EmployeePostReportInfosCompleted(object sender, EmployeePostReportInfosCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null && e.Result.Count()>0)
                {
                    DtGrid.ItemsSource = e.Result;
                    if (e.Result != null)
                    {
                        ListInfos = new ObservableCollection<V_EmployeeChangeInfos>(e.Result.ToList());
                        //ListInfos.Concat<V_EmployeeBasicInfo>( e.Result.ToList());// as ObservableCollection<V_EmployeeBasicInfo>;
                    }
                }
                else
                {
                    MessageBox.Show("该公司在该月没有查询到数据!");
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
            
            //选定月的 1号
            DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1", out dtLandStart);
            DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1", out dtLandEnd);
            //选定的月份加1个月的1号
            dtLandEnd = dtLandEnd.AddMonths(1);
            
            loadbar.Start();            
            personClient.EmployeePostReportInfosAsync(StrCompanyIDsList,"COMPANYNAME,DEPARTMENTNAME",
                filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dtLandStart,dtLandEnd);
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
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

                //选定月的 1号
                DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1", out dtLandStart);
                DateTime.TryParse(nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1", out dtLandEnd);
                //选定的月份加1个月的1号
                dtLandEnd = dtLandEnd.AddMonths(1);
                loadbar.Start();
                if (ListInfos.Count() > 0)
                {
                    personClient.ExportEmployeePostChangeNoqueryReportAsync(StrCompanyIDsList,ListInfos,dtLandStart);
                }
                else
                {
                    personClient.EmployeePostReportInfosAsync(StrCompanyIDsList, "COMPANYNAME,DEPARTMENTNAME",
                    filter, paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dtLandStart, dtLandEnd);
                }
            }

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
                        StrCompanyName = StrCompanyName.Substring(0, StrCompanyName.Length - 1);
                    }
                    ToolTipService.SetToolTip(lkSelectObj.TxtLookUp, StrCompanyName);

                }
            };

            ogzLookup.Show<string>(DialogMode.ApplicationModal, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, userID);
                
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //V_LEFTOFFICEVIEW view = e.Row.DataContext as V_LEFTOFFICEVIEW;
            int rowNumber = e.Row.GetIndex() + 1;
            TextBlock Tnumber = DtGrid.Columns[0].GetCellContent(e.Row).FindName("Tnumber") as TextBlock;
            Tnumber.Text = rowNumber.ToString();
        }
        #region IClient
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

