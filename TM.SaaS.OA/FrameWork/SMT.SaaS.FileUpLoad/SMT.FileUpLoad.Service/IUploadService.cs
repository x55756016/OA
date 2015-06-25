using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using TM_SaaS_OA_EFModel;
namespace SMT.FileUpLoad.Service
{
    /// <summary>
    /// 用于描述客户端调用服务端方法的返回结果基类
    /// </summary>
    [DataContract]
    public class CallBackResult
    {
        /// <summary>
        /// 获取或设置当前方法调用时服务器端发生的异常，Null表示没有异常
        /// </summary>
        [DataMember]
        public System.String Error { get; set; }
        /// <summary>
        /// 返回给客户端的文件列表
        /// </summary>
        [DataMember]
        public List<SMT_FILELIST> FileList { get; set; }
        /// <summary>
        /// 下载地址
        /// </summary>
        [DataMember]
        public string DownloadUrl { get; set; }
      
    }
    [ServiceContract]
    public interface IUploadService
    {
        /// <summary>
        ///  获取公司对上传文件的设置信息
        /// </summary>
        /// <param name="companycode">公司代码</param>
        /// <returns></returns>
        [OperationContract]
        UserFile GetCompanyFileSetInfo(string companycode);

        /// <summary>
        /// 获取公司对上传文件的设置信息
        /// </summary>
        /// <param name="companycode">公司代码</param>
        /// <param name="CompanyName">公司名称</param>
        /// <returns></returns>
        [OperationContract]
        UserFile GetCompanyFileByCompanyCodeAndName(string companycode,string CompanyName);
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <param name="parameters"></param>
        /// <param name="firstChunk"></param>
        /// <param name="lastChunk"></param>
        [OperationContract]
        string SaveUpLoadFile(string strSystemCode, string strModelCode, string strFileName, string strMd5Name, string strID, string strGuid, byte[] data, int BytesUploaded, int dataLength, bool firstChunk, bool lastChunk, string strCreateUserID, UserFile model);

        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <param name="parameters"></param>
        /// <param name="firstChunk"></param>
        /// <param name="lastChunk"></param>
        [OperationContract]
        string SaveUploadFileForMvc(string strSystemCode, string strModelCode, string strFileName, string strMd5Name, string strID, string strGuid, string strCreateUserID, UserFile model);
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <param name="parameters"></param>
        /// <param name="firstChunk"></param>
        /// <param name="lastChunk"></param>
        [OperationContract]
        string SaveUpLoadFileIsExist(string strSystemCode, string strModelCode, string strFileName, string strMd5Name, string strID, string strGuid, byte[] data, int BytesUploaded, int dataLength, bool firstChunk, bool lastChunk, string strCreateUserID, UserFile model);
      
        /// <summary>
        /// 取消上传
        /// </summary>
        /// <param name="filename"></param>
        [OperationContract]
        void CancelUpload(string filename);

        /// <summary>
        /// 检查文件是否已上传
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        [OperationContract]
        UserFile CheckFileExists(UserFile model, string md5Name);
        #region 获取文件列表
        [OperationContract]
        CallBackResult GetFileListByCompanyCode(string companycode);
        [OperationContract]
        CallBackResult GetFileListBySystemCode(string companycode, string systemcode);
        [OperationContract]
        CallBackResult GetFileListByModelCode(string companycode, string systemcode, string modelcode);
        [OperationContract]
        CallBackResult GetFileListByApplicationID(string companycode, string systemcode, string modelcode, string applicationid,string createname);
        [OperationContract]
        CallBackResult GetFileListByOnlyApplicationID(string applicationid);
        #endregion
        #region 删除文件
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string DeleteFile(string smtfilelistid, string filepath);
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string DeleteFileByUrl(string filepath);
        [OperationContract]
        string DeleteFileByApplicationID(string companycode, string systemcode, string modelcode, string applicationid);
        [OperationContract]
        string DeleteFileByModelCode(string companycode, string systemcode, string modelcode);
        [OperationContract]
        string DeleteFileBySystemCode(string companycode, string systemcode);
        [OperationContract]
        string DeleteFileByCompanycode(string companycode);
        [OperationContract]
        string DeleteFileByOnlyApplicationID(string applicationid);
        [OperationContract]
        string DeleteFileByApplicationIDAndFileName(string applicationid,string FileName);
        [OperationContract]
        string CreateCompanyDirectory(string companyID,string companyName,string companyCode);
        #endregion
        //[OperationContract]
        //bool AddSMT_FILELIST(SMT_FILELIST model);
        //[OperationContract]
        //bool DeleteSMT_FILELIST(string serviceid);
        //[OperationContract]       
    }

}
