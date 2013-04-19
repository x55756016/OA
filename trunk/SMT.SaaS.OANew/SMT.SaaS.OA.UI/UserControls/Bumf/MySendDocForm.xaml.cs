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
using SMT.Saas.Tools.OrganizationWS;
using System.Windows.Browser;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using System.Runtime.Serialization;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MySendDocForm : BaseForm, IClient, IEntityEditor
    {
        public MySendDocForm(V_BumfCompanySendDoc obj)
        {
            InitializeComponent();
            tblcontent.HideControls();
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            GetSendDocDetailInfo(obj);
            tblcontent.HideControls();//屏蔽富文本框的头部
            DetailSendClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(SendDocClient_GetSendDocSingleInfoByIdCompleted);
        }
        SmtOACommonOfficeClient DetailSendClient = new SmtOACommonOfficeClient();
        //OrganizationServiceClient Organ
        private Action action;
        private T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
        public delegate void refreshGridView();
        void SendDocClient_GetSendDocSingleInfoByIdCompleted(object sender, GetSendDocSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        
                        senddoc = e.Result;
                        //ctrFile.Load_fileData(senddoc.SENDDOCID);
                        this.tbltitle.Text = senddoc.SENDDOCTITLE;
                        this.tblsend.Text = senddoc.SEND;
                        this.tblcopy.Text = senddoc.CC;
                        tblcontent.RichTextBoxContext = senddoc.CONTENT;
                        tblcontent.HideControls();
                        this.tbldepartment.Text = senddoc.DEPARTID;
                        this.tbldoctype.Text = senddoc.T_OA_SENDDOCTYPE.SENDDOCTYPE;
                        this.tblprioritity.Text = senddoc.PRIORITIES;
                        this.tblgrade.Text = senddoc.GRADED;
                        string StrPublish = "";
                        
                        //if(!string.IsNullOrEmpty(senddoc.PUBLISHDATE.ToString()))
                        //{
                        //   StrPublish = System.Convert.ToDateTime(senddoc.PUBLISHDATE.ToString()).ToShortDateString();
                        //}
                        if (!string.IsNullOrEmpty(senddoc.CREATEDATE.ToString()))
                        {
                            this.tbladddate.Text = System.Convert.ToDateTime(senddoc.CREATEDATE.ToString()).ToShortDateString(); ;
                        }
                        //if (!ctrFile._files.HasAccessory)
                        //{
                        //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(GridInfo,16);
                        //}
                        if (senddoc.PUBLISHDATE != null)
                        {
                            this.tblPublishDate.Text = System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToLongDateString()+"印发";
                            //StrPublish = dt.ToString("yyyymmdd"); 
                        }
                        //this.tblupdatedate.Text = obj.OACompanySendDoc.UPDATEDATE.ToString();
                        //senddoctab.TabStripPlacement = Dock.Left;
                        this.tblKeyWord.Text = senddoc.KEYWORDS;
                        this.tblStatus.Text = "发布";
                        this.tblnum.Text = senddoc.NUM;
                        this.tblcontenttitle.Text = senddoc.SENDDOCTITLE;
                        GetCompanyName(senddoc.OWNERCOMPANYID);

                        GetDepartmentName(senddoc.OWNERDEPARTMENTID);
                       
                        
                        

                        //ctrFile.IsEnabled = false;
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    return;
                }
            }

        }

        #region 获取公司信息


        /// <summary>
        /// 从资源文件中获取公司信息
        /// </summary>
        /// <param name="StrCompanyID"></param>
        private void GetCompanyName(string StrCompanyID)
        {
            if (Application.Current.Resources["SYS_CompanyInfo"] != null)
            {
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> ListCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                ListCompany = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

                if (ListCompany != null)
                {
                    var objc = from a in ListCompany
                               where a.COMPANYID == StrCompanyID
                               select a.CNAME;
                    if (objc.FirstOrDefault() == "集团本部")
                    {
                        this.tbltitlecompany.Text = "深圳市神州通投资集团有限公司";
                    }
                    else
                    {
                        this.tbltitlecompany.Text = objc.FirstOrDefault();
                    }
                    if (senddoc.ISREDDOC == "0")
                    {                        
                        this.tbltitlecompany.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }



            }
        }
        #endregion

        #region 获取部门信息

        /// <summary>
        /// 从资源文件中获取部门信息
        /// </summary>
        /// <param name="StrCompanyID"></param>
        private void GetDepartmentName(string StrDepartmentID)
        {
            if (Application.Current.Resources["SYS_DepartmentInfo"] != null)
            {
                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> ListCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                ListCompany = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

                if (ListCompany != null)
                {
                    var objc = from a in ListCompany
                               where a.DEPARTMENTID == StrDepartmentID
                               select a.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    this.tbldepartment.Text = objc.FirstOrDefault();
                }



            }
        }
        #endregion

        private void GetSendDocDetailInfo(V_BumfCompanySendDoc obj)
        {
            DetailSendClient.GetSendDocSingleInfoByIdAsync(obj.senddoc.SENDDOCID);
            

        }

        void DetailSendClient_GetDocDistrbuteInfosCompleted(object sender, GetDocDistrbuteInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    string StrViewType = "";
                    string ViewType = "";
                    string StrViewer = "发布对象  \n";
                    string StrDate = "";
                    string StrReturn = "";  //总输出
                    List<T_OA_DISTRIBUTEUSER> ListDistrbute = e.Result.ToList();
                    foreach (var bb in ListDistrbute)
                    {
                        //bb.VIEWTYPE

                        switch (bb.VIEWTYPE)
                        {
                            case "0":
                                ViewType = "按公司发布";
                                //获取公司名称
                                StrViewer += bb.VIEWER + "\n";
                                break;
                            case "1":
                                ViewType = "按部门发布";
                                //获取部门名称
                                StrViewer += bb.VIEWER + "\n";
                                break;
                            case "2":
                                ViewType = "按个人发布";
                                //获取个人名称
                                StrViewer += bb.VIEWER + "\n";
                                break;
                        }
                        if (bb.VIEWTYPE != StrViewType)
                        {
                            StrViewType = bb.VIEWTYPE;
                            StrReturn += "发布类型：" + ViewType + "\n";
                        }
                        if (bb.CREATEDATE.ToShortDateString() != StrDate)
                        {
                            StrDate = bb.CREATEDATE.ToShortDateString();
                            StrReturn += "发布时间：" + StrDate + "\n";
                        }
                        StrReturn += StrViewer;


                    }


                }
            }
        }





        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VIEWTITLE", "COMPANYDOCUMENT");

        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            SaveAndClose();

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

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/16_close.png"
            };
            items.Add(item);

            return items;
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

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            DetailSendClient.DoClose();
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
