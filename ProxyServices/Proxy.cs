using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using service.proxy.Repository;

namespace service.proxy
{
    public class Proxy
    {

        #region Properties

        public Assembly OutputAssembly { get; set; }
        public string ServiceName { get; set; }

        #endregion

        #region Ctors

        public Proxy(byte[] ServiceFile, string ServiceName)
        {
            this.ServiceName = ServiceName;
            this.OutputAssembly = Assembly.Load(ServiceFile);
        }

        #endregion

        #region Methods

        public List<ProxyResult> InvokeMethod(string MethodName)
        {
            return this.InvokeMethod(MethodName, null);
        }

        public List<ProxyResult> InvokeMethod(string MethodName, params ProxyMember[] Parameters)
        {
            List<ProxyResult> result = new List<ProxyResult>();

            var objService = this.OutputAssembly.GetType(this.ServiceName);
            var newobj = Activator.CreateInstance(objService);
            var method = objService.GetMethod(MethodName);
            var param = MapParameters(method, Parameters);
            var retobj = method.Invoke(newobj, param);

            string retType = method.ReturnType.ToString().ToLower();
            if (retType.StartsWith("system."))
            {
                if (retType.EndsWith("[]"))
                {
                    ProxyResult member = new ProxyResult(retType);
                    foreach (var item in (Array)retobj)
                        member.Members.Add("value", new ProxyMember("value", item.GetType(), item));
                    result.Add(member);
                }
                else if (retType == "system.void") return result;
                else
                {
                    ProxyResult member = new ProxyResult(retType);
                    member.Members.Add("value", new ProxyMember("value", retobj.GetType(), retobj));
                    result.Add(member);
                }
            }
            else
            {
                if (retType.EndsWith("[]"))
                {
                    foreach (var item in (Array)retobj)
                    {
                        ProxyResult member = new ProxyResult(item.GetType());

                        foreach (var prop in item.GetType().GetProperties())
                            member.Members.Add(prop.Name, new ProxyMember(prop.Name, prop.PropertyType, prop.GetValue(item)));

                        result.Add(member);
                    }
                }
                else
                {
                    ProxyResult member = new ProxyResult(retType);
                    foreach (var prop in retobj.GetType().GetProperties())
                        member.Members.Add(prop.Name, new ProxyMember(prop.Name, prop.PropertyType, prop.GetValue(retobj)));
                    result.Add(member);
                }
            }

            return result;
        }

        private object[] MapParameters(MethodBase method, ProxyMember[] namedParameters)
        {
            var paraminfo = method.GetParameters();

            if (paraminfo.Length == 1 && !paraminfo[0].ParameterType.ToString().ToLower().StartsWith("system."))
            {
                var paramtype = paraminfo[0].ParameterType;
                var newobj = Activator.CreateInstance(paramtype);

                PropertyInfo[] Parameters = newobj.GetType().GetProperties();
                if (namedParameters != null)
                    foreach (var item in namedParameters)
                    {
                        var paramName = item.Name;
                        Type T = Parameters.First(r => r.Name == paramName).PropertyType;
                        Parameters.SetValue(CastTo(T, item.Value), 0);
                    }
                //CreateMapInnerProperties(ref Parameters, namedParameters);
                return new[] { newobj };
            }
            else return CreateMapPatameters(paraminfo, namedParameters);
        }

        private object[] CreateMapPatameters(ParameterInfo[] Parameters, ProxyMember[] namedParameters)
        {
            string[] paramNames = Parameters.Select(p => p.Name.ToLower()).ToArray();
            object[] parameters = new object[paramNames.Length];

            for (int i = 0; i < parameters.Length; ++i)
                parameters[i] = Type.Missing;

            if (namedParameters != null)
            {
                foreach (var item in namedParameters)
                {
                    var paramName = item.Name.ToLower();
                    var paramIndex = Array.IndexOf(paramNames, paramName);

                    Type T = Parameters.First(r => r.Name.ToLower() == paramName).ParameterType;
                    parameters[paramIndex] = CastTo(T, item.Value);
                }
            }
            return parameters;
        }

        private void CreateMapInnerProperties(ref PropertyInfo[] Properties, ProxyMember[] namedParameters)
        {
            if (namedParameters != null)
                foreach (var item in namedParameters)
                {
                    var paramName = item.Name;
                    Type T = Properties.First(r => r.Name == paramName).PropertyType;
                    Properties.SetValue(CastTo(T, item.Value), 0);
                }
        }


        private object CastTo(Type t, object value)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value.ToString() == "null") return null;
                else return Convert.ChangeType(value, Nullable.GetUnderlyingType(t));
            }
            else return Convert.ChangeType(value, t);
        }

        #endregion

    }
}
