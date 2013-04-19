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
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.FlowWFService;
using SMT.SAAS.Main.CurrentContext;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ApprovalTypeList : System.Windows.Controls.Window
    {
        private const int wpheight = 20;
        private string strCode = string.Empty;
        public Dictionary<string, string> Result { get; set; }
        public event EventHandler SelectedClicked;
        public event EventHandler Close;
        private string StrSelectedPost = "";
        private List<T_SYS_DICTIONARY> ListDicts = new List<T_SYS_DICTIONARY>();
        private List<T_SYS_DICTIONARY> ListSelectedDicts = new List<T_SYS_DICTIONARY>();
        ObservableCollection<string> lsApprovalTypeValues = new ObservableCollection<string>();//用户有的事项审批的类型
        //Dictionary<int, string> DtFlow = new Dictionary<int, string>();
        string StrCompanyid = "";//公司id
        string StrDepartmentid = "";//部门ID
        string StrFlows = "";
        string SelectDictid = "";
        string XmlString = "";
        public ApprovalTypeList()
        {
            InitializeComponent();
            LoadUIData("", "");
            Result = new Dictionary<string, string>();
           
        }
        public ApprovalTypeList(string SelectedPost, string PostValue, ObservableCollection<string> lsapprovalids,string companyid,string departmentid,string xml)
        {
            InitializeComponent();
            StrCompanyid = companyid;
            StrDepartmentid = departmentid;
            lsApprovalTypeValues = lsapprovalids;
            BindApprovalTree();
            LoadUIData(SelectedPost, PostValue);
            Result = new Dictionary<string, string>();
            GetFlows();
            XmlString = xml;
        }

        private void LoadUIData(string StrPost, string PostValue)
        {


            if (!string.IsNullOrEmpty(StrPost))
                CheckedApproval.Text = StrPost ;
            if (!string.IsNullOrEmpty(PostValue))
                strCode = PostValue + ",";//加上，还原原来的字符串
            

        }

        #region 事项审批类型树

        private void BindApprovalTree()
        {
            if (Application.Current.Resources.Contains("SYS_DICTIONARY"))
            {

                BindApproval();
            }
        }

        private void BindApproval()
        {
            treeApproval.Items.Clear();
            List<T_SYS_DICTIONARY> Dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;



            if (ListDicts == null)
            {
                return;
            }

            List<T_SYS_DICTIONARY> TopApproval = new List<T_SYS_DICTIONARY>();
            List<T_SYS_DICTIONARY> SecondApproval = new List<T_SYS_DICTIONARY>();
            List<T_SYS_DICTIONARY> ThreeApproval = new List<T_SYS_DICTIONARY>();
            var ents = from p in Dicts
                       where p.DICTIONCATEGORY == "TYPEAPPROVAL"
                       orderby p.ORDERNUMBER
                       select p;
            ListDicts = ents.Count() > 0 ? ents.ToList() : null;
            if (ListDicts != null)
            {

                foreach (T_SYS_DICTIONARY dict in ListDicts)
                {
                    if (dict.T_SYS_DICTIONARY2 == null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = dict.DICTIONARYNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = dict;
                        item.Tag = dict;

                        
                        if (lsApprovalTypeValues.Contains(dict.DICTIONARYVALUE.ToString()) )
                        {
                            treeApproval.Items.Add(item);
                            TopApproval.Add(dict);
                        }
                    }
                }
                if (TopApproval.Count() > 0)
                {
                    foreach (var topApp in TopApproval)
                    {
                        //存在子记录的则添加子节点
                        List<T_SYS_DICTIONARY> lstDict = new List<T_SYS_DICTIONARY>();
                        var lsents = from ent in ListDicts
                                     where ent.T_SYS_DICTIONARY2 != null && ent.T_SYS_DICTIONARY2.DICTIONARYID == topApp.DICTIONARYID
                                     select ent;
                        if (lsents.Count() > 0)
                        {
                            lstDict = lsents.ToList();
                            TreeViewItem parentItem = GetApprovalParentItem(topApp.DICTIONARYID);
                            
                            AddApprovalNode(lstDict, parentItem);
                        }


                    }

                }


            }


        }



        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private TreeViewItem GetApprovalParentItem(string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeApproval.Items)
            {
                tmpItem = GetApprovalParentItemFromChild(item, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetApprovalParentItemFromChild(TreeViewItem item, string parentID)
        {
            TreeViewItem tmpItem = null;
            T_SYS_DICTIONARY tmpDict = item.DataContext as T_SYS_DICTIONARY;
            if (tmpDict != null)
            {
                if (tmpDict.DICTIONARYID == parentID)
                    return item;
            }

            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (TreeViewItem childitem in item.Items)
                {
                    tmpItem = GetApprovalParentItemFromChild(childitem, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }

        private void AddApprovalNode(List<T_SYS_DICTIONARY> lsDict, TreeViewItem FatherNode)
        {
            //绑定事项审批的子项目
            foreach (var childDict in lsDict)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childDict.DICTIONARYNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childDict;
                item.Tag = childDict;
                
                if (lsApprovalTypeValues.Contains(childDict.DICTIONARYVALUE.ToString()))
                {
                    if(FatherNode != null)
                        FatherNode.Items.Add(item);
                }

                if (lsDict.Count() > 0)
                {
                    List<T_SYS_DICTIONARY> Dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;


                    
                    var ents = from p in Dicts
                       where p.DICTIONCATEGORY == "TYPEAPPROVAL"
                       orderby p.ORDERNUMBER
                       select p;
                    ListDicts = ents.Count() > 0 ? ents.ToList() : null;
                    if (ListDicts == null)
                    {
                        return;
                    }

                    
                    //存在子记录的则添加子节点
                    List<T_SYS_DICTIONARY> lstDict = new List<T_SYS_DICTIONARY>();
                    var lsents = from ent in ListDicts
                                    where ent.T_SYS_DICTIONARY2 != null && ent.T_SYS_DICTIONARY2.DICTIONARYID == childDict.DICTIONARYID
                                    && lsApprovalTypeValues.Contains(ent.DICTIONARYVALUE.ToString())
                                    select ent;
                        
                    if (lsents.Count() > 0)
                    {
                        
                        lstDict = lsents.ToList();
                        TreeViewItem parentItem = GetApprovalParentItem(childDict.DICTIONARYID);
                        if(parentItem != null)
                        {
                            AddApprovalNode(lstDict, parentItem);

                        }
                    }


                    
                }
            }

        }

        #endregion

        #region 确定按钮

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(strCode))
            {
                ComfirmWindow.ConfirmationBoxs(SMT.SaaS.FrameworkUI.Common.Utility.GetResourceStr("ERROR"), "事项审批类型不能为空",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            //存在子项目的
            var ents = from ent in ListDicts
                       where ent.T_SYS_DICTIONARY2 != null && ent.T_SYS_DICTIONARY2.DICTIONARYID == SelectDictid
                       select ent;
            //根目录但没子记录
            //var parentents = from ent in ListDicts
            //                 where ent.DICTIONARYID == SelectDictid && ent.T_SYS_DICTIONARY2Reference.EntityKey == null
            //                 select ent;

            if (ents.Count() > 0 )
            {
                ComfirmWindow.ConfirmationBoxs(SMT.SaaS.FrameworkUI.Common.Utility.GetResourceStr("ERROR"), "该项目下有子项目，请选择子类型",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            Result.Add(CheckedApproval.Text.ToString(), strCode);
            if (SelectedClicked != null)
            {
                SelectedClicked(this, e);
            }
            if (this.Close != null)
                Close(this, EventArgs.Empty);
        }
        #endregion

        #region 取消按钮


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CheckedApproval.Text = strCode = string.Empty;
            if (this.Close != null)
                Close(this, EventArgs.Empty);
        }
        #endregion

        #region 事项审批树改变事件

        private void treeApproval_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            CheckedApproval.Text = "";
            strCode = "";
            if (treeApproval.SelectedItem != null)
            {
                T_SYS_DICTIONARY selectdic = new T_SYS_DICTIONARY();
                TreeViewItem selectedItemapp = treeApproval.SelectedItem as TreeViewItem;
                selectdic = selectedItemapp.Tag as T_SYS_DICTIONARY;
                if (selectdic != null)
                {
                    
                    CheckedApproval.Text = selectdic.DICTIONARYNAME;
                    strCode = selectdic.DICTIONARYVALUE.ToString();
                    SelectDictid = selectdic.DICTIONARYID;
                    ShowApprovalTypeFlow(selectdic.DICTIONARYVALUE.ToString());
                }
            }
        }
        #endregion

        #region 显示相应的流程
        private void GetFlows()
        {
            ServiceClient Flow = new ServiceClient();

            SMT.Saas.Tools.FlowWFService.SubmitData SubmitData = new SMT.Saas.Tools.FlowWFService.SubmitData();
            SubmitData.FlowSelectType = SMT.Saas.Tools.FlowWFService.FlowSelectType.FixedFlow;
            //SubmitData.FormID = "Test02";
            SubmitData.ModelCode = "T_OA_APPROVALINFO";
            SubmitData.ApprovalUser = new SMT.Saas.Tools.FlowWFService.UserInfo();
            SubmitData.ApprovalUser.CompanyID = StrCompanyid;

            SubmitData.ApprovalUser.DepartmentID = StrDepartmentid;
            //SubmitData.ApprovalUser.PostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            //SubmitData.ApprovalUser.UserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserP;
            //SubmitData.ApprovalUser.UserName = "杜春雷";
            //SubmitData.ApprovalContent = "sgsg";
            //SubmitData.NextStateCode = "";

            //SubmitData.NextApprovalUser = new SMT.Saas.Tools.FlowWFService.UserInfo();
            //SubmitData.NextApprovalUser.CompanyID = "7cd6c0a4-9735-476a-9184-103b962d3383";
            //SubmitData.NextApprovalUser.DepartmentID = "a907b9ba-179d-44e6-8aae-6883cf29f8d0";
            //SubmitData.NextApprovalUser.PostID = "d19517f2-efc3-417b-b154-b9b46bcfec75";
            //SubmitData.NextApprovalUser.UserID = "";
            //SubmitData.NextApprovalUser.UserName = "";
            //SubmitData.SubmitFlag = SMT.Saas.Tools.FlowWFService.SubmitFlag.New;
            // SubmitData.SubmitFlag = Flow.SubmitFlag.Approval;
            SubmitData.XML = "";
            SubmitData.FlowType = SMT.Saas.Tools.FlowWFService.FlowType.Approval;

            SubmitData.ApprovalResult = SMT.Saas.Tools.FlowWFService.ApprovalResult.Pass;
            //SubmitData.ApprovalContent = "审核通过";
            Flow.GetFlowDefineCompleted += new EventHandler<GetFlowDefineCompletedEventArgs>(Flow_GetFlowDefineCompleted);
            Flow.GetFlowDefineAsync(SubmitData);


            //string ttt = "<?xml version=\"1.0\" encoding=\"utf-8\"  ?>\r\n<WorkFlow><System>OA</System> \r\n<Activitys>\r\n<Activity  Name=\"StartFlow\" X=\"-3\" Y=\"90\" RoleName=\"\" UserType=\"\" Remark=\"\">\r\n</Activity>\r\n<Activity  Name=\"EndFlow\" X=\"397\" Y=\"85\" RoleName=\"\" UserType=\"\" Remark=\"\">\r\n</Activity>\r\n<Activity  Name=\"Statedefc214506c847ae8896f583ff225d6d\" X=\"125\" Y=\"43\" RoleName=\"Superior\" UserType=\"CREATEUSER\" Remark=\"直接上级\">\r\n</Activity>\r\n<Activity  Name=\"Statecc20ed7dd45249c59b00fbef9544eaf2\" X=\"254\" Y=\"34\" RoleName=\"DepartHead\" UserType=\"CREATEUSER\" Remark=\"部门负责人\">\r\n</Activity>\r\n<Activity  Name=\"Statec93a72329894474a8a3ff26df46ce961\" X=\"208\" Y=\"132\" RoleName=\"644b87e7-e2c3-440f-925e-86aaefad3a11\" UserType=\"CREATEUSER\" Remark=\"人力资源部部长\">\r\n</Activity>\r\n<Activity  Name=\"Stated8eb91a6b907495f96c98e71b0934223\" X=\"295\" Y=\"132\" RoleName=\"d5066d91-692b-4c32-b027-a8572f112c64\" UserType=\"CREATEUSER\" Remark=\"总裁\">\r\n</Activity>\r\n<Activity  Name=\"State401f0615af38460d9956126ab5a7274a\" X=\"116\" Y=\"146\" RoleName=\"Superior\" UserType=\"CREATEUSER\" Remark=\"直接上级2\">\r\n</Activity>\r\n</Activitys>\r\n<Rules>\r\n<Rule  Name=\"9df78e23-4772-46b0-bc19-79dc73fccccc\" StrStartActive=\"StartFlow\" StrEndActive=\"Statedefc214506c847ae8896f583ff225d6d\">\r\n</Rule>\r\n<Rule  Name=\"52f3da1d-2c4c-499b-a4d6-43dc8ddb8842\" StrStartActive=\"Statedefc214506c847ae8896f583ff225d6d\" StrEndActive=\"Statecc20ed7dd45249c59b00fbef9544eaf2\">\r\n</Rule>\r\n<Rule  Name=\"80848651-f709-4757-bc5e-0067d15a3c53\" StrStartActive=\"Statecc20ed7dd45249c59b00fbef9544eaf2\" StrEndActive=\"EndFlow\">\r\n</Rule>\r\n<Rule  Name=\"135e843f-3fcf-477a-8e52-ab24982d02ad\" StrStartActive=\"StartFlow\" StrEndActive=\"State401f0615af38460d9956126ab5a7274a\">\r\n<Conditions  Name=\"62870a18-947b-43fd-adc9-e58abbba7767\" Object=\"ApprovalForm\" CodiCombMode=\"AND\"><Condition  Name=\"4cdbfb19-7433-43b9-a962-9267efa13fbe\" Description=\"事项类型\" CompAttr=\"TYPEAPPROVAL\" DataType=\"string\" Operate=\"==\" CompareValue=\"2\"></Condition></Conditions></Rule>\r\n<Rule  Name=\"b29252b8-1bd8-4716-a4b1-ce62704a84ed\" StrStartActive=\"State401f0615af38460d9956126ab5a7274a\" StrEndActive=\"Statec93a72329894474a8a3ff26df46ce961\">\r\n</Rule>\r\n<Rule  Name=\"bd1d1e82-d28b-4da2-b087-bcc9758ab31e\" StrStartActive=\"Statec93a72329894474a8a3ff26df46ce961\" StrEndActive=\"Stated8eb91a6b907495f96c98e71b0934223\">\r\n</Rule>\r\n<Rule  Name=\"95a6f800-4da1-464c-90a9-39cfdea4834e\" StrStartActive=\"Stated8eb91a6b907495f96c98e71b0934223\" StrEndActive=\"EndFlow\">\r\n</Rule>\r\n</Rules>\r\n</WorkFlow>";
           
        }

        void Flow_GetFlowDefineCompleted(object sender, GetFlowDefineCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if(e.Result !=null)
                    StrFlows = e.Result;
            }
            else
            {
                ComfirmWindow.ConfirmationBox("警告", "没获取到相关审核流程!", Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            
            
        }

        private void ShowApprovalTypeFlow(string TypeValue)
        {
            StringBuilder xml = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            xml.Append(Environment.NewLine);
            xml.Append(@"    <System>");
            xml.Append(Environment.NewLine);
            xml.Append(@"       <Name>OA</Name>");
            xml.Append(Environment.NewLine);
            xml.Append(@"       <Object Name=""Test"">");
            xml.Append(Environment.NewLine);
            //xml.Append(@"           <Attribute Name=""TYPEAPPROVAL""  DataType=""string"" DataValue="""+TypeValue+ @"""></Attribute>");
            //xml.Append(Environment.NewLine);
            //xml.Append(@"           <Attribute  Name=""Price""  DataType=""decimal"" DataValue=""1""></Attribute>");
            //xml.Append(@"       </Object>");
            //xml.Append(Environment.NewLine);
            //xml.Append(@"</System>");
            //XmlString

            Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(XmlString);
            XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
            var eGFunc = from ent in xele.Descendants("Attribute")
                         select ent;
            foreach (var StrElement in eGFunc)
            {
                
                if (StrElement.Attribute("Name").Value == "TYPEAPPROVAL")
                {
                    StrElement.Attribute("DataValue").Value = TypeValue.ToString();
                }
                xml.Append(StrElement);

            }
            xml.Append(@"       </Object>");
            xml.Append(Environment.NewLine);
            xml.Append(@"</System>");



            if (!string.IsNullOrEmpty(StrFlows))
            {
                Dictionary<string, string> FlowListDict = new Dictionary<string, string>();
                if (ListDicts.Count() > 0)
                {
                    FlowListDict.Clear();
                    foreach (T_SYS_DICTIONARY dict in ListDicts)
                    {
                        string StrValue ="";
                        if (dict.DICTIONARYVALUE != null)
                        {
                            StrValue = dict.DICTIONARYVALUE.ToString();
                            if (!string.IsNullOrEmpty(StrValue))
                            {

                                var ents = from ent in FlowListDict.Keys
                                           where ent == StrValue
                                           select ent;
                                if (ents.Count() == 0)
                                {
                                    FlowListDict.Add(dict.DICTIONARYVALUE.ToString(), dict.DICTIONARYNAME);
                                }
                            }
                        }
                          
                                              
                    }
                }
                else
                {
                    return;
                }
                string StrShowFlow = SMT.SaaS.FrameworkUI.Common.Utility.GetFlowPathByTypeApprovalValue(StrFlows, TypeValue, FlowListDict);
                if (string.IsNullOrEmpty(StrShowFlow))
                {
                    tblFlow.Text = "没有流程";
                }
                else
                {
                    tblFlow.Text = StrShowFlow;
                }
                //var Dic = SMT.SaaS.FrameworkUI.Common.Utility.GetFlowListResult(StrFlows, xml.ToString());

                //if (Dic != null)
                //{
                //    if (Dic.Count > 0)
                //    {
                //        string StrResult = "";
                //        Dic.ToList().ForEach(item =>
                //        {
                //            StrResult += item.Value + "->";
                //        });
                //        if (!string.IsNullOrEmpty(StrResult))
                //            tblFlow.Text = StrResult.Substring(0, StrResult.Length - 2);
                //    }
                //}
                //else
                //{
                //    tblFlow.Text = "没有流程";
                //}
            }
            else
            {
                tblFlow.Text = "没有流程";
            }
        }
        #endregion
    }
}
