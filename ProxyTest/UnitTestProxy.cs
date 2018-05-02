using Microsoft.VisualStudio.TestTools.UnitTesting;

using service.proxy;
using service.proxy.Repository;

namespace ProxyTest
{
    [TestClass]
    public class UnitTestProxy
    {

        private ProxyService createService()
        {
            //how to use service proxy
            var factory = new ProxyFactory();
            var service = factory.ImportService("http://www.site.com/webservice.asmx");
            return service;
        }

        [TestMethod]
        public void TestFindMethod()
        {
            var service = this.createService();
            Assert.IsTrue(service.ServiceMethods.ContainsKey("CallMe"));
        }


        [TestMethod]
        public void TestFindCallMethod()
        {
            var service = this.createService();

            //how to call service method
            var proxy = new Proxy(service.ServiceFile, service.ServiceName);
            var result = proxy.InvokeMethod("CallMe", new ProxyMember("Id", 123));

            Assert.IsTrue(result.Count != 0);
        }

    }
}
