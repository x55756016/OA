using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;
using SMT.SaaS.PublicInterface.BLL;
using SMT_System_EFModel;
using SMT.Foundation.Log;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;

namespace SMT.SaaS.PublicInterface
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "PublicService" in code, svc and config file together.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class PublicService : IPublicService
    {
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>内容</returns>
        public byte[] GetContent(string FORMID)
        {
            try
            {
                using (SysRtfBLL bll = new SysRtfBLL())
                {
                    try
                    {


                        return bll.GetContent(FORMID);
                    }
                    catch (Exception e)
                    {
                        Tracer.Debug("调用GetContent" + e.InnerException.ToString());
                        throw e;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("调用GetContent" + ex.InnerException.ToString());
                throw ex;
            }
        }


        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>内容</returns>
        public string GetContentFormatImgToUrl(string FORMID)
        {
            string strContent = string.Empty;
            string strcon = string.Empty;
            try
            {
                using (SysRtfBLL bll = new SysRtfBLL())
                {
                    byte[] context = bll.GetContent(FORMID);
                    strcon =UTF8Encoding.UTF8.GetString(context);
                }
                //string strcon = System.IO.File.ReadAllText("c:/神州通在线协同办公平台.htm");
                try
                {
                   
                    // 定义正则表达式用来匹配 img 标签
                    Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

                    // 搜索匹配的字符串
                    MatchCollection matches = regImg.Matches(strcon);
                    int n = 0, m = 0;
                    string[] sUrlList = new string[matches.Count];
                    string[] img = new string[matches.Count];

                    strContent = strcon;
                    // 取得匹配项列表
                    foreach (Match match in matches)
                    {
                        img[n++] = match.Groups["imgobj"].Value; //整个IMG 标签
                        sUrlList[m++] = match.Groups["imgUrl"].Value; //IMG SRC地址
                        // 判断src的格式是不是内嵌格式。
                        var temp = match.Groups["imgUrl"].Value;
                        if ( temp.Split(',').Count() > 1)
                        {
                            string base64Str = temp.Split(',')[1];
                            string imageUrl=Base64ToImage(base64Str, FORMID, m.ToString());

                            strContent = strContent.Replace(match.Groups["imgUrl"].Value, imageUrl);
                        }
                    }
                    string oldString = "<span class=\"s_E6FD2046\"> </span>";
                    
                    string newString = "<span class=\"s_E6FD2046\"></br></span>";
                    strContent = strContent.Replace(oldString, newString);
                    return strContent;
                    
        //            string imgLocalPath = @"D:/web/LoadImge";//存放下载图片的路径
        //            for (int i = 0; i < sUrlList.Length; i++)
        //            {
        //                string fileName
        //= sUrlList[i].Substring(sUrlList[i].LastIndexOf("/") + 1, sUrlList[i].Length - sUrlList[i].LastIndexOf("/") - 1);
        //                string urlName = sUrlList[i];


        //                WebClient wc = new WebClient();
        //                if (!System.IO.File.Exists(imgLocalPath + "//" + fileName))
        //                {
        //                    wc.DownloadFile(urlName, imgLocalPath + "//" + fileName);

        //                }
        //                //替换掉整个IMG 标签
        //                strcon = strcon.Replace(img[i], "<img width=220  src=http://localhost/CathImge/" + fileName + "/>");
        //                //只替换Url
        //                strcon = strcon.Replace(urlName, @"http://localhost/CathImge/" + fileName);

        //            }
                    //return "";
                }
                catch (Exception e)
                {
                    Tracer.Debug("调用GetContent" + e.InnerException.ToString());
                    throw e;
                }
                // }
            }
            catch (Exception ex)
            {
                Tracer.Debug("调用GetContent" + ex.InnerException.ToString());
                throw ex;
            }
        }
        /// <summary>                /// 获得字符串中开始和结束字符串中间得值 
         /// /// </summary>    
        /// <param name="str"></param>      
        /// <param name="beginStr">开始</param>    
         /// <param name="endStr">结束</param>       
        /// <returns></returns>       
        private static string GetStr(string str, string beginStr, string endStr)
        {
            Regex rg = new Regex("(?<=(" + beginStr + "))[.\\s\\S]*?(?=(" + endStr + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }

        /// <summary>   
/// Base64编码转换为图像   
/// </summary>   
/// <param name="base64String">Base64字符串</param>   
/// <returns>转换成功返回图像；失败返回null</returns>   
        public static string Base64ToImage(string base64String, string FORMID, string imageName)
        {
            string Urlpath = string.Empty;
            MemoryStream ms = null;
            Image image = null;
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\image\" + FORMID;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!File.Exists(path + @"\" + imageName + ".Jpeg"))
                {
                    byte[] imageBytes = Convert.FromBase64String(base64String);
                    ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    image = Image.FromStream(ms, true);
                    //按指定格式保存图像   
                    ImageFormat imageFormat = ImageFormat.Jpeg;
                    Bitmap bitmap = new Bitmap(image);

                    bitmap.Save(path + @"\" + imageName + ".Jpeg", imageFormat);
                    bitmap.Dispose();
                }
                //Tracer.Debug(HttpContext.Current.Request.Url.ToString());
                //Tracer.Debug(HttpContext.Current.Request.Url.AbsolutePath);
                Urlpath = HttpContext.Current.Request.Url.ToString().Replace(@"/PublicService.svc", "")
                    + @"/image/" + FORMID + @"/" + imageName + ".Jpeg";


            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
            finally
            {
                if (ms != null) ms.Close();
            }

            return Urlpath;
        }
    


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>bool</returns>
        public bool DeleteContent(string FORMID)
        {
            using (SysRtfBLL bll = new SysRtfBLL())
            {
                return bll.DeleteContent(FORMID);
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="content">内容</param>
        /// <param name="UPDATEUSERID">用户ID</param>
        /// <param name="UPDATEUSERNAME">用户名</param>
        /// <returns>bool</returns>
        public bool UpdateContent(string FormID, byte[] content, UserInfo userinfo)
        {
            using (SysRtfBLL bll = new SysRtfBLL())
            {
                return bll.UpdateContent(FormID, content, userinfo);
            }
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="content">富文本框内容</param>
        /// <param name="userinfo">用户实体</param>
        /// <returns>bool</returns>
        public bool AddContent(string FormID, byte[] content, string CompanyID, string SystemCode, string ModelName, UserInfo userinfo)
        {
            using (SysRtfBLL bll = new SysRtfBLL())
            {
                T_SYS_RTF t_SYS_RTF = new T_SYS_RTF();
                t_SYS_RTF.RTFID = Guid.NewGuid().ToString();
                t_SYS_RTF.SYSTEMCODE = SystemCode;
                t_SYS_RTF.COMPANYID = CompanyID;
                t_SYS_RTF.MODELNAME = ModelName;
                t_SYS_RTF.FORMID = FormID;
                t_SYS_RTF.OWNERID = userinfo.USERID;
                t_SYS_RTF.OWNERNAME = userinfo.USERNAME;
                t_SYS_RTF.OWNERCOMPANYID = userinfo.COMPANYID;
                t_SYS_RTF.OWNERDEPARTMENTID = userinfo.DEPARTMENTID;
                t_SYS_RTF.OWNERPOSTID = userinfo.POSTID;
                t_SYS_RTF.CREATEUSERID = userinfo.USERID;
                t_SYS_RTF.CREATEUSERNAME = userinfo.USERNAME;
                t_SYS_RTF.CREATECOMPANYID = userinfo.COMPANYID;
                t_SYS_RTF.CREATEDEPARTMENTID = userinfo.DEPARTMENTID;
                t_SYS_RTF.CREATEPOSTID = userinfo.POSTID;
                t_SYS_RTF.CREATEDATE = DateTime.Now;
                t_SYS_RTF.CONTENT = content;
                return bll.AddContent(t_SYS_RTF);
            }
        }

        /// <summary>
        /// 数据是否存在
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>bool</returns>
        public bool IsExits(string FORMID)
        {
            using (SysRtfBLL bll = new SysRtfBLL())
            {
                return bll.IsExits(FORMID);
            }
        }


        public void AddContent(string p, string p_2, string p_3, string p_4, string p_5, UserInfo userInfo)
        {
            throw new NotImplementedException();
        }


        public string GetBusinessObject(string SystemCode, string BusinessObjectName)
        {
            Tracer.Debug("获取元数据表单，系统代码：" + SystemCode + " 业务模块代码：" + BusinessObjectName);
            BusinessObject BO = new BusinessObject();
            string xml=BO.GetBusinessObject(SystemCode, BusinessObjectName);
            return xml;
        }
    }
}
