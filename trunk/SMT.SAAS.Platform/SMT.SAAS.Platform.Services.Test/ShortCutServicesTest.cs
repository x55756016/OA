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
    public class ShortCutServicesTest
    {
        public ShortCutServicesTest()
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
        public void TestRemoveShortCutByUser()
        {
            //ADMIN DAD32DB2-B07A-49b1-9710-61158D81B863
            PlatformWS.PlatformServicesClient client = new PlatformWS.PlatformServicesClient();
            bool actual = false;
            //ZWP 981f9458-471f-4865-9f27-682cae3f21d6
            //ADMIN DAD32DB2-B07A-49b1-9710-61158D81B863
            string userid = "DAD32DB2-B07A-49b1-9710-61158D81B863";

            bool result = client.RemoveShortCutByUser("BBDDFF0C-0E6C-4401-AA87-9FAD7E7BC07A",userid);

            actual = result;

            bool expected = true;
            Assert.AreEqual(expected, actual);
        }

    }
}
