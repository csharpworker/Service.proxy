using System;

namespace service.proxy.Repository
{
    public class ProxyMember
    {

        #region Property

        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }

        #endregion

        #region Ctors

        public ProxyMember(string Name, Type Type) { this.Name = Name; this.Type = Type; }
        public ProxyMember(string Name, Type Type, object Value) : this(Name, Type) { this.Value = Value; }
        public ProxyMember(string Name, object Value) : this(Name, Value.GetType(), Value) { }

        #endregion

    }
}
