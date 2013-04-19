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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Controls.Primitives;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SolutionItemForm : BaseForm, IEntityEditor,IClient
    {
        private string SolutionItemID { get; set; }
        private FormTypes FormType { get; set; }
        private T_HR_SALARYSOLUTIONITEM salarySolutionItem { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        public SolutionItemForm(FormTypes formType, string ID)
        {
            InitializeComponent();
            FormType = formType;
          
            InitParas();
            if (FormType == FormTypes.Edit)
            {
                SolutionItemID = ID;
                lkSolutionItem.IsEnabled = false;
                client.GetSalarySolutionItemBysolutionItemIDAsync(SolutionItemID);
            }
            else if (FormType == FormTypes.New)
            {
                salarySolutionItem = new T_HR_SALARYSOLUTIONITEM();
                salarySolutionItem.SOLUTIONITEMID = Guid.NewGuid().ToString();
                salarySolutionItem.T_HR_SALARYSOLUTION = new T_HR_SALARYSOLUTION();
                salarySolutionItem.T_HR_SALARYSOLUTION.SALARYSOLUTIONID = ID;
            }
        }

        void InitParas()
        {
            client.GetSalarySolutionItemBysolutionItemIDCompleted += new EventHandler<GetSalarySolutionItemBysolutionItemIDCompletedEventArgs>(client_GetSalarySolutionItemBysolutionItemIDCompleted);
            client.SalarySolutionItemUpdateCompleted += new EventHandler<SalarySolutionItemUpdateCompletedEventArgs>(client_SalarySolutionItemUpdateCompleted);
            client.SalarySolutionItemAddCompleted += new EventHandler<SalarySolutionItemAddCompletedEventArgs>(client_SalarySolutionItemAddCompleted);
        }

       


        #region  完成事件
        void client_GetSalarySolutionItemBysolutionItemIDCompleted(object sender, GetSalarySolutionItemBysolutionItemIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    salarySolutionItem = e.Result;
                    this.DataContext = salarySolutionItem;
                    lkSolutionItem.DataContext = salarySolutionItem.T_HR_SALARYITEM;
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                }
            }

        }
        void client_SalarySolutionItemUpdateCompleted(object sender, SalarySolutionItemUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != "SAVESUCCESSED")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYITEM"));
                }
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            RefreshUI(RefreshedTypes.All);
        }
        void client_SalarySolutionItemAddCompleted(object sender, SalarySolutionItemAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != "SAVESUCCESSED")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYITEM"));
                }
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            RefreshUI(RefreshedTypes.All);
        }
        #endregion
        #region 保存
   
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if(string.IsNullOrEmpty(lkSolutionItem.TxtLookUp.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYITEMNAME"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            if (FormType == FormTypes.Edit)
            {
                if (salarySolutionItem != null)
                {
                    client.SalarySolutionItemUpdateAsync(salarySolutionItem);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
                }
                
            }
            else
            {
                  client.SalarySolutionItemAddAsync(salarySolutionItem);
            }


            return true;
        }
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void SaveAndClose()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            string filter = "";
            ObservableCollection<object> paras = new ObservableCollection<object>();
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("SALARYITEMNAME", "SALARYITEMNAME");
            filter += "CREATECOMPANYID==@" + paras.Count().ToString();
            paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            LookupForm lookup = new LookupForm(EntityNames.SalaryItemSet,
                typeof(List<T_HR_SALARYITEM>), cols, filter, paras);
            lookup.TitleContent = Utility.GetResourceStr("SALARYITEM");

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SALARYITEM ent = lookup.SelectedObj as T_HR_SALARYITEM;
                if (ent != null)
                {
                    salarySolutionItem.T_HR_SALARYITEM = new T_HR_SALARYITEM();
                    salarySolutionItem.T_HR_SALARYITEM.SALARYITEMID = ent.SALARYITEMID;
                    lkSolutionItem.DataContext = ent;
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYITEMNAME");
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
                    SaveAndClose();
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
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
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

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

      
    }
}
