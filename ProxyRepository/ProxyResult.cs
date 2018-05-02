using System;
using System.Collections.Generic;

namespace service.proxy.Repository
{
    public class ProxyResult
    {

        #region Property

        public string ReturnType { get; set; }
        public Dictionary<string, ProxyMember> Members { get; set; }

        #endregion

        #region Ctor

        public ProxyResult(string ReturnType)
        {
            this.ReturnType = ReturnType;
            this.Members = new Dictionary<string, ProxyMember>();
        }

        public ProxyResult(Type ReturnType) : this(ReturnType.ToString())
        { }

        #endregion

    }
}
