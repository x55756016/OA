/********************************************************************************
//出差申请form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Input;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class  TravelRequestForm
    {

        #region IEntityEditor 成员
        public string GetTitle()
        {
            if (formType == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "EVECTIONFORM");
            }
            else if (formType == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "EVECTIONFORM");
            }
            else if (formType == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT1", "EVECTIONFORM");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "EVECTIONFORM");
            }
        }

        public string GetStatus()
        {
            return "";
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
            if (formType != FormTypes.Browse && formType != FormTypes.Audit)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
                };
                items.Add(item);
            }
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
        private void Close()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion



        /// <summary>
        ///     回到提交前的状态
        /// </summary>
        private void BackToSubmit()
        {
            needsubmit = false;
            #region 隐藏entitybrowser中的toolbar按钮
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            if (entBrowser.EntityEditor is IEntityEditor)
            {
                List<ToolbarItem> bars = GetToolBarItems();
                if (bars != null)
                {
                    ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    if (bar != null)
                    {
                        bar.Visibility = Visibility.Collapsed;
                    }
                }
            }

            #endregion
            RefreshUI(RefreshedTypes.AuditInfo);
        }


        #region MobileXml
        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text, string keyValue)
        {
            string[] strlist = new string[5];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            strlist[4] = keyValue;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryChiledList(strlist);
            return ad;
        }

        private string GetXmlString(T_OA_BUSINESSTRIP Info, string StrSource)
        {
            string goouttomeet = string.Empty;
            string privateaffair = string.Empty;
            string companycar = string.Empty;
            string isagent = string.Empty;
            string path = string.Empty;

            SMT.SaaS.MobileXml.MobileXml mx = new MobileXml.MobileXml();
            SMT.SaaS.MobileXml.AutoDictionary ad = new MobileXml.AutoDictionary();

            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_OA_BUSINESSTRIP", "CHECKSTATE", Master_Golbal.CHECKSTATE, GetCheckState(Master_Golbal.CHECKSTATE)));//审核状态
            if (Master_Golbal.OWNERID != null && !string.IsNullOrEmpty(Master_Golbal.OWNERNAME))//出差人
            {
                AutoList.Add(basedata("T_OA_BUSINESSTRIP", "OWNERID", Master_Golbal.OWNERID, Master_Golbal.OWNERNAME + "-" + Master_Golbal.OWNERPOSTNAME + "-" + Master_Golbal.OWNERDEPARTMENTNAME + "-" + Master_Golbal.OWNERCOMPANYNAME));
            }
            if (Master_Golbal.OWNERCOMPANYID != null && !string.IsNullOrEmpty(Master_Golbal.OWNERCOMPANYNAME))//所属公司
            {
                AutoList.Add(basedata("T_OA_BUSINESSTRIP", "OWNERCOMPANYID", Master_Golbal.OWNERCOMPANYID, Master_Golbal.OWNERCOMPANYNAME));
            }
            if (Master_Golbal.OWNERDEPARTMENTID != null && !string.IsNullOrEmpty(Master_Golbal.OWNERDEPARTMENTNAME))//所属部门
            {
                AutoList.Add(basedata("T_OA_BUSINESSTRIP", "OWNERDEPARTMENTID", Master_Golbal.OWNERDEPARTMENTID, Master_Golbal.OWNERDEPARTMENTNAME));
            }
            if (Master_Golbal.OWNERPOSTID != null && !string.IsNullOrEmpty(Master_Golbal.OWNERPOSTNAME))//所属岗位
            {
                AutoList.Add(basedata("T_OA_BUSINESSTRIP", "OWNERPOSTID", Master_Golbal.OWNERPOSTID, Master_Golbal.OWNERPOSTNAME));
            }
            if (fbCtr.Order.TOTALMONEY != null && fbCtr.Order.TOTALMONEY > 0)//借款金额
            {
                AutoList.Add(basedata("T_OA_BUSINESSTRIP", "CHARGEMONEY", Master_Golbal.CHARGEMONEY.ToString(), fbCtr.Order.TOTALMONEY.ToString()));
            }
            AutoList.Add(basedata("T_OA_BUSINESSTRIP", "POSTLEVEL", Master_Golbal.POSTLEVEL, string.Empty));//出差人的岗位级别
            //AutoList.Add(basedata("T_OA_BUSINESSTRIP", "DEPARTMENTNAME", string.Empty, Master_Golbal.OWNERDEPARTMENTNAME));//出差人所在部门
            if (Master_Golbal.ISAGENT != null)//是否启用代理
            {
                if (Master_Golbal.ISAGENT == "0")
                {
                    isagent = "不启用";
                }
                else
                {
                    isagent = "启用";
                }
                AutoList.Add(basedata("T_OA_BUSINESSTRIP", "ISAGENT", Master_Golbal.ISAGENT, isagent));
            }
            AutoList.Add(basedata("T_OA_BUSINESSTRIP", "CONTENT", Master_Golbal.CONTENT, Master_Golbal.CONTENT));//出差事由

            foreach (T_OA_BUSINESSTRIPDETAIL objDetail in TraveDetailList_Golbal)//填充子表
            {
                if (objDetail.DEPCITY != null)
                {
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "DEPCITY", objDetail.DEPCITY, GetCityName(objDetail.DEPCITY), objDetail.BUSINESSTRIPDETAILID));
                }
                if (objDetail.DESTCITY != null)
                {
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "DESTCITY", objDetail.DESTCITY, GetCityName(objDetail.DESTCITY), objDetail.BUSINESSTRIPDETAILID));
                }
                if (objDetail.BUSINESSDAYS != null)//出差天数
                {
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "BUSINESSDAYS", objDetail.BUSINESSDAYS, objDetail.BUSINESSDAYS, objDetail.BUSINESSTRIPDETAILID));
                }
                else
                {
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "BUSINESSDAYS", string.Empty, string.Empty, objDetail.BUSINESSTRIPDETAILID));
                }
                if (objDetail.PRIVATEAFFAIR != null)
                {
                    if (objDetail.PRIVATEAFFAIR == "0")//是否私事
                    {
                        privateaffair = "否";
                    }
                    else
                    {
                        privateaffair = "是";
                    }
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "PRIVATEAFFAIR", objDetail.PRIVATEAFFAIR, privateaffair, objDetail.BUSINESSTRIPDETAILID));
                }
                if (objDetail.GOOUTTOMEET != null)
                {
                    if (objDetail.GOOUTTOMEET == "0")//内部会议\培训
                    {
                        goouttomeet = "否";
                    }
                    else
                    {
                        goouttomeet = "是";
                    }
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "GOOUTTOMEET", objDetail.GOOUTTOMEET, goouttomeet, objDetail.BUSINESSTRIPDETAILID));
                }
                if (objDetail.COMPANYCAR != null)
                {
                    if (objDetail.COMPANYCAR == "0")//是否是公司派车
                    {
                        companycar = "否";
                    }
                    else
                    {
                        companycar = "是";
                    }
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "COMPANYCAR", objDetail.COMPANYCAR, companycar, objDetail.BUSINESSTRIPDETAILID));
                }
                if (objDetail.TYPEOFTRAVELTOOLS != null)
                {
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "TYPEOFTRAVELTOOLS", objDetail.TYPEOFTRAVELTOOLS, GetTypeName(objDetail.TYPEOFTRAVELTOOLS), objDetail.BUSINESSTRIPDETAILID));
                }
                if (objDetail.TAKETHETOOLLEVEL != null)
                {
                    AutoList.Add(basedata("T_OA_BUSINESSTRIPDETAIL", "TAKETHETOOLLEVEL", objDetail.TAKETHETOOLLEVEL, GetLevelName(objDetail.TAKETHETOOLLEVEL, GetTypeId(objDetail.TYPEOFTRAVELTOOLS)), objDetail.BUSINESSTRIPDETAILID));
                }
            }
            string a = mx.TableToXml(Info, TraveDetailList_Golbal, StrSource, AutoList);

            return a;
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            if (formType == FormTypes.Edit || formType == FormTypes.Resubmit)
            {
                if (Master_Golbal.CHECKSTATE == "0" || Master_Golbal.CHECKSTATE == "3")
                {
                    EntityBrowser browser = this.FindParentByType<EntityBrowser>();
                    browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
                }
            }
            string strXmlObjectSource = string.Empty;
            entity.SystemCode = "OA";
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = GetXmlString(Master_Golbal, entity.BusinessObjectDefineXML);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", Master_Golbal.OWNERID);
            paraIDs.Add("CreatePostID", Master_Golbal.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", Master_Golbal.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", Master_Golbal.OWNERCOMPANYID);

            if (Master_Golbal.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_OA_BUSINESSTRIP", Master_Golbal.BUSINESSTRIPID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_OA_BUSINESSTRIP", Master_Golbal.BUSINESSTRIPID, strXmlObjectSource);
            }
        }



        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (Common.CurrentLoginUserInfo.EmployeeID != Master_Golbal.OWNERID && Master_Golbal.CHECKSTATE == "0")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
        
        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            Utility.InitFileLoad(FormTypes.Audit, uploadFile, Master_Golbal.BUSINESSTRIPID, false);
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中
                    state = Utility.GetCheckState(CheckStates.Approving);
                    if (Master_Golbal.CHARGEMONEY > 0)
                    {
                        fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.Approving); //审核中
                    }
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
                    state = Utility.GetCheckState(CheckStates.Approved);
                    formType = FormTypes.Audit;
                    if (Master_Golbal.CHARGEMONEY > 0)
                    {
                        fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.Approved); //审核通过
                    }
                    if (Master_Golbal.ISAGENT == "1")//如果启用代理
                    {
                        AddAgent(TraveDetailList_Golbal.Count() - 1);
                        OaCommonOfficeClient.AgentDataSetAddAsync(AgentDateSet);//插入代理
                    }
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    if (Master_Golbal.CHARGEMONEY > 0)
                    {
                        fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnApproved); //审核不通过
                    }
                    break;
            }
            if (Master_Golbal.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                //UserState = "Submit";
            }
            if (formType == FormTypes.Resubmit || formType == FormTypes.New || formType == FormTypes.Edit)
            {
                SetTraveRequestValue();
            }
            Master_Golbal.CHECKSTATE = state;
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (formType == FormTypes.Edit || formType == FormTypes.Resubmit)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            if (formType == FormTypes.Audit || formType == FormTypes.Browse)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

            //approvalInfo.CHECKSTATE = state;
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //Travelmanagement.UpdateTravelmanagementAsync(Businesstrip, buipList, actions.ToString(), UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (Master_Golbal != null)
                state = Master_Golbal.CHECKSTATE;
            if (formType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
    }
}
