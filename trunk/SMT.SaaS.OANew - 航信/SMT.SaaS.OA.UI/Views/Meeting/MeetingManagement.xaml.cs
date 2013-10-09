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
using System.Windows.Navigation;
using System.Windows.Browser;
using System.Windows.Data;
using System.Globalization;



namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingManagement : Page
    {

       
        
        public MeetingManagement()
        {
            InitializeComponent();
            
        }

        

        


        #region 頁面控件樣式
        private int iStyleCategory = 0;

        public int StyleCategory
        {
            set { this.iStyleCategory = value; }
        }

        public void SetStyle()
        {

            switch (iStyleCategory)
            {
                case -1:
                    break;
                case 0:
                    this.LayoutRoot.Style = (Style)App.Current.Resources["LayoutRootStyle"];
                    //this.txtContent.Style = (Style)App.Current.Resources["TextBoxStyle"];
                    //this.txtMeetingTitle.Style = (Style)App.Current.Resources["TextBoxStyle"];
                    //this.cbDepartment.Style = (Style)App.Current.Resources["ComboxStyle"];
                    //this.cbMeetingType.Style = (Style)App.Current.Resources["ComboxStyle"];
                    //this.AddBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];
                    //this.UpdateBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];
                    //this.DelBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];
                    //this.SearchBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];
                    //this.cbIsReadOnly.Style = (Style)App.Current.Resources["CheckBoxStyle"];
                    //this.grsplSplitterColumn.Style = (Style)App.Current.Resources["SPGridSplitterStyle"];
                    //this.controlsToolkitTUV.Style = (Style)App.Current.Resources["FramecontrolsPagerStyle"];
                    break;
                case 1:
                    this.LayoutRoot.Style = (Style)App.Current.Resources["LayoutRootStyle1"];                    
                    //this.LayoutRoot.Style = (Style)App.Current.Resources["LayoutRootStyle"];
                    //this.txtContent.Style = (Style)App.Current.Resources["TextBoxStyle"];
                    //this.txtMeetingTitle.Style = (Style)App.Current.Resources["TextBoxStyle"];
                    //this.cbDepartment.Style = (Style)App.Current.Resources["ComboxStyle"];
                    //this.cbMeetingType.Style = (Style)App.Current.Resources["ComboxStyle"];
                    //this.AddBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];
                    //this.UpdateBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];
                    //this.DelBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];
                    //this.SearchBtn.Style = (Style)App.Current.Resources["ButtonToolBarStyle"];                                        
                    //this.controlsToolkitTUV.Style = (Style)App.Current.Resources["FramecontrolsPagerStyle"];
                    break;
            }
        }
        #endregion

              

    }


    #region 会议申请状态
    public class ConverterNumberToWayString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            { 
                case "0":
                    StrReturn = "未提交";
                    break;
                case "1":
                    StrReturn = "审核中";
                    break;
                case "2":
                    StrReturn = "审核通过";
                    break;
                case "3":
                    StrReturn = "审核未通过";
                    break;  
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "未提交":
                    StrReturn = "0";
                    break;
                case "审核中":
                    StrReturn = "1";
                    break;
                case "审核通过":
                    StrReturn = "2";
                    break;
                case "审核未通过":
                    StrReturn = "3";
                    break;
            }
            return StrReturn;
            
        }
    }
    #endregion

    #region 日期时间格式化
    public class ConverterDateToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime Dt = new DateTime();
            if (value !=null && value !="")
            {
                Dt = (DateTime)value;
                return Dt.ToShortDateString() + " " + Dt.ToShortTimeString();
            }
            else
            {
                return "";
            }

            
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return System.Convert.ToDateTime(value).ToString();            

        }
    }

    #endregion

    #region 会议内容格式化
    public class ConverterMeetingContentToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            int StringLength = 60;
            if (parameter == null)
            {
                if (value.ToString().Length > StringLength)
                {
                    StrReturn = value.ToString().Substring(0, StringLength) + "......";
                }
                else
                {
                    StrReturn = value.ToString();
                }
            }
            else
            {
                if (value.ToString().Length > parameter.ToInt32())
                {
                    StrReturn = value.ToString().Substring(0, parameter.ToInt32()) + "......";
                }
                else
                {
                    StrReturn = value.ToString();
                }

            }
            //if (value.ToString().Length > 60)
            //{
            //    StrReturn = value.ToString().Substring(0, 60) + "......";
            //}
            //else
            //{
            //    StrReturn = value.ToString();
            //}
            return StrReturn;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            //return System.Convert.ToDateTime(value).ToString();
            return value.ToString();

        }
    }

    #endregion

    #region 会议内容格式化
    public class ConverterWebPartContentToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            if (value.ToString().Length > 20)
            {
                StrReturn = value.ToString().Substring(0, 20) + "......";
            }
            else
            {
                StrReturn = value.ToString();
            }
            return StrReturn;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            //return System.Convert.ToDateTime(value).ToString();
            return value.ToString();

        }
    }

    #endregion

    #region 订餐状态
    public class ConverterOrderMealToState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "取消";
                    break;
                case "1":
                    StrReturn = "已订";
                    break;
                case "2":
                    StrReturn = "待订";
                    break;                
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "审核中":
                    StrReturn = "0";
                    break;
                case "审核通过":
                    StrReturn = "1";
                    break;
                case "取消申请":
                    StrReturn = "2";
                    break;
                case "审核未通过":
                    StrReturn = "3";
                    break;
            }
            return StrReturn;

        }
    }
    #endregion

    #region 会议类型发起状态
    public class ConverterMeetingTypeState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "不自动";
                    break;
                case "1":
                    StrReturn = "自动";
                    break;
                
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "不自动":
                    StrReturn = "0";
                    break;
                case "自动":
                    StrReturn = "1";
                    break;
                
            }
            return StrReturn;

        }
    }
    #endregion

    #region 参加会议状态
    public class ConverterMeetingJoinState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "未确认";
                    break;
                case "1":
                    StrReturn = "确认参加";
                    break;
                case "2":
                    StrReturn = "不参加但上传材料";
                    break;
                case "3":
                    StrReturn = "不参加";
                    break;

            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "未确认":
                    StrReturn = "0";
                    break;
                case "确认参加":
                    StrReturn = "1";
                    break;
                case "不参加但上传材料":
                    StrReturn = "2";
                    break;
                case "不参加":
                    StrReturn = "3";
                    break;

            }
            return StrReturn;

        }
    }
    #endregion

    #region 会议申请状态
    public class ConverterWebPart : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "Notice":
                    StrReturn = "会议通知";
                    break;
                case "HouseIssue":
                    StrReturn = "房源发布";
                    break;
                case "CompanyDoc":
                    StrReturn = "公司发文";
                    break;                
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "会议通知":
                    StrReturn = "Notice";
                    break;
                case "房源发布":
                    StrReturn = "HouseIssue";
                    break;
                case "公司发文":
                    StrReturn = "CompanyDoc";
                    break;
                case "审核未通过":
                    StrReturn = "3";
                    break;
            }
            return StrReturn;

        }
    }
    #endregion

    #region 平台时间转换
    public class OAWebPartDateTimeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string _temp = value.ToString();
                return DateTime.Parse(_temp).ToString("yyyy年MM月dd日HH:mm");
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    #endregion


    #region 公文发布状态转换
    public class ConverterDocDistrbute : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "未发布";
                    break;
                case "1":
                    StrReturn = "已发布";
                    break;
                

            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "未发布":
                    StrReturn = "0";
                    break;
                case "已发布":
                    StrReturn = "1";
                    break;
                

            }
            return StrReturn;

        }
    }
    #endregion
}
