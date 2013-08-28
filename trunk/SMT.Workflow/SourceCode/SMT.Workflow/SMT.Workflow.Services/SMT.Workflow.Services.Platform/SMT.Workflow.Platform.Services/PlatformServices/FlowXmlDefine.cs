using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Common.Model.FlowXml;
using System.Xml.Linq;
using System.IO;


namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : IFlowXmlDefine
    {
        #region  读取Xml
        public List<AppSystem> ListSystem()
        {
            List<AppSystem> List = new List<AppSystem>();
            string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BOSystemList.xml");
           // string Path = System.Web.HttpContext.Current.Server.MapPath("../BusinessObjects/BOSystemList.xml");
            XDocument xdoc = XDocument.Load(Path);
            var xmlTree = from c in xdoc.Descendants("System")
                          select c;
            if (xmlTree.Count() > 0)
            {
                foreach (var v in xmlTree)
                {
                    AppSystem sys = new AppSystem();
                    sys.Name = v.Attribute("Name").Value;
                    sys.Description = v.Attribute("Description").Value;
                    sys.ObjectFolder = v.Attribute("ObjectFolder").Value;
                    List.Add(sys);
                }
            }
            xdoc = null;
            return List;
        }
        public List<AppModel> AppModel(string ObjectFolder)
        {
            List<AppModel> List = new List<AppModel>();
            string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + ObjectFolder + "/BOList.xml");
            XDocument xdoc = XDocument.Load(Path);
            var xmlTree = from c in xdoc.Descendants("ObjectList").Descendants<XElement>("Object")
                          select c;
            if (xmlTree.Count() > 0)
            {
                foreach (var v in xmlTree)
                {
                    AppModel model = new AppModel();
                    model.Name = v.Attribute("Name").Value;
                    model.Description = v.Attribute("Description").Value;
                    model.ObjectFolder = ObjectFolder;
                    List.Add(model);
                }
            }
            xdoc = null;
            return List;
        }
        /// <summary>
        /// 获取业务对象
        /// </summary>
        /// <param name="ObjectFolder"></param>
        /// <returns></returns>
        public List<AppModel> ListModel(List<string> ObjectFolder)
        {
            List<AppModel> List = new List<AppModel>();
            foreach (var item in ObjectFolder)
            {
                string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + item + "/BOList.xml");
                //D:\smt2010\SMT.Workflow\SourceCode\SMT.Workflow\SMT.Workflow.Services\SMT.Workflow.Services.Platform\SMT.Workflow.Platform.Services\BusinessObjects\EDM\BOList.xml
                //SMT.Workflow.Common.DataAccess.LogHelper.WriteLog("获取业务对象路径:" + Path);
                if (File.Exists(Path))
                {
                    XDocument xdoc = XDocument.Load(Path);
                    var xmlTree = from c in xdoc.Descendants("ObjectList").Descendants<XElement>("Object")
                                  select c;
                    if (xmlTree.Count() > 0)
                    {
                        foreach (var v in xmlTree)
                        {
                            AppModel model = new AppModel();
                            model.Name = v.Attribute("Name").Value;
                            model.Description = v.Attribute("Description").Value;
                            model.ObjectFolder = item;
                            List.Add(model);
                        }
                    }
                    xdoc = null;
                }
            }
            return List;
        }

        public List<AppFunc> ListSystemFunc(string ObjectFolder, string strFolderName, ref string MsgLinkUrl)
        {
            try
            {
                List<AppFunc> list = new List<AppFunc>();
                string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + ObjectFolder + "/" + strFolderName + ".xml");
                XDocument xdoc = XDocument.Load(Path);
                try
                {
                    string strXml = xdoc.Document.ToString();
                    int Start = strXml.IndexOf("<MsgOpen>") + "<MsgOpen>".Length;
                    int End = strXml.IndexOf("</MsgOpen>");
                    MsgLinkUrl = strXml.Substring(Start, End - Start);
                }
                catch { }
                var xmlTree = from c in xdoc.Descendants("Function")
                              select c;
                if (xmlTree.Count() > 0)
                {
                    foreach (var v in xmlTree)
                    {
                        AppFunc sys = new AppFunc();
                        sys.Language = v.Attribute("Description").Value;
                        sys.FuncName = v.Attribute("FuncName").Value;

                        // sys.Parameter = v.Attribute("Parameter").Value;
                        sys.Address = v.Attribute("Address").Value;
                        sys.Binding = v.Attribute("Binding").Value;
                        sys.SplitChar = v.Attribute("SplitChar").Value;
                        List<Parameter> ListParam = new List<Parameter>();
                        var param = v.Descendants("ParaStr").Descendants<XElement>("Para");
                        if (param.Count() > 0)
                        {
                            foreach (var p in param)
                            {
                                Parameter para = new Parameter();
                                para.Description = p.Attribute("Description").Value.CvtString();
                                para.Name = p.Attribute("Name").Value.CvtString();
                                para.Value = p.Attribute("Value").Value.CvtString();
                                para.TableName = p.Attribute("TableName").Value.CvtString();
                                ListParam.Add(para);
                            }
                        }
                        sys.Parameter = ListParam;
                        list.Add(sys);
                    }
                }
                xdoc = null;
                return list;
            }
            catch (Exception ex)
            {
                //ErrorLog.WriteErrorMessage(ex.Message + "|" + DateTime.Now.ToString() + "|" + ex.Source + "|" + "UserDefine.svc方法GetSystemFuncList");
            }
            return null;
        }
        public List<TableColumn> ListTableColumn(string ObjectFolder, string strFileName)
        {
            try
            {
                List<TableColumn> list = new List<TableColumn>();
                string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + ObjectFolder + "/" + strFileName + ".xml");
                XDocument xdoc = XDocument.Load(Path);
                var xmlTree = from c in xdoc.Descendants("Object").Descendants<XElement>("Attribute")
                              select c;
                if (xmlTree.Count() > 0)
                {
                    foreach (var v in xmlTree)
                    {
                        TableColumn sys = new TableColumn();
                        sys.Language = v.Attribute("Description").Value;
                        sys.DataType = v.Attribute("DataType").Value;
                        sys.DataValue = v.Attribute("DataValue").Value;
                        sys.Description = v.Attribute("Description").Value;
                        sys.FieldName = v.Attribute("Name").Value;
                        list.Add(sys);
                    }
                }
                xdoc = null;
                return list;
            }
            catch (Exception ex)
            {
                //ErrorLog.WriteErrorMessage(ex.Message + "|" + DateTime.Now.ToString() + "|" + ex.Source + "|" + "UserDefine.svc方法GetSystemFuncList");
            }
            return null;
        }
        public Dictionary< List<AppFunc>,List<TableColumn>> ListFuncTableColumn(string ObjectFolder, string strFileName, ref string MsgLinkUrl)
        {
            Dictionary<List<AppFunc>, List<TableColumn>> FuncTableColumn = new Dictionary<List<AppFunc>, List<TableColumn>>();
            try
            {
                List<AppFunc> list = new List<AppFunc>();
                string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + ObjectFolder + "/" + strFileName + ".xml");
                XDocument xdoc = XDocument.Load(Path);
                try
                {
                    string strXml = xdoc.Document.ToString();
                    int Start = strXml.IndexOf("<MsgOpen>") + "<MsgOpen>".Length;
                    int End = strXml.IndexOf("</MsgOpen>");
                    MsgLinkUrl = strXml.Substring(Start, End - Start);
                }
                catch { }
                var xmlTree = from c in xdoc.Descendants("Function")
                              select c;
                if (xmlTree.Count() > 0)
                {
                    foreach (var v in xmlTree)
                    {
                        AppFunc sys = new AppFunc();
                        sys.Language = v.Attribute("Description").Value;
                        sys.FuncName = v.Attribute("FuncName").Value;

                        // sys.Parameter = v.Attribute("Parameter").Value;
                        sys.Address = v.Attribute("Address").Value;
                        sys.Binding = v.Attribute("Binding").Value;
                        sys.SplitChar = v.Attribute("SplitChar").Value;
                        List<Parameter> ListParam = new List<Parameter>();
                        var param = v.Descendants("ParaStr").Descendants<XElement>("Para");
                        if (param.Count() > 0)
                        {
                            foreach (var p in param)
                            {
                                Parameter para = new Parameter();
                                para.Description = p.Attribute("Description").Value.CvtString();
                                para.Name = p.Attribute("Name").Value.CvtString();
                                para.Value = p.Attribute("Value").Value.CvtString();
                                para.TableName = p.Attribute("TableName").Value.CvtString();
                                ListParam.Add(para);
                            }
                        }
                        sys.Parameter = ListParam;
                        list.Add(sys);
                    }
                }
                List<TableColumn> lists = new List<TableColumn>();
                var xmlTrees = from c in xdoc.Descendants("Object").Descendants<XElement>("Attribute")
                              select c;
                if (xmlTrees.Count() > 0)
                {
                    foreach (var v in xmlTrees)
                    {
                        TableColumn sys = new TableColumn();
                        sys.Language = v.Attribute("Description").Value;
                        sys.DataType = v.Attribute("DataType").Value;
                        sys.DataValue = v.Attribute("DataValue").Value;
                        sys.Description = v.Attribute("Description").Value;
                        sys.FieldName = v.Attribute("Name").Value;
                        lists.Add(sys);
                    }
                }
                xdoc = null;
                FuncTableColumn.Add(list, lists);
                return FuncTableColumn;
            }
            catch (Exception ex)
            {
                //ErrorLog.WriteErrorMessage(ex.Message + "|" + DateTime.Now.ToString() + "|" + ex.Source + "|" + "UserDefine.svc方法GetSystemFuncList");
            }
            return null;
        }
        /// <summary>
        /// 获取条件属性列表
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public List<WFBOAttribute> GetSystemBOAttributeList(string systemName, string objectName)
        {
            string folderName = GetObjectFolderName(systemName);
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + folderName + "/" + objectName + ".xml");
            if (!System.IO.File.Exists(path))
            {
                SMT.Workflow.Common.DataAccess.LogHelper.WriteLog("获取条件时找不到路径:"+path);
                return null; 
            }
            XElement doc = XElement.Load(path);

            List<WFBOAttribute> AttrList = (from el in doc.Element("Object").Elements("Attribute")
                                            select new WFBOAttribute()
                                            {
                                                Name = el.Attribute("Name").Value,
                                                Description = el.Attribute("Description").Value,
                                                DataType = el.Attribute("DataType").Value,
                                                IsSelected=(el.Attribute("IsSelected")==null||el.Attribute("IsSelected").Value!="true")?false:true
                                            }).ToList<WFBOAttribute>();

            return AttrList;
        }

        private string GetObjectFolderName(string sysName)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/BOSystemList.xml");
            XDocument xdoc = XDocument.Load(path);

            var item = (from c in xdoc.Descendants("System")
                        where c.Attribute("Name").Value.ToString() == sysName
                        select c
                       ).FirstOrDefault();

            if (item != null)
                return item.Attribute("ObjectFolder").Value.ToString();
            else
                return "";
        }
        #endregion
    }
}
