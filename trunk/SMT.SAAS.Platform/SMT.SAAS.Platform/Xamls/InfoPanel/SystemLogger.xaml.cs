using System.Windows.Controls;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 系统日志展示页面
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Xamls
{
    /// <summary>
    /// 系统日志展示页面
    /// </summary>
    public partial class SystemLogger : UserControl
    {
        /// <summary>
        /// 创建一个日志展示的新实例。
        /// </summary>
        public SystemLogger()
        {
            InitializeComponent();

            dgLogList.ItemsSource = null;
            dgLogList.ItemsSource = Logging.Logger.Current.Logs;
        }
    }
}
