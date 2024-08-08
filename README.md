# FlurlAsyncEnumerableStreaming
## Overview
A set of lightweight and easy to use Async Enumerable Byte Streaming extensions for Flurl.Http.

This provides a streamlined wasy to stream binary data for Http web requests in a lightweight, simplified, asynchronous, fluent manner using the amazing Flurl Http library!

The key benefit here is that you can stream the binary data of any Http Reqeust efficiently without having to buffer the entire stream/response into memory before
directing it to it's proper destination. Using `AsyncEnumerable` you are able to decouple and greatly simplify/streamline your logic from the 
complexities of the streaming process.

### Give Star 🌟
**If you like this project and/or use it the please give it a Star 🌟 (c'mon it's free, and it'll help others find the project)!**

### [Buy me a Coffee ☕](https://www.buymeacoffee.com/cajuncoding)
*I'm happy to share with the community, but if you find this useful (e.g for professional use), and are so inclinded,
then I do love-me-some-coffee!*

<a href="https://www.buymeacoffee.com/cajuncoding" target="_blank">
<img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174">
</a> 

## Usage
Using any valid HTTP request url with `Flurl.Http`, you can call the extension method `GetStreamAsAsyncEnumerable()` provided which
returns a `FlurlAsyncEnumerableByteStream` that implements `IAsyncEnumerable<byte>` & `IAsyncDisposable`. 

It also provides a helper method `GetHeadersAsync()`
which is often needed to access details such as content headers (e.g. Content-Type, Content-Length) very often needed for
binary file streaming.

In general, you will use `await foreach(...)` to enumerate the async stream which will automatically dispose of it,
ensuring that all resources, including the IFlurlResposne, are all properly disposed of.  However, if you do not enumerate it then you should
explicitly dispose of the `FlurlAsyncEnumerableByteStream`.

In addition, the `FlurlAsyncEnumerableByteStream` is re-entrant and can be safely enumerated multiple times -- even after disposal which can help simplify complex code flows.
It will lazily re-initialize the Http Response as needed and can be disposed of safely again; usually automatically by the `await forech(...)` process.

### Simple Example
```csharp

//A valid Url to a Binary file you want to stream using Flurl.Http...
var downloadUrl = "https://www.dropbox.com/abc/de/Your_Test_File.mov?raw=1";

//A local or remote streaming destination... here we use the FileSystem...
var tempDirectory = "D:\Your_Temp_Directory";
var fileInfo = new FileInfo(Path.Combine(tempDirectory, Path.GetFileName(downloadUrl.Path)));
if (fileInfo.Exists) fileInfo.Delete();

await using (var fileStream = fileInfo.OpenWrite())
{
    //Now we can easily iterate over all binary bytes by streaming in chunks...
    //NOTE: GetStreamAsAsyncEnumerable() is the Flurl Extension provided by this library...
    await foreach (var byteChunk in downloadUrl.GetStreamAsAsyncEnumerable())
    {
        await fileStream.WriteAsync(byteChunk);
    }
}

//Now you've finished efficiently streaming without having to buffer the entire binary/stream into Memory!
Process.Start("explorer.exe", $"/open, \"{tempDirectory}\"");
```

## Accessing Content Headers (e.g. Content-Type, Content-Length, etc.)
```csharp

//A valid Url to a Binary file you want to stream using Flurl.Http...
var downloadUrl = "https://www.dropbox.com/abc/de/Your_Test_File.mov?raw=1"

await using var asyncEnumerableStream = downloadUrl.GetStreamAsAsyncEnumerable();
var contentHeaders = await asyncEnumerableStream.GetHeadersAsync();

//Access any Content Headers from the Properties provided...
Console.WriteLine($"Content-Type = [{contentHeaders.ContentType}]");

//NOTE: ContentLength is already parsed/converted to strongly typed Long...
Console.WriteLine($"Content-Length = [{contentHeaders.ContentLength}]");

//Or lookup any Header you like via string name (if availalbe in the response)...
var lastModified = contentHeaders.HeadersLookup["Last-Modified"].FirstOrDefault();

//Or use some of the known Content Header constants provided...
var md5 = contentHeaders.HeadersLookup[KnownHttpResponseContentHeaderNames.ContentMD5].FirstOrDefault();

```



