## Using HTTP/2 with .NET HttpClient (2025 update)

Modern .NET (7/8/9) negotiates HTTP/2 automatically when supported by the server, TLS, and platform. You usually do not need a custom handler; `HttpClient` will use HTTP/2 or HTTP/3 when available. You can also set `HttpRequestMessage.Version = HttpVersion.Version20` or `new HttpRequestMessage { VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher }` for intent.

The legacy guidance below is preserved for older .NET Framework/Windows stacks that required WinHttpHandler.

---

## [How to make the .NET HttpClient use HTTP/2?](http://stackoverflow.com/questions/32685151/how-to-make-the-net-httpclient-use-http-2-0)

### Q

I have an asp.net web api hosted on IIS 10 (windows server 2016). When I make a GET request to this from a Microsoft Edge browser, I see that HTTP 2.0 is used in IIS logs

```
2015-09-20 21:57:59 100.76.48.17 GET /RestController/Native - 443 - 73.181.195.76 HTTP/2.0 Mozilla/5.0+(Windows+NT+10.0;+Win64;+x64)+AppleWebKit/537.36+(KHTML,+like+Gecko)+Chrome/42.0.2311.135+Safari/537.36+Edge/12.10240 - 200 0 0 7299
```

However, when a GET request is made through a .net 4.6 client as below,

```c#
using (var client = new HttpClient())
{
    client.BaseAddress = new Uri("https://myapp.cloudapp.net/");

    HttpResponseMessage response = await client.GetAsync("RestController/Native");
    if (response.IsSuccessStatusCode)
    {
        await response.Content.CopyToAsync(new MemoryStream(buffer));
    }
}
```

I see the following HTTP 1.1 log in the server logs

```
2015-09-20 20:57:41 100.76.48.17 GET /RestController/Native - 443 - 131.107.160.196 HTTP/1.1 - - 200 0 0 707
```

How can I make the .net client use HTTP/2.0 ?



### A

1.Make sure you are on the latest version of Windows 10.

2.Install WinHttpHandler:

```
Install-Package System.Net.Http.WinHttpHandler
```

3.Extend WinHttpHandler to add http2.0 support:

```c#
public class Http2CustomHandler : WinHttpHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        request.Version = new Version("2.0");
        return base.SendAsync(request, cancellationToken);
    }
}
```

4.Pass the handler above to the HttpClient constructor

```c#
using (var httpClient = new HttpClient(new Http2CustomHandler()))
{
      // your custom code
}
```