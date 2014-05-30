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
using System.IO;

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public partial class SilUpFile : UserControl
    {
        private string missionReportsID = "";
        public SilUpFile()
        {
            InitializeComponent();

            byte[] br = { 1, 1 };

            //  fileUp.InitBtn(Visibility.Visible, Visibility.Collapsed);
            //SMT.SAAS.Main.CurrentContext.LoginUserInfo login = new SAAS.Main.CurrentContext.LoginUserInfo("a", "", "", "", "", "", "", "", "", br, "", "");
            //SMT.SAAS.Main.CurrentContext.Common.LoginUserInfo = login;
            fileUp.SystemName = "OA";
            fileUp.ModelName = "MissionReports";
            fileUp.Event_AllFilesFinished += new EventHandler<SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs>(ctrFile_Event_AllFilesFinished);
            Loaded += new RoutedEventHandler(SilUpFile_Loaded);
        }

        void SilUpFile_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(missionReportsID))
            {
                fileUp.Load_fileData(missionReportsID);
            }
        }

       
        void ctrFile_Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        {

            //RefreshUI(RefreshedTypes.ProgressBar);
        }

        private void UploadFiles()
        {
            System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
            openFileWindow.Multiselect = true;
            if (openFileWindow.ShowDialog() == true)
                foreach (FileInfo file in openFileWindow.Files)
                    fileUp.InitFiles(file.Name, file.OpenRead());

            fileUp.FormID = "aaa";
            fileUp.Save();
        }
    }
}
