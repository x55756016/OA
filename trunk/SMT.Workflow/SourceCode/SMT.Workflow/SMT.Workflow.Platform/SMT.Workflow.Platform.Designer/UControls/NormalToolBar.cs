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
using System.Windows.Media.Imaging;

namespace SMT.Workflow.Platform.Designer.UControls
{
    public class NormalToolBar
    {
        public NormalToolBar()
        {

        }

        public ImgButton ImgBtnAdd()
        {
            ImgButton imgbtn_add = new ImgButton();
            imgbtn_add.Foreground = new SolidColorBrush(Colors.White);
            imgbtn_add.Content = "新建";
            imgbtn_add.Icon = new BitmapImage(new Uri("/SMT.Workflow.Platform.Designer;component/Images/Toolbar/16_add.png", UriKind.Relative));
            return imgbtn_add;
        }
        public ImgButton ImgBtnDel()
        {
            ImgButton imgbtn_del = new ImgButton();
            imgbtn_del.Content = "删除";
            imgbtn_del.Foreground = new SolidColorBrush(Colors.White);
            imgbtn_del.Icon = new BitmapImage(new Uri("/SMT.Workflow.Platform.Designer;component/Images/Toolbar/16_delete.png", UriKind.Relative));
            return imgbtn_del;
        }
        public ImgButton ImgBtnUpdate()
        {
            ImgButton imgbtn_update = new ImgButton();
            imgbtn_update.Content = "编辑";
            imgbtn_update.Foreground = new SolidColorBrush(Colors.White);
            imgbtn_update.Icon = new BitmapImage(new Uri("/SMT.Workflow.Platform.Designer;component/Images/Toolbar/16_edit.png", UriKind.Relative));
            return imgbtn_update;
        }
        public ImgButton ImgBtnPrint()
        {
            ImgButton imgbtn_print = new ImgButton();
            imgbtn_print.Content = "打印";
            imgbtn_print.Foreground = new SolidColorBrush(Colors.White);
            imgbtn_print.Icon = new BitmapImage(new Uri("/SMT.Workflow.Platform.Designer;component/Images/Toolbar/16_print.png", UriKind.Relative));
            return imgbtn_print;
        }
        public ImgButton ImgBtnSearch()
        {
            ImgButton imgbtn_search = new ImgButton();
            imgbtn_search.Content = "查找";
            imgbtn_search.Foreground = new SolidColorBrush(Colors.White);
            imgbtn_search.Icon = new BitmapImage(new Uri("/SMT.Workflow.Platform.Designer;component/Images/Toolbar/16_search.png", UriKind.Relative));
            return imgbtn_search;
        }
    }
}
