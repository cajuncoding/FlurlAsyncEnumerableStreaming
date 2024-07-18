using System;
using System.Collections.Generic;


namespace Flurl.Http
{
    public static class FlurlAsyncEnumerableByteStreamExtensions
    {
        /// <summary>
        /// Initialize the async enumerable byte stream from a String Url.
        /// </summary>
        /// <param name="url">The string Url</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static IAsyncEnumerable<byte[]> GetStreamAsAsyncEnumerable(this string url, int bufferSize = FlurlAsyncEnumerableByteStream.DefaultBufferSize) 
            => new FlurlRequest(url).GetStreamAsAsyncEnumerable(bufferSize);

        /// <summary>
        /// Initialize the async enumerable byte stream from a Uri.
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static IAsyncEnumerable<byte[]> GetStreamAsAsyncEnumerable(this Uri uri, int bufferSize = FlurlAsyncEnumerableByteStream.DefaultBufferSize) 
            => new FlurlRequest(uri).GetStreamAsAsyncEnumerable(bufferSize);

        /// <summary>
        /// Initialize the async enumerable byte stream from a Flurl Url (e.g. Url Builder).
        /// </summary>
        /// <param name="url">The Flurl Url</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static IAsyncEnumerable<byte[]> GetStreamAsAsyncEnumerable(this Url url, int bufferSize = FlurlAsyncEnumerableByteStream.DefaultBufferSize)
            => new FlurlRequest(url).GetStreamAsAsyncEnumerable(bufferSize);

        /// <summary>
        /// Initialize the async enumerable byte stream from a Flurl Request.
        /// </summary>
        /// <param name="request">The Flurl Request</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static IAsyncEnumerable<byte[]> GetStreamAsAsyncEnumerable(this IFlurlRequest request, int bufferSize = FlurlAsyncEnumerableByteStream.DefaultBufferSize) 
            => new FlurlAsyncEnumerableByteStream(request, bufferSize);
    }
}
