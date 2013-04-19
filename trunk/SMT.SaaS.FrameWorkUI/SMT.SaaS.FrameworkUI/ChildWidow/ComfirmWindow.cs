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
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.SaaS.FrameworkUI.ChildWidow
{
    public class ComfirmWindow
    {
        public static string[] titlename = { Utility.GetResourceStr("CONFIRM"), Utility.GetResourceStr("CANCELBUTTON") };

        public static string[] confirmation = { Utility.GetResourceStr("CONTINUEGENERATE"), Utility.GetResourceStr("CANCELGENERATE") };   

        /// <summary>
        /// 确认框
        /// </summary>
        /// <param name="tilte">标题</param>
        /// <param name="message">信息</param>
        /// <param name="ButtonName">按钮名称</param>
        public static void ConfirmationBox(string tilte,string message,string ButtonName)
        {
            
            string _text="";
            MessageWindow.Show<string>(tilte, message, MessageIcon.Exclamation,result => _text = result, "Default", ButtonName);
        }
        /// <summary>
        /// 重载 确认框
        /// 增加AutoDisappear参数
        /// </summary>
        /// <param name="tilte">标题</param>
        /// <param name="message">信息</param>
        /// <param name="ButtonName">按钮名称</param>
        public static void ConfirmationBox(string tilte, string message, string ButtonName,AutoDisappear autoDisappear)
        {
            string _text = "";
            MessageWindow.Show<string>(tilte, message, MessageIcon.Exclamation, result => _text = result, "Default",autoDisappear, ButtonName);
        }


        /// <summary>
        /// 错误框
        /// </summary>
        /// <param name="tilte">标题</param>
        /// <param name="message">信息</param>
        /// <param name="ButtonName">按钮名称</param>
        public static void ConfirmationBoxs(string tilte, string message, string ButtonName,MessageIcon icon)
        {
            string _text = "";
            
            MessageWindow.Show<string>(tilte, message, icon, result => _text = result, "Default", ButtonName);
        }

        /// <summary>
        /// 选择框
        /// </summary>
        /// <param name="tilte">标题</param>
        /// <param name="message">信息</param>
        /// <param name="ButtonName">按钮名称组</param>
        public void SelectionBox(string tilte, string message, string[] ButtonName, string Result)
        {
            MessageWindow.Show<string>(tilte, message, MessageIcon.Question,
                GetResult, "Default", ButtonName);

        }

        public event EventHandler<OnSelectionBoxClosedArgs> OnSelectionBoxClosed;
        private void GetResult(string result)
        {
            if(titlename[0]==result)
                OnSelectionBoxClosed(this, new OnSelectionBoxClosedArgs(result));
            if (confirmation[0] == result)
                OnSelectionBoxClosed(this, new OnSelectionBoxClosedArgs(result)); 
        }
    }

    public class OnSelectionBoxClosedArgs : EventArgs 
    {
        public OnSelectionBoxClosedArgs(string result)
        {
            this.Result = result;
        }
        public string Result { get; set; }
    }
}
