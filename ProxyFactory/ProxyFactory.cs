using System;
using System.IO;
using System.Net;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Xml.Serialization;
using System.Web.Services.Description;

using service.proxy.Repository;

namespace service.proxy
{
    public class ProxyFactory
    {

        #region Properties

        private ServiceDescriptionImporter ServiceImporter { get; set; }

        #endregion

        #region Actions

        public ProxyService ImportService(string WsdlUrl)
        {
            //append ?wsdl
            if (!WsdlUrl.ToLower().EndsWith("?wsdl")) WsdlUrl += "?wsdl";
            Uri uri = new Uri(WsdlUrl);

            // Get a WSDL file describing a service
            WebRequest webRequest = WebRequest.Create(uri);
            Stream requestStream = webRequest.GetResponse().GetResponseStream();

            ServiceDescription serviceDesc = ServiceDescription.Read(requestStream);

            if (serviceDesc.Services.Count == 0) throw new Exception("There is no service found!");
            string sdName = serviceDesc.Services[0].Name.Replace(".", "").Replace("-", "");

            //Initialize a service description ServImport
            this.ServiceImporter = new ServiceDescriptionImporter();
            this.ServiceImporter.AddServiceDescription(serviceDesc, String.Empty, String.Empty);
            this.ServiceImporter.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties;
            this.ServiceImporter.ProtocolName = "Soap";

            new SchemaInjection(this.ServiceImporter, serviceDesc, uri);

            string outputFilePath; Assembly outputAssembly;
            bool result = this.CompileAssembly(out outputAssembly, out outputFilePath);
            byte[] outputFile = File.ReadAllBytes(outputFilePath);

            return new ProxyService(outputFile, sdName, outputAssembly);
        }

        #endregion

        #region Methods

        private bool CompileAssembly(out Assembly outputAssembly, out string outpurFilePath)
        {
            CodeNamespace nameSpace = new CodeNamespace();
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(nameSpace);
            // Set Warnings
            ServiceDescriptionImportWarnings warnings = ServiceImporter.Import(nameSpace, codeCompileUnit);

            if (warnings == 0 || warnings.ToString().EndsWith("Ignored"))
            {
                StringWriter stringWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
                Microsoft.CSharp.CSharpCodeProvider prov = new Microsoft.CSharp.CSharpCodeProvider();
                prov.GenerateCodeFromNamespace(nameSpace, stringWriter, new CodeGeneratorOptions());

                // Compile the assembly with the appropriate references
                string[] assemblyReferences = new string[5] { "System.Web.Services.dll", "System.Xml.dll", "System.dll", "System.Web.dll", "System.Data.dll" };

                CompilerParameters param = new CompilerParameters(assemblyReferences);
                param.GenerateExecutable = false;
                param.GenerateInMemory = false;
                param.TreatWarningsAsErrors = false;
                param.WarningLevel = 4;

                CompilerResults results = new CompilerResults(new TempFileCollection());
                results = prov.CompileAssemblyFromDom(param, codeCompileUnit);
                outpurFilePath = results.PathToAssembly;
                outputAssembly = results.CompiledAssembly;
                return true;
            }
            else
            {
                outpurFilePath = warnings.ToString();
                outputAssembly = null;
                return false;
            }
        }

        #endregion

    }
}
