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

namespace SMT.FileUpLoad.Classes
{
    public class UserConfig
    {
        #region 数据表的文件属性
        /// <summary>
        /// 主键ID(数据库)
        /// </summary>            
        public string SmtFileListId { get; set; }
        /// <summary>
        /// 文件名（用户自定义）
        /// </summary>    
        public string CustomFileName { get; set; }
        /// <summary>
        /// 文件名(原始文件名)
        /// </summary>    
        public string FileName { get; set; }
        /// <summary>
        /// 文件类型(.doc、.xls、.txt、.pdf......)
        /// </summary>    
        public string FileType { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>  
        public string FileSize { get; set; }
        /// <summary>
        /// 文件地址
        /// </summary>  
        public string FileUrl { get; set; }
        /// <summary>
        /// 公司代号
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 是否限制公司查询条件，默认为true
        /// </summary>
        private bool _IsLimitCompanyCode = true;
        public bool IsLimitCompanyCode {

            get {
                return _IsLimitCompanyCode;
            }
            set {
                _IsLimitCompanyCode = value;
            }
        }
        /// <summary>
        /// 公司名字
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 系统代号
        /// </summary> 
        public string SystemCode { get; set; }
        /// <summary>
        /// 模块代号
        /// </summary>  
        public string ModelCode { get; set; }
        /// <summary>
        /// 缩略图地址
        /// </summary>   
        public string ThumbnailUrl { get; set; }
        /// <summary>
        /// 排序
        /// </summary>  
        public decimal Indexl { get; set; }
        /// <summary>
        /// 备注
        /// </summary>    
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>      
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>  
         public string CreateName { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>    
       public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>    
        public string UpdateName { get; set; }
        /// <summary>
        /// 文件类型（配置文件）
        /// </summary>     
        private string Type { get; set; }
        /// <summary>
        /// 最大上传大小如：（3.GB、4.MB、4.KB）
        /// </summary>            
        public string MaxSize { get; set; }
        /// <summary>
        /// 最大上传速度
        /// </summary>  
        private double UploadSpeed { get; set; }

        #endregion
        #region 变量定义

        /// <summary>
        /// 最大允许上传（一次最大可上传多少个文件）
        /// </summary>
        public int MaxConcurrentUploads { get; set; }
        /// <summary>
        /// 每次上传块大小
        /// </summary>
        private long UploadChunkSize { get; set; }
        /// <summary>
        /// 是否允许切换图片显示
        /// </summary>
        private bool ResizeImage { get; set; }
        /// <summary>
        /// 图片的大小
        /// </summary>
        private int ImageSize { get; set; }
        /// <summary>
        /// 开始上传时间
        /// </summary>
        private DateTime start;
        /// <summary>
        /// 文件上传过滤
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// 是否允许选择多个文件
        /// </summary>
        public bool Multiselect { get; set; }
        /// <summary>
        /// 上传指定的Url
        /// </summary>
        private Uri UploadUrl { get; set; }
        /// <summary>
        /// 是否上传中
        /// </summary>
        private bool uploading { get; set; }
        /// <summary>
        /// 可上传数(数量)
        /// </summary>
        private double TotalUploadSize { get; set; }
        /// <summary>
        /// 已上传总量(数量)
        /// </summary>
        private double TotalUploaded { get; set; }
        /// <summary>
        /// 还能上传多少量(大小)
        /// </summary>
        private long MaximumTotalUpload { get; set; }
        /// <summary>
        /// 可上传量（大小）
        /// </summary>
        private long MaximumUpload = 1073741824000;
        private int MaxNumberToUpload { get; set; }       
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 业务ID
        /// </summary>
        public string ApplicationID { get; set; }
        
        #endregion
        #region 控钮的控制
        /// <summary>
        /// 只显示自己的记录
        /// </summary>
        public bool OnlyShowMySelf { get; set; }
        /// <summary>
        /// 不显示[删除]按钮
        /// </summary>
        public bool NotShowDeleteButton { get; set; }
        /// <summary>
        /// 不显示[上传]按钮
        /// </summary>
        public bool NotShowUploadButton { get; set; }
         /// <summary>
        /// 不显示[选择文件]按钮
        /// </summary>
        public bool NotShowSelectButton { get; set; }
         /// <summary>
        /// 不显示[清空]按钮
        /// </summary>
        public bool NotShowClearButton { get; set; }
         /// <summary>
        /// 不显示[缩略图]选项
        /// </summary>
        public bool NotShowThumbailChckBox { get; set; }
        /// <summary>
        /// 每页显示的记录数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 不允许删除(DataGrid里的删除)
        /// </summary>
        public bool NotAllowDelete { get; set; }
        /// <summary>
        /// 不允许下载(DataGrid里的下载)
        /// </summary>
        public bool NotAllowDownload { get; set; }
        #endregion
    }
}
