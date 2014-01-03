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
using SMT.Saas.Tools.FBServiceWS;
using System.Xml.Linq;
using System.Reflection;
using System.Globalization;

namespace SMT.SaaS.FrameworkUI.FBControls
{
public partial class SelectedForm : System.Windows.Controls.Window
    {
        private FBDataGrid fbDataGrid = null;

        public FBDataGrid FBdataGrid
        {
            get { return fbDataGrid; }
            set { fbDataGrid = value; }
        }


        #region 属性
        private SelectedDataManager selectedDataManager;
        public SelectedDataManager SelectedDataManager
        {
            get
            {
                return selectedDataManager;
            }
            set
            {
                if (!object.Equals(selectedDataManager, value))
                {
                    selectedDataManager = value;
                    selectedDataManager.GetUnSelectedItemsCompleted +=
                        new EventHandler<ActionCompletedEventArgs<List<FBEntity>>>(selectedDataManager_GetUnSelectedItemsCompleted);
                }
            }
        }

        public QueryExpression Query { get; set; }

        public string QueryType { get; set; }

        public List<FBEntity> OriginalItems
        {

            get
            {
                return SelectedDataManager.OriginalItems;
            }
            set
            {
                SelectedDataManager.OriginalItems = value;
            }

        }

        public List<FBEntity> SelectedItems
        {
            get
            {
                return SelectedDataManager.SelectedItems;
            }
            set
            {
                SelectedDataManager.SelectedItems = value;
            }
        }

        #endregion

        public event EventHandler SelectedCompleted;

        
        public SelectedForm()
        {

            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SelectedForm_Loaded);
            FBBasePageLoaded += new EventHandler(SelectedForm_FBBasePageLoaded);
        }

        private void SelectedForm_FBBasePageLoaded(object sender, EventArgs e)
        {
            InitData();
            InitControl();
            FBdataGrid = this.SelectedGrid;
        }

        private void InitData()
        {
            SelectedDataManager.GetUnSelectedItems();
        }

        private void InitControl()
        {
            this.SelectedGrid.GridItems = GetItems();
            this.SelectedGrid.InitControl();
        }

        private void selectedDataManager_GetUnSelectedItemsCompleted(object sender, ActionCompletedEventArgs<List<FBEntity>> e)
        {
            this.SelectedGrid.ItemsSource = e.Result;
            this.OriginalItems = e.Result;
            CloseProcess();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            this.SelectedItems = this.OriginalItems.Where(item =>
                {
                    return item.Actived;
                }).ToList();

            if (SelectedCompleted != null)
            {
                SelectedCompleted(this, null);
            }
            //this.DialogResult = true;
            this.Close();

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
           // this.DialogResult = false;
        }


        #region Init

        public List<FBDataGrid.FBGridItem> GetItems()
        {
            string xmlDoc = @"<?xml version=""1.0"" encoding=""utf-8"" ?><GridItems>
                <GridItem PropertyDisplayName="""" PropertyName=""Actived"" Width=""50"" CType=""6"" />
                <GridItem PropertyDisplayName=""科目编码"" PropertyName=""Entity.T_FB_SUBJECT.SUBJECTCODE"" Width=""75"" IsReadOnly=""true"" />
                <GridItem PropertyDisplayName=""预算项目"" PropertyName=""Entity.T_FB_SUBJECT.SUBJECTNAME"" Width=""150"" IsReadOnly=""true""/>
                <GridItem PropertyDisplayName=""可用结余"" PropertyName=""Entity.USABLEMONEY"" Width=""75"" IsReadOnly=""true"" />
            </GridItems>";
            XElement xml = XElement.Parse(xmlDoc);

            XElement xElement = xml;
            List<FBDataGrid.FBGridItem> list = new List<FBDataGrid.FBGridItem>();
            foreach (XElement xeItem in xElement.Elements("GridItem"))
            {
                FBDataGrid.FBGridItem gridItem = new FBDataGrid.FBGridItem();
                Type type = typeof(FBDataGrid.FBGridItem);
                xeItem.Attributes().ForEach(item =>
                {
                    PropertyInfo p = type.GetProperty(item.Name.LocalName);
                    if (p != null)
                    {
                        object v = item.Value.ConvertOrNull(p.PropertyType, null, null, DateTimeStyles.None, null);
                        p.SetValue(gridItem, v, null);
                    }

                });
                list.Add(gridItem);
            }
            return list;

        }

        public event EventHandler FBBasePageLoaded;

        void SelectedForm_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("SelectedForm_Loaded");
            InitProcess();
            ShowProcess();
            if (this.FBBasePageLoaded != null)
            {
                this.FBBasePageLoaded(this, null);
            }
        }

        void SelectorHelper_InitSelectorInfoCompleted(object sender, EventArgs e)
        {
            if (FBBasePageLoaded != null)
            {
                FBBasePageLoaded(this, null);
            }
            
        }

        SMTLoading loadbar = null;
        private void InitProcess()
        {
            Panel parent = this.Content as Panel;
            if (parent != null)
            {
                Grid g = new Grid();

                this.Content = g;
                g.Children.Add(parent);
                loadbar = new SMTLoading(); //全局变量
                g.Children.Add(loadbar);
            }
        }
        public void ShowProcess()
        {
            if (loadbar != null)
            {
                loadbar.Start();//调用服务时写
            }
        }
        public void CloseProcess()
        {
            if (loadbar != null)
            {
                loadbar.Stop();
            }

        }

        #endregion
    }


          

}

