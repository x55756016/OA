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

using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class AreaSortForm : BaseForm, IEntityEditor, IClient
    {
        public AreaSortForm()
        {
            InitializeComponent();
        }
        private bool sign = false;
        SmtOAPersonOfficeClient client;
        private FormTypes formType;
        private string citycode = string.Empty;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        System.Collections.ObjectModel.ObservableCollection<T_OA_AREACITY> citys = new System.Collections.ObjectModel.ObservableCollection<T_OA_AREACITY>();
        public T_OA_AREADIFFERENCE area { get; set; }
        private string areaID { get; set; }
        public T_OA_AREACITY areacity { get; set; }
        private T_OA_TRAVELSOLUTIONS solutionsObj = new T_OA_TRAVELSOLUTIONS();
        private string ExistedCode; //用于保存旧的城市代码
        private string AddCode; //用于增加的城市代码字串
        private string DelCode="";//用于删除的城市代码字串
        private bool hasSaved = true;//判断是否已经保存

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }


        public AreaSortForm(FormTypes type, string areaID, T_OA_TRAVELSOLUTIONS travelObj)
        {
            InitializeComponent();
            solutionsObj = travelObj;
            this.Loaded += (o, e) =>
            {
                #region 原来的

                InitParas();
                this.areaID = areaID;
                FormType = type;
                if (string.IsNullOrEmpty(areaID))
                {
                    area = new T_OA_AREADIFFERENCE();
                    area.AREADIFFERENCEID = Guid.NewGuid().ToString();
                    area.T_OA_TRAVELSOLUTIONS = solutionsObj;
                    area.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    area.CREATEDATE = System.DateTime.Now;
                    area.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    area.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    area.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    area.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                    area.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    area.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    area.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    area.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    area.UPDATEDATE = System.DateTime.Now;
                    area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    this.DataContext = area;
                }
                else
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    client.GetAreaCategoryByIDAsync(areaID);
                }
                #endregion
            };
        }

        private void InitParas()
        {
            areacity = new T_OA_AREACITY();
            client = new SmtOAPersonOfficeClient();
            client.GetAreaCategoryByIDCompleted += new EventHandler<GetAreaCategoryByIDCompletedEventArgs>(client_GetAreaCategoryByIDCompleted);
            client.AreaCategoryADDCompleted += new EventHandler<AreaCategoryADDCompletedEventArgs>(client_AreaCategoryADDCompleted);
            client.AreaCategoryUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCategoryUpdateCompleted);

            client.AreaCityCheckCompleted += new EventHandler<AreaCityCheckCompletedEventArgs>(client_AreaCityCheckCompleted);
            client.AreaCityByCategoryDeleteCompleted += new EventHandler<AreaCityByCategoryDeleteCompletedEventArgs>(client_AreaCityByCategoryDeleteCompleted);
            client.AreaCityAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCityAddCompleted);
            client.AreaCityLotsofAddCompleted += new EventHandler<AreaCityLotsofAddCompletedEventArgs>(client_AreaCityLotsofAddCompleted);
            client.GetAreaCityByCategoryCompleted += new EventHandler<GetAreaCityByCategoryCompletedEventArgs>(client_GetAreaCityByCategoryCompleted);
        }

        void client_AreaCityCheckCompleted(object sender, AreaCityCheckCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result.Count > 0)
                {
                    string resultStr = string.Empty;
                    Dictionary<string, string> tmp = e.Result;
                    foreach (var a in tmp.Keys)
                    {
                        resultStr += tmp[a].ToString() + Utility.GetResourceStr("MIDDLETYPES") + "[" + GetCityName(a) + "]\n";
                    }
                    resultStr += Utility.GetResourceStr("ALREADYEXISTNOTADD");
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(resultStr));
                }
                else
                {
                    if (e.UserState.ToString() != "Add")
                        client.AreaCityByCategoryDeleteAsync(area.AREADIFFERENCEID,DelCode);
                    else
                        client.AreaCityLotsofAddAsync(citys, "Add");
                }
            }
        }

        void client_AreaCityByCategoryDeleteCompleted(object sender, AreaCityByCategoryDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (string.IsNullOrEmpty(citycode))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CITY"));
                    return;
                }
                client.AreaCityLotsofAddAsync(citys, "Update");
            }
        }

        void client_GetAreaCityByCategoryCompleted(object sender, GetAreaCityByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    try
                    {
                        scSearchCity.TxtSelectedCity.Text = string.Empty;
                        foreach (var ent in e.Result.ToList())
                        {
                            scSearchCity.TxtSelectedCity.Text += GetCityName(ent.CITY) + " ";
                            citycode += ent.CITY + ",";
                        }
                        ExistedCode = citycode;
                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }
                    catch (Exception ex)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("获取城市出错"));
                    }
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
        }

        private string GetCityName(string cityvalue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CITY" && a.DICTIONARYVALUE == Convert.ToDecimal(cityvalue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        void client_AreaCityLotsofAddCompleted(object sender, AreaCityLotsofAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.UserState.ToString() == "Add")
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "AREADIFFERENCECATEGORY"));
                else
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "AREADIFFERENCECATEGORY"));
            }
            hasSaved = true;
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (sign)
            {
                sign = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        void client_AreaCityAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            //else
            //{
            //   // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CITY"));
            //}
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_AreaCategoryADDCompleted(object sender, AreaCategoryADDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == "SUCCESSED")
                {
                    client.AreaCityCheckAsync(citys, "Add", area.AREADIFFERENCEID);
                    //client.AreaCityLotsofAddAsync(citys,"Add");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }

            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_AreaCategoryUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //client.AreaCityByCategoryDeleteAsync(area.AREADIFFERENCEID, DelCode);
                client.AreaCityCheckAsync(citys, area.AREADIFFERENCEID, "Update");
            }
            if (sign)
            {
                sign = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        void client_GetAreaCategoryByIDCompleted(object sender, GetAreaCategoryByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                area = e.Result;
                this.DataContext = area;
                //加载城市
                client.GetAreaCityByCategoryAsync(area.AREADIFFERENCEID);
            }
        }

        public void Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //   Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            else
            {
                GetChangedList(citycode, ExistedCode);
                citys.Clear();
                //string[] codes = citycode.Split(',');
                //by luojie 增加城市组
                if (!string.IsNullOrEmpty(AddCode))
                {
                    string[] codes = AddCode.Split(',');
                    foreach (string code in codes)
                    {
                        if (!string.IsNullOrEmpty(code))
                        {
                            T_OA_AREACITY temp = new T_OA_AREACITY();
                            temp.AREACITYID = Guid.NewGuid().ToString();
                            temp.T_OA_AREADIFFERENCE = area;
                            //temp.T_OA_AREADIFFERENCE = new T_OA_AREADIFFERENCE();
                            //temp.T_OA_AREADIFFERENCE.AREADIFFERENCEID = area.AREADIFFERENCEID;
                            //temp.T_OA_AREADIFFERENCEReference.EntityKey = area.AREADIFFERENCEID;
                            temp.CITY = code;
                            citys.Add(temp);
                        }
                    }
                }
                if (AddCode != null || DelCode != null || formType==FormTypes.New)
                {
                    if (FormType == FormTypes.Edit)
                    {
                        area.UPDATEDATE = System.DateTime.Now;
                        area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        client.AreaCategoryUpdateAsync(area);
                    }
                    else
                    {
                        client.AreaCategoryADDAsync(area, solutionsObj.TRAVELSOLUTIONSID, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                    }
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("未作修改"));
                }
            }

        }

        public void Cancel()
        {
            Save();
            sign = true;
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("AREACATEGORY");
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
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = "详细信息",
            //    Tooltip = "详细信息"
            //};
            //items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            return items;
        }

        //public List<ToolbarItem> GetToolBarItems()
        //{
        //    return ToolbarItems;
        //}

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

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }


        void browser_ReloadDataEvent()
        {

        }

        private void scSearchCity_SelectClick(object sender, EventArgs e)
        {
            SearchCity txt = (SearchCity)sender;
            AreaSortCity SelectCity;
            if (hasSaved)
            {
                ExistedCode = citycode; //获取旧的城市代码
                hasSaved = false;
            }
            if (!string.IsNullOrEmpty(citycode))
                SelectCity = new AreaSortCity(citycode);
            else
                SelectCity = new AreaSortCity();

            SelectCity.SelectMulti = true;
            
            SelectCity.SelectedClicked += (obj, ea) =>
            {
                txt.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                citycode = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                if (string.IsNullOrEmpty(citycode))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTEDCITY"));
                }
                //if (citycode.Split(',').Count() > 2)
                //{
                //    txt.TxtSelectedCity.Text = "";
                //    citycode = "";
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "DEPARTURECITY"));
                //    return;
                //}
                //if (txt.Name == "txtDEPARTURECITY")
                //{
                //    buipList[DaGrs.SelectedIndex].DEPCITY = citycode;
                //}
                //foreach (Object obje in DaGrs.ItemsSource)//判断所选的出发城市是否与目标城市相同
                //{
                //    SearchCity City = ((SearchCity)((StackPanel)DaGrs.Columns[3].GetCellContent(obje)).Children.FirstOrDefault()) as SearchCity;
                //    if (City != null)
                //    {
                //        if (txt.TxtSelectedCity.Text == City.TxtSelectedCity.Text)
                //        {
                //            txt.TxtSelectedCity.Text = "";
                //            citycode = "";
                //            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTTHESAMEASTHETARGECITY", "DEPARTURECITY"));
                //            return;
                //        }
                //    }
                //}
            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("CITY"), "", "123", SelectCity, false, false, null);
            if (SelectCity is AreaSortCity)
            {
                (SelectCity as AreaSortCity).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
        }

        /// <summary>
        ///     by luojie 根据返回的字符串得出增加城市列表和删除城市列表
        /// </summary>
        /// <param name="CodeAfter">更改后的字符串</param>
        /// <param name="CodeBefore">更改前的字符串</param>
        /// <returns></returns>
        private bool GetChangedList(string CodeAfter, string CodeBefore)
        {
            DelCode = null;
            AddCode = null;
            string delCode = null, addCode = null;
            
            //获取删除列表        
            if (!string.IsNullOrEmpty(CodeBefore))
            {
                if (CodeBefore[0] != ',') CodeBefore = "," + CodeBefore;
                string[] ArrayBefore = CodeBefore.Split(',');
                foreach (string codeBf in ArrayBefore)
                {
                    if (!string.IsNullOrEmpty(codeBf) && CodeAfter.IndexOf(","+codeBf+",") < 0)
                    {
                        delCode += codeBf + ",";
                    }
                }
                DelCode = delCode;
            }
            //获取添加列表
            if (!string.IsNullOrEmpty(CodeAfter))
            {
                if (CodeAfter[0] != ',') CodeAfter = "," + CodeAfter;
                string[] ArrayAfter = CodeAfter.Split(',');
                foreach (string codeAf in ArrayAfter)
                {
                    if (!string.IsNullOrEmpty(codeAf) && CodeBefore.IndexOf(","+codeAf+",") < 0)
                    {
                        addCode += codeAf + ",";
                    }
                }
                AddCode = addCode;
            }
            return true;
        }

    }
}
