using SMT.HRM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TM_SaaS_OA_EFModel;


namespace SMT.SaaS.Permission.Services
{
    public partial class FileDownload : System.Web.UI.Page
    {
        private string username = string.Empty;
        private string password = string.Empty;
        private string filename = string.Empty;
        PermissionService perm = new PermissionService();
        protected void Page_Load(object sender, EventArgs e)
        {

            username = dosomething(this.Request["username"]);
            password = dosomething(this.Request["password"]);
            filename = this.Request["filename"];
            filename = HttpUtility.UrlDecode(filename);
            //password = HttpUtility.UrlDecode(password);

            //string bb = HttpUtility.UrlDecode("");
            //username = "Admin";
            //password = "elRG2otBnxgLSC25NDd04Q==";
            //string aa = HttpUtility.UrlEncode(password);
            //string cc = HttpUtility.UrlDecode(aa);
            //filename = "UpLoadFiles\\初始化公司\\OA\\Archives\\在线通讯录20101227(1).xls";

            T_SYS_USER user = new T_SYS_USER();
            user = perm.UserLogin(username, password);

            //if()

            if (user != null)
            {
                System.IO.Stream iStream = null;
                string filepath = Server.MapPath(filename);
                string files = System.IO.Path.GetFileName(filepath);
                long dataToRead;
                int length;
                byte[] buffer = new Byte[300000];

                System.IO.FileInfo file = new System.IO.FileInfo(filepath);

                try
                {
                    iStream = new System.IO.FileStream(filepath, System.IO.FileMode.Open,
                    System.IO.FileAccess.Read, System.IO.FileShare.Read);

                    //   Total   bytes   to   read:   
                    dataToRead = iStream.Length;

                    Response.ContentType = "application/octet-stream";
                    Response.Charset = "UTF-8";
                    Response.AddHeader("Content-Disposition", "attachment;   filename=" + System.Web.HttpUtility.UrlEncode(file.Name, System.Text.Encoding.UTF8));

                    //   Read   the   bytes.   
                    while (dataToRead > 0)
                    {
                        //   Verify   that   the   client   is   connected.   
                        if (Response.IsClientConnected)
                        {
                            //   Read   the   data   in   buffer.   
                            length = iStream.Read(buffer, 0, 300000);

                            //   Write   the   data   to   the   current   output   stream.   
                            Response.OutputStream.Write(buffer, 0, length);

                            //   Flush   the   data   to   the   HTML   output.   
                            Response.Flush();

                            buffer = new Byte[300000];
                            dataToRead = dataToRead - length;
                        }
                        else
                        {
                            //prevent   infinite   loop   if   user   disconnects   
                            dataToRead = -1;
                        }

                    }
                    Response.Clear();
                    Response.End();
                }
                catch (Exception ex)
                {
                    //   Trap   the   error,   if   any.   
                    Response.Write("Error   :   " + ex.Message);
                    Response.Clear();
                    Response.End();
                }
                finally
                {
                    if (iStream != null)
                    {
                        //Close   the   file.   
                        iStream.Close();
                    }
                    Response.Clear();
                    Response.End();
                }
            }
            else
            {
                string StrScript;
                string strMsg = "你无权限下载，请联系管理员！";
                StrScript = ("<script language=javascript>");
                StrScript += ("alert('" + strMsg + "');");
                //StrScript += ("window.location='" + URL + "';");
                StrScript += ("</script>");
                System.Web.HttpContext.Current.Response.Write(StrScript);

            }

        }

        private string dosomething(string value)
        {
            return value;
        }

    }
}