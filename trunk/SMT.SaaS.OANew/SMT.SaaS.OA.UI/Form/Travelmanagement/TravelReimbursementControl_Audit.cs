/********************************************************************************
//出差报销form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.TravelExpApplyMaster;
using SMT.Saas.Tools.FBServiceWS;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class TravelReimbursementControl 
    {
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

        private string GetXmlString(T_OA_TRAVELREIMBURSEMENT Info, string StrSource)
        {
            string goouttomeet = string.Empty;
            string privateaffair = string.Empty;
            string companycar = string.Empty;
            string isagent = string.Empty;
            string path = string.Empty;
            string chargetype = string.Empty;
            string ExtTotal = "";
            if (fbCtr.ListDetail.Count() > 0)
            {
                decimal totalMoney = this.fbCtr.ListDetail.Sum(item =>
                {
                    return (item.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY;
                });
                ExtTotal = totalMoney.ToString();
            }

            SMT.SaaS.MobileXml.MobileXml mx = new MobileXml.MobileXml();
            SMT.SaaS.MobileXml.AutoDictionary ad = new MobileXml.AutoDictionary();

            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "CHECKSTATE", TravelReimbursement_Golbal.CHECKSTATE, GetCheckState(TravelReimbursement_Golbal.CHECKSTATE)));//审核状态
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "BUSINESSTRIPID", businesstrID, string.Empty));//出差申请ID
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "AVAILABLECREDIT", UsableMoney, string.Empty));//可用额度
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "REIMBURSEMENTSTANDARDS", bxbz, string.Empty));//报销标准
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "REIMBURSEMENTOFCOSTS", fbCtr.Order.TOTALMONEY.ToString(), string.Empty));//报销总计
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "POSTLEVEL", EmployeePostLevel, string.Empty));//出差人的岗位级别
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "CONTENT", TravelReimbursement_Golbal.CONTENT, TravelReimbursement_Golbal.CONTENT));//报告内容
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "REMARKS", TravelReimbursement_Golbal.REMARKS, string.Empty));//备注
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "PAYMENTINFO", fbCtr.Order.PAYMENTINFO, string.Empty));//支付信息
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "PAYTARGET", ExtTotal, string.Empty));//小计
            StrPayInfo = txtPAYMENTINFO.Text.ToString();
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "PAYMENTINFO", StrPayInfo, string.Empty));//支付信息
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "AttachMent", TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID, TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID));//附件

            if (!string.IsNullOrEmpty(TravelReimbursement_Golbal.NOBUDGETCLAIMS))//报销单号
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "NOBUDGETCLAIMS", TravelReimbursement_Golbal.NOBUDGETCLAIMS, string.Empty));//报销单号
            }
            if (TravelReimbursement_Golbal.CLAIMSWERE != null && !string.IsNullOrEmpty(EmployeeName))//报销人
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "CLAIMSWERE", TravelReimbursement_Golbal.CLAIMSWERE, EmployeeName + "-" + postName + "-" + depName + "-" + companyName));
            }
            if (TravelReimbursement_Golbal.OWNERID != null && !string.IsNullOrEmpty(EmployeeName))//所属人
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERID", TravelReimbursement_Golbal.OWNERID, EmployeeName));
            }
            if (TravelReimbursement_Golbal.OWNERCOMPANYID != null && !string.IsNullOrEmpty(companyName))//所属公司
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERCOMPANYID", TravelReimbursement_Golbal.OWNERCOMPANYID, companyName));
            }
            if (TravelReimbursement_Golbal.OWNERDEPARTMENTID != null && !string.IsNullOrEmpty(depName))//所属部门
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERDEPARTMENTID", TravelReimbursement_Golbal.OWNERDEPARTMENTID, depName));
            }
            if (TravelReimbursement_Golbal.OWNERPOSTID != null && !string.IsNullOrEmpty(postName))//所属岗位
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERPOSTID", TravelReimbursement_Golbal.OWNERPOSTID, postName));
            }
            foreach (T_OA_REIMBURSEMENTDETAIL objDetail in TravelDetailList_Golbal)//填充子表
            {
                if (objDetail.BUSINESSDAYS != null)//出差天数
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "BUSINESSDAYS", objDetail.BUSINESSDAYS, objDetail.BUSINESSDAYS, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.THENUMBEROFNIGHTS != null)//住宿天数
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "THENUMBEROFNIGHTS", objDetail.THENUMBEROFNIGHTS, objDetail.THENUMBEROFNIGHTS, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.DEPCITY != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "DEPCITY", objDetail.DEPCITY, SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(objDetail.DEPCITY), objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.DESTCITY != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "DESTCITY", objDetail.DESTCITY, SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(objDetail.DESTCITY), objDetail.REIMBURSEMENTDETAILID));
                }
                if (TravelReimbursement_Golbal.CREATEUSERID != null)//创建人
                {
                    AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERID", TravelReimbursement_Golbal.CREATEUSERID, Common.CurrentLoginUserInfo.EmployeeName, objDetail.REIMBURSEMENTDETAILID));
                }
                if (TravelReimbursement_Golbal.UPDATEUSERID != null)//修改人
                {
                    AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERID", TravelReimbursement_Golbal.UPDATEUSERID, Common.CurrentLoginUserInfo.EmployeeName, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.OTHERCOSTS != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "OTHERCOSTS", objDetail.OTHERCOSTS.ToString(), string.Empty, objDetail.REIMBURSEMENTDETAILID));
                }
                else //如果没有其他费用就传空值给Xml
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "OTHERCOSTS", string.Empty, string.Empty, objDetail.REIMBURSEMENTDETAILID));
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
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "PRIVATEAFFAIR", objDetail.PRIVATEAFFAIR, privateaffair, objDetail.REIMBURSEMENTDETAILID));
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
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "GOOUTTOMEET", objDetail.GOOUTTOMEET, goouttomeet, objDetail.REIMBURSEMENTDETAILID));
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
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "COMPANYCAR", objDetail.COMPANYCAR, companycar, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.TYPEOFTRAVELTOOLS != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "TYPEOFTRAVELTOOLS", objDetail.TYPEOFTRAVELTOOLS, GetTypeName(objDetail.TYPEOFTRAVELTOOLS), objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.TAKETHETOOLLEVEL != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "TAKETHETOOLLEVEL", objDetail.TAKETHETOOLLEVEL, GetLevelName(objDetail.TAKETHETOOLLEVEL, GetTypeId(objDetail.TYPEOFTRAVELTOOLS)), objDetail.REIMBURSEMENTDETAILID));
                }
            }
            ObservableCollection<Object> TrListObj = new ObservableCollection<Object>();
            foreach (var item in TravelDetailList_Golbal)
            {
                TrListObj.Add(item);
            }

            if (fbCtr.ListDetail.Count > 0)//获取算控件中的数据
            {
                //SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER entext = fbCtr
                //fbCtr.Order.REMARK
                string StrType = "";
                if (fbCtr.Order.REMARK != null)
                {
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "FBREMARK", fbCtr.Order.REMARK, fbCtr.Order.REMARK, fbCtr.Order.EXTENSIONALORDERID));//科目报销备注
                }
                if (fbCtr.Order.APPLYTYPE == 1)
                {
                    StrType = "个人费用报销";
                }
                if (fbCtr.Order.APPLYTYPE == 2)
                {
                    StrType = "冲借款";
                }
                if (fbCtr.Order.REMARK != null)
                {
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "EXTENSIONTYPE", StrType, StrType, fbCtr.Order.EXTENSIONALORDERID));//科目报销备注
                }
                foreach (FBEntity item in fbCtr.ListDetail)//预算费用报销明细
                {
                    SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONORDERDETAIL entTemp = item.Entity as SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONORDERDETAIL;

                    TrListObj.Add(entTemp);

                    if (entTemp.CHARGETYPE != null)
                    {
                        if (entTemp.CHARGETYPE.ToString() == "1")
                        {
                            chargetype = "个人预算费用";
                        }
                        else
                        {
                            chargetype = "部门预算费用";
                        }
                        AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "CHARGETYPE", entTemp.CHARGETYPE.ToString(), chargetype, entTemp.EXTENSIONORDERDETAILID));//费用类型
                    }
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "SUBJECTCODE", entTemp.T_FB_SUBJECT.SUBJECTCODE, entTemp.T_FB_SUBJECT.SUBJECTCODE, entTemp.EXTENSIONORDERDETAILID));//科目编号
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "SUBJECTNAME", entTemp.T_FB_SUBJECT.SUBJECTNAME, entTemp.T_FB_SUBJECT.SUBJECTNAME, entTemp.EXTENSIONORDERDETAILID));//科目名称
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "USABLEMONEY", entTemp.USABLEMONEY.ToString(), entTemp.USABLEMONEY.ToString(), entTemp.EXTENSIONORDERDETAILID));//可用金额
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "REMARK", entTemp.REMARK, entTemp.REMARK, entTemp.EXTENSIONORDERDETAILID));//摘要
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "APPLIEDMONEY", entTemp.APPLIEDMONEY.ToString(), entTemp.APPLIEDMONEY.ToString(), entTemp.EXTENSIONORDERDETAILID));//申领金额
                }
            }
            //冲借款明细

            if (fbCtr.ListBorrowDetail.Count() > 0)
            {

                foreach (T_FB_CHARGEAPPLYREPAYDETAIL item in fbCtr.ListBorrowDetail)//预算费用报销明细
                {
                    TrListObj.Add(item);

                    if (item.REPAYTYPE != null)
                    {
                        switch (item.REPAYTYPE.ToString())
                        {
                            case "1":
                                chargetype = "现金还普通借款";
                                break;
                            case "2":
                                chargetype = "现金还备用金借款";
                                break;
                            case "3":
                                chargetype = "现金还专项借款";
                                break;

                        }
                        AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "CHARGETYPE", item.REPAYTYPE.ToString(), chargetype, item.CHARGEAPPLYREPAYDETAILID));//费用类型
                    }
                    if (item.BORROWMONEY != null)
                    {
                        AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "BORROWMONEY", item.BORROWMONEY.ToString(), item.BORROWMONEY.ToString(), item.CHARGEAPPLYREPAYDETAILID));//科目编号
                    }

                    AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "REMARK", item.REMARK, item.REMARK, item.CHARGEAPPLYREPAYDETAILID));//摘要
                    if (item.REPAYMONEY != null)
                    {
                        AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "REPAYMONEY", item.REPAYMONEY.ToString(), item.REPAYMONEY.ToString(), item.CHARGEAPPLYREPAYDETAILID));//申领金额
                    }
                }
            }
            //string a = mx.TableToXml(Info, TrListObj, StrSource, AutoList);
            string a = mx.TableToXmlForTravel(Info, TrListObj, StrSource, AutoList);

            return a;
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            if (formType == FormTypes.Edit && formType == FormTypes.Resubmit)
            {
                EntityBrowser browser = this.FindParentByType<EntityBrowser>();
                browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            }
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("REIMBURSEMENTOFCOSTS", fbCtr.Order.TOTALMONEY.ToString());
            parameters.Add("POSTLEVEL", EmployeePostLevel);
            parameters.Add("DEPARTMENTNAME", depName);
            parameters.Add("BUSINESSTRIPID", businesstrID);

            if (TravelReimbursement_Golbal != null)
            {
                entity.SystemCode = "OA";
                if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                {
                    strXmlObjectSource = GetXmlString(TravelReimbursement_Golbal, entity.BusinessObjectDefineXML);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销表单数据不能为空!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                if (clickSubmit == true)
                {
                    RefreshUI(RefreshedTypes.All);
                    clickSubmit = false;
                }
                return;
            }
            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", TravelReimbursement_Golbal.OWNERID);
            paraIDs.Add("CreatePostID", TravelReimbursement_Golbal.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", TravelReimbursement_Golbal.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", TravelReimbursement_Golbal.OWNERCOMPANYID);

            if (TravelReimbursement_Golbal.REIMBURSEMENTOFCOSTS > 0 || fbCtr.Order.TOTALMONEY > 0)
            {
                if (TravelReimbursement_Golbal.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    Utility.SetAuditEntity(entity, "T_OA_TRAVELREIMBURSEMENT", TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID, strXmlObjectSource, paraIDs);
                }
                else
                {
                    Utility.SetAuditEntity(entity, "T_OA_TRAVELREIMBURSEMENT", TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID, strXmlObjectSource);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销费用不能为零，请填写报销费用!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                if (clickSubmit == true)
                {
                    RefreshUI(RefreshedTypes.All);
                    clickSubmit = false;
                }
                return;
            }
        }

        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (Common.CurrentLoginUserInfo.EmployeeID != TravelReimbursement_Golbal.OWNERID)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if ((formType == FormTypes.Resubmit || formType == FormTypes.Edit) && canSubmit == false)
            {
                //RefreshUI(RefreshedTypes.ShowProgressBar);
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            Utility.InitFileLoad(FormTypes.Audit, uploadFile, TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID, false);
            RefreshUI(RefreshedTypes.HideProgressBar);

            if (formType == FormTypes.Audit)
            {
                IsAudit = false;
            }
            if (formType == FormTypes.Resubmit)
            {
                Resubmit = false;
            }

            if (TravelReimbursement_Golbal.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }

            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中
                    state = Utility.GetCheckState(CheckStates.Approving);//提示提交成功
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"),
                        Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    textStandards.Text = string.Empty;//清空报销标准说明
                    OaPersonOfficeClient.GetTravelReimbursementByIdAsync(travelReimbursementID);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
                    state = Utility.GetCheckState(CheckStates.Approved);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"),
                        Utility.GetResourceStr("SUCCESSAUDIT"));//提示审核成功
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"),
                        Utility.GetResourceStr("SUCCESSAUDIT"));//提示审核成功
                    break;
            }
            TravelReimbursement_Golbal.CHECKSTATE = state;
            clickSubmit = false;
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            if (TravelReimbursement_Golbal.CHECKSTATE == Utility.GetCheckState(CheckStates.Approving))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
            }
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (TravelReimbursement_Golbal != null)
                state = TravelReimbursement_Golbal.CHECKSTATE;
            if (formType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion


        #region 查看时屏蔽控件(不需要调整时间时查看&审核时调用)
        private void BrowseShieldedControl()
        {
            txtTELL.IsReadOnly = true;
            fbChkBox.IsEnabled = false;
            fbCtr.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            textStandards.IsReadOnly = true;
            FormToolBar1.Visibility = Visibility.Collapsed;
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
        }
        #endregion

        #region 审核时屏蔽控件(需要调整出差时间时调用)
        private void AuditShieldedControl()
        {
            txtTELL.IsReadOnly = true;
            fbChkBox.IsEnabled = false;
            fbCtr.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            textStandards.IsReadOnly = true;
            FormToolBar1.Visibility = Visibility.Collapsed;
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
        }
        #endregion
    }
}
