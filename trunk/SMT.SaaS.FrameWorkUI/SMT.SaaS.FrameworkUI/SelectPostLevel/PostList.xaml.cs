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
using SMT.Saas.Tools.PermissionWS;
using System.Text;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.SaaS.FrameworkUI.SelectPostLevel
{
    public partial class PostList : System.Windows.Controls.Window
    {
        /// <summary>
        /// 获取  选择不同的岗位级别信息
        /// </summary>
        private const int wpheight = 20;
        private string strCode = string.Empty;
        public Dictionary<string, string> Result { get; set; }
        public event EventHandler SelectedClicked;
        public event EventHandler Close;
        private string StrSelectedPost = "";
        public PostList()
        {
            InitializeComponent();
            LoadUIData("","");
            Result = new Dictionary<string, string>();
        }
        public PostList(string SelectedPost,string PostValue)
        {
            InitializeComponent();
            LoadUIData(SelectedPost,PostValue);
            Result = new Dictionary<string, string>();
        }
        private void LoadUIData(string StrPost,string PostValue)
        {
            if (!string.IsNullOrEmpty(StrPost))
                CheckedPost.Text = StrPost+",";
            if (!string.IsNullOrEmpty(PostValue))
                strCode = PostValue+",";//加上，还原原来的字符串
            var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                       where a.DICTIONCATEGORY == "POSTLEVEL"
                       select new
                       {
                           DICTIONARYID = a.DICTIONARYID,
                           DICTIONARYNAME = a.DICTIONARYNAME,
                           DICTIONARYVALUE = a.DICTIONARYVALUE
                       };
            if (ents.Count() > 0)
            {
                wpPost.Children.Clear();
                
                foreach (var ent in ents)
                {
                    CheckBox cbx = new CheckBox();
                    cbx.Style = Application.Current.Resources["CheckBoxStyle"] as Style;
                    if (!string.IsNullOrEmpty(CheckedPost.Text) && CheckedPost.Text.IndexOf(ent.DICTIONARYNAME) != -1)
                        cbx.IsChecked = true;
                    else
                        cbx.IsChecked = false;
                    cbx.Content = ent.DICTIONARYNAME;
                    cbx.Tag = ent.DICTIONARYVALUE;
                    cbx.Click += new RoutedEventHandler(cbx_Checked);
                    cbx.Margin = new Thickness(2);
                    
                    cbx.Width = 80;
                    wpPost.Children.Add(cbx);

                    
                }
            }
        }


        void cbx_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                if (cb.IsChecked != true)
                {
                    StrSelectedPost = CheckedPost.Text.Replace(cb.Content.ToString() + ",", "").Trim();
                    if (StrSelectedPost.Length > 2)
                        CheckedPost.Text = StringSort(StrSelectedPost);
                    else
                        CheckedPost.Text = StrSelectedPost;
                    strCode = strCode.Replace(cb.Tag.ToString() + ",", "");
                }
                else
                {
                    if (CheckedPost.Text.IndexOf(cb.Content.ToString()) == -1)
                    {
                        CheckedPost.Text += cb.Content.ToString() + ",";
                        if (CheckedPost.Text.Length > 2)//多于2个的才进行排序
                            CheckedPost.Text = StringSort(CheckedPost.Text);
                        strCode += cb.Tag.ToString() + ",";
                    }
                }
            }
        }
        /// <summary>
        /// 岗位级别排序
        /// </summary>
        /// <param name="CheckedString"></param>
        /// <returns></returns>
        private string StringSort(string CheckedString)
        {
            string StrReturn = "";
            CheckedString = CheckedString.Replace(",","");
            string[] ArrString = CheckedString.Split(',');
            
            char[] arr;
            
            arr = CheckedString.ToCharArray();
            for (int i = 0; i < CheckedString.Length; i++)
            {
                for (int j = i + 1; j < CheckedString.Length; j++)
                {
                    if (string.CompareOrdinal(Convert.ToString(arr[i]), Convert.ToString(arr[j])) > 0)
                    {
                        char temp = arr[i];
                        arr[i] = arr[j];
                        arr[j] = temp;
                    }
                }
            }
            foreach (char m in arr)
                StrReturn += m + ",";

            

            return StrReturn;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(strCode))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "岗位级别不能为空",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            Result.Add(CheckedPost.Text.ToString(), strCode);
            if (SelectedClicked != null)
            {
                SelectedClicked(this, e);
            }
            if (this.Close != null)
                Close(this, EventArgs.Empty);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CheckedPost.Text = strCode = string.Empty;
            if (this.Close != null)
                Close(this, EventArgs.Empty);
        }




    }
}
