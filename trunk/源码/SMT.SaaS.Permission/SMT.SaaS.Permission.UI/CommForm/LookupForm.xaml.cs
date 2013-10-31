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
using System.Collections;
using System.Reflection;
using System.Windows.Data;

using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.Permission.UI;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.Permission.UI
{
    public partial class LookupForm : System.Windows.Controls.Window
    {
        private PermissionServiceClient client = new PermissionServiceClient();

        public EntityNames EntityName { get; set; }
        public Type type { get; set; }
        public string[] ColumnNames { get; set; }
        //  public Dictionary<string, string> ColumnNames { get; set; }
        public Dictionary<string, string> para;
        string filter = "";
        System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
        public object SelectedObj
        {
            get
            {
                return DtGrid.SelectedItem;
            }
        }

        public event EventHandler SelectedClick;
        //public LookupForm(EntityNames entName, Type ttype, Dictionary<string, string> cols)
        //{
        //    EntityName = entName;
        //    type = ttype;
        //    ColumnNames = cols;

        //    InitializeComponent();
        //    client.GetLookupOjbectsCompleted += new EventHandler<GetLookupOjbectsCompletedEventArgs>(client_GetLookupOjbectsCompleted);
        //    TitleContent = "系统字典-查询窗口";
        //    BindData();
        //}
        public LookupForm(EntityNames entName, Type ttype, string[] cols, Dictionary<string, string> para)
        {
            EntityName = entName;
            type = ttype;
            ColumnNames = cols;
            this.para = para;
            InitializeComponent();
            client.GetLookupOjbectsCompleted += new EventHandler<GetLookupOjbectsCompletedEventArgs>(client_GetLookupOjbectsCompleted);

            BindData();
            //client.CloseAsync();//龙康才新增
            //client.Abort();//龙康才新增
        }
        private IEnumerable dataSource;

        public IEnumerable DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }
        //private object localResource = new Resources.Resource();
        //public object LocalResource
        //{
        //    get { return localResource; }
        //    set { localResource = value; }
        //}

        void client_GetLookupOjbectsCompleted(object sender, GetLookupOjbectsCompletedEventArgs e)
        {
            DtGrid.ItemsSource = null;
            if (e.Error != null && e.Error.Message != "")
            {

                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            //绑定系统类型
            if (e.Result != null)
            {
                string rslt = e.Result;
                if (string.IsNullOrEmpty(rslt)) return;
                var objs = Utility.DeserializeObject(rslt, type);

                //DtGrid.ItemsSource = (IEnumerable)objs;
                //DataSource = (IEnumerable)objs;
                paging((IEnumerable)objs);
                DtGrid.Columns.Clear();
                if (ColumnNames != null && ColumnNames.Length > 0)
                {
                    //显示用户指定的列
                    foreach (string col in ColumnNames)
                    {
                        string colName = GetLocalName(col);
                        if (!string.IsNullOrEmpty(colName))
                        {
                            DataGridTextColumn txtCol = new DataGridTextColumn();
                            txtCol.Header = colName;
                            txtCol.Binding = new Binding(col);
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
        }
        //分页
        void paging(IEnumerable objs)
        {
            PagedCollectionView pcv = null;
            if (objs != null)
            {
                pcv = new PagedCollectionView(objs);
                pcv.PageSize = 16;
            }
            dataPager.DataContext = pcv;
            DtGrid.ItemsSource = pcv;
            DataSource = pcv;

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
            // client.GetLookupOjbectsAsync(EntityName, para);
            client.GetLookupOjbectsAsync(EntityName, para, 1, 10, "DICTIONARYNAME", filter, paras, 0);
            //client.CloseAsync();//龙康才新增
            //client.Abort();//龙康才新增
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = true;
            this.Close();
            if (SelectedClick != null)
                SelectedClick(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            this.Close();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            paras.Clear();
            filter = string.Empty;
            //字典类型编码
            if (!string.IsNullOrEmpty(txtSearchSystemType.Text.Trim()))
            {
                filter = " @" + paras.Count.ToString() + ".Contains(DICTIONCATEGORY)";
                //filter = "DICTIONCATEGORY" + ".Contains(@" + paras.Count.ToString() + ")";
                paras.Add(txtSearchSystemType.Text.Trim());

            }
            //字典类型
            if (!string.IsNullOrEmpty(txtSearchSystemName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter = " @" + paras.Count.ToString() + ".Contains(DICTIONCATEGORYNAME)";
                //filter = "DICTIONCATEGORY" + ".Contains(@" + paras.Count.ToString() + ")";
                paras.Add(txtSearchSystemName.Text.Trim());
            }
            //字典名称
            if (!string.IsNullOrEmpty(txtSearchName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter = " @" + paras.Count.ToString() + ".Contains(DICTIONARYNAME)";
                //filter = "DICTIONCATEGORY" + ".Contains(@" + paras.Count.ToString() + ")";
                paras.Add(txtSearchName.Text.Trim());
            }
            BindData();

        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (SelectedClick != null)
                SelectedClick(sender, e);
        }
    }
}

