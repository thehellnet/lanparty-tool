using LanPartyTool.agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static LanPartyTool.utility.HttpUtility;

namespace LanPartyTool.utility.Tests
{
    [TestClass]
    public class HttpUtilityTests
    {
        [TestMethod]
        public void DoPostTest()
        {
            const string url = "http://127.0.0.1:8080/lanparty_manager/api/v1/tool/getCfg";
            dynamic requestBody = new {barcode = "0006668893"};
            JsonResponse response = DoPost(url, requestBody);
            Assert.IsTrue(response.Success);
        }
    }
}