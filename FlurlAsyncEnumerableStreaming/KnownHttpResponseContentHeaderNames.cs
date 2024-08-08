namespace Flurl.Http
{
    /// <summary>
    /// Contains the standard set of 'Content' headers applicable to an HTTP response.
    /// </summary>
    public static class KnownHttpResponseContentHeaderNames
    {
        ///<summary>The MIME type of this content</summary>
        public const string ContentType = "Content-Type";
        ///<summary>The length of the response body in octets (8-bit bytes)</summary>
        public const string ContentLength = "Content-Length";
        ///<summary>The type of encoding used on the data. See HTTP compression.</summary>
        public const string ContentEncoding = "Content-Encoding";
        ///<summary>The language the content is in</summary>
        public const string ContentLanguage = "Content-Language";
        ///<summary>An alternate location for the returned data</summary>
        public const string ContentLocation = "Content-Location";
        ///<summary>A Base64-encoded binary MD5 sum of the content of the response</summary>
        public const string ContentMD5 = "Content-MD5";
        ///<summary>An opportunity to raise a File Download dialogue box for a known MIME type with binary format or suggest a filename for dynamic content. Quotes are necessary with special characters.</summary>
        public const string ContentDisposition = "Content-Disposition";
        ///<summary>Where in a full body message this partial message belongs</summary>
        public const string ContentRange = "Content-Range";
        ///<summary>The last modified date for the requested object, inRFC 2822 format</summary>
        public const string LastModified = "Last-Modified";
    }
}

