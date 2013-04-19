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

using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.SaaS.FrameworkUI;

namespace SMT.HRM.UI
{
    public partial class LookupForm : System.Windows.Controls.Window
    {
        private OrganizationServiceClient client = new OrganizationServiceClient();

        public EntityNames EntityName { get; set; }
        public Type EntityType { get; set; }
        public Dictionary<string, string> ColumnNames { get; set; }
        public string OrderBy { get; set; }
        public string filter { get; set; }
        System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
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

            this.TitleContent = Utility.GetResourceStr(entName.ToString().ToUpper());
            BindData();
        }


        public LookupForm(EntityNames entName, Type type, Dictionary<string, string> cols, string filter, System.Collections.ObjectModel.ObservableCollection<object> paras)
        {
            EntityName = entName;
            EntityType = type;
            ColumnNames = cols;

            InitializeComponent();

            client.GetLookupOjbectsCompleted += new EventHandler<GetLookupOjbectsCompletedEventArgs>(client_GetLookupOjbectsCompleted);

            this.TitleContent = Utility.GetResourceStr(entName.ToString().ToUpper());
            this.filter = filter;
            this.paras = paras;
            BindData();
        }
        private IEnumerable dataSource;

        public IEnumerable DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }
        //private object localResource = new Resources.HRSystem();
        //public object LocalResource
        //{
        //    get { return localResource; }
        //    set { localResource = value; }
        //}

        void client_GetLookupOjbectsCompleted(object sender, GetLookupOjbectsCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            //绑定系统类型
            if (!string.IsNullOrEmpty(e.Result))
            {
                string rslt = e.Result.ToString();

                object objs = Utility.XmlToContractObject(rslt, EntityType);

                DtGrid.ItemsSource = (IEnumerable)objs;
                DataSource = (IEnumerable)objs;
                dataPager.PageCount = e.pageCount;
                DtGrid.Columns.Clear();
                if (ColumnNames != null && ColumnNames.Count > 0)
                {
                    //显示用户指定的列
                    foreach (var col in ColumnNames)
                    {
                        string colName = GetLocalName(col.Key);
                        if (!string.IsNullOrEmpty(colName))
                        {
                            DataGridTextColumn txtCol = new DataGridTextColumn();
                            txtCol.Header = colName;
                            txtCol.Binding = GetColumnBinding(col.Value);
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

        /// <summary>
        /// 对绑定值进行转换
        /// </summary>
        /// <param name="strColumnValue"></param>
        /// <returns></returns>
        private static Binding GetColumnBinding(string strColumnValue)
        {
            Binding binding = new Binding();
            if (string.IsNullOrWhiteSpace(strColumnValue))
            {
                binding = new Binding(strColumnValue);
                return binding;
            }

            string[] strlist = strColumnValue.Split(',');
            if (strlist.Length != 3)
            {
                binding = new Binding(strColumnValue);
                return binding;
            }

            switch (strlist[2].ToString().ToUpper())
            {
                case "DICTIONARYCONVERTER":
                    DictionaryConverter dic = new DictionaryConverter();
                    binding = new Binding(strlist[0].ToString()) { Converter = dic, ConverterParameter = strlist[1].ToString() };
                    break;
                case "CUSTOMDATECONVERTER":
                    CustomDateConverter dat = new CustomDateConverter();
                    binding = new Binding(strlist[0].ToString()) { Converter = dat, ConverterParameter = strlist[1].ToString() };
                    break;
            }

            return binding;
        }

        private string GetLocalName(string columnName)
        {
            string localName = string.Empty;
            localName = Utility.GetResourceStr(columnName);
            return localName;

        }
        private void BindData()
        {
            //client.GetLookupOjbectsAsync(EntityName, null);
            int pageCount = 0;
            //string filter = "";
            //System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            client.GetLookupOjbectsAsync(EntityName, dataPager.PageIndex, dataPager.PageSize, string.IsNullOrEmpty(OrderBy) ? "OWNERID" : OrderBy,
                filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, this.TitleContent.ToString(), Utility.GetResourceStr("PLEASESELECT", this.TitleContent.ToString()));
                return;
            }

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

        }
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindData();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClick != null)
                SelectedClick(null, e);
            this.Close();
        }
    }
}

