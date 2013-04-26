using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SMT.SAAS.Platform.Services.Test
{
    [TestClass]
    public class ShortCutSiServicesTest
    {
        [TestMethod]
        public void TestAddShortCutByList()
        {
            PlatformSiWS.PlatformSiServicesClient client = new PlatformSiWS.PlatformSiServicesClient();
            bool actual = false;
            List<PlatformSiWS.ShortCut> models = CreateShortCut();

            actual = client.AddShortCutByList(models.ToArray());

            //异常：字段大小问题
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAddShortCutByUser()
        {
           
            PlatformSiWS.PlatformSiServicesClient client = new PlatformSiWS.PlatformSiServicesClient();
            bool actual = false;
            List<PlatformSiWS.ShortCut> models = CreateShortCut2();

            //ZWP 981f9458-471f-4865-9f27-682cae3f21d6
            //ADMIN DAD32DB2-B07A-49b1-9710-61158D81B863

            string userID = "DAD32DB2-B07A-49b1-9710-61158D81B863";
            actual = client.AddShortCutByUser(models.ToArray(), userID);

            //异常：字段大小问题
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetShortCutByUser()
        {
            PlatformSiWS.PlatformSiServicesClient client = new PlatformSiWS.PlatformSiServicesClient();
            bool actual = false;
            List<PlatformSiWS.ShortCut> models = CreateShortCut();
            string userID = "DAD32DB2-B07A-49b1-9710-61158D81B863";
            var result = client.GetShortCutByUser(userID);

            if (result.Count() > 0)
                actual = true;

            //异常：字段大小问题
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

        private List<PlatformSiWS.ShortCut> CreateShortCut2()
        {
            List<PlatformSiWS.ShortCut> list = new List<PlatformSiWS.ShortCut>();
            for (int i = 0; i < 8; i++)
            {
                list.Add(new PlatformSiWS.ShortCut()
                {
                    AssemplyName = "AssemplyName",
                    FullName = "FullName",
                    IconPath = "IconPath",
                    IsSysNeed = "1",
                    ModuleID = "ModuleID",
                    Params = "ModuleID",
                    ShortCutID = "ShortCutID_"+i.ToString(),
                    Titel = "Titel",
                    UserState = "1"
                });
            }
            return list;
        }

        private List<PlatformSiWS.ShortCut> CreateShortCut()
        {
            List<PlatformSiWS.ShortCut> list = new List<PlatformSiWS.ShortCut>();

            list.Add(new PlatformSiWS.ShortCut()
            {
                AssemplyName = "SMT.SAAS.Platform.WebParts",
                FullName = "SMT.SAAS.Platform.WebParts.Views.NewsManager, SMT.SAAS.Platform.WebParts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                IconPath = "/Images/icons/config.png",
                IsSysNeed = "1",
                ModuleID = "NewsManager",
                Params = "",
                ShortCutID = Guid.NewGuid().ToString(),
                Titel = "新闻管理",
                UserState = "1"
            });

            list.Add(new PlatformSiWS.ShortCut()
            {
                AssemplyName = "SMT.SAAS.Platform",
                FullName = "SMT.SAAS.Platform.Xamls.MainPagePart.CustomMenusSet, SMT.SAAS.Platform, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                IconPath = "/Images/icons/config.png",
                IsSysNeed = "1",
                ModuleID = "CustomMenusSet",
                Params = "",
                ShortCutID = Guid.NewGuid().ToString(),
                Titel = "菜单列表",
                UserState = "1"
            });

            list.Add(new PlatformSiWS.ShortCut()
            {
                AssemplyName = "SMT.SAAS.Platform",
                FullName = "SMT.SAAS.Platform.Xamls.MainPagePart.EmployeeComplain, SMT.SAAS.Platform.WebParts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                IconPath = "/Images/icons/config.png",
                IsSysNeed = "1",
                ModuleID = "EmployeeComplain",
                Params = "",
                ShortCutID = Guid.NewGuid().ToString(),
                Titel = "意见反馈",
                UserState = "1"
            });

            return list;

        }

        //new ToolsModel(){ Titel="系统设置",Tag="SysCofigSet", Icon="/Images/icons/config.png"},
        //       new ToolsModel(){ Titel="菜单列表",Tag="CustomMenusSet", Icon="/Images/icons/config.png"},
        //       new ToolsModel(){ Titel="意见投诉",Tag="EmployeeComplain", Icon="/Images/icons/ico_EmployeeComplain.png"}
    }
}
