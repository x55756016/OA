using System;
using SMT.SAAS.Platform.DALFactory;
using SMT.SAAS.Platform.Model;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using SMT.SAAS.Platform.DAL;
using TM_SaaS_OA_EFModel;
using SMT.HRM.BLL.Permission;
using SMT.Foundation.Log;
using SMT.HRM.DAL.Permission;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using SMT.HRM.CustomModel.Permission;
namespace SMT.SAAS.Platform.BLL
{
    /// <summary>
    /// 应用系统业务操作接口，提供了对数据库、第三方服务、文件系统的访问提供支持。
    /// </summary>
    public class ModuleBLL:IDisposable
    {
        private static readonly ModuleInfoDAL dal = new ModuleInfoDAL();

        private static List<ModuleInfo> _cacheTaskModuleInfo = null;

        private static string _configFileVersion = "0.0";
        /// <summary>
        /// 新增一个ModuleInfo到数据库中
        /// </summary>
        /// <param name="model">ModuleInfo数据</param>
        /// <returns>是否新增成功</returns>
        public bool Add(ModuleInfo model)
        {
            try
            {
                return dal.Add(model);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }

        /// <summary>
        /// 新增一个ModuleInfo到数据库中,并保存其对应的XAP文件
        /// </summary>
        /// <param name="model">ModuleInfo数据</param>
        /// <returns>是否新增成功</returns>
        public bool AddModuleInfoByFile(Model.ModuleInfo model, string folderName, byte[] xapFileStream)
        {
            try
            {
                string filePath = UploadFile(folderName, model.FileName, xapFileStream, false);
                if (filePath.Length > 0)
                {
                    model.FilePath = filePath;
                    return this.Add(model);
                }
                return false;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }

        /// <summary>
        /// 根据文件路径获取文件流
        /// </summary>
        /// <param name="xapFolder">所属文件夹</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件流</returns>
        public byte[] GetFileStream(string folderName, string fileName)
        {
            try
            {
                if (Directory.Exists(folderName))
                {
                    string filePath = folderName + @"\" + fileName;
                    FileMode fileMode = FileMode.Open;
                    if (File.Exists(filePath))
                    {
                        using (FileStream fileStream = new FileStream(filePath, fileMode, FileAccess.Read, FileShare.ReadWrite))
                        {
                            byte[] fileBytes = new byte[fileStream.Length];
                            fileStream.Read(fileBytes, 0, fileBytes.Length);
                            fileStream.Close();
                            return fileBytes;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return null;
            }
        }

        /// <summary>
        /// 写入一个文件
        /// </summary>
        /// <param name="xapFolder">所属文件夹</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件流</returns>
        public string UploadFile(string folderName, string fileName, byte[] fileData, bool isAppend)
        {
            try
            {
                //文件上传所在目录,目录不存在则新建一个
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                string path = folderName + @"\" + fileName;
                if (File.Exists(path))
                    File.Delete(path);

                //文件读写模式，如果参数为True则表示续传，否则以附加模式写到现有文件中
                FileMode fileMode = isAppend ? FileMode.Append : FileMode.Create;
                using (System.IO.FileStream fs = new System.IO.FileStream(path, fileMode, System.IO.FileAccess.Write))
                {
                    fs.Write(fileData, 0, fileData.Length);
                    fs.Close();
                    fs.Dispose();
                }
                return path;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return string.Empty;
            }
        }

        /// <summary>
        /// 根据给定的子应用编号集合获取其详细信息，一个编号代办一个系统，
        /// 单个系统可以包含多个子系统 
        /// </summary>
        /// <param name="appCodes">系统编号</param>
        /// <returns>结果，详细信息</returns>
        public List<ModuleInfo> GetModuleByCodes(string[] appCodes)
        {
            try
            {
                return GetModuleListByCodes(appCodes);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return null;
            }
        }



        public bool IsExists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");


            var ents = from ent in Modules
                       where ent.MODULEID == key
                       select ent;
            return ents.Count() > 0 ? true : false;

        }


        public ModuleInfo GetEntityByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");


            var ents = from ent in Modules
                       where ent.MODULEID == key
                       select ent;
            T_PF_MODULEINFO application = ents.Count() > 0 ? ents.FirstOrDefault() : null;

            return application.CloneObject<ModuleInfo>(new ModuleInfo());
        }

        public List<ModuleInfo> GetModuleListByCodes(string[] appCodes)
        {

            if (appCodes == null)
                throw new ArgumentNullException("appCodes");


            var ents = from ent in Modules
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
        private static List<T_PF_MODULEINFO> modules;


        public List<T_PF_MODULEINFO> Modules
        {
            get
            {
                if (modules == null)
                {
                    using (CommonDAL<T_PF_MODULEINFO> dal = new CommonDAL<T_PF_MODULEINFO>())
                    {
                        modules = (from ent in dal.GetObjects()
                                   select ent).ToList();
                        return modules;
                    }

                }
                else
                    return modules;
            }
            set { modules = value; }
        }

        public List<ModuleInfo> GetModulesBySystemCode(string[] appCodes)
        {

            if (appCodes == null)
                throw new ArgumentNullException("appCodes");


            var ents = from ent in Modules
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


            var ents = from ent in Modules
                       where appCodes.Contains(ent.PARENTMODULEID)
                       select new ModuleInfo
                       {
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



        /// <summary>
        /// 根据用户系统ID获取用户所拥有的系统模块目录信息。
        /// </summary>
        /// <param name="userSysID">用户系统ID</param>
        /// <returns>生成后的模块集合</returns>
        public List<ModuleInfo> GetModuleCatalogByUser(string userSysID)
        {
            Tracer.Debug("GetModuleCatalogByUser Start,! userSysID:" + userSysID);
            DateTime dtstart = DateTime.Now;
            try
            {
                string result = string.Empty;
                SysUserRoleBLL RoleBll = new SysUserRoleBLL();
                string syscodes = RoleBll.GetSystemTypeByUserID(userSysID, ref result);
                string[] codelist = syscodes.Split(',');

                List<ModuleInfo> moduleinfos = GetModulesBySystemCode(codelist);
                List<ModuleInfo> tempModules = new List<ModuleInfo>();

                SysEntityMenuBLL pmbll = new SysEntityMenuBLL();

                List<V_UserMenuPermission> pmList = pmbll.GetSysLeftMenuFilterPermissionToNewFrame(userSysID);
                var menuList = from ent in pmList
                               orderby ent.SYSTEMTYPE, ent.ORDERNUMBER
                               select ent;



                foreach (var item in menuList)
                {
                    var childs = menuList.Where(mm => mm.EntityMenuFatherID == item.ENTITYMENUID);

                    if (item.ENTITYMENUID == "9b58888d-cf4e-40cf-bab6-de4ee00d0ceb")
                    {
                        Tracer.Debug("！已获取到(新)流程定义");
                    }
                    if (item.ENTITYMENUID == "709D9380-5405-4429-B047-20100401D255")
                    {
                        Tracer.Debug("！已获取到系统科目字典维护");
                    }
                    else
                    {
                        Tracer.Debug("已获取到菜单:" + item.MENUNAME);
                    }

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
                Tracer.Debug("Dal GetModuleCatalogByUser 异常：" + ex.ToString());
                //throw ex;
                return new List<ModuleInfo>();
            }
            finally
            {
                TimeSpan ts = DateTime.Now.Subtract(dtstart);
                Tracer.Debug("GetModuleCatalogByUser completed !耗时：" + ts.TotalSeconds + "s userSysID:" + userSysID);
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
            SysEntityMenuBLL pmbll = new SysEntityMenuBLL();

            var menuList = pmbll.GetSysLeftMenuFilterPermissionToNewFrameAndPermision(userSysID, "0");

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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
