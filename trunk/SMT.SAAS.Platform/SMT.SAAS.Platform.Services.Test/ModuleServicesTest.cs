using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SMT.SAAS.Platform.Services.Test
{
    [TestClass]
    public class ModuleServicesTest
    {
        [TestMethod]
        public void TestAddApp()
        {
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            bool actual = false;

            PlatformWS.ModuleInfo model = new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "13",
                ModuleIcon = "无",
                ModuleName = "SMT.EM.UI",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "人力资源管理系统",
                EnterAssembly = "SMT.EM.UI",
                ParentModuleID = "0",
                ModuleType = "SMT.EM.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                FileName = "SMT.EM.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            };
            actual = client.AddModule(model);

            //异常：字段大小问题
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAddAppList()
        {
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            bool actual = false;
            List<PlatformWS.ModuleInfo> models = CreateTestDate();

            foreach (var itemModel in models)
            {
                actual = client.AddModule(itemModel);
            }
            //The socket connection was aborted. 
            //This could be caused by an error processing your message or a receive timeout being exceeded by the remote host, 
            //or an underlying network resource issue. Local socket timeout was '00:00:59.9840000'.
            //套接字连接中断，可能是由于消息处理错误，或者远程宿主接受超时引起，或者是底层网络资源问题导致，本地套接字时间是'00:00:59.7656250'。
            //修改：receiveTimeout="10:10:10"
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetAppFileStream()
        {
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            bool actual = false;
            string filename = "SMT.xap";
            var result = client.GetModuleFileStream(filename);
            if (result.Length > 0)
                actual = true;
            //异常1：修改maxReceivedMessageSize大小
            //异常2：maxReceivedMessageSize与maxBufferSize设置相等
            //异常3：修改maxArrayLength大小
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAddAppByFile()
        {
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            PlatformWS.ModuleInfo model = new PlatformWS.ModuleInfo()
            {
                ModuleID = "AppID_Upload2",
                ModuleCode = "AppCode_Upload2",
                ModuleIcon = "AppIcon_Upload2",
                ModuleName = "AppName_Upload2",
                UseState = "1",
                ClientID = "ClientID",
                Description = "Description",
                EnterAssembly = "EnterAssembly",
                FileName = "SMT_Upload2.xap",
                FilePath = "FilePath",
                HostAddress = "HostAddress",

                IsSave = "1",
                ServerID = "ServerID",
                Version = "Version"
            };
            bool actual = false;
            string filename = "SMT.xap";
            var result = client.GetModuleFileStream(filename);

            actual = client.AddModuleByFile(model, result);
            //异常：The underlying connection was closed: An unexpected error occurred on a receive
            //修改IIS上传文件大小限制和设置web.config的<httpRuntime executionTimeout="3600" maxRequestLength="2147483647" />

            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetAppByCodes()
        {
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            bool actual = false;

            string[] codes = new string[] { "0", "1" };

            var result = client.GetModuleByCodes(codes);

            if (result.Count() > 0)
                actual = true;

            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetModuleCatalogByUser()
        {
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            bool actual = false;
            //ZWP 981f9458-471f-4865-9f27-682cae3f21d6
            //ADMIN DAD32DB2-B07A-49b1-9710-61158D81B863
            //guojing c16f11e6-6020-479b-970f-484f5f308b7e
            string userid = "c16f11e6-6020-479b-970f-484f5f308b7e";

            var result = client.GetModuleCatalogByUser(userid);


            if (result.Count() > 0)
                actual = true;

            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

    
        private List<PlatformWS.ModuleInfo> CreateTestDate()
        {
            List<PlatformWS.ModuleInfo> models = new List<PlatformWS.ModuleInfo>();
            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "0",
                ModuleIcon = "无",
                ModuleName = "SMT.HRM.UI",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "人力资源管理系统",
                EnterAssembly = "SMT.HRM.UI",
                ParentModuleID = "0",
                ModuleType = "SMT.HRM.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                FileName = "SMT.HRM.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });
            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "4",
                ModuleIcon = "无",
                ModuleName = "SMT.RM.UI",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "人力资源管理系统",
                EnterAssembly = "SMT.RM.UI",
                ParentModuleID = "0",
                ModuleType = "SMT.RM.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                FileName = "SMT.RM.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "14",
                ModuleIcon = "无",
                ModuleName = "SMT.TM.UI",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "人力资源管理系统",
                ParentModuleID = "0",
                ModuleType = "SMT.TM.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                EnterAssembly = "SMT.TM.UI",
                FileName = "SMT.TM.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "13",
                ModuleIcon = "无",
                ModuleName = "SMT.EM.UI",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "人力资源管理系统",
                EnterAssembly = "SMT.EM.UI",
                ParentModuleID = "0",
                ModuleType = "SMT.EM.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                FileName = "SMT.EM.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "10",
                ModuleIcon = "无",
                ModuleName = "SMT.EDM.UI",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "进销存管理系统",
                ParentModuleID = "10",
                ModuleType = "SMT.EDM.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                EnterAssembly = "SMT.EDM.UI",
                FileName = "SMT.EDM.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "1",
                ModuleIcon = "无",
                ModuleName = "SMT.SaaS.OA.UI",
                ParentModuleID = "1",
                ModuleType = "SMT.SaaS.OA.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "办公系统",
                EnterAssembly = "SMT.SaaS.OA.UI",
                FileName = "SMT.SaaS.OA.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });
            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "6",
                ModuleIcon = "无",
                ModuleName = "SMT.SaaS.OA",
                ParentModuleID = "1",
                ModuleType = "SMT.SaaS.OA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "办公系统",
                EnterAssembly = "SMT.SaaS.OA",
                FileName = "SMT.SaaS.OA.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });
            models.Add(new PlatformWS.ModuleInfo()
            {

                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "2",
                ModuleIcon = "无",
                ModuleName = "SMT.SaaS.LM.UI",
                ParentModuleID = "2",
                ModuleType = "SMT.SaaS.LM.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "物流系统",
                EnterAssembly = "SMT.SaaS.LM.UI",
                FileName = "SMT.SaaS.LM.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",

                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });
            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "3",
                ModuleIcon = "无",
                ModuleName = "SMT.FB.UI",
                ParentModuleID = "3",
                ModuleType = "SMT.SaaS.FB.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",

                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "预算系统",
                EnterAssembly = "SMT.FB.UI",
                FileName = "SMT.FB.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",

                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "15",
                ModuleIcon = "无",
                ModuleName = "SMT.FBAnalysis.UI",
                ParentModuleID = "3",
                ModuleType = "SSMT.FBAnalysis.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "预算系统",
                EnterAssembly = "SMT.FBAnalysis.UI",
                FileName = "SMT.FBAnalysis.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",
                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "7",
                ModuleIcon = "无",
                ModuleName = "SMT.SaaS.Permission.UI",
                ParentModuleID = "7",
                ModuleType = "SMT.SaaS.Permission.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "权限系统",
                EnterAssembly = "SMT.SaaS.Permission.UI",
                FileName = "SMT.SaaS.Permission.UI.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",

                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "8",
                ModuleIcon = "无",
                ModuleName = "SMT.FlowDesigner",
                ParentModuleID = "7",
                ModuleType = "SMT.FlowDesigner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "流程系统",
                EnterAssembly = "SMT.FlowDesigner",
                FileName = "SMT.FlowDesigner.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",

                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            models.Add(new PlatformWS.ModuleInfo()
            {
                ModuleID = Guid.NewGuid().ToString(),
                ModuleCode = "9",
                ModuleIcon = "无",
                ModuleName = "SMT.SaaS.EG",
                ParentModuleID = "7",
                ModuleType = "SMT.FlowDesigner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                UseState = "1",
                ClientID = "1.0.0.1000",
                Description = "引擎系统",
                EnterAssembly = "SMT.SaaS.EG",
                FileName = "SMT.SaaS.EG.xap",
                FilePath = "无",
                HostAddress = "172.30.50.13",

                IsSave = "1",
                ServerID = "1.0.0.1000",
                Version = "1.0.0.1000"
            });

            return models;

        }

        private List<PlatformWS.ModuleInfo> CreateTestDate2()
        {
            List<PlatformWS.ModuleInfo> models = new List<PlatformWS.ModuleInfo>();
            //models.Add(new PlatformWS.ModuleInfo()
            //{
            //    AppID = Guid.NewGuid().ToString(),
            //    AppCode = "0",
            //    AppIcon = "无",
            //    AppName = "测试系统—A",
            //    UseState = "1",
            //    ClientID = "1.0.0.1000",
            //    Description = "测试系统—A",
            //    EnterAssembly = "Application_A",
            //    FileName = "Application_A.xap",
            //    FilePath = "无",
            //    HostAddress = "172.30.50.13",
            //    IsCache = "1",
            //    IsSave = "1",
            //    ServerID = "1.0.0.1000",
            //    Version = "1.0.0.1000"
            //});
            //models.Add(new PlatformWS.App()
            //{
            //    AppID = Guid.NewGuid().ToString(),
            //    AppCode = "1",
            //    AppIcon = "无",
            //    AppName = "测试系统—B",
            //    UseState = "1",
            //    ClientID = "1.0.0.1000",
            //    Description = "测试系统—B",
            //    EnterAssembly = "Application_B",
            //    FileName = "Application_B.xap",
            //    FilePath = "无",
            //    HostAddress = "172.30.50.13",
            //    IsCache = "1",
            //    IsSave = "1",
            //    ServerID = "1.0.0.1000",
            //    Version = "1.0.0.1000"
            //});
            //models.Add(new PlatformWS.App()
            //{
            //    AppID = Guid.NewGuid().ToString(),
            //    AppCode = "0",
            //    AppIcon = "无",
            //    AppName = "测试系统—C",
            //    UseState = "1",
            //    ClientID = "1.0.0.1000",
            //    Description = "测试系统—C",
            //    EnterAssembly = "Application_C",
            //    FileName = "Application_C.xap",
            //    FilePath = "无",
            //    HostAddress = "172.30.50.13",
            //    IsCache = "1",
            //    IsSave = "1",
            //    ServerID = "1.0.0.1000",
            //    Version = "1.0.0.1000"
            //});
            //models.Add(new PlatformWS.App()
            //{
            //    AppID = Guid.NewGuid().ToString(),
            //    AppCode = "0",
            //    AppIcon = "无",
            //    AppName = "测试系统—D",
            //    UseState = "1",
            //    ClientID = "1.0.0.1000",
            //    Description = "测试系统—D",
            //    EnterAssembly = "Application_D",
            //    FileName = "Application_D.xap",
            //    FilePath = "无",
            //    HostAddress = "172.30.50.13",
            //    IsCache = "1",
            //    IsSave = "1",
            //    ServerID = "1.0.0.1000",
            //    Version = "1.0.0.1000"
            //});
            return models;
        }
    }
}
