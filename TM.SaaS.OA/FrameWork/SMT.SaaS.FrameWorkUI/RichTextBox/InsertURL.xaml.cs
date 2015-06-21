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
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Media.Imaging;

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public partial class InsertURL : ChildWindow
    {
       public FileStream fs=null;
       public BitmapImage bmp = null;
       private double width = 0;
        public InsertURL(string selectedText,bool isPic)
        {
            InitializeComponent();
            fileUp.FileUpEvent += new FileUpload.CtrlFileUploadM.FileUpLoad(fileUp_FileUpEvent);
            fileUp.UpFileType = "图片文件(*.jpg,*.gif,*.png)|*.jpg;*.gif;*.png|All Files(*.*)|*.*";
          // txtURLDesc.Text = selectedText;

           if (!isPic)
           {
               txtbl.Visibility = Visibility.Collapsed;
               droptype.Visibility = Visibility.Collapsed;
               txtdesc.Visibility = Visibility.Collapsed;
             //  txtURLDesc.Visibility = Visibility.Collapsed;
           }
           else
           {
               byte[] br = { 1, 1 };

               fileUp.InitBtn(Visibility.Visible, Visibility.Collapsed);

               //SMT.SAAS.Main.CurrentContext.LoginUserInfo login = new SAAS.Main.CurrentContext.LoginUserInfo("a", "1", "1", "1", "1", "1", "1", "1", "1", br, "1", "1");
               //SMT.SAAS.Main.CurrentContext.Common.loginUserInfo = login;
               fileUp.SystemName = "OA";
               fileUp.ModelName = "MissionReports";
               fileUp.Event_AllFilesFinished += new EventHandler<SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs>(ctrFile_Event_AllFilesFinished);
              
           }
            this.Closing += (s, e) =>
            {
                if (this.DialogResult.Value)
                {
                    if (txtURL.Text.Length == 0 && txtURL.Visibility != Visibility.Collapsed)
                        e.Cancel = true;
                    else
                    {
                        if (txtURL.Visibility == Visibility.Visible)
                        {
                            if (txtURL.Text.IndexOf("http://") == -1 &&
                                txtURL.Text.IndexOf("https://") == -1 &&
                                txtURL.Text.IndexOf("ftp://") == -1)
                                txtURL.Text = "http://" + txtURL.Text;

                            if (!Regex.IsMatch(txtURL.Text,
                                                  @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                                e.Cancel = true;
                        }
                        else
                        {
                               
                        }
                    }
                    if (e.Cancel)
                        MessageBox.Show("Url地址不能为空，并且是有效路径");
                }
            };
        }

        void fileUp_FileUpEvent(FileInfo info)
        {
            fs = info.OpenRead();

             byte[] image = new byte[fs.Length];
                        fs.Read(image, 0, Convert.ToInt32(fs.Length));
                        //fs.Close();
                        //fs.Dispose();
                       

                        try
                        {
                            bmp = new BitmapImage();
                            bmp.SetSource(new MemoryStream(image));
                            double i = 0;
                            txtWidth.Text = bmp.PixelWidth.ToString(); //图片宽度
                            txtHeight.Text = bmp.PixelHeight.ToString(); //图片高度
                            if (!double.TryParse(txtWidth.Text,out i))
                            {
                                txtWidth.Text = "200";
                            }
                            if (!double.TryParse(txtHeight.Text, out i))
                            {
                                txtHeight.Text = "150";
                            }
                          
                        }
                        catch (Exception e) { throw e; }
          

           
        }         
        private void OKButton_Click(object sender, RoutedEventArgs e)
        { 
           
            //fileUp.FormID = "1";
            //fileUp.Save();
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void droptype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (droptype != null)
            {
                string tag = (droptype.SelectedItem as ComboBoxItem).Tag.ToString();
                if (tag == "网络上传")
                {
                    txtURL.Visibility = Visibility.Visible;
                    fileUp.Visibility = Visibility.Collapsed;
                }
                else
                {
                    txtURL.Visibility = Visibility.Collapsed;
                    fileUp.Visibility = Visibility.Visible;
                }

            }
        }
        private void UploadFiles()
        {
            System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
            openFileWindow.Multiselect = true;
            if (openFileWindow.ShowDialog() == true)
                foreach (FileInfo file in openFileWindow.Files)
                {
                    fileUp.InitFiles(file.Name, file.OpenRead());
                }
            fileUp.FormID = "aaa";
            fileUp.Save();
        }
        void ctrFile_Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        {
        }

       

        private void txtWidth_LostFocus(object sender, RoutedEventArgs e)
        {
            string tag = (droptype.SelectedItem as ComboBoxItem).Tag.ToString();
            double i = 0;  //改变后的宽
            double j = 0;  //之前高
            double z = 0;  //改变后的高
            if (!double.TryParse(txtWidth.Text, out i))
            {
                txtWidth.Text = "200";
                i = 200;
            }
            if (!double.TryParse(txtHeight.Text, out j))
            {
                txtHeight.Text = "150";
                j = 150;
            }
            if (tag != "网络上传")
            {
               
                if (width != i)
                {
                    z = i * (j / width);
                    txtHeight.Text = z.ToString().Split('.')[0];
                }
               

            }
        }


        private void txtWidth_GotFocus(object sender, RoutedEventArgs e)
        {
            double i = 0;
            if (!double.TryParse(txtWidth.Text, out i))
            {
                txtWidth.Text = "200";
                i = 200;
            }
            width = i;
        }

        private void txtHeight_LostFocus(object sender, RoutedEventArgs e)
        {
            double j = 0;  //之前高
            if (!double.TryParse(txtHeight.Text, out j))
            {
                txtHeight.Text = "150";
                j = 150;
            }
        }

    }
}
