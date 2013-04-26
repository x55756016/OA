using SMT.SAAS.Platform.DAL.Interface;
using SMT.SAAS.Platform.Model;
using System.Collections.Generic;
using SMT.Foundation.Core;
using SMT_Platform_EFModel;
using System.Linq.Dynamic;
using System.Linq;
using System;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供基于Oracle数据库的子系统(应用)信息访问实现
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.OracleDAL
{
    /// <summary>
    /// 提供基于Oracle数据库的子系统(应用)信息访问实现
    /// SMT.SAAS.Platform.OracleDAL.AppDAL
    /// </summary>
    public class ModuleInfoDAL : BaseDAL, IModuleInfoDAL
    {
        private CommonDAL<T_PF_MODULEINFO> _commonDAL;
        private SMT.SAAS.Platform.CommonServices.UserLoginWS.MainUIServicesClient _client;
        private SMT.SAAS.Platform.CommonServices.PermissionWS.PermissionServiceClient _prmClient;

        private static string _configFileVersion = "0.0";
        private static List<ModuleInfo> _cacheTaskModuleInfo = null;

        public ModuleInfoDAL()
        {
            _commonDAL = new CommonDAL<T_PF_MODULEINFO>();
            _client = new CommonServices.UserLoginWS.MainUIServicesClient();
            _prmClient = new CommonServices.PermissionWS.PermissionServiceClient();
        }

        public ModuleInfo GetEntityByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");


            var ents = from ent in _commonDAL.GetObjects()
                       where ent.MODULEID == key
                       select ent;
            T_PF_MODULEINFO application = ents.Count() > 0 ? ents.FirstOrDefault() : null;

            return application.CloneObject<ModuleInfo>(new ModuleInfo());
        }

        public bool IsExists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");


            var ents = from ent in _commonDAL.GetObjects()
                       where ent.MODULEID == key
                       select ent;
            return ents.Count() > 0 ? true : false;

        }

        public bool Add(ModuleInfo entity)
        {
            T_PF_MODULEINFO model = entity.CloneObject<T_PF_MODULEINFO>(new T_PF_MODULEINFO());
            model = Utility.CreateCommonProperty().CloneObject<T_PF_MODULEINFO>(model);

            return _commonDAL.Add(model);
        }

        public bool Update(ModuleInfo entity)
        {
            T_PF_MODULEINFO model = entity.CloneObject<T_PF_MODULEINFO>(new T_PF_MODULEINFO());

            model = Utility.CreateCommonProperty().CloneObject<T_PF_MODULEINFO>(model);


            return _commonDAL.Update(model);
        }

        public bool Delete(ModuleInfo entity)
        {
            throw new System.NotImplementedException();
        }

        public List<ModuleInfo> GetListByPager(int index, int size, ref int count, string sort)
        {
            throw new System.NotImplementedException();
        }

        public List<ModuleInfo> GetModuleListByCodes(string[] appCodes)
        {
            if (appCodes == null)
                throw new ArgumentNullException("appCodes");


            var ents = from ent in _commonDAL.GetObjects()
                       where appCodes.Contains(ent.MODULECODE)
                       select ent;
            List<ModuleInfo> result = new List<ModuleInfo>();
            if (ents.Count() > 0)
            {
                foreach (var item in ents)
                {
                    result.Add(item.CloneObject<ModuleInfo>(new ModuleInfo()));
                }
            }
            return result;
        }

        public List<ModuleInfo> GetModulesBySystemCode(string[] appCodes)
        {
            if (appCodes == null)
                throw new ArgumentNullException("appCodes");


            var ents = from ent in _commonDAL.GetObjects()
                       where appCodes.Contains(ent.PARENTMODULEID)
                       select ent;
            List<ModuleInfo> result = new List<ModuleInfo>();
            if (ents.Count() > 0)
            {
                foreach (var item in ents)
                {
                    result.Add(item.CloneObject<ModuleInfo>(new ModuleInfo()));
                }
            }
            return result;
        }

        public List<ModuleInfo> GetModulesBySystemCodeAsNew(string[] appCodes)
        {
            if (appCodes == null)
                throw new ArgumentNullException("appCodes");


            var ents = from ent in _commonDAL.GetObjects()
                       where appCodes.Contains(ent.PARENTMODULEID)
                       select new ModuleInfo {
                           ModuleID = ent.MODULEID,
                           ModuleCode = ent.MODULECODE,
                           ModuleName = ent.MODULENAME,
                           ModuleType = ent.MODULETYPE,
                           ModuleIcon = ent.MODULEICON,
                           ParentModuleID = ent.PARENTMODULEID,
                           ClientID = ent.CLIENTID,
                           ServerID = ent.SERVERID,
                           Version = ent.VERSION,
                           FileName = ent.FILENAME,
                           FilePath = ent.FILEPATH,
                           EnterAssembly = ent.ENTERASSEMBLY,
                           HostAddress = ent.HOSTADDRESS,
                           Description = ent.DESCRIPTION,
                           UseState = "0"
                       };
            List<ModuleInfo> result = new List<ModuleInfo>();
            if (ents.Count() > 0)
            {
                result = ents.ToList();
            }
            return result;
        }

        public List<ModuleInfo> GetModuleCatalogByUser(string userSysID)
        {
            try
            {
                string result = string.Empty;
                string syscodes = _client.GetSystemTypeByUserID(userSysID, ref result);
                string[] codelist = syscodes.Split(',');

                List<ModuleInfo> moduleinfos = GetModulesBySystemCode(codelist);
                List<ModuleInfo> tempModules = new List<ModuleInfo>();

                var menuList = from ent in _prmClient.GetSysLeftMenuFilterPermissionToNewFrame(userSysID)
                               orderby ent.SYSTEMTYPE, ent.ORDERNUMBER
                               select ent;


                foreach (var item in menuList)
                {
                    var childs = menuList.Where(mm => mm.EntityMenuFatherID == item.ENTITYMENUID);

                    ModuleInfo parentModule = moduleinfos.FirstOrDefault(e => (e.ModuleCode == item.SYSTEMTYPE));

                    ModuleInfo module = new ModuleInfo()
                    {
                        ModuleCode = item.MENUCODE,
                        ModuleIcon = item.MENUICONPATH,
                        ModuleName = item.ENTITYMENUID,
                        ParentModuleID = item.EntityMenuFatherID,
                        ModuleID = item.ENTITYMENUID,
                        SystemType = item.SYSTEMTYPE,
                        ClientID = item.CHILDSYSTEMNAME,
                        ModuleType = item.URLADDRESS,
                        Description = item.MENUNAME,
                        UseState = "0"

                    };

                    if ((childs.Count() > 0))
                    {
                        module.ParentModuleID = item.SYSTEMTYPE;
                    }
                    //配置菜单的依赖系统关系
                    module.DependsOn = new System.Collections.ObjectModel.Collection<string>();
                    if (parentModule != null)
                    {
                        module.DependsOn.Add(parentModule.ModuleName);

                        if (item.CHILDSYSTEMNAME != null)
                        {
                            //SMT.FBAnalysis.UI
                            if (parentModule.ModuleName != item.CHILDSYSTEMNAME && parentModule.ModuleName != "SMT.SaaS.LM.UI")
                            {
                                module.DependsOn.Add(item.CHILDSYSTEMNAME);
                                //Debug.WriteLine(parentModule.ModuleName+" => "+item.CHILDSYSTEMNAME);
                            }
                        }
                    }

                    if (item.URLADDRESS != null)
                    {
                        if (item.URLADDRESS.IndexOf("[mvc]") >= 0)
                            module.ModuleType = item.URLADDRESS;   //增加平台兼容地址获取
                        else
                            RefreshModuleType(parentModule, module, item.CHILDSYSTEMNAME, item.URLADDRESS);
                    }

                    tempModules.Add(module);
                }

                moduleinfos.AddRange(tempModules);
                return moduleinfos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 从新刷新每个菜单的所属类型。此处数据原本是由权限系统完整的提供
        /// 包含的基本信息为：AssemblyQualifiedName，InitParams
        /// 后续要考虑 系统 与 模块之间的关系 如何使用权限系统 结合平台进行统一控制。
        /// 平台中的所有系统以及 模块信息  与权限系统是相同的。
        /// 授权机制 与 访问模块控制均要由权限系统提供基础数据的支持
        /// </summary>
        /// <param name="menu"></param>
        private void RefreshModuleType(ModuleInfo parentModule, ModuleInfo itemModule, string childSystemName, string uriAddress)
        {
            try
            {
                if (itemModule == null)
                    throw new ArgumentNullException("itemModule");
                if (uriAddress == null)
                    throw new ArgumentNullException("uriAddress");

                string temp = " , Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                string assemblyName = "";
                StringBuilder typeName = new StringBuilder();
                if (uriAddress != null)
                {
                    if (parentModule != null)
                        assemblyName = parentModule.EnterAssembly;

                    if (childSystemName != null)
                    {
                        assemblyName = childSystemName;
                    }

                    if (assemblyName != "SMT.FB.UI")
                    {
                        if (assemblyName == "SMT.EDM.UI")
                        {
                            #region 处理进销存

                            string tempUri = "";

                            string typetemp = uriAddress;
                            if (typetemp.Contains("?"))
                            {
                                string[] partUri = typetemp.Split('?');
                                tempUri = partUri[0].Replace('\\', '.');
                                tempUri = tempUri.Replace('/', '.');
                                Dictionary<string, string> initparam = new Dictionary<string, string>();
                                if (partUri.Length > 1)
                                {
                                    string[] parames = partUri[1].Split(',');
                                    if (parames.Length > 0)
                                    {
                                        foreach (var item in parames)
                                        {
                                            string[] param = item.Split('=');
                                            initparam.Add(param[0], param[1]);
                                        }
                                    }
                                    itemModule.InitParams = initparam;
                                }
                            }
                            else
                            {
                                tempUri = uriAddress.Replace('\\', '.');
                                tempUri = tempUri.Replace('/', '.');
                            }

                            typeName.Append(assemblyName);
                            typeName.Append(".Views");
                            typeName.Append(tempUri);
                            typeName.Append(", ");
                            typeName.Append(assemblyName);
                            typeName.Append(temp);


                            itemModule.ModuleType = typeName.ToString();
                            #endregion
                        }
                        else
                        {
                            #region 正常处理

                            string defaultName = ".Views";
                            if (assemblyName == "SMT.FlowDesigner")
                                defaultName = ".BMP";

                            if (assemblyName == "SMT.Workflow.Platform.Designer")
                                defaultName = "";

                            string newaddress = uriAddress.Replace("/", ".");

                            typeName.Append(assemblyName);
                            typeName.Append(defaultName);
                            typeName.Append(newaddress);
                            typeName.Append(", ");
                            typeName.Append(assemblyName);
                            typeName.Append(temp);

                            itemModule.ModuleType = typeName.ToString();

                            #endregion
                        }
                    }
                    else
                    {
                        #region 处理预算

                        string typetemp = uriAddress;

                        string[] partUri = typetemp.Split('?');
                        typeName.Append(assemblyName);
                        typeName.Append(".Views");
                        typeName.Append(partUri[0].Replace('/', '.'));
                        typeName.Append(", ");
                        typeName.Append(assemblyName);
                        typeName.Append(temp);

                        itemModule.ModuleType = typeName.ToString();

                        Dictionary<string, string> initparam = new Dictionary<string, string>();
                        if (partUri.Length > 1)
                        {
                            string[] parames = partUri[1].Split(',');
                            if (parames.Length > 0)
                            {
                                foreach (var item in parames)
                                {
                                    string[] param = item.Split('=');
                                    initparam.Add(param[0], param[1]);
                                }
                            }
                            itemModule.InitParams = initparam;
                        }


                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<ModuleInfo> GetTaskConfigInfoByUser(string userSysID, string configFilePath)
        {
            var modules = GetTaskFromConfigInfo(configFilePath);


            /// <param name="PermissionValue">权限值：0：新建；1：修改； 2：删除；3:查看；4：公文发布；5：新闻发布；6：转发 </param>
            var menuList = _prmClient.GetSysLeftMenuFilterPermissionToNewFrameAndPermission(userSysID,"0");

            List<string> codes = new List<string>();
            //获取所有的menuCode
            foreach (var item in menuList)
            {
                if (!string.IsNullOrEmpty(item.MENUCODE))
                {
                    codes.Add(item.MENUCODE);

                    if (!codes.Contains(item.SYSTEMTYPE))
                        codes.Add(item.SYSTEMTYPE);

                }
            }

            var userMOdules = from item in modules
                              where codes.Contains(item.ModuleCode)
                              select item;

            //根据用户过滤
            //获取用户有权限的模块。

            return userMOdules.ToList();
        }

        private List<ModuleInfo> GetTaskFromConfigInfo(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            XElement xmlClient = XElement.Load(filePath);
            //检测版本号
            var version = (from element in xmlClient.DescendantsAndSelf("ConfigTask")
                           select element).FirstOrDefault().
                           Attributes(XName.Get("Version")).FirstOrDefault().Value;

            if (version != _configFileVersion)
            {
                _configFileVersion = version;
                _cacheTaskModuleInfo = AnalysisTaskConfig(xmlClient);
            }

            if (_cacheTaskModuleInfo == null)
            {
                _cacheTaskModuleInfo = AnalysisTaskConfig(xmlClient);
            }

            return _cacheTaskModuleInfo;
        }

        private List<ModuleInfo> AnalysisTaskConfig(XElement xmlClient)
        {
            var sysElement = from element in xmlClient.DescendantsAndSelf("SystemType")
                             select element;
            List<ModuleInfo> taskModules = new List<ModuleInfo>();

            foreach (var item in sysElement)
            {
                XElement xitem = item;

                var list = from element in item.DescendantsAndSelf("Task")
                           select element;

                var systemModule = new Model.ModuleInfo
                {
                    ModuleID = xitem.Attributes(XName.Get("TypeId")).FirstOrDefault().Value,
                    ModuleName = xitem.Attributes(XName.Get("Name")).FirstOrDefault().Value,
                    ModuleCode = xitem.Attributes(XName.Get("TypeId")).FirstOrDefault().Value,
                    ModuleType = xitem.Attributes(XName.Get("Type")).FirstOrDefault().Value
                   
                };
                taskModules.Add(systemModule);

                foreach (var taskElement in list)
                {
                    var moduleinfo = new Model.ModuleInfo
                    {
                        ModuleName = taskElement.Attributes(XName.Get("Name")).FirstOrDefault().Value,
                        ModuleCode = taskElement.Attributes(XName.Get("ModuleCode")).FirstOrDefault().Value,
                        ModuleType = taskElement.Attributes(XName.Get("TaskType")).FirstOrDefault().Value,
                        ModuleIcon = taskElement.Attributes(XName.Get("Icon")).FirstOrDefault().Value
                    };

                    //指定所属系统
                    moduleinfo.SystemType = xitem.Attributes(XName.Get("TypeId")).FirstOrDefault().Value;
                    //解析参数列表
                    var param = taskElement.Attributes(XName.Get("Params")).FirstOrDefault().Value;
                    if (param.Length > 0)
                    {
                        Dictionary<string, string> paramsList = new Dictionary<string, string>();
                        foreach (var itemparam in param.Split(';'))
                        {
                            var keyValue = itemparam.Split('=');
                            paramsList.Add(keyValue[0], keyValue[1]);
                        }
                        moduleinfo.InitParams = paramsList;
                    }
                    //解析依赖项

                    var depends = taskElement.Attributes(XName.Get("Depends")).FirstOrDefault().Value;
                    if (depends.Length > 0)
                    {
                        Collection<string> dependsList = new Collection<string>();
                        foreach (var dependsitem in depends.Split(';'))
                        {
                            dependsList.Add(dependsitem);
                        }
                        moduleinfo.DependsOn = dependsList;
                    }

                    taskModules.Add(moduleinfo);
                }
            }

            return taskModules;
        }
    }
}

