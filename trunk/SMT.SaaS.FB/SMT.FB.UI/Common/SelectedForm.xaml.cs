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
using SMT.FB.UI.FBCommonWS;
using System.Collections;
using SMT.SaaS.FrameworkUI;

namespace SMT.FB.UI.Common
{
    public partial class SelectedForm :System.Windows.Controls.Window
    {

        #region 属性
        private SelectedDataManager selectedDataManager;
        public SelectedDataManager SelectedDataManager
        {
            get
            {
                if (selectedDataManager == null)
                {
                    selectedDataManager = new SelectedDataManager();
                }
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

        private SelectorInfo SelectorInfo { get; set; }
        
        public string ReferenceDataType { get; set; }

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
        
        public DataGridSelectionMode SelectionMode { get; set; }
        #endregion

        public event EventHandler SelectedCompleted;

        private object currentItem;
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
        }

        private void InitData()
        {
            this.SelectedDataManager.QueryExpression.QueryType = this.SelectorInfo.Type;
            this.SelectedDataManager.GetUnSelectedItems();
        }

        private void InitControl()
        {
            

            this.SelectedGrid.GridItems = this.SelectorInfo.Items;
            this.SelectedGrid.InitControl(OperationTypes.Add);

        }

        private void selectedDataManager_GetUnSelectedItemsCompleted(object sender, ActionCompletedEventArgs<List<FBEntity>> e)
        {
            this.SelectedGrid.ItemsSource = e.Result;
            CloseProcess();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            this.SelectedItems = this.OriginalItems.FindAll(item =>
                {
                    return item.Actived;
                });

            if (SelectedCompleted != null)
            {
                SelectedCompleted(this, null);
            }
            // this.DialogResult = true;
            this.Close();

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //if (SelectedCompleted != null)
            //{
            //    SelectedFormEventArgs items = new SelectedFormEventArgs();
            //    items.SelectedItems = new List<FBEntity>();
            //    SelectedCompleted(this, items);
            //}

          //  this.DialogResult = false;
            this.Close();
        }


        #region Init
        public event EventHandler FBBasePageLoaded;

        void SelectedForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitProcess();
            ShowProcess();
            SelectorHelper.InitSelectorInfoCompleted += new EventHandler(SelectorHelper_InitSelectorInfoCompleted);
            SelectorHelper.InitSelectorInfo();
        }

        void SelectorHelper_InitSelectorInfoCompleted(object sender, EventArgs e)
        {
            SelectorHelper.InitSelectorInfoCompleted -= new EventHandler(SelectorHelper_InitSelectorInfoCompleted);
            this.SelectorInfo = SelectorHelper.GetSelectorInfo(this.ReferenceDataType);
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

        private void SelectedGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectionMode == DataGridSelectionMode.Single)
            {
                FBEntity old = currentItem as FBEntity;
                if (old != null)
                {
                    old.Actived = false;
                }

                FBEntity current = this.SelectedGrid.SelectedItem as FBEntity;
                if (current != null)
                {
                    current.Actived = true;
                    currentItem = current;
                }
            }
        }

    }

}

