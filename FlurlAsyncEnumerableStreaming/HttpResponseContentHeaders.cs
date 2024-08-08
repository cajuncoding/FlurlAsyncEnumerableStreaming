using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Flurl.Http
{
    public class HttpResponseContentHeaders
    {
        public HttpResponseContentHeaders(IFlurlResponse flurlResponse)
        {
            HeadersLookup = flurlResponse.Headers.ToLookup(
                h => h.Name, 
                h => h.Value, 
                StringComparer.OrdinalIgnoreCase
            );
        }

        /// <summary>
        /// A Lookup of all Headers available from the Response...
        /// </summary>
        private ILookup<string, string> HeadersLookup { get; }

        private string GetHeader(string name) => HeadersLookup?[name].FirstOrDefault();

        private T GetHeader<T>(string name)
        {
            var value = GetHeader(name);
            return string.IsNullOrWhiteSpace(value)
                ? default
                : (T)Convert.ChangeType(value, typeof(T));
        }

        ///<summary>The MIME type of this content</summary>
        public string ContentType => GetHeader(KnownHttpResponseContentHeaderNames.ContentType);
        ///<summary>The length of the response body in octets (8-bit bytes)</summary>
        public long ContentLength => GetHeader<long>(KnownHttpResponseContentHeaderNames.ContentLength);
        ///<summary>The type of encoding used on the data. See HTTP compression.</summary>
        public string ContentEncoding => GetHeader(KnownHttpResponseContentHeaderNames.ContentEncoding);
        ///<summary>The language the content is in</summary>
        public string ContentLanguage => GetHeader(KnownHttpResponseContentHeaderNames.ContentLanguage);
        ///<summary>An alternate location for the returned data</summary>
        public string ContentLocation => GetHeader(KnownHttpResponseContentHeaderNames.ContentLocation);
        ///<summary>A Base64-encoded binary MD5 sum of the content of the response</summary>
        public string ContentMD5 => GetHeader(KnownHttpResponseContentHeaderNames.ContentMD5);
        ///<summary>An opportunity to raise a File Download dialogue box for a known MIME type with binary format or suggest a filename for dynamic content. Quotes are necessary with special characters.</summary>
        public string ContentDisposition => GetHeader(KnownHttpResponseContentHeaderNames.ContentDisposition);
        ///<summary>Where in a full body message this partial message belongs</summary>
        public string ContentRange => GetHeader(KnownHttpResponseContentHeaderNames.ContentRange);
        ///<summary>The last modified date for the requested object, inRFC 2822 format</summary>
        public string LastModified => GetHeader(KnownHttpResponseContentHeaderNames.LastModified);
    }
}
