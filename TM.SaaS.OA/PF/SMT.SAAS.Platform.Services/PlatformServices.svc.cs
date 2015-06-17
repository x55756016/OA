using SMT.SAAS.Platform.BLL;
using SMT.SAAS.Platform.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web.Hosting;

namespace SMT.SAAS.Platform.Services
{
    // 注意: 如果更改此处的类名 "PlatformServices"，也必须更新 Web.config 中对 "PlatformServices" 的引用。
 
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceContract]
    public partial class PlatformServices
    {
        private readonly ModuleBLL moduleBll = new ModuleBLL();
        private ShortCutBLL _bll = new ShortCutBLL();
        private static string folderName = ConfigurationManager.AppSettings["XapFileName"];
        private static string xmlfilePath = ConfigurationManager.AppSettings["TaskConfigFilePath"];

        /// <summary>
        /// 新增一个ModuleInfo到数据库中
        /// </summary>
        /// <param name="model">ModuleInfo数据</param>
        /// <returns>是否新增成功</returns>
        [OperationContract]
        public bool AddModule(Model.ModuleInfo model)
        {
            return moduleBll.Add(model);
        }

        /// <summary>
        /// 根据系统应用编号获取其详细信息
        /// </summary>
        /// <param name="codes">系统编号列表</param>
        /// <returns>结果，详细信息</returns>
        [OperationContract]
        public System.Collections.Generic.List<Model.ModuleInfo> GetModuleByCodes(string[] moduleCodes)
        {

            return moduleBll.GetModuleByCodes(moduleCodes);

        }

        /// <summary>
        /// 根据用户系统ID获取用户所拥有的系统模块目录信息。
        /// </summary>
        /// <param name="userSysID">用户系统ID</param>
        /// <returns>生成后的模块集合</returns>
        [OperationContract]
        public System.Collections.Generic.List<Model.ModuleInfo> GetModuleCatalogByUser(string userSysID)
        {
            return moduleBll.GetModuleCatalogByUser(userSysID);
        }

        /// <summary>
        /// 根据App文件名称获取对应的详细XAP文件流
        /// </summary>
        /// <param name="fileName">ModuleInfo文件名</param>
        /// <returns>结果，获取到的文件流</returns>
        [OperationContract]
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
        [OperationContract]
        public bool AddModuleByFile(Model.ModuleInfo model, byte[] xapFileStream)
        {
            string xapFolder = HostingEnvironment.MapPath(folderName);

            return moduleBll.AddModuleInfoByFile(model, xapFolder, xapFileStream);
        }

        [OperationContract]
        public List<ModuleInfo> GetTaskConfigInfoByUser(string userID)
        {
            string configFilePath = HostingEnvironment.MapPath(xmlfilePath);

            return moduleBll.GetTaskConfigInfoByUser(userID, configFilePath);
        }

        [OperationContract]
        public List<Model.ShortCut> GetShortCutByUser(string userSysID)
        {
            return _bll.GetShortCutByUser(userSysID);
        }

        [OperationContract]
        public bool AddShortCutByUser(List<Model.ShortCut> models, string userID)
        {
            return _bll.AddByListAndUser(models, userID);
        }
        [OperationContract]
        public bool RemoveShortCutByUser(string shortCutID, string userID)
        {
            return _bll.DeleteShortCutByUser(shortCutID, userID);
        }


        #region 用户快捷菜单
        [OperationContract]
        public bool AddShortCut(Model.ShortCut model)
        {
            return _bll.Add(model);
        }
        [OperationContract]
        public bool AddShortCutByList(List<Model.ShortCut> models)
        {
            return _bll.AddByList(models);
        }
        #endregion
    }
}
