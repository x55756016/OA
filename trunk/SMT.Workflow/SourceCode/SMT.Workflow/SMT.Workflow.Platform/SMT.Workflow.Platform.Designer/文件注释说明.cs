/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/5/18 9:25:36   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer 
	 * 描　　述： 规范说明文件的注释
	 * 模块名称：工作流设计器
* ---------------------------------------------------------------------*/
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
/*-----------------------------命名规则----------------------------------------  
 * 所有属性,方法,常量的第一个字母都要大写,如果是变量的第一个字母小写
 * ◆类：使用名词、名词+短语组合命名。比如：publlic class FileStream；
 * ◆属性：使用名词、形容词英文大小写组合。比如：publlic string UserName；
 * ◆方法：使用动词、动词+短语英文大小写组合。比如：CreateUser（）， RemoveAt（）等；
 * ◆接口：以 I 开始，后面加上名词、名词+短语组合、形容词命名。比如：IDisposable；
 * ◆常量：所有单词大写，多个单词之间用 "_" 隔开
 * 	常用动词:
 * 	Get:获取\得到\返回
 * 	Set:设置
 * 	Query:查看
 * 	Add:增加
 * 	Edit:修改\编辑
 * 	Delete:删除
 * 	Save:保存
 * 	Cancel:取消
 * 	Audit: 审核
 * 	Open:打开
 * 	Close:关闭
 * 	Send:发送
 * 	Receive:接收
 * Initi: 初始化 initialize的简写
* ---------------------------------------------------------------------*/
namespace SMT.Workflow.Platform.Designer
{
    /// <summary>
    /// 文件注释说明
    /// </summary>
    public class FileNotes
    {
        #region 事件定义
        /// <summary>
        ///  点击事件
        /// </summary>
        public event EventHandler Onclick;
        /// <summary>
        /// 执行操作完成事件 比如增删改查
        /// </summary>
        public event EventHandler OnExectNoQueryCompleted;      
        #endregion
        #region 变量定义
        /// <summary>
        /// 文件名
        /// </summary>
        private string fileName;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime startTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime endTime;
        /// <summary>
        /// 当前页引索
        /// </summary>
        int curentIndex = 1;
        #endregion
        #region 属性定义
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 创建人的姓名
        /// </summary>
        public string CreateName { get; set; }
        /// <summary>
        /// 创建时间(日期+时间)如:2011/5/18 9:25:36   
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 异常/错误信息
        /// </summary>
        public Exception Error { get; set; }
        #endregion
        #region 私有方法
        #region WCF注册事件和返回结果
        private void IntiWcfEvent()
        {
        }
        #endregion
        #region 加载数据
        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
        }
        #endregion
        #endregion
        #region 公共方法
        #region 通过用户的ID获取其所在的部门名称
        /// <summary>
        /// 通过用户的ID获取其所在的部门名称
        /// </summary>
        /// <param name="userId">用户的ID</param>
        /// <returns></returns>
        public string GetDepartmentNameByUserID(string userId)
        {
            return "";
        }
        #endregion
        #endregion
        #region UI控件按钮事件
        #region 查看事件
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
        }       
        #endregion
        #region 新增事件
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
        }    
        #endregion
        #region 修改事件
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {

        }    
        #endregion
        #region 删除事件
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
        }  
        #endregion            
        #endregion

    }
}
