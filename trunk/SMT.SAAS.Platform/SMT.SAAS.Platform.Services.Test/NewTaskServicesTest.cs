using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SMT.SAAS.Platform.Services.Test
{
    /// <summary>
    /// Summary description for ShortCutServicesTest
    /// </summary>
    [TestClass]
    public class NewTaskServicesTest
    {
        public NewTaskServicesTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestGetTaskByUser()
        {
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();

            bool actual = false;
            //ZWP 981f9458-471f-4865-9f27-682cae3f21d6
            var userid = "981f9458-471f-4865-9f27-682cae3f21d6";
            var rst = client.GetTaskConfigInfoByUser(userid);

            if(rst.Length>0)
                actual = true;           

            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

    }
}
