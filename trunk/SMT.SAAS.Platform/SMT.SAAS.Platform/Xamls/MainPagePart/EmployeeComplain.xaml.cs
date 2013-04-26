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

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class EmployeeComplain : UserControl, ICleanup
    {
        public EmployeeComplain()
        {
            InitializeComponent();
        }

        public void Cleanup()
        {

            //MessageBox.Show("Cleanup : EmployeeComplain");
          
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SAAS.ClientUtility.DictionaryManager dicManager = new ClientUtility.DictionaryManager();
            List<string> dicCategorys = new List<string>();
            dicCategorys.Add("CHECKSTATE");
            //dicCategorys.Add("PAYTYPE");
            dicManager.OnDictionaryLoadCompleted += (o, args) =>
            {
                if (args.Error == null && args.Result)
                    MessageBox.Show("Load Dictionary 1");
            };
            dicManager.LoadDictionary(dicCategorys);

          
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            LoadDictionary();
        }

        private static void LoadDictionary()
        {
            SAAS.ClientUtility.DictionaryManager dicManager = new ClientUtility.DictionaryManager();
            List<string> dicCategorys = new List<string>();

            dicCategorys.Add("POSTLEVEL");
            dicCategorys.Add("PAYTYPE");
            dicCategorys.Add("CONTRACTLEVEL");
            dicCategorys.Add("SALARYLEVEL");
            dicManager.OnDictionaryLoadCompleted += (o, args) =>
            {
                if (args.Error == null && args.Result)
                    MessageBox.Show("Load Dictionary 2");
            };
            dicManager.LoadDictionary(dicCategorys);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {


            //创建字典管理对象
            SAAS.ClientUtility.DictionaryManager dicManager = new ClientUtility.DictionaryManager();

            //创建用于加载字典的字典类型集合
            List<string> dicCategorys = new List<string>();
            dicCategorys.Add("POSTLEVEL");
            dicCategorys.Add("PAYTYPE");
            dicCategorys.Add("CONTRACTLEVEL");
            dicCategorys.Add("SALARYLEVEL");
            dicCategorys.Add("SALARYPROPERTY ");
            dicCategorys.Add("COMPANYCATEGORY");
            dicCategorys.Add("BLOODTYPE");
            dicCategorys.Add("SECONDLANGUAGEDEGREE");
            dicCategorys.Add("REGRESIDENCETYPE");
            dicCategorys.Add("IDTYPE");

            //注册完成事件，用于通知字典加载完成或失败。
            dicManager.OnDictionaryLoadCompleted += (o, args) =>
            {
                if (args.Error == null && args.Result)
                    MessageBox.Show("Load Dictionary 3");
            };

            //调用字典加载方法
            dicManager.LoadDictionary(dicCategorys);



        }

       
    }
}
