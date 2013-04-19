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

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Browser;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.CommForm
{
    public partial class LookupForm : ChildWindow
    {
        public EntityNames EntityName { get; set; }
        public Type EntityType { get; set; }
        public Dictionary<string,string> ColumnNames { get; set; }
        private SmtOADocumentAdminClient client = new SmtOADocumentAdminClient();
        
        public object SelectedObj
        {
            get 
            {
                return DtGrid.SelectedItem;
            }
        }        
   
        public event EventHandler SelectedClick;
        public LookupForm(EntityNames entName, Type type, Dictionary<string, string> cols)
        {
            EntityName = entName;
            EntityType = type;
            ColumnNames = cols;
            
            InitializeComponent();
            client.GetLookupOjbectsCompleted += new EventHandler<GetLookupOjbectsCompletedEventArgs>(client_GetLookupOjbectsCompleted);

            BindData();
        }
        private IEnumerable dataSource;

        public IEnumerable DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }

        //private object localResource = new SMT.SaaS.OA.UI.Assets.Resources.OASystem();
        //public object LocalResource
        //{
        //    get { return localResource; }
        //    set { localResource = value; }
        //}

        void client_GetLookupOjbectsCompleted(object sender, GetLookupOjbectsCompletedEventArgs e)
        {
            //绑定系统类型
            if (!string.IsNullOrEmpty(e.Result))
            {
                string rslt = e.Result.ToString();

                object objs = Utility.XmlToContractObject(rslt, EntityType);

                DtGrid.ItemsSource = (IEnumerable)objs;
                DataSource = (IEnumerable)objs;


                if (ColumnNames != null && ColumnNames.Count > 0)
                {
                    //显示用户指定的列
                    DtGrid.Columns.Clear();    //add by zl
                    foreach (var col in ColumnNames)
                    {
                        //string colName = GetLocalName(col.Key, LocalResource);
                        string colName = GetLocalName(col.Key);
                        if (!string.IsNullOrEmpty(colName))
                        {
                            DataGridTextColumn txtCol = new DataGridTextColumn();
                            txtCol.Header = colName;
                            txtCol.Binding = new Binding(col.Value);
                            DtGrid.Columns.Add(txtCol);
                        }
                    }
                }
                else
                {
                    
                    //TODO:反射实体取得所有列的信息
                    //Type a = ((PagedCollectionView)dataSource).CurrentItem.GetType();
                    //PropertyInfo[] infos = a.GetProperties();
                    //DataGridTextColumn[] columns = new DataGridTextColumn[infos.Length];
                    //string bindColumnName = string.Empty;
                    //for (int i = 0; i < infos.Count(); i++)
                    //{
                    //    PropertyInfo pinfo = infos[i];
                    //    bindColumnName = GetLocalName(pinfo.Name, LocalResource);
                    //    if (!string.IsNullOrEmpty(bindColumnName))
                    //    {
                    //        columns[i] = new DataGridTextColumn();
                    //        columns[i].Header = bindColumnName;
                    //        columns[i].Binding = new Binding(pinfo.Name);
                    //        DtGrid.Columns.Add(columns[i]);
                    //    }
                    //}

                }

            }
            else
            {
                DtGrid.ItemsSource = null;
            }
        }

        private string GetLocalName(string columnName)
        {
            //string localName = string.Empty;
            //Type a = obj.GetType();
            //PropertyInfo[] infos = a.GetProperties();
            //for (int i = 0; i < infos.Count(); i++)
            //{
            //    PropertyInfo pinfo = infos[i];
            //    if (columnName == pinfo.Name)
            //    {
            //        localName = pinfo.GetValue(obj, null).ToString();
            //    }
            //}
            //return localName;

            string localName = string.Empty;
            localName = Utility.GetResourceStr(columnName);
            return localName;
        }
        private void BindData()
        {
            string StrLicens = "";
            StrLicens = this.txtLICENSENAME.Text.ToString().Trim();
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值 
            //filter += "licenseUser.CREATEUSERID==@" + paras.Count().ToString();
            //paras.Add(Common.CurrentLoginUserInfo.EmployeeID);

            if (!string.IsNullOrEmpty(StrLicens))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "LICENSENAME ^@" + paras.Count().ToString();
                paras.Add(StrLicens);
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;

            client.GetLookupOjbectsAsync(EntityName, ColumnNames, dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo.userID);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            
            if(SelectedClick!=null)
                SelectedClick(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            BindData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindData();
        }
    }
}

