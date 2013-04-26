using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT.SAAS.Platform.Model;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 声明平台使用服务接口，此服务接口主要为平台使用过程中提供支持。
// 完成日期：2011-04-20 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Services
{
    // 注意: 如果更改此处的接口名称 "IPlatformServices"，也必须更新 Web.config 
    // 中对 "IPlatformServices" 的引用。
    [ServiceContract]
    public interface IPlatformServices
    {
        [OperationContract]
        bool AddModule(ModuleInfo model);

        [OperationContract]
        List<ModuleInfo> GetModuleByCodes(string[] moduleCodes);

        [OperationContract]
        byte[] GetModuleFileStream(string fileName);

        [OperationContract]
        bool AddModuleByFile(Model.ModuleInfo model, byte[] xapFileStream);

        [OperationContract]
        List<ModuleInfo> GetModuleCatalogByUser(string userSysID);

        [OperationContract]
        List<ShortCut> GetShortCutByUser(string userSysID);

        [OperationContract]
        bool AddShortCutByUser(List<ShortCut> models, string userID);

        [OperationContract]
        bool RemoveShortCutByUser(string shortCutID, string userID);

        [OperationContract]
        List<ModuleInfo> GetTaskConfigInfoByUser(string userID);
        ///// <summary>
        ///// 包括系统基础参数集合 xml格式
        ///// 快捷方式列表 List<ShortCut>
        ///// WebPart列表 List<WebPart>
        ///// </summary>
        ///// <param name="userSysID">
        ///// 
        ///// </param>
        ///// <returns></returns>
        //[OperationContract]
        //List<ShortCut> GetUserConfigInfo(string userSysID);
    }
}
