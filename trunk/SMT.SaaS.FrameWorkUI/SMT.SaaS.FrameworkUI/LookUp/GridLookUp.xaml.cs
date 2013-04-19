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

namespace SMT.SaaS.FrameworkUI
{
    public partial class GridLookUp : System.Windows.Controls.Window
    {
       // private object selectEntity;

        private LookUp lookup;

        public GridLookUp()
        {
            InitializeComponent();
           
        }

        public GridLookUp(LookUp obj)
        {
            this.lookup = obj;
            InitializeComponent();
        }
        #region "属性构造"
        /// <summary>
        /// 获取当前DataGrid
        /// </summary>
        public Button ButtonOK
        {
            get { return this.btnOK; }
        }


        /// <summary>
        /// 获取当前DataGrid
        /// </summary>
        public DataGrid DataGrid
        {
            get { return this.dataGrid; }
        }

        /// <summary>
        /// 获取当前DataGrid Footer
        /// </summary>
        public DataPager GridPageFooter
        {
            get { return this.lookUpPageFooter; }
        }
        #endregion
       
        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
          
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
        }
    }
}

