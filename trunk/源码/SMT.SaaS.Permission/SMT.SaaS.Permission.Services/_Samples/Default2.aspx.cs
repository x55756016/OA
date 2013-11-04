using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;

public partial class Default2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        //string aa = CKEditor1.Text;
        string aa = "";
        string filepath_003 = @"f:\test_003.html";
        
        if (!File.Exists(filepath_003))
        {
            FileStream fs = File.Open(filepath_003, FileMode.Append);
            Console.WriteLine("创建文件" + filepath_003 + "!");
            byte[] b = Encoding.UTF8.GetBytes(aa);
            if (fs.CanWrite)
            {
                fs.Write(b, 0, b.Length);
            }
            fs.Close();
            //StreamWriter写入数据
            StreamWriter sw = new StreamWriter(filepath_003, true);
            sw.Write("使用StreamWriter写入数据：");
            sw.Close();
        }
        else
        {
            //StreamWriter写入数据
            FileStream fs = File.Open(filepath_003, FileMode.Open, FileAccess.ReadWrite);
            byte[] b = Encoding.UTF8.GetBytes(aa);
            if (fs.CanWrite)
            {
                fs.Write(b, 0, b.Length);
            }
            fs.Close();
            //StreamWriter写入数据
            StreamWriter sw = new StreamWriter(filepath_003, true);
            sw.Write("使用StreamWriter写入数据：");
            sw.Close();
        }


        
    }
    protected void Button2_Click(object sender, EventArgs e)
    {
        //CKEditor1.Text = "<p>我是谁";
    }
}