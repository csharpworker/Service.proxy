# Service.proxy
Dynamically soap services real-time usage  

## Sample Usage

```csharp

//how to use service proxy
var factory = new ProxyFactory();
var service = factory.ImportService("http://www.site.com/webservice.asmx");

//how to call service method
var proxy = new Proxy(service.ServiceFile, service.ServiceName);
var result = proxy.InvokeMethod("CallMe", new ProxyMember("Id", 123));

//how to use method result
foreach (var item in result)
{
    var Id = item.Members["Id"].Value;
}


```
