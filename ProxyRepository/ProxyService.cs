using System;
using System.Collections.Generic;
using System.Reflection;

namespace service.proxy.Repository
{

    public class ProxyService
    {

        #region Properties

        private Assembly OutputAssembly { get; set; }
        public string ServiceName { get; set; }
        public byte[] ServiceFile { get; set; }

        public Dictionary<string, ProxyMethod> ServiceMethods { get; set; }

        #endregion

        #region Ctors

        public ProxyService(byte[] ServiceFile, string ServiceName, Assembly OutputAssembly)
        {
            this.ServiceFile = ServiceFile;
            this.ServiceName = ServiceName;
            this.OutputAssembly = OutputAssembly;
            this.ServiceMethods = new Dictionary<string, ProxyMethod>();

            this.FindMethods(this.OutputAssembly.GetType(ServiceName));
        }

        public ProxyService(byte[] ServiceFile, string ServiceName) :
            this(ServiceFile, ServiceName, Assembly.Load(ServiceFile))
        { }

        #endregion

        #region Methods

        private void FindMethods(Type T)
        {
            var methods = T.GetMethods();
            foreach (var item in methods)
            {
                if (item.Name == "Discover") break;
                this.ServiceMethods.Add(item.Name, new ProxyMethod(item));
            }
        }

        #endregion

    }

}
