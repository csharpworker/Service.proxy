using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using ir.fava.mashhad.coding.ProxyRepository;

namespace ir.fava.mashhad.coding.ProxyServices
{

    public class ProxyInvocation
    {
        public List<ProxyResult> InvokeService(Proxy Service)
        {
            return this.InvokeService(Service, new List<ProxyMember>());
        }

        public List<ProxyResult> InvokeService(Proxy Service, List<ProxyMember> Parameters)
        {
            List<ProxyResult> result = new List<ProxyResult>();

            var objService = Service.OutputAssembly.GetType(Service.ServiceName);
            var newobj = Activator.CreateInstance(objService);
            var method = objService.GetMethod(Service.MethodName);
            var retobj = method.Invoke(newobj, MapParameters(method, Parameters));

            string retType = method.ReturnType.ToString().ToLower();
            if (retType.StartsWith("system."))
            {
                if (retType.EndsWith("[]"))
                {
                    ProxyResult member = new ProxyResult(retType);
                    foreach (var item in (Array)retobj)
                        member.Members.Add(new ProxyMember("Value", item.GetType(), item));
                    result.Add(member);
                }
                else if (retType == "system.void") return result;
                else
                {
                    ProxyResult member = new ProxyResult(retType);
                    member.Members.Add(new ProxyMember("Value", retobj.GetType(), retobj));
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
                            member.Members.Add(new ProxyMember(prop.Name, prop.PropertyType, prop.GetValue(item)));

                        result.Add(member);
                    }
                }
                else
                {
                    ProxyResult member = new ProxyResult(retType);
                    foreach (var prop in retobj.GetType().GetProperties())
                        member.Members.Add(new ProxyMember(prop.Name, prop.PropertyType, prop.GetValue(retobj)));
                    result.Add(member);
                }
            }

            return result;
        }

        private object[] MapParameters(MethodBase method, List<ProxyMember> namedParameters)
        {
            string[] paramNames = method.GetParameters().Select(p => p.Name).ToArray();
            object[] parameters = new object[paramNames.Length];

            for (int i = 0; i < parameters.Length; ++i)
                parameters[i] = Type.Missing;

            foreach (var item in namedParameters)
            {
                var paramName = item.Name;
                var paramIndex = Array.IndexOf(paramNames, paramName);
                parameters[paramIndex] = item.Value;
            }
            return parameters;
        }

    }
}
