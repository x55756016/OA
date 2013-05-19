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

using SMT.Saas.Tools.SalaryWS;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
//using SMT.Saas.Tools.FBServiceWS;
using System.Text;

using SMT.HRM.UI.Form.Salary;

using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Controls.Primitives;


namespace SMT.HRM.UI.Form.Salary
{
    public partial class GenerateSalaryForm : BaseForm, IEntityEditor, IClient
    {
        private bool flag = false;
        private System.Windows.Threading.DispatcherTimer timer;
        private T_HR_COMPANY salarycompany;
        SalaryServiceClient client = new SalaryServiceClient();
        List<V_RETURNFBI> vr = new List<V_RETURNFBI>();
        List<V_RETURNFBI> vrfb = new List<V_RETURNFBI>();

        public GenerateSalaryForm()
        {
            InitializeComponent();

            client.GenerateSalaryRecordCompleted += new EventHandler<GenerateSalaryRecordCompletedEventArgs>(client_GenerateSalaryRecordCompleted);
            client.SalaryRecordAccountCompleted += new EventHandler<SalaryRecordAccountCompletedEventArgs>(client_SalaryRecordAccountCompleted);
            client.SalaryRecordAccountCheckCompleted += new EventHandler<SalaryRecordAccountCheckCompletedEventArgs>(client_SalaryRecordAccountCheckCompleted);
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(10000);
            timer.Tick += new EventHandler(timer_Tick);
            //  progressGenerate.Maximum = 100;
            lkSalaryCompany.SearchButton.IsEnabled = false;
            lkSalaryCompany.TxtLookUp.IsReadOnly = true;
            salarycompany = new T_HR_COMPANY();
            salarycompany.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            salarycompany.CNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            lkSalaryCompany.DataContext = salarycompany;
            if (System.DateTime.Now.Month <= 1)
            {
                numYear.Value = System.DateTime.Now.Year - 1;
                numMonth.Value = 12;
            }
            else
            {
                numMonth.Value = System.DateTime.Now.Month - 1;
                numYear.Value = System.DateTime.Now.Year;
            }
        }

        void client_SalaryRecordAccountCheckCompleted(object sender, SalaryRecordAccountCheckCompletedEventArgs e)
        {
            #region ....
            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            object obj = e.UserState;
            if (e.Error != null)
            {
                //MessageBox.Show(e.Error.Message);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                Recovery();
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                StringBuilder sbresult = new StringBuilder();
                Dictionary<object, object> endresult = e.Result as Dictionary<object, object>;
                sbresult.Append(Utility.GetResourceStr("PAYFAIL"));
                if (endresult.Count > 0)
                {
                    switch (endresult["END"].ToString())
                    {
                        case "OK":
                            //CalculateSalary(obj);
                            #region 可以去除的代码
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEESALARYRECORD"));
                            Recovery();
                            #endregion
                            break;
                        case "NODATA":
                            Recovery();
                            StringBuilder strtmp = new StringBuilder();
                            foreach (var ent in endresult)
                            {
                                strtmp.Append(ent.Key.ToString() + Utility.GetResourceStr(ent.Value.ToString()) + "\r\n");
                            }
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), strtmp.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                            break;
                        case "NODATAGENERATE":
                            Recovery();
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NODATAGENERATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                            //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NODATAGENERATE"));
                            break;
                        case "ERROR":
                            foreach (string temp in endresult.Keys)
                            {
                                if (temp != "END")
                                {
                                    string str = " ";
                                    try
                                    {
                                        str = temp + "'/";
                                        str += endresult[temp].ToString() + "\x20\x20\x20";
                                    }
                                    catch
                                    {
                                        str += "\x20\x20\x20";
                                    }
                                    sbresult.Append(str);
                                }
                            }
                            Recovery();
                            com.OnSelectionBoxClosed += (objects, result) =>
                            {
                                #region  有用代码
                                //RefreshUI(RefreshedTypes.ShowProgressBar);
                                //CalculateSalary(obj);
                                #endregion
                                #region 可以去除的代码
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEESALARYRECORD"));
                                Recovery();
                                #endregion
                            };
                            //sbresult.Remove(0, sbresult.Length);
                            //sbresult.Append(Utility.GetResourceStr("PARTERROR"));
                            com.SelectionBox(Utility.GetResourceStr("PAYSELECTWINDOW"), Utility.GetResourceStr(sbresult.ToString()), ComfirmWindow.confirmation, Result);
                            txtResult.Text = sbresult.ToString()+System.Environment.NewLine+txtResult.Text;
                            //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILINFORMATION"), Utility.GetResourceStr(sbresult.ToString()));
                            break;
                    }
                }
                else
                {
                    Recovery();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FAIL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FAIL"));
                }
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("BUDGETERROR")+": "+Utility.GetResourceStr(e.Result));
            }
            #endregion
        }

        void Recovery()
        {
            // timer.Stop();
            if (flag)
            {
                //  timer.Stop();
                //   progressGenerate.Value = progressGenerate.Minimum;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
            isSaving = false;
            RefreshUI(RefreshedTypes.ToolBar);
            // progressGenerate.Value = progressGenerate.Maximum;
            lkSelectObj.IsEnabled = true;
            numYear.IsEnabled = true;
            numMonth.IsEnabled = true;
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        //void CalculateSalary(object obj)
        //{
        //    if (obj is T_HR_COMPANY)
        //    {
        //        client.SalaryRecordAccountAsync(0, ((T_HR_COMPANY)obj).COMPANYID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

        //    }
        //    else if (obj is T_HR_DEPARTMENT)
        //    {
        //        client.SalaryRecordAccountAsync(1, ((T_HR_DEPARTMENT)obj).DEPARTMENTID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

        //    }
        //    else if (obj is T_HR_POST)
        //    {
        //        client.SalaryRecordAccountAsync(2, ((T_HR_POST)obj).POSTID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

        //    }
        //    else if (obj is T_HR_EMPLOYEE)
        //    {
        //        client.SalaryRecordAccountAsync(3, ((T_HR_EMPLOYEE)obj).EMPLOYEEID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

        //    }
        //    else
        //    {
        //        Recovery();
        //    }
        //}

        void client_SalaryRecordAccountCompleted(object sender, SalaryRecordAccountCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEESALARYRECORD"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEESALARYRECORD"));
            }
            //  timer.Stop();
            if (flag)
            {
                //timer.Stop();
                //progressGenerate.Value = progressGenerate.Minimum;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
            Recovery();
        }

        void client_GenerateSalaryRecordCompleted(object sender, GenerateSalaryRecordCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "EMPLOYEESALARYRECORD"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "EMPLOYEESALARYRECORD"));
            }
            // timer.Stop();
            //  progressGenerate.Value = progressGenerate.Maximum;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //progressGenerate.Value += 1;
            //if (progressGenerate.Value >= 100)
            //    progressGenerate.Value = 1;
        }

        private void Save()
        {
            //this.DialogResult = true;
            //   timer.Start();
            //  objrecord = null;
            //object obj = lkSelectObj.DataContext;
            int GernerateType = cbxPayType.SelectedIndex; // 结算类型（0：发薪机构；1：公司；2：离职薪资 ，3 指定结算岗位）

            isSaving = true;
            RefreshUI(RefreshedTypes.ToolBar);
            RefreshUI(RefreshedTypes.ShowProgressBar);
            //lkSelectObj.IsEnabled = false;
            //numYear.IsEnabled = false;
            //numMonth.IsEnabled = false;
            //client.SalaryRecordAccountAsync(0, "7cd6c0a4-9735-476a-9184-103b962d3383", 2010, 3);
            GetCreateInfor();
            // objrecord = obj;
            if (GernerateType == 0)//发薪机构
            {
                var ent = lkSalaryCompany.DataContext;
                if (ent == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("请选择发薪机构"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (ent is T_HR_COMPANY)
                {
                    Dictionary<string, string> GeneratePrameter = new Dictionary<string, string>();
                    GeneratePrameter.Add("GernerateType", GernerateType.ToString());
                    GeneratePrameter.Add("GenerateEmployeePostid", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                    GeneratePrameter.Add("GenerateCompanyid", ((T_HR_COMPANY)ent).COMPANYID);

                    client.SalaryRecordAccountCheckAsync(GeneratePrameter, 4, ((T_HR_COMPANY)ent).COMPANYID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor(), ent);

                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            else if (GernerateType == 1)//组织架构
            {
                var ent = lkSelectObj.DataContext;
                if (ent == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("为选择有效的组织架构"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }

                Dictionary<string, string> GeneratePrameter = new Dictionary<string, string>();
                GeneratePrameter.Add("GernerateType", GernerateType.ToString());
                GeneratePrameter.Add("GenerateEmployeePostid", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                
                if (ent is T_HR_COMPANY)
                {
                    GeneratePrameter.Add("GenerateCompanyid", ((T_HR_COMPANY)ent).COMPANYID);
                    client.SalaryRecordAccountCheckAsync(GeneratePrameter,0, ((T_HR_COMPANY)ent).COMPANYID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor(), ent);

                }
                else if (ent is T_HR_DEPARTMENT)
                {
                    GeneratePrameter.Add("GenerateCompanyid", ((T_HR_DEPARTMENT)ent).T_HR_COMPANY.COMPANYID);
                    client.SalaryRecordAccountCheckAsync(GeneratePrameter,1, ((T_HR_DEPARTMENT)ent).DEPARTMENTID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

                }
                else if (ent is T_HR_POST)
                {
                    GeneratePrameter.Add("GenerateCompanyid", ((T_HR_POST)ent).COMPANYID);
                    client.SalaryRecordAccountCheckAsync(GeneratePrameter,2, ((T_HR_POST)ent).POSTID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

                }
                else if (ent is T_HR_EMPLOYEE)
                {
                    string strOrgId = string.Empty;
                    strOrgId = CombinSalaryEmployeeInfo(((T_HR_EMPLOYEE)ent));
                    if (string.IsNullOrWhiteSpace(strOrgId))
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "非法结算对象",
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    GeneratePrameter.Add("GenerateCompanyid", ((T_HR_EMPLOYEE)ent).OWNERCOMPANYID);
                    client.SalaryRecordAccountCheckAsync(GeneratePrameter,3, strOrgId, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

                }
            }
            else if (GernerateType == 2)//离职薪资
            {
                var ent = lkEmployee.DataContext;
                if (ent == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("请选择员工"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (ent is T_HR_EMPLOYEE)
                {
                    string strOrgId = string.Empty;
                    strOrgId = CombinSalaryEmployeeInfo(((T_HR_EMPLOYEE)ent));
                    if (string.IsNullOrWhiteSpace(strOrgId))
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "非法结算对象",
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    Dictionary<string, string> GeneratePrameter = new Dictionary<string, string>();
                    GeneratePrameter.Add("GernerateType", GernerateType.ToString());
                    GeneratePrameter.Add("GenerateEmployeePostid", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                    GeneratePrameter.Add("GenerateCompanyid", ((T_HR_EMPLOYEE)ent).OWNERCOMPANYID);
                    client.SalaryRecordAccountCheckAsync(GeneratePrameter, 3, strOrgId, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());

                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            } 
            if (GernerateType == 3)//指定的结算岗位
            {
                var ent = lkSelectObj.DataContext;
                if (ent == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("请选择结算岗位"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (ent is T_HR_POST)
                {
                    Dictionary<string, string> GeneratePrameter = new Dictionary<string, string>();
                    GeneratePrameter.Add("GernerateType", GernerateType.ToString());
                    GeneratePrameter.Add("GenerateEmployeePostid", ((T_HR_POST)ent).POSTID);
                    GeneratePrameter.Add("GenerateCompanyid", ((T_HR_POST)ent).T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                    client.SalaryRecordAccountCheckAsync(GeneratePrameter, 4, ((T_HR_POST)ent).POSTID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor(), ent);
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }


        }

        private string CombinSalaryEmployeeInfo(T_HR_EMPLOYEE entEmployee)
        {
            string strTemp = string.Empty;
            if (entEmployee == null)
            {
                return strTemp;
            }

            strTemp = entEmployee.EMPLOYEEID + ";" + entEmployee.OWNERCOMPANYID;
            return strTemp;
        }

        private string GetCreateInfor()
        {
            string sbstr = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID + ";";

            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            return sbstr;
        }

        private void Cancel()
        {
            //this.DialogResult = false;
            flag = true;
            Save();
            //timer.Stop();
            //progressGenerate.Value = progressGenerate.Minimum;
            //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            //entBrowser.Close();
        }

        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.All;

            lookup.TitleContent = Utility.GetResourceStr("ORGANNAME");

            lookup.SelectedClick += (obj, ev) =>
            {
                if (cbxPayType.SelectedIndex == 3)//结算岗位
                {
                    //var ent = lookup.SelectedObj as T_HR_POST;
                    if (!(lookup.SelectedObj is T_HR_POST))
                    {
                        MessageBox.Show("只能选择有效的岗位！");
                        return;
                    }
                    else
                    {
                        var ent = lookup.SelectedObj as T_HR_POST;
                        var q = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.PostID == ent.POSTID);
                        if (q.Count()<1)
                        {
                            MessageBox.Show("你不在此结算岗位上任职，不能结算！");
                            return;
                        }
                        //var ent = lookup.SelectedObj as T_HR_POST;
                        lkSelectObj.DataContext = lookup.SelectedObj;
                        lkSelectObj.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME"; ;
                    }
                }
                else
                {
                    lkSelectObj.DataContext = lookup.SelectedObj;
                    if (lookup.SelectedObj is T_HR_COMPANY)
                    {
                        lkSelectObj.DisplayMemberPath = "CNAME";
                    }
                    else if (lookup.SelectedObj is T_HR_DEPARTMENT)
                    {
                        lkSelectObj.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    }
                    else if (lookup.SelectedObj is T_HR_POST)
                    {
                        lkSelectObj.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                    }
                    else if (lookup.SelectedObj is T_HR_EMPLOYEE)
                    {
                        lkSelectObj.DisplayMemberPath = "EMPLOYEECNAME";
                    }
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        #region IEntityEditor 成员

        public bool isSaving = false;

        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYGENERATE");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }
        }

        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (isSaving == false)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("CREATE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_workrules.png"
                };

                items.Add(item);

                //item = new ToolbarItem
                //{
                //    DisplayType = ToolbarItemDisplayTypes.Image,
                //    Key = "1",
                //    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                //    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
                //};

                //items.Add(item);
            }
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        private void lkSalaryCompany_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Company;

            lookup.TitleContent = Utility.GetResourceStr("ORGANNAME");

            lookup.SelectedClick += (obj, ev) =>
            {
                var ent = lookup.SelectedObj as T_HR_COMPANY;
                lkSalaryCompany.DataContext = ent;

            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void cbxPayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxPayType.SelectedIndex == 0)//发薪机构
            {
                lkSalaryCompany.SearchButton.IsEnabled = false;
                lkSalaryCompany.DataContext = salarycompany;
                lkSalaryCompany.Visibility = Visibility.Visible;
                tbSalaryCompany.Visibility = Visibility.Visible;

                lkSelectObj.Visibility = Visibility.Collapsed;
                tbselectObj.Visibility = Visibility.Collapsed;

                lkEmployee.Visibility = Visibility.Collapsed;
                tbEmployee.Visibility = Visibility.Collapsed;
            }
            if (cbxPayType.SelectedIndex == 1)//组织架构
            {
                lkSalaryCompany.Visibility = Visibility.Collapsed;
                tbSalaryCompany.Visibility = Visibility.Collapsed;

                lkSelectObj.Visibility = Visibility.Visible;
                tbselectObj.Visibility = Visibility.Visible;
                tbselectObj.Text = "组织架构";

                lkEmployee.Visibility = Visibility.Collapsed;
                tbEmployee.Visibility = Visibility.Collapsed;
            }
            if (cbxPayType.SelectedIndex == 2)//离职薪资
            {
                lkSalaryCompany.Visibility = Visibility.Collapsed;
                tbSalaryCompany.Visibility = Visibility.Collapsed;

                lkSelectObj.Visibility = Visibility.Collapsed;
                tbselectObj.Visibility = Visibility.Collapsed;
                tbselectObj.Text = "离职员工";

                lkEmployee.Visibility = Visibility.Visible;
                tbEmployee.Visibility = Visibility.Visible;
            }
            if (cbxPayType.SelectedIndex == 3)//结算岗位
            {
                lkSalaryCompany.Visibility = Visibility.Collapsed;
                tbSalaryCompany.Visibility = Visibility.Collapsed;

                lkSelectObj.Visibility = Visibility.Visible;
                tbselectObj.Visibility = Visibility.Visible;
                tbselectObj.Text = "结算岗位";

                lkEmployee.Visibility = Visibility.Collapsed;
                tbEmployee.Visibility = Visibility.Collapsed;

            }
        }

        private void lkEmployee_FindClick(object sender, EventArgs e)
        {
            T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
            Form.Salary.ResignForm form = new SMT.HRM.UI.Form.Salary.ResignForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.SaveClicked += (obj, ev) =>
            {

                temp.EMPLOYEEID = form.SelectedEmployees.FirstOrDefault().EMPLOYEEID;
                temp.EMPLOYEECNAME = form.SelectedEmployees.FirstOrDefault().EMPLOYEENAME;
                temp.EMPLOYEECODE = form.SelectedEmployees.FirstOrDefault().EMPLOYEECODE;
                temp.OWNERID = form.SelectedEmployees.FirstOrDefault().OWNERID;
                temp.OWNERPOSTID = form.SelectedEmployees.FirstOrDefault().OWNERPOSTID;
                temp.OWNERDEPARTMENTID = form.SelectedEmployees.FirstOrDefault().OWNERDEPARTMENTID;
                temp.OWNERCOMPANYID = form.SelectedEmployees.FirstOrDefault().OWNERCOMPANYID;
                lkEmployee.DataContext = temp;
                lkEmployee.TxtLookUp.Text = temp.EMPLOYEECNAME + "-" + form.SelectedEmployees.FirstOrDefault().PostName 
                    + "-" + form.SelectedEmployees.FirstOrDefault().DepartmentName + "-" + form.SelectedEmployees.FirstOrDefault().CompanyName;
            };
            //form.MinWidth = 450;
            form.Height = 450;
            browser.Height = 400;
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        private void cbxPayType_Loaded(object sender, RoutedEventArgs e)
        {
            cbxPayType.SelectedIndex = 0;
        }
    }
}

