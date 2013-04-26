using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.SAAS.Platform.BLL;
using System.Configuration;
using SMT.SAAS.Platform.Model;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Collections.ObjectModel;
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 为子系统(应用)信息的数据访问提供接口规范
// 完成日期：2011-04-20 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Services
{
    public partial class PlatformServices : IPlatformServices
    {
        private readonly ModuleBLL moduleBll = new ModuleBLL();
        private static string folderName = ConfigurationManager.AppSettings["XapFileName"];

        private static string xmlfilePath = ConfigurationManager.AppSettings["TaskConfigFilePath"];

        /// <summary>
        /// 新增一个ModuleInfo到数据库中
        /// </summary>
        /// <param name="model">ModuleInfo数据</param>
        /// <returns>是否新增成功</returns>
        public bool AddModule(Model.ModuleInfo model)
        {
            return moduleBll.Add(model);
        }

        /// <summary>
        /// 根据系统应用编号获取其详细信息
        /// </summary>
        /// <param name="codes">系统编号列表</param>
        /// <returns>结果，详细信息</returns>
        public System.Collections.Generic.List<Model.ModuleInfo> GetModuleByCodes(string[] moduleCodes)
        {
            return moduleBll.GetModuleByCodes(moduleCodes);
        }

        /// <summary>
        /// 根据用户系统ID获取用户所拥有的系统模块目录信息。
        /// </summary>
        /// <param name="userSysID">用户系统ID</param>
        /// <returns>生成后的模块集合</returns>
        public System.Collections.Generic.List<Model.ModuleInfo> GetModuleCatalogByUser(string userSysID)
        {
            return moduleBll.GetModuleCatalogByUser(userSysID);
        }

        /// <summary>
        /// 根据App文件名称获取对应的详细XAP文件流
        /// </summary>
        /// <param name="fileName">ModuleInfo文件名</param>
        /// <returns>结果，获取到的文件流</returns>
        public byte[] GetModuleFileStream(string fileName)
        {
            string xapFolder = HostingEnvironment.MapPath(folderName);
            return moduleBll.GetFileStream(xapFolder, fileName);
        }

        /// <summary>
        /// 新增一个ModuleInfo到数据库中,并保存其对应的XAP文件
        /// </summary>
        /// <param name="model">ModuleInfo数据</param>
        /// <returns>是否新增成功</returns>
        public bool AddModuleByFile(Model.ModuleInfo model, byte[] xapFileStream)
        {
            string xapFolder = HostingEnvironment.MapPath(folderName);

            return moduleBll.AddModuleInfoByFile(model, xapFolder, xapFileStream);
        }


        public List<ModuleInfo> GetTaskConfigInfoByUser(string userID)
        {
            string configFilePath = HostingEnvironment.MapPath(xmlfilePath);

            return moduleBll.GetTaskConfigInfoByUser(userID, configFilePath);
        }
    }
}