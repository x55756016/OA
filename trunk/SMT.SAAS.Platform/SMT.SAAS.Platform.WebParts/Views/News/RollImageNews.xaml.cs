using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using SMT.SAAS.Platform.Controls.InfoPanel;
using SMT.SAAS.Platform.WebParts.NewsWS;
using SMT.SAAS.Platform.WebParts.Models;
using System.Windows.Media.Imaging;
using System.Windows.Browser;
using System.Net;
using System.IO;
using SMT.Saas.Tools.NewFileUploadWS;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class RollImageNews : UserControl
    {
        private InfoPanel infoPanel;
        private PlatformServicesClient client = null;
        private BasicServices servers = null;
        WebClient wc = null;
        public RollImageNews()
        {
            InitializeComponent();
            servers = new BasicServices();
            client = servers.PlatformClient;

            InitTop();
        }

        #region 初始主界面上的新闻列表

        /// <summary>
        /// 初始化界面顶部的新闻
        /// </summary>
        private void InitTop()
        {
            infoPanel = new InfoPanel();
            infoPanel.OnInfoClick += new EventHandler<OnInfoClickEventArgs>(infoPanel_OnInfoClick);
            GetNewsList();
            ShowinfoPanel.Children.Add(infoPanel);

        }

        void infoPanel_OnInfoClick(object sender, OnInfoClickEventArgs e)
        {
            T_PF_NEWS news = e.Info.DataContext as T_PF_NEWS;

            NewsShow newsview = new NewsShow();
            newsview.LoadNewsDetails(news.NEWSID);
            string titel = "";
            switch (news.NEWSTYPEID)
            {
                case "0": titel = "新    闻"; break;
                case "1": titel = "动    态"; break;
                case "2": titel = "公    告"; break;
                case "3": titel = "通    知"; break;
                default:
                    break;
            }
            System.Windows.Controls.Window.Show(titel, "", news.NEWSID, true, true, newsview, null);
        }
        private List<T_PF_NEWS> ImageNews = new List<T_PF_NEWS>();
        private int listIndex = 0;
        private void GetNewsList()
        {
            client.GetImageNewsListAsync(6, "1");
            client.GetImageNewsListCompleted += (obj, args) =>
            {
                if (args.Error.IsNull())
                {
                    if (args.Result.IsNotNull())
                    {
                        ImageNews = args.Result.ToList();
                        DownLoadNews(ImageNews[listIndex]);
                    }
                }
            };
        }
        private byte[] StreamToBytes(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[8 * 1024];
                int read = 0;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        private void DownLoadNews(T_PF_NEWS newsmodel)
        {
            try
            {
                //FileUploadManagerClient fileUpload = new FileUploadManagerClient();
                WebClient wc = new WebClient();
                SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient bb = new UploadServiceClient();
                bb.GetFileListByOnlyApplicationIDCompleted += (obj, args) =>
                {
                    try
                    {
                        var result = args.Result.FileList;
                        if (result != null && result.Count > 0)
                        {
                            string path = result[0].FILEURL;
                            string filename = path.Substring(path.LastIndexOf('\\') + 1);
                            //string filepath = HttpUtility.UrlEncode(result[0].THUMBNAILURL + "\\" + result[0].FILENAME);
                            //var url = args.Result.DownloadUrl + "?filename=" + filepath;//文件地址
                            string filepath = HttpUtility.UrlEncode(result[0].THUMBNAILURL + "\\" + result[0].FILENAME);
                            string url = args.Result.DownloadUrl + "?flag=1&filename=" + filepath;

                            wc.OpenReadAsync(new Uri(url, UriKind.Absolute));
                            wc.OpenReadCompleted += (r, c) =>
                            {
                                try
                                {
                                    Info info = new Info();
                                    info.DataContext = newsmodel;
                                    info.InfoID = newsmodel.NEWSID;
                                    info.Titel = "◇" + newsmodel.NEWSTITEL;
                                    info.Uri = "";
                                    BitmapImage imgsource = new BitmapImage();
                                    System.IO.MemoryStream stream = new System.IO.MemoryStream(StreamToBytes(c.Result));
                                    imgsource.SetSource(stream);
                                    stream.Close();
                                    info.ImageSource = imgsource;
                                    infoPanel.InfoList.Add(info);
                                }
                                catch (Exception ex)
                                {
                                    Main.CurrentContext.AppContext.SystemMessage("webpart RollImageNews err:"
                                        + ex.ToString() + " url=" + url);
                                }
                            };
                        }

                        if (listIndex < ImageNews.Count - 1)
                        {
                            listIndex++;

                            DownLoadNews(ImageNews[listIndex]);

                        }
                        else
                        {

                            if (infoPanel.InfoList.Count > 0)
                                infoPanel.Start();
                        }

                    }
                    catch
                    {
                        if (listIndex < ImageNews.Count - 1)
                        {
                            listIndex++;

                            DownLoadNews(ImageNews[listIndex]);

                        }
                        else
                        {

                            if (infoPanel.InfoList.Count > 0)
                                infoPanel.Start();
                        }
                    }

                };
                bb.GetFileListByOnlyApplicationIDAsync(newsmodel.NEWSID);
            }
            catch
            {

            }
        }

        //void fileUpload_Get_ParentIDCompleted(object sender, Get_ParentIDCompletedEventArgs e)
        //{

        //}
        #endregion

    }
}
