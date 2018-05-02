using System;
using System.Reflection;
using System.Collections.Generic;

namespace service.proxy.Repository
{
    public class ProxyMethod
    {

        #region Properties

        private const string ErrorTypeMessage = "Unknown method parameter {0} type : {1}. Parameter type must be in generic types";

        public string Name { get; set; }
        public Type Type { get; set; }

        public List<ProxyMember> ServiceResults { get; set; }
        public List<ProxyMember> ServiceParameters { get; set; }

        #endregion

        #region Ctors

        public ProxyMethod(MethodInfo info)
        {
            this.Name = info.Name;
            this.Type = info.ReturnType;

            this.ServiceResults = new List<ProxyMember>();
            this.ServiceParameters = new List<ProxyMember>();

            this.FindParameters(info);
            this.FindResults(info);
        }

        #endregion

        #region Methods

        private void FindParameters(MethodInfo info)
        {
            foreach (var item in info.GetParameters())
            {
                string TypeName = item.ParameterType.ToString().ToLower();
                Type pType = item.ParameterType;
                if ((pType.IsByRef || pType.IsEnum || pType.IsInterface) ||
                    (!TypeName.StartsWith("system.") && TypeName.EndsWith("[]")) ||
                    (TypeName.StartsWith("system.") && TypeName.EndsWith("[]")))
                    throw new Exception(string.Format(ErrorTypeMessage, item.Name, item.ParameterType.ToString(), info.Name));
                else if (!TypeName.StartsWith("system."))
                {
                    foreach (var prop in pType.GetProperties(BindingFlags.Public))
                    {
                        Type propType = prop.PropertyType;
                        if ((pType.IsByRef || pType.IsEnum || pType.IsInterface) ||
                            (!TypeName.StartsWith("system.")) ||
                            (TypeName.StartsWith("system.") && TypeName.EndsWith("[]")))
                            throw new Exception(string.Format(ErrorTypeMessage, item.Name, item.ParameterType.ToString(), info.Name));
                        else this.ServiceParameters.Add(new ProxyMember(prop.Name, prop.PropertyType));
                    } 
                }
                else this.ServiceParameters.Add(new ProxyMember(item.Name, item.ParameterType));
            }
        }
       
        private void FindResults(MethodInfo info)
        {
            if (!info.ReturnType.ToString().ToLower().StartsWith("system."))
            {
                Type T = info.ReturnType;
                if (T.IsArray) T = GetEnumeratedType(T);
                foreach (var item in T.GetProperties())
                {
                    string TypeName = item.PropertyType.ToString().ToLower();
                    Type pType = item.PropertyType;
                    if (pType.IsByRef || pType.IsEnum || pType.IsInterface)
                        throw new Exception(string.Format(ErrorTypeMessage, item.Name, item.PropertyType.ToString(), info.Name));
                    else if (TypeName.StartsWith("system.") && !TypeName.EndsWith("[]"))
                        this.ServiceResults.Add(new ProxyMember(item.Name, item.PropertyType));
                    else if (!TypeName.StartsWith("system.") && !TypeName.EndsWith("[]"))
                    {
                        this.ServiceResults.Clear();
                        foreach (var inneritem in item.PropertyType.GetProperties())
                        {
                            if (pType.IsByRef || pType.IsEnum || pType.IsInterface)
                                throw new Exception(string.Format(ErrorTypeMessage, item.Name, item.PropertyType.ToString(), info.Name));
                            else this.ServiceResults.Add(new ProxyMember(inneritem.Name, inneritem.PropertyType));
                        }
                        return;
                    }
                    else throw new Exception(string.Format(ErrorTypeMessage, item.Name, item.PropertyType.ToString(), info.Name));
                }
            }
        }

        public Type GetEnumeratedType(Type type)
        {
            // provided by Array
            var elType = type.GetElementType();
            if (null != elType) return elType;

            // otherwise provided by collection
            var elTypes = type.GetGenericArguments();
            if (elTypes.Length > 0) return elTypes[0];

            // otherwise is not an 'enumerated' type
            return null;
        }

        #endregion

    }
}
