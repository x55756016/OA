using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SMT.SAAS.Platform.Services.Test
{
    [TestClass]
    public class ResourceServicesTest
    {
        [TestMethod]
        public void TestReadResource()
        {
            //PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            bool actual = false;
            //string filename = "SMT.xap";
            //var result=client.GetXapFile(filename);
            //if (result.Length > 0)
            //    actual = true;
            ////异常1：修改maxReceivedMessageSize大小
            ////异常2：maxReceivedMessageSize与maxBufferSize设置相等
            ////异常3：修改maxArrayLength大小
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }
    }
}
