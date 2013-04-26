using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.Diagnostics;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: XAP文件流的核心解析组件。
//           用于解析XAP文件并将其包含的DLL加载到当前AppDomain中。
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// XAP文件流的核心解析组件。
    /// 用于解析XAP文件并将其包含的DLL加载到当前AppDomain中
    /// </summary>
    internal class XapAnalysis
    {
        /// <summary>
        /// 根据给定的XAP文件流，对其进行解析。
        /// </summary>
        /// <param name="xapStream">XAP包数据</param>
        public void Analysis(Stream xapStream)
        {
            try
            {
                foreach (AssemblyPart part in GetParts(xapStream))
                {
                    LoadAssemblyFromStream(xapStream, part);
                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Current.Log(ex.Message, Logging.Category.Exception, Logging.Priority.Medium);
            }
            finally
            {
                //xapStream.Close();
            }
        }

        /// <summary>
        /// 获取流中的所有DLL。
        /// 通过AppManifest.xaml获取包中包含的DLL列表。
        /// 其格式为XML，再根据AssemblyPart节点获取所有的DLL名称。
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static IEnumerable<AssemblyPart> GetParts(Stream stream)
        {
            List<AssemblyPart> assemblyParts = new List<AssemblyPart>();
            var streamReader = new StreamReader(Application.GetResourceStream(new StreamResourceInfo(stream, null), new Uri("AppManifest.xaml", UriKind.Relative)).Stream);
            using (XmlReader xmlReader = XmlReader.Create(streamReader))
            {
                xmlReader.MoveToContent();
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Deployment.Parts")
                    {
                        using (XmlReader xmlReaderAssemblyParts = xmlReader.ReadSubtree())
                        {
                            while (xmlReaderAssemblyParts.Read())
                            {
                                if (xmlReaderAssemblyParts.NodeType == XmlNodeType.Element && xmlReaderAssemblyParts.Name == "AssemblyPart")
                                {
                                    AssemblyPart assemblyPart = new AssemblyPart();
                                    //获取每个DLL的名称。并添加到AssemblyPart信息中。
                                    assemblyPart.Source = xmlReaderAssemblyParts.GetAttribute("Source");
                                    assemblyParts.Add(assemblyPart);
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return assemblyParts;
        }
         
        /// <summary>
        /// 根据AssemblyPart信息，从XAP流中读取具体DLL信息，并加载到AppDomain中。
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <param name="assemblyPart"></param>
        private static void LoadAssemblyFromStream(Stream sourceStream, AssemblyPart assemblyPart)
        {
            StreamResourceInfo resourceStream = Application.GetResourceStream(new StreamResourceInfo(sourceStream, null),
               new Uri(assemblyPart.Source, UriKind.Relative));
            //此处有可能找不到对应的DLL 文件流
            if (resourceStream != null)
            {
                var assebblyinfo = assemblyPart.Load(resourceStream.Stream);

                Platform.Logging.Logger.Current.Log(assebblyinfo.FullName, Logging.Category.Info, Logging.Priority.Low);

                Debug.WriteLine(assebblyinfo.FullName);

            }
        }
    }
}
