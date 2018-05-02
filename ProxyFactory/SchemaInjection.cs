using System;
using System.IO;
using System.Net;
using System.Xml.Schema;
using System.Web.Services.Description;

namespace service.proxy
{
    public class SchemaInjection
    {

        #region Properties

        private ServiceDescriptionImporter serviceImport { get; set; }

        #endregion

        #region Ctor

        public SchemaInjection(ServiceDescriptionImporter serviceImport, ServiceDescription ServiceDesc, Uri baseUri)
        {
            this.serviceImport = serviceImport;
            this.InjectInnerNameSpaces(ServiceDesc, baseUri);
        }

        #endregion

        #region Methods

        private void InjectInnerNameSpaces(ServiceDescription ServiceDesc, Uri InnerUri)
        {
            this.InjectTypes(ServiceDesc, InnerUri);

            // Add any child namespaces First
            foreach (Import schemaImport in ServiceDesc.Imports)
            {
                string schemaLocation = schemaImport.Location;
                if (schemaLocation == null) continue;

                Uri schemaUri = new Uri(InnerUri, schemaLocation);
                using (Stream schemaStream = new WebClient().OpenRead(schemaUri))
                {
                    try
                    {
                        ServiceDescription sdImport = ServiceDescription.Read(schemaStream, true);
                        sdImport.Namespaces.Add("wsdl", schemaImport.Namespace);
                        this.serviceImport.AddServiceDescription(sdImport, null, null);

                        this.InjectInnerNameSpaces(sdImport, schemaUri);
                    }
                    catch { }  // ignore schema import errors
                }
            }
        }

        private void InjectTypes(ServiceDescription ServiceDesc, Uri InnerUri)
        {
            // Download and inject any imported schemas (ie. WCF generated WSDL)            
            foreach (XmlSchema wsdlSchema in ServiceDesc.Types.Schemas)
                InjectInnerType(wsdlSchema, InnerUri);
        }

        private void InjectInnerType(XmlSchema wsdlSchema, Uri baseUri)
        {
            // Loop through all detected imports in the main schema
            foreach (XmlSchemaObject externalSchema in wsdlSchema.Includes)
            {
                // Read each external schema into a schema object and add to importer
                if (externalSchema is XmlSchemaImport)
                {
                    string exSchemaLocation = ((XmlSchemaExternal)externalSchema).SchemaLocation;
                    if (string.IsNullOrEmpty(exSchemaLocation)) continue;

                    Uri schemaUri = new Uri(baseUri, exSchemaLocation);
                    try
                    {
                        using (Stream schemaStream = new WebClient().OpenRead(schemaUri))
                        {
                            try
                            {
                                XmlSchema schema = XmlSchema.Read(schemaStream, null);
                                this.serviceImport.Schemas.Add(schema);
                                // this.InjectInnerType(schema, baseUri);
                            }
                            catch { }  // ignore schema import errors                                                        
                        }
                    }
                    catch { }  // ignore schema import errors                                                        
                }
            }
        }

        #endregion

    }
}
