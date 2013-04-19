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
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class AddDistrbuteForm : BaseForm,IClient, IEntityEditor
    {
        public AddDistrbuteForm()
        {
            InitializeComponent();
        }

        #region 初始化数据
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        private List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST>();
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        private V_BumfCompanySendDoc tmpSenddoc = new V_BumfCompanySendDoc();
        private ObservableCollection<T_OA_DISTRIBUTEUSER> ViewInfosList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
        private string tmpstrtype = "";  //记录选择状态
        SmtOACommonOfficeClient DocDistrbuteClient = new SmtOACommonOfficeClient();
        private OrganizationServiceClient organClient = new OrganizationServiceClient();
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<ExtOrgObj> issuanceExtOrgObj;
        private ObservableCollection<T_OA_DISTRIBUTEUSER> distributeLists;
        private ObservableCollection<T_OA_DISTRIBUTEUSER> distributeList;
        private T_OA_SENDDOC tmpdoc = new T_OA_SENDDOC();
        public delegate void refreshGridView();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private Action action;
        private List<V_CompanyDocNum> ListNums = new List<V_CompanyDocNum>();//公文文号集合

        //发布公文后发送邮件
        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<string> StrDepartmentIDsList = new ObservableCollection<string>();  //获取部门ID
        private ObservableCollection<string> StrPositionIDsList = new ObservableCollection<string>();  //获取岗位ID
        private ObservableCollection<string> StaffList = new ObservableCollection<string>();  //员工ID

        public event refreshGridView ReloadDataEvent;
        string StrDepartmentID = "";//发文部门

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        List<ExtOrgObj> entall = null;//初始化组织架构集合


        public AddDistrbuteForm(V_BumfCompanySendDoc obj)
        {
            InitializeComponent();
            InitialForm(obj);
        }

        /// <summary>
        /// 专用与待办里打开的情况
        /// </summary>
        /// <param name="sendDocId"></param>
        public AddDistrbuteForm(string sendDocId,bool isExistDel)
        {
            InitializeComponent();
            DocDistrbuteClient.GetBumfDocInfoCompleted += (o,e) =>
                {
                    if(e.Result!=null)
                    {
                        V_BumfCompanySendDoc bumfDoc=e.Result;
                        InitialForm(bumfDoc);
                        string LoginUserID = Common.CurrentLoginUserInfo.EmployeeID;
                        if (bumfDoc.senddoc.ISDISTRIBUTE == "0" && isExistDel
                            && (bumfDoc.senddoc.OWNERID == LoginUserID || bumfDoc.senddoc.CREATEUSERID==LoginUserID))
                        {
                            PermissionServiceClient PermClient = new PermissionServiceClient();
                            PermClient.GetCustomerPermissionByUserIDAndEntityCodeCompleted += (to, te) =>
                                {
                                    if (te.Result != null)
                                    {
                                        CloseTask.Visibility = Visibility.Visible;
                                    }
                                };
                            PermClient.GetCustomerPermissionByUserIDAndEntityCodeAsync(Common.CurrentLoginUserInfo.SysUserID, "T_OA_SENDDOC");
                        }
                        GetCompanDocInfo(bumfDoc);
                    }
                };
            DocDistrbuteClient.GetBumfDocInfoAsync(sendDocId);
        }

        private void InitialForm(V_BumfCompanySendDoc obj)
        {
            allCompanys = new List<Saas.Tools.OrganizationWS.T_HR_COMPANY>();
            distributeLists = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            distributeList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            //DocDistrbuteClient.DocDistrbuteBatchAddCompleted += new EventHandler<DocDistrbuteBatchAddCompletedEventArgs>(DocDistrbuteClient_DocDistrbuteBatchAddCompleted);
            //DocDistrbuteClient.BatchAddCompanyDocDistrbuteCompleted += new EventHandler<BatchAddCompanyDocDistrbuteCompletedEventArgs>(DocDistrbuteClient_BatchAddCompanyDocDistrbuteCompleted);
            DocDistrbuteClient.BatchAddCompanyDocDistrbuteForNewCompleted += new EventHandler<BatchAddCompanyDocDistrbuteForNewCompletedEventArgs>(DocDistrbuteClient_BatchAddCompanyDocDistrbuteForNewCompleted);
            DocDistrbuteClient.GetDocDistrbuteInfosByFormIDCompleted += new EventHandler<GetDocDistrbuteInfosByFormIDCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosByFormIDCompleted);
            tmpSenddoc = obj;
            tmpdoc = obj.OACompanySendDoc;
            tmpdoc.T_OA_SENDDOCTYPE = obj.doctype;
            //txtContent.HideControls();//屏蔽富文本框的头部
            //GetCompanDocInfo(obj);
            issuanceExtOrgObj = new List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
            DocDistrbuteClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosCompleted);
            //SendDocClient.GetSendDocSingleInfoByIdAsync(TypeObj.OACompanySendDoc.SENDDOCID);
            DocDistrbuteClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(DocDistrbuteClient_GetSendDocSingleInfoByIdCompleted);
            DocDistrbuteClient.SendDocInfoUpdateCompleted += new EventHandler<SendDocInfoUpdateCompletedEventArgs>(DocDistrbuteClient_SendDocInfoUpdateCompleted);
            DocDistrbuteClient.GetDocDistrbuteInfosByFormIDAsync(obj.OACompanySendDoc.SENDDOCID);
            personclient.GetEmployeeDetailByIDsCompleted += new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
            DocDistrbuteClient.GetCompanyDocNumsByUseridCompleted += new EventHandler<GetCompanyDocNumsByUseridCompletedEventArgs>(SendDocClient_GetCompanyDocNumsByUseridCompleted);
            DocDistrbuteClient.CloseDocTaskCompleted += new EventHandler<CloseDocTaskCompletedEventArgs>(DocDistributeClient_CloseDocTask);
            personclient.GetEmployeeDetailByParasCompleted += new EventHandler<GetEmployeeDetailByParasCompletedEventArgs>(personclient_GetEmployeeDetailByParasCompleted);
            //personclient.GetEmployeeIDsByParasCompleted += new EventHandler<GetEmployeeIDsByParasCompletedEventArgs>(personclient_GetEmployeeIDsByParasCompleted);
            personclient.GetEmployeeIDsWithParasCompleted += new EventHandler<GetEmployeeIDsWithParasCompletedEventArgs>(personclient_GetEmployeeIDsWithParasCompleted);
            organClient.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(organClient_GetALLCompanyViewCompleted);
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            //dpPublishDate.Text = System.DateTime.Now.ToShortDateString();
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";
            //ctrFile.Load_fileData(tmpdoc.SENDDOCID);
            string filter = "";
            StrDepartmentID = tmpdoc.DEPARTID;
            DocDistrbuteClient.GetCompanyDocNumsByUseridAsync(Common.CurrentLoginUserInfo.EmployeeID, "CREATEDATE", filter);
            this.Loaded += new RoutedEventHandler(AddDistrbuteForm_Loaded);
            organClient.GetALLCompanyViewAsync("");//加载公司信息
            //ctrFile.Load_fileData(obj.OACompanySendDoc.SENDDOCID);
        }

        void DocDistrbuteClient_BatchAddCompanyDocDistrbuteForNewCompleted(object sender, BatchAddCompanyDocDistrbuteForNewCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ISSUEDOCUMENTSUCCESED"));

                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
                    return;
                }
            }
            ViewInfosList.Clear();
        }


        #region 获取公司信息
        void organClient_GetALLCompanyViewCompleted(object sender, GetALLCompanyViewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    List<V_COMPANY> entTemps = e.Result.ToList();
                    allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                    var ents = entTemps.OrderBy(c => c.FATHERID);
                    foreach (var ent in ents)
                    {
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                        company.COMPANYID = ent.COMPANYID;
                        company.CNAME = ent.CNAME;
                        company.ENAME = ent.ENAME;
                        if (!string.IsNullOrEmpty(ent.BRIEFNAME))
                        {
                            company.BRIEFNAME = ent.BRIEFNAME;
                        }
                        else
                        {
                            company.BRIEFNAME = ent.CNAME;
                        }

                        company.COMPANRYCODE = ent.COMPANRYCODE;
                        company.SORTINDEX = ent.SORTINDEX;
                        if (!string.IsNullOrEmpty(ent.FATHERCOMPANYID))
                        {
                            company.T_HR_COMPANY2 = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                            company.T_HR_COMPANY2.COMPANYID = ent.FATHERCOMPANYID;
                            company.T_HR_COMPANY2.CNAME = entTemps.Where(s => s.COMPANYID == ent.FATHERCOMPANYID).FirstOrDefault().CNAME;
                        }
                        company.FATHERID = ent.FATHERID;
                        company.FATHERTYPE = ent.FATHERTYPE;
                        company.CHECKSTATE = ent.CHECKSTATE;
                        company.EDITSTATE = ent.EDITSTATE;
                        allCompanys.Add(company);
                    }
                    if (App.Current.Resources["SYS_CompanyInfo"] != null)
                    {
                        App.Current.Resources.Remove("SYS_CompanyInfo");
                        App.Current.Resources.Add("SYS_CompanyInfo", allCompanys);
                    }
                    else
                    {
                        App.Current.Resources.Add("SYS_CompanyInfo", allCompanys);
                    }

                    
                }
            }
        }
        #endregion
        


        void personclient_GetEmployeeIDsWithParasCompleted(object sender, GetEmployeeIDsWithParasCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                vemployeeObj.Clear();
                //StaffList.Clear();//清空员工ID集合 否则会逐条记录添加
                StrDepartmentIDsList.Clear();
                StrCompanyIDsList.Clear();
                StrPositionIDsList.Clear();
                if (e.Result != null)
                {
                    List<string> lstemployeeids = e.Result.ToList();
                    if (lstemployeeids.Count() > 0)
                    {

                        lstemployeeids.ForEach(
                            item =>
                            {

                                if (!StaffList.Contains(item))
                                {
                                    StaffList.Add(item);
                                }

                            }
                            );
                    }
                    //vemployeeObj = e.Result.ToList();

                }
            }
            else
            {
                //HtmlPage.Window.Alert(e.Error.ToString());

                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        void AddDistrbuteForm_Loaded(object sender, RoutedEventArgs e)
        {
            GetCompanDocInfo(tmpSenddoc);
        }

        
        #region 获取公文编号


        #region 获取员工信息
        void personclient_GetEmployeeDetailByParasCompleted(object sender, GetEmployeeDetailByParasCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    vemployeeObj.Clear();
                    StaffList.Clear();//清空员工ID集合 否则会逐条记录添加
                    StrDepartmentIDsList.Clear();
                    StrCompanyIDsList.Clear();
                    StrPositionIDsList.Clear();
                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();
                        
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }
        #endregion

        void SendDocClient_GetCompanyDocNumsByUseridCompleted(object sender, GetCompanyDocNumsByUseridCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    ListNums = e.Result.ToList();
                    GetCurrentCompanyDocNum(ListNums, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                }
            }
        }
        /// <summary>
        /// 获取当前的公文编号
        /// </summary>
        /// <param name="list"></param>
        private void GetCurrentCompanyDocNum(List<V_CompanyDocNum> list, string StrDempartmentid, string StrCompanyid)
        {
            if (list.Count > 0)
            {
                var ents = from ent in list
                           where ent.OWNERCOMPANYID == StrDempartmentid && ent.OWNERDEPARTMENTID == StrCompanyid
                           select ent;
                if (ents.Count() > 0)
                {
                    if (string.IsNullOrEmpty(this.txtNUM.Text))//公文编号为空则获取最大值
                    {
                        this.txtNUM.Text = GetMaxCompanyDocNum(ents.ToList());
                    }
                }
            }
        }
        private string GetMaxCompanyDocNum(List<V_CompanyDocNum> listnums)
        {
            string StrResult = "";
            string StrYear = "";
            List<int> list = new List<int>();
            listnums.ForEach(item =>
            {
                list.Add(GetCompanyNums(item.NUM));
            });
            StrResult = list.Max().ToString();
            StrYear = StrResult.Substring(0, 4);//获取年份
            StrResult = StrResult.Substring(4, StrResult.Length - 4);//取后面的数字
            var ents = from ent in listnums
                       where ent.NUM !=null && ent.NUM.Contains(StrResult)
                       select ent;
            if (ents.Count() > 0)
            {
                string StrMax = ents.FirstOrDefault().NUM;
                StrResult = StrMax.Replace(StrResult, (System.Convert.ToInt32(StrResult) + 1).ToString());


            }
            return StrResult;

        }
        /// <summary>
        /// 根据公文编号  获取公文的序号
        /// </summary>
        /// <param name="CompanyDoc"></param>
        /// <returns></returns>
        private int GetCompanyNums(string StrCompanyDoc)
        {
            int IntResut = -1;
            try
            {

                int IntPosition = -1;//]或】所处的位置
                int IntStart = -1;//[所处的位置
                string StrYear = "";//[]里的数字
                string StrYearNum = "";
                //int IntLastChar=-1;//“号”所处的位置
                if (StrCompanyDoc.IndexOf(']') > 0)//英文状态下的]
                {
                    IntPosition = StrCompanyDoc.IndexOf(']');
                }
                if (StrCompanyDoc.IndexOf('】') > 0)//中文下半角]
                {
                    IntPosition = StrCompanyDoc.IndexOf('】');
                }
                if (StrCompanyDoc.IndexOf('[') > 0)//英文状态下的[
                {
                    IntStart = StrCompanyDoc.IndexOf('[');
                }
                if (StrCompanyDoc.IndexOf('【') > 0)//中文下半角【
                {
                    IntStart = StrCompanyDoc.IndexOf('【');
                }
                StrYear = StrCompanyDoc.Substring(IntStart + 1, IntPosition - IntStart - 1);
                if (IntPosition > 0)
                {
                    StrYearNum = StrYear + StrCompanyDoc.Substring(IntPosition + 1, StrCompanyDoc.Length - 2 - IntPosition);
                    IntResut = System.Convert.ToInt32(StrYearNum);// -2是去掉最后一个“号”字
                }

            }
            catch (Exception ex)
            {
                IntResut = 0;
            }
            return IntResut;
        }

        #endregion

        void DocDistrbuteClient_GetSendDocSingleInfoByIdCompleted(object sender, GetSendDocSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        //T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
                        //senddoc = e.Result;
                        //txtContent.RichTextBoxContext = e.Result.CONTENT;
                        tmpdoc.CREATEDATE = e.Result.CREATEDATE;
                        tmpdoc.CREATEUSERNAME = e.Result.CREATEUSERNAME;
                        tmpdoc.CONTENT = e.Result.CONTENT;
                        tmpdoc.UPDATEUSERID = e.Result.UPDATEUSERID;
                        tmpdoc.UPDATEUSERNAME = e.Result.UPDATEUSERNAME;
                        //if (!ctrFile._files.HasAccessory)
                        //{
                        //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(GridInfo,7);
                        //}
                        
                    }
                }
            }
        }

        void DocDistrbuteClient_SendDocInfoUpdateCompleted(object sender, SendDocInfoUpdateCompletedEventArgs e)
        {
            ViewInfosList.Clear();
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.StrResult == "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ISSUEDOCUMENTSUCCESED"));

                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.StrResult, "COMPANYDOCUMENT"));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
                    return;
                }
            }
            
        }

        void DocDistrbuteClient_GetDocDistrbuteInfosByFormIDCompleted(object sender, GetDocDistrbuteInfosByFormIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error ==null)
                {
                    StrStaffList = e.Result;
                    if (StrStaffList != null) //发布对象有选择单个员工
                    {
                        GetDistrbuteStaff(StrStaffList);
                    }
                    else //发布对象没有选择单个员工
                    {
                        DocDistrbuteClient.GetDocDistrbuteInfosAsync(tmpSenddoc.OACompanySendDoc.SENDDOCID);
                    }
                }
            }
        }

        /// <summary>
        /// 获取参会人员并填充
        /// </summary>
        private void GetDistrbuteStaff(ObservableCollection<string> staffs)
        {
            personclient.GetEmployeeDetailByIDsAsync(staffs);

        }

        void personclient_GetEmployeeDetailByIDsCompleted(object sender, GetEmployeeDetailByIDsCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    vemployeeObj.Clear();
                    StrStaffList.Clear();//清空员工ID集合 否则会逐条记录添加
                    
                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();
                        
                        
                    }
                    DocDistrbuteClient.GetDocDistrbuteInfosAsync(tmpSenddoc.OACompanySendDoc.SENDDOCID);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        
        void GetCompanDocInfo(V_BumfCompanySendDoc obj)
        {
            this.txtChaoSend.Text = obj.OACompanySendDoc.CC;
            this.txtTemplateTitle.Text = obj.OACompanySendDoc.SENDDOCTITLE;
            this.txtGrade.Text = obj.OACompanySendDoc.GRADED;
            this.txtProritity.Text = obj.OACompanySendDoc.PRIORITIES;
            this.cbxDocType.Text = obj.doctype.SENDDOCTYPE;
            //this.txtContent.Text = obj.OACompanySendDoc.CONTENT;            
            tmpSenddoc = obj;
            DocDistrbuteClient.GetSendDocSingleInfoByIdAsync(obj.OACompanySendDoc.SENDDOCID);
            this.txtZhuSend.Text = obj.OACompanySendDoc.SEND;
            if (!string.IsNullOrEmpty(obj.OACompanySendDoc.NUM))
            {
                this.txtNUM.Text = obj.OACompanySendDoc.NUM;
            }
            if (!string.IsNullOrEmpty(obj.OACompanySendDoc.KEYWORDS))
            {
                this.txtKey.Text = obj.OACompanySendDoc.KEYWORDS;
            }
            if (!string.IsNullOrEmpty(obj.OACompanySendDoc.TEL))
            {
                this.txtTel.Text = obj.OACompanySendDoc.TEL;
            }
            //if (!ctrFile._files.HasAccessory)
            //{
            //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.GridInfo, 8);
            //}
            
            GetDepartmentNameByDepartmentID(obj.OACompanySendDoc.DEPARTID);
        }
        private void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        {
            OrganizationServiceClient Organ = new OrganizationServiceClient();
            Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
            Organ.GetDepartmentByIdAsync(StrDepartmentID);

        }
        void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                    department = e.Result;
                    
                    PostsObject.Text = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    //PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    //PostsObject.DataContext = department;
                }
            }
        }
        


        void DocDistrbuteClient_GetDocDistrbuteInfosCompleted(object sender, GetDocDistrbuteInfosCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        distributeList = e.Result;

                        
                        

                        foreach (var h in distributeList)
                        {
                            object obj = new object();
                            SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj extOrgObj = new SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj();
                            if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company).ToString())
                            {
                                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmp = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                                tmp.COMPANYID = h.VIEWER;

                                tmp.CNAME = Utility.GetCompanyName(tmp.COMPANYID);
                                obj = tmp;
                                StrCompanyIDsList.Add(h.VIEWER);
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department).ToString())
                            {
                                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmp = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                                tmp.DEPARTMENTID = h.VIEWER;
                                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENTDICTIONARY tmpdict = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENTDICTIONARY();
                                tmpdict.DEPARTMENTNAME = Utility.GetDepartmentName(h.VIEWER);
                                tmp.T_HR_DEPARTMENTDICTIONARY = tmpdict;
                                obj = tmp;
                                StrDepartmentIDsList.Add(h.VIEWER);
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post).ToString())
                            {
                                SMT.Saas.Tools.OrganizationWS.T_HR_POST tmp = new SMT.Saas.Tools.OrganizationWS.T_HR_POST();
                                tmp.POSTLEVEL = System.Convert.ToDecimal(h.VIEWER);
                                SMT.Saas.Tools.OrganizationWS.T_HR_POSTDICTIONARY tmpdict = new SMT.Saas.Tools.OrganizationWS.T_HR_POSTDICTIONARY();
                                tmpdict.POSTNAME = Utility.GetPostName(h.VIEWER);
                                tmp.T_HR_POSTDICTIONARY = tmpdict;
                                
                                obj = tmp;
                                StrPositionIDsList.Add(h.VIEWER);
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel).ToString())
                            {
                                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE tmp = new SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE();
                                tmp.EMPLOYEEID = h.VIEWER;
                                tmp.EMPLOYEECNAME = Utility.GetDistrbuteUserName(tmp.EMPLOYEEID,vemployeeObj);
                                obj = tmp;
                                if (!(StrStaffList.IndexOf(h.VIEWER) >0))
                                {
                                    StaffList.Add(h.VIEWER);
                                }
                            }
                            extOrgObj.ObjectInstance = obj;
                            issuanceExtOrgObj.Add(extOrgObj);
                        }
                        //personclient.GetEmployeeDetailByParasAsync(StrCompanyIDsList, StrDepartmentIDsList, StrPositionIDsList, StaffList);
                        //RefreshUI(RefreshedTypes.ShowProgressBar);
                        //personclient.GetEmployeeIDsByParasAsync(StrCompanyIDsList,StrDepartmentIDsList,StrPositionIDsList);
                        //StaffList.Clear();//将对象员工清除
                        //personclient.GetEmployeeIDsWithParasAsync(StrCompanyIDsList, true, StrDepartmentIDsList, true, StrPositionIDsList);
                        BindData();
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());

                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

       
        #endregion

        #region 发布按钮
        private void SaveAddDistrbute()
        {
            string StrNum = "";
            StrNum = this.txtNUM.Text.ToString().Trim();
            string StrPublishDate = "";
            if (!string.IsNullOrEmpty(this.dpPublishDate.Text.ToString()))
            {
                StrPublishDate = this.dpPublishDate.Text.ToString() +" "+ System.DateTime.Now.ToShortTimeString();
                DateTime DtStart = System.Convert.ToDateTime(StrPublishDate);
                tmpdoc.PUBLISHDATE = DtStart;
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATENOTNULL","PUBLISHDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATENOTNULL", "PUBLISHDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            //是红头文件则公文编号 一定要填写2011-4-29
            if (tmpdoc.ISREDDOC == "1")
            {
                if (string.IsNullOrEmpty(StrNum))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("COMPANYDOCNUMNOTNULL"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("COMPANYDOCNUMNOTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(StrNum))
                    {
                        bool IsCheck = true;
                        if (!((StrNum.IndexOf('[') > 0 && StrNum.IndexOf(']') > 0) || (StrNum.IndexOf('【') > 0 && StrNum.IndexOf('】') > 0)))
                        {
                            IsCheck = false;
                        }
                        if (StrNum.Substring(StrNum.Length - 1, 1) != "号")
                        {
                            IsCheck = false;
                        }

                        if (!IsCheck)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "公文编号格式不对，请参考集团本部人力[2011]01号或集团本部人力【2011】01号",
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }

                        int IntStart = -1;
                        int IntEnd = -1;
                        string StrYear = "";//[]里的数字

                        //int IntLastChar=-1;//“号”所处的位置
                        if (StrNum.IndexOf(']') > 0)//英文状态下的]
                        {
                            IntEnd = StrNum.IndexOf(']');
                        }
                        if (StrNum.IndexOf('】') > 0)//中文下半角]
                        {
                            IntEnd = StrNum.IndexOf('】');
                        }
                        if (StrNum.IndexOf('[') > 0)//英文状态下的[
                        {
                            IntStart = StrNum.IndexOf('[');
                        }
                        if (StrNum.IndexOf('【') > 0)//中文下半角【
                        {
                            IntStart = StrNum.IndexOf('【');
                        }
                        StrYear = StrNum.Substring(IntStart + 1, IntEnd - IntStart - 1);
                        if (StrYear != DateTime.Now.Year.ToString())
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "公文编号格式不对，[]或【】里面应该为当前年份如：【2011】",
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                    }
                    //替换到有空格的编号
                    if (StrNum.IndexOf(' ') > -1)
                    {
                        tmpdoc.NUM = StrNum.Replace(" ", "");
                    }
                    else
                    {
                        tmpdoc.NUM = StrNum;
                    }
                }
                if (string.IsNullOrEmpty(StrDepartmentID))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "发文部门不能为空",Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                else
                {
                    tmpdoc.DEPARTID = StrDepartmentID;
                }
            }

            

            if (issuanceExtOrgObj.Count >0)
            {
                distributeLists.Clear();
                foreach (var h in issuanceExtOrgObj)
                {
                    AddDistributeObjList(h, tmpSenddoc.OACompanySendDoc.SENDDOCID);
                    
                }
                
                //DocDistrbuteClient.DocDistrbuteBatchAddAsync(distributeLists,StaffList,tmpSenddoc.OACompanySendDoc);
                RefreshUI(RefreshedTypes.ShowProgressBar);
                //DocDistrbuteClient.BatchAddCompanyDocDistrbuteAsync(distributeLists, StaffList, tmpSenddoc.OACompanySendDoc);
                DocDistrbuteClient.BatchAddCompanyDocDistrbuteForNewAsync(distributeLists, StrCompanyIDsList, StrDepartmentIDsList, StrPositionIDsList, StrStaffList, tmpSenddoc.OACompanySendDoc);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "DISTRBUTEOBJECT"));
                return;
            }


        }

        private void AddDistributeObjList(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj, string issuanceID)
        {
            T_OA_DISTRIBUTEUSER distributeTmp = new T_OA_DISTRIBUTEUSER();
            distributeTmp.DISTRIBUTEUSERID = Guid.NewGuid().ToString();
            distributeTmp.MODELNAME = "CompanySendDoc";
            distributeTmp.FORMID = issuanceID;
            distributeTmp.VIEWTYPE = ((int)GetObjectType(issuanceExtOrgObj)).ToString();
            if (distributeTmp.VIEWTYPE == ((int)IssuanceObjectType.Post).ToString())    //如果是选择岗位，则保存岗位级别
            {
                //if (!string.IsNullOrEmpty((issuanceExtOrgObj.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST).POSTLEVEL.ToString()))
                //{
                //    distributeTmp.VIEWER = (issuanceExtOrgObj.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST).POSTLEVEL.ToString();
                //}
                //else
                //{
                //    distributeTmp.VIEWER = (issuanceExtOrgObj.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST).T_HR_POSTDICTIONARY.POSTLEVEL.ToString();
                //}
            }
            else
            {
                distributeTmp.VIEWER = issuanceExtOrgObj.ObjectID;
            }
            distributeTmp.CREATEDATE = DateTime.Now;
            distributeTmp.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            distributeTmp.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            distributeTmp.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            distributeTmp.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            distributeTmp.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            distributeTmp.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            distributeTmp.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            distributeTmp.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            distributeTmp.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            distributeTmp.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            if (distributeTmp.VIEWTYPE != ((int)IssuanceObjectType.Post).ToString())    //如果是选择岗位，则保存岗位级别
            {
                var ents = from ent in distributeLists
                           where ent.FORMID == distributeTmp.FORMID
                           && ent.VIEWER == distributeTmp.VIEWER
                           && ent.VIEWTYPE == distributeTmp.VIEWTYPE
                           select ent;
                if (ents.Count() == 0)
                {
                    distributeLists.Add(distributeTmp);
                }
            }
        }
        private IssuanceObjectType GetObjectType(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj)
        {
            if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)
            {
                return IssuanceObjectType.Company;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)
            {
                return IssuanceObjectType.Department;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post)
            {
                return IssuanceObjectType.Post;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
            {
                return IssuanceObjectType.Employee;
            }
            return IssuanceObjectType.Company;
        }

        void DocDistrbuteClient_BatchAddCompanyDocDistrbuteCompleted(object sender, BatchAddCompanyDocDistrbuteCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ISSUEDOCUMENTSUCCESED"));

                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
                    return;
                }
            }
            ViewInfosList.Clear();
        }

        void DocDistrbuteClient_DocDistrbuteBatchAddCompleted(object sender, DocDistrbuteBatchAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ISSUEDOCUMENTSUCCESED"));
                    tmpdoc.ISDISTRIBUTE = "1";
                    string StrReturn = "";
                    DocDistrbuteClient.SendDocInfoUpdateAsync(tmpdoc,StrReturn);
                    //RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
                    return;
                }
            }
            ViewInfosList.Clear();
        }
        #endregion 

        #region 取消按钮

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;

        }
        #endregion

        #region 添加发布对象按钮

        private void GetCompanyExtOrgObj(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> LstOldCompanys, List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> LstCompanys)
        {
            if (LstCompanys.Count() > 0)
            {
                LstCompanys.ToList().ForEach(child =>
                {
                    StrCompanyIDsList.Add(child.COMPANYID);
                    //issuanceExtOrgObj.Add(item);
                    ExtOrgObj objSecond = new ExtOrgObj();
                    objSecond.ObjectID = child.COMPANYID;
                    objSecond.ObjectName = child.CNAME;
                    objSecond.ObjectType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
                    var ExistEnts = from ext in entall
                                    where ext.ObjectID == child.COMPANYID
                                    && ext.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company
                                    select ext;
                    if (ExistEnts.Count() == 0)
                    {
                        entall.Add(objSecond);
                    }
                    var ents = from childcompany in LstOldCompanys
                               where childcompany.T_HR_COMPANY2 != null && childcompany.T_HR_COMPANY2.COMPANYID == child.COMPANYID
                               select childcompany;
                    if (ents.Count() > 0)
                    {
                        GetCompanyExtOrgObj(LstOldCompanys,ents.ToList());
                    }

                });
            }
        }
        
        private void AddIssuanObj()
        {
            entall = new List<ExtOrgObj>();
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    //issuanceExtOrgObj = ent;
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
                            var ExistEnts = from ext in entall
                                            where ext.ObjectID == obj2.ObjectID
                                            && ext.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company
                                            select ext;
                            if (ExistEnts.Count() == 0)
                            {
                                entall.Add(obj2);
                            }
                            //entall.Add(obj2);
                            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> listcompany = App.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                            var ents = from childcompany in listcompany
                                       where childcompany.T_HR_COMPANY2 != null && childcompany.T_HR_COMPANY2.COMPANYID == h.ObjectID
                                       select childcompany;
                            if (ents != null)
                            {
                                if (ents.Count() > 0)
                                {
                                    GetCompanyExtOrgObj(listcompany,ents.ToList());
                                //    ents.ToList().ForEach(item =>
                                //    {
                                //        if (!StrCompanyIDsList.Contains(item.COMPANYID))
                                //        {
                                //            StrCompanyIDsList.Add(item.COMPANYID);
                                //            //issuanceExtOrgObj.Add(item);
                                //            ExtOrgObj obj1 = new ExtOrgObj();
                                //            obj1.ObjectID = item.COMPANYID;
                                //            obj1.ObjectName = item.CNAME;
                                //            obj1.ObjectType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
                                //            entall.Add(obj1);
                                //            #region 2级子公司
                                            
                                //            var entsChilds = from childcompany in listcompany
                                //                             where childcompany.T_HR_COMPANY2 != null && childcompany.T_HR_COMPANY2.COMPANYID == item.COMPANYID
                                //                             select childcompany;
                                //            if (entsChilds.Count() > 0)
                                //            {
                                //                entsChilds.ToList().ForEach(child =>
                                //                {
                                //                    StrCompanyIDsList.Add(child.COMPANYID);
                                //                    //issuanceExtOrgObj.Add(item);
                                //                    ExtOrgObj objSecond = new ExtOrgObj();
                                //                    objSecond.ObjectID = child.COMPANYID;
                                //                    objSecond.ObjectName = child.CNAME;
                                //                    objSecond.ObjectType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
                                //                    var ExistEnts = from ext in entall
                                //                                    where ext.ObjectID == child.COMPANYID
                                //                                    && ext.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company
                                //                                    select ext;
                                //                    if (ExistEnts.Count() == 0)
                                //                    {
                                //                        entall.Add(objSecond);
                                //                    }
                                //                });

                                //            }

                                //            #endregion
                                //        }
                                //    });
                                }
                            }
                            

                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)//部门
                        {
                            StrDepartmentIDsList.Add(h.ObjectID);
                            ExtOrgObj objDepart = new ExtOrgObj();
                            objDepart.ObjectID = h.ObjectID;
                            objDepart.ObjectName = h.ObjectName;
                            objDepart.ObjectType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
                            var ExistEnts = from ext in entall
                                            where ext.ObjectID == objDepart.ObjectID
                                            && ext.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department
                                            select ext;
                            if (ExistEnts.Count() == 0)
                            {
                                entall.Add(objDepart);
                            }
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post)//岗位
                        {
                            StrPositionIDsList.Add(h.ObjectID);
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
                        {
                            StaffList.Add(h.ObjectID);
                            ExtOrgObj objPerson = new ExtOrgObj();
                            objPerson.ObjectID = h.ObjectID;
                            objPerson.ObjectName = h.ObjectName;
                            objPerson.ObjectType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
                            var ExistEnts = from ext in entall
                                            where ext.ObjectID == objPerson.ObjectID
                                            && ext.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel
                                            select ext;
                            if (ExistEnts.Count() == 0)
                            {
                                entall.Add(objPerson);
                            }
                        }
                    }
                    issuanceExtOrgObj = entall;
                    //personclient.GetEmployeeDetailByParasAsync(StrCompanyIDsList, StrDepartmentIDsList, StrPositionIDsList, StaffList);
                    //personclient.GetEmployeeIDsByParasAsync(StrCompanyIDsList, StrDepartmentIDsList, StrPositionIDsList);
                    //StaffList.Clear();
                    //personclient.GetEmployeeIDsWithParasAsync(StrCompanyIDsList,true, StrDepartmentIDsList,true, StrPositionIDsList);
                    BindData();
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }
        #endregion

        #region 预览按钮
		
        private void ShowPreviewSendDoc()
        {
            string StrNum = this.txtNUM.Text.ToString();
            string StrPublish = this.dpPublishDate.Text.ToString();
            if (!string.IsNullOrEmpty(StrPublish))
            {
                StrPublish = System.Convert.ToDateTime(StrPublish).ToLongDateString();
            }
            if (string.IsNullOrEmpty(StrNum))
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("PROMPT"), "公文文号不能为空");

            }
            else
            {
                //V_BumfCompanySendDoc SendDocInfoT = new V_BumfCompanySendDoc();
                //PriviewSendDoc AddWin = new PriviewSendDoc(tmpdoc.SENDDOCID,StrNum,StrPublish);
                CompanyDocWebPart AddWin = new CompanyDocWebPart(tmpdoc.SENDDOCID, StrNum, StrPublish);
                System.Windows.Controls.Window wd = new System.Windows.Controls.Window();
                wd.MinWidth = 900;
                wd.MinHeight = 500;
                wd.Content = AddWin;
                wd.TitleContent = "公文预览";
                wd.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true, false, tmpdoc.SENDDOCID);
            }
        }
        #endregion

        #region 绑定数据
        private void BindData()
        {
            dgIssunanceObj.ItemsSource = null;
            if (issuanceExtOrgObj == null || issuanceExtOrgObj.Count < 1)
            {                
                return;
            }
            else
            {                
                dgIssunanceObj.ItemsSource = issuanceExtOrgObj;
            }

        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {   
            return Utility.GetResourceStr("ADDTITLE", "ISSUEDOCUMENT");
            
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
                    saveType = RefreshedTypes.LeftMenu;
                    Save();
                    break;                
                //case "2":
                //    AddIssuanObj();
                //    break;
                //case "1":
                //    ShowPreviewSendDoc();
                //    break;

            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            //ToolbarItem item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "1",
            //    Title = Utility.GetResourceStr("PREVIEW"),
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/(09,24).png"

                
            //};
                        
            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "2",
            //    Title = Utility.GetResourceStr("CHOOSEDISTRBUTEOBJECT"),
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
            //};
            //items.Add(item);
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("ISSUEDOCUMENT"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);
            

            return items;
        }

        private void Close()
        {
            RefreshUI(saveType);
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 确定、取消
        private void Save()
        {

            if (action == Action.Add)
            { SaveAddDistrbute(); }
            
        }

        private void SaveAndClose()
        {
            Save();
            //RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region 删除按钮
        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            Button delBtn = sender as Button;
            //SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST MeetingV = delBtn.Tag as SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST;
            ExtOrgObj ext = delBtn.Tag as ExtOrgObj;
            //foreach (var ent in vemployeeObj)
            //{ 
                
            //}
            
            //var ents = null ;
            
            
            List<Saas.Tools.PersonnelWS.V_EMPLOYEEPOST> listpost = new List<Saas.Tools.PersonnelWS.V_EMPLOYEEPOST>();
            
            switch (ext.ObjectType)
            { 
                case SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company:
                    var companys = from ent in vemployeeObj
                               where ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == ext.ObjectID
                               select ent;
                    listpost = companys.Count() > 0 ? companys.ToList() : null;
                    break;
                case SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department:
                     var departs = from ent in vemployeeObj
                               where ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID == ext.ObjectID
                               select ent;
                     listpost = departs.Count() > 0 ? departs.ToList() : null;
                    break;
                case SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post:
                     var posts = from ent in vemployeeObj
                               where ent.EMPLOYEEPOSTS[0].T_HR_POST.POSTID == ext.ObjectID
                               select ent;
                     listpost = posts.Count() > 0 ? posts.ToList() : null;
                    break;
                case SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel:
                     var persons = from ent in vemployeeObj
                               where ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == ext.ObjectID
                               select ent;
                     listpost = persons.Count() > 0 ? persons.ToList() : null;
                    break;
            }
            if (listpost != null)
            {
                foreach (var ent in listpost)
                {
                    StaffList.Remove(ent.T_HR_EMPLOYEE.EMPLOYEEID);
                }
            }
            //vemployeeObj.Remove(MeetingV);
            //dgmember.ItemsSource = null;
            issuanceExtOrgObj.Remove(ext);
            BindData();
        }

        private void dgIssunanceObj_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST StaffV = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST)e.Row.DataContext;
            ExtOrgObj ExtObj = (ExtOrgObj)e.Row.DataContext;
            Button DelBtn = dgIssunanceObj.Columns[3].GetCellContent(e.Row).FindName("BtnDel") as Button;
            DelBtn.Tag = ExtObj;
            int index = e.Row.GetIndex();
            var cell = dgIssunanceObj.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();

        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            DocDistrbuteClient.DoClose();
            personclient.DoClose();
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

        #region 选择发布对象按钮        
        
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            AddIssuanObj();
        }
        #endregion

        #region 预览按钮        
        
        private void PriviewBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowPreviewSendDoc();
        }
        #endregion

        #region 选择部门

        private void PostsObject_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    PostsObject.Text = companyInfo.ObjectName;
                    StrDepartmentID = companyInfo.ObjectID;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }

        //private void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        //{
        //    OrganizationServiceClient Organ = new OrganizationServiceClient();
        //    Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
        //    Organ.GetDepartmentByIdAsync(StrDepartmentID);

        //}
        //void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        //{
        //    if (!e.Cancelled)
        //    {
        //        if (e.Result != null)
        //        {
        //            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
        //            department = e.Result;
        //            StrDepartmentID = department.DEPARTMENTID;
        //            PostsObject.Text = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //            //PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
        //            //PostsObject.DataContext = department;
        //        }
        //    }
        //}

        #endregion

        private void CloseTask_Click(object sender, RoutedEventArgs e)
        {
            var tmpDoc=tmpSenddoc.senddoc;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            DocDistrbuteClient.CloseDocTaskAsync(tmpDoc.SENDDOCID, tmpDoc.OWNERID);
        }

        private void DocDistributeClient_CloseDocTask(object sender,CloseDocTaskCompletedEventArgs e)
        {
            if(e.Error!=null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "删除操作发生错误");
            }
            if (e.Result != null)
            {
                bool result = e.Result;
                if (result)
                {
                    CloseTask.Visibility = Visibility.Collapsed;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Message, "消息", "删除此公司发文的待办成功");
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "删除操作发生错误");
            }
        }
    }
}
