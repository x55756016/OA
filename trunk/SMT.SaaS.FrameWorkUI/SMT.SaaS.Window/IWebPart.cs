using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SaaS.Platform
{
    /// <summary>
    /// 平台WebPart接口.
    /// 若需要设置当前控件为webpart 可实现此接口
    /// 将控件位置进行配置即可
    /// </summary>
    public interface IWebPart
    {
        /// <summary>
        ///WebPart刷新时间
        /// </summary>
        int RefreshTime { get; set; }

        /// <summary>
        /// webPart初始显示数据行数
        /// </summary>
        int RowCount { get; set; }

        /// <summary>
        /// webPart最大化时执行此方法
        /// </summary>
        void ShowMaxiWebPart();

        /// <summary>
        /// webPart最小化时执行此方法
        /// </summary>
        void ShowMiniWebPart();

        /// <summary>
        /// 刷新当前数据
        /// </summary>
        void RefreshData();

        /// <summary>
        /// webPart获取更多数据事件
        /// </summary>
        event EventHandler OnMoreChanged;
    }
}
