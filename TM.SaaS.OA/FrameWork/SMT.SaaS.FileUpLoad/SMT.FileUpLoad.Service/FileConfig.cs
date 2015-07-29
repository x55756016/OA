using System;
using System.Xml;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.IO;
using System.Configuration;
using SMT.Foundation.Log;

namespace SMT.FileUpLoad.Service
{
    
    public class FileConfig
    {
        public static string path = "FileConfig.config";

        #region 方法
        /// <summary>
        /// 获取公司对上传文件的设置信息
        /// </summary>
        /// <param name="companycode">公司代码</param>
        /// <returns></returns>
        public static UserFile GetCompanyItem(string companycode)
        {
            UserFile userFile = new UserFile();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(System.Web.HttpContext.Current.Server.MapPath(path));
            XmlNode node = xDoc.SelectSingleNode("//item[@companycode='" + companycode + "']");
            if (node != null)
            {
                userFile.SavePath = node.Attributes["savepath"] != null ? node.Attributes["savepath"].Value : "";
                userFile.CompanyCode = node.Attributes["companycode"] != null ? node.Attributes["companycode"].Value : "";
                userFile.CompanyName = node.Attributes["companyname"] != null ? node.Attributes["companyname"].Value : "";
                userFile.FileUrl = node.Attributes["fileurl"] != null ? node.Attributes["fileurl"].Value : "";
                userFile.SystemCode = node.Attributes["systemcode"] != null ? node.Attributes["systemcode"].Value : "";
                userFile.ThumbnailUrl = node.Attributes["thumbnailurl"] != null ? node.Attributes["thumbnailurl"].Value : "";
                userFile.MaxNumber = String.IsNullOrEmpty(node.Attributes["maxnumber"].Value) ? 0 : Convert.ToInt32(node.Attributes["maxnumber"].Value);

                userFile.MaxSize = String.IsNullOrEmpty(node.Attributes["maxsize"].Value) ? 0 : GetByte(node.Attributes["maxsize"].Value);
                userFile.UploadSpeed = String.IsNullOrEmpty(node.Attributes["uploadspeed"].Value) ? 0 : GetByte(node.Attributes["uploadspeed"].Value);
                userFile.ModelCode = node.Attributes["modelcode"] != null ? node.Attributes["modelcode"].Value : "";
                userFile.Type = node.Attributes["type"] != null ? node.Attributes["type"].Value : "";
            }
            else
            {
                //StreamWriter sw = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath(path), false, System.Text.Encoding.Default);
                //try
                //{
                //    sw.Write(TextBox1.Text);
                //    sw.Close();
                //    //Response.Write("<b>文件写入成功！</b>");
                //}
                //catch
                //{
                //    Response.Write("<b>文件写入失败！</b>");
                //}


            }
            return userFile;
        }

        public static UserFile GetCompanyItem(string companycode,string StrName)
        {
            UserFile userFile = new UserFile();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(System.Web.HttpContext.Current.Server.MapPath(path));
            XmlNode node = xDoc.SelectSingleNode("//item[@companycode='" + companycode + "']");
            if (node != null)
            {
                userFile.SavePath = node.Attributes["savepath"] != null ? node.Attributes["savepath"].Value : "";
                userFile.CompanyCode = node.Attributes["companycode"] != null ? node.Attributes["companycode"].Value : "";
                userFile.CompanyName = node.Attributes["companyname"] != null ? node.Attributes["companyname"].Value : "";
                userFile.FileUrl = node.Attributes["fileurl"] != null ? node.Attributes["fileurl"].Value : "";
                userFile.SystemCode = node.Attributes["systemcode"] != null ? node.Attributes["systemcode"].Value : "";
                userFile.ThumbnailUrl = node.Attributes["thumbnailurl"] != null ? node.Attributes["thumbnailurl"].Value : "";
                userFile.MaxNumber = String.IsNullOrEmpty(node.Attributes["maxnumber"].Value) ? 0 : Convert.ToInt32(node.Attributes["maxnumber"].Value);

                userFile.MaxSize = String.IsNullOrEmpty(node.Attributes["maxsize"].Value) ? 0 : GetByte(node.Attributes["maxsize"].Value);
                userFile.UploadSpeed = String.IsNullOrEmpty(node.Attributes["uploadspeed"].Value) ? 0 : GetByte(node.Attributes["uploadspeed"].Value);
                userFile.ModelCode = node.Attributes["modelcode"] != null ? node.Attributes["modelcode"].Value : "";
                userFile.Type = node.Attributes["type"] != null ? node.Attributes["type"].Value : "";
            }
            else
            {
                StreamWriter sw = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath(path),false, System.Text.Encoding.Default);
                try
                {
                    string Filepath =@"C:\文件上传\{0}\{1}\{2}";
                    string aa=@"<item companycode="+companycode+" savepath="+Filepath+" companyname="+ StrName+"   fileurl=\"\" thumbnailurl=\"\" maxnumber=\"2\" maxsize=\"2.MB\" uploadspeed=\"52428800\"/>";
                    sw.Write(aa);
                    sw.Close();
                    //Response.Write("<b>文件写入成功！</b>");
                }
                catch
                {
                    //Response.Write("<b>文件写入失败！</b>");
                }


            }
            return userFile;
        }


        /// <summary>
        /// 根据文件的大小获取字节（3.GB、4.MB、4.KB）
        /// </summary>
        /// <param name="value">3.GB、4.MB、4.KB</param>
        /// <returns></returns>
        public static double GetByte(string value)
        {
            double byteCount = 0.0;

            if (value.IndexOf('.') > 0)
            {
                string name = value.Split('.')[1];
                double v = System.Convert.ToDouble(value.Substring(0, value.Length - 2));
                if (name.ToUpper() == "GB")
                {
                    byteCount = v * 1073741824;
                }
                if (name.ToUpper() == "MB")
                {
                    byteCount = v * 1048576;
                }
                if (name.ToUpper() == "KB")
                {
                    byteCount = v * 1024;
                }
            }
            else
            {
                if (value != "")
                {
                    byteCount = System.Convert.ToDouble(value);
                }
            }
            return byteCount;

        }
        #endregion


        #region 创建路径


        public static string CreateCompanyDirecttory(string companycode, string CompanyName, string companyID)
        {
            string strReturn = string.Empty;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(System.Web.HttpContext.Current.Server.MapPath(path));
            XmlNode node = xDoc.SelectSingleNode("//item[@companycode='" + companyID + "']");
            if (node == null)
            {

                //StreamWriter sw = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath(path), false, System.Text.Encoding.Default);
                try
                {
                    
                    string fileDirectory = string.Concat(ConfigurationManager.AppSettings["CompanyDirctoryPath"]);
                    //string Filepath =@"C:\文件上传\{0}\{1}\{2}";
                    string Filepath = fileDirectory + @"\{0}\{1}\{2}";
                    //string aa = @"<item companycode=" + companycode + " savepath=" + Filepath + " companyname=" + StrName + "   fileurl=\"\" thumbnailurl=\"\" maxnumber=\"2\" maxsize=\"2.MB\" uploadspeed=\"52428800\"/>";
                    //sw.Write(aa);
                    //sw.Close();
                    string dir = fileDirectory + "\\" + companyID;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    XmlNode root = xDoc.SelectSingleNode("root");//查找<root>
                    XmlElement xe1 = xDoc.CreateElement("item");//创建一个<item>节点
                    //companycode="cafdca8a-c630-4475-a65d-490d052dca36" 
                    //savepath="C:\文件上传\{0}\{1}\{2}" 
                    //companyname="疾风科技"   fileurl="" thumbnailurl="" 
                    //maxnumber="2" maxsize="2.MB" uploadspeed="52428800"
                    xe1.SetAttribute("companycode", companyID);//设置该节点genre属性
                    xe1.SetAttribute("savepath", Filepath);//设置该节点ISBN属性
                    xe1.SetAttribute("companyname", CompanyName);//设置该节点genre属性
                    xe1.SetAttribute("fileurl", "");//设置该节点ISBN属性
                    xe1.SetAttribute("thumbnailurl", "");//设置该节点genre属性
                    xe1.SetAttribute("maxnumber", "2");//设置该节点ISBN属性
                    xe1.SetAttribute("maxsize", "2.0MB");//设置该节点genre属性
                    xe1.SetAttribute("uploadspeed", "52428800");//设置该节点ISBN属性                    
                    root.AppendChild(xe1);//添加到<bookstore>节点中
                    xDoc.Save(System.Web.HttpContext.Current.Server.MapPath(path));
                    //Response.Write("<b>文件写入成功！</b>");
                }
                catch(Exception ex)
                {
                    Tracer.Debug("添加配置文件出错：" + ex.ToString());
                }


            }
            return strReturn;
        }
        #endregion
    }
}
