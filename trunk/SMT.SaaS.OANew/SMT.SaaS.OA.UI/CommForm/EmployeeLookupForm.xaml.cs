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

using SMT.Saas.Tools.OrganizationWS;
using System.Windows.Browser;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.CommForm
{
    public partial class EmployeeLookupForm : ChildWindow
    {
        public EntityNames EntityName { get; set; }
        public Type EntityType { get; set; }
        public Dictionary<string,string> ColumnNames { get; set; }
        public string OrderBy { get; set; }
        private OrganizationServiceClient client = new OrganizationServiceClient();
        //private OrganizationServiceClient client = new OrganizationServiceClient();
        public object SelectedObj
        {
            get 
            {
                return DtGrid.SelectedItem;
            }
        }        
   
        public event EventHandler SelectedClick;
        public EmployeeLookupForm(EntityNames entName, Type type, Dictionary<string, string> cols)
        {
            EntityName = entName;
            EntityType = type;
            ColumnNames = cols;
            
            InitializeComponent();
            client.GetLookupOjbectsCompleted += new EventHandler<GetLookupOjbectsCompletedEventArgs>(client_GetLookupOjbectsCompleted);

            BindData();
        }

        void client_GetLookupOjbectsCompleted(object sender, GetLookupOjbectsCompletedEventArgs e)
        {
            
                if (!string.IsNullOrEmpty(e.Result))
                {
                    string rslt = e.Result.ToString();

                    object objs = Utility.XmlToContractObject(rslt, EntityType);

                    DtGrid.ItemsSource = (IEnumerable)objs;
                    DataSource = (IEnumerable)objs;


                    if (ColumnNames != null && ColumnNames.Count > 0)
                    {
                        //显示用户指定的列
                        foreach (var col in ColumnNames)
                        {
                            string colName = GetLocalName(col.Key, LocalResource);
                            DataGridTextColumn txtCol = new DataGridTextColumn();
                            if (!string.IsNullOrEmpty(colName))
                            {
                                txtCol.Header = colName;
                            }
                            else
                                txtCol.Header = col.Key;
                            txtCol.Binding = new Binding(col.Value);
                            DtGrid.Columns.Add(txtCol);
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
        }
        private IEnumerable dataSource;

        public IEnumerable DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }
        private object localResource = new SMT.SaaS.OA.UI.Assets.Resources.OASystem();
        public object LocalResource
        {
            get { return localResource; }
            set { localResource = value; }
        }



        private string GetLocalName(string columnName, object obj)
        {
            string localName = string.Empty;
            Type a = obj.GetType();
            PropertyInfo[] infos = a.GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                PropertyInfo pinfo = infos[i];
                if (columnName == pinfo.Name)
                {
                    localName = pinfo.GetValue(obj, null).ToString();
                }
            }
            return localName;

        }
        private void BindData()
        {
            //client.GetLookupOjbectsAsync(EntityName, null);
            int pageCount = 0;
            string filter = "";
            OrderBy = "CREATEUSERID";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            client.GetLookupOjbectsAsync(EntityName, dataPager.PageIndex, dataPager.PageSize, string.IsNullOrEmpty(OrderBy) ? "OWNERID" : OrderBy,
                filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)//确定
        {
            this.DialogResult = true;
            
            if(SelectedClick!=null)
                SelectedClick(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)//取消
        {
            this.DialogResult = false;
        }
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

            BindData();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)//清空
        {

        }
    }
}

