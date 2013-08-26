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
using SMT.Workflow.Platform.Designer.Form;

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class TriggerEdit : ChildWindow
    {
        public TriggerEdit()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

         //赋值参数注册事件
        private void ParamRows_OnSubmitComplated(object sender, DefineClickedArgs args)
        {
        }

        private void row_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmbModelCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void rbUser_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbSystem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rbService_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rbSMS_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void cmbParameter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmbFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSaveParam_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmbCycle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

