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
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.FrameworkUI
{
    public partial class GridToolBar : UserControl
    {
        /// <summary>
        /// 获取当前的按钮容器
        /// </summary>
        public StackPanel ButtonContainer
        {
            get { return this.btnContainer; }
        }

        /// <summary>
        /// 获取当前的新增按钮
        /// </summary>
        public Button BtnAdd
        {
            get { return this.btnAdd; }
        }
        /// <summary>
        /// 获取当前的更新按钮
        /// </summary>
        public Button BtnAlter
        {
            get { return this.btnAlt; }
        }
        /// <summary>
        /// 获取当前Grid的删除按钮
        /// </summary>
        public Button BtnDelete
        {
            get { return this.btnDel; }
        }


        public GridToolBar()
        {
            InitializeComponent();
			
        }
    }
}
