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

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 系统设置页面
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    /// <summary>
    /// 系统设置页面
    /// </summary>
    public partial class SysCofigSet : UserControl,ICleanup
    {
        
        public SysCofigSet()
        {
            InitializeComponent();
        }

        public void Cleanup()
        {
            //MessageBox.Show("Cleanup : SysCofigSet");
        }
    }
}
