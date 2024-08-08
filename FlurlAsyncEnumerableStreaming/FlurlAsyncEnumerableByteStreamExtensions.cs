using System;
using System.Threading;


namespace Flurl.Http
{
    public static class FlurlAsyncEnumerableByteStreamExtensions
    {
        /// <summary>
        /// Initialize the async enumerable byte stream from a String Url.
        /// </summary>
        /// <param name="url">The string Url</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static FlurlAsyncEnumerableByteStream GetStreamAsAsyncEnumerable(this string url, int bufferSize = ByteStreamAsyncEnumerator.DefaultBufferSize, CancellationToken cancellationToken = default) 
            => new FlurlRequest(url).GetStreamAsAsyncEnumerable(bufferSize, cancellationToken);

        /// <summary>
        /// Initialize the async enumerable byte stream from a Uri.
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static FlurlAsyncEnumerableByteStream GetStreamAsAsyncEnumerable(this Uri uri, int bufferSize = ByteStreamAsyncEnumerator.DefaultBufferSize, CancellationToken cancellationToken = default) 
            => new FlurlRequest(uri).GetStreamAsAsyncEnumerable(bufferSize, cancellationToken);

        /// <summary>
        /// Initialize the async enumerable byte stream from a Flurl Url (e.g. Url Builder).
        /// </summary>
        /// <param name="url">The Flurl Url</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static FlurlAsyncEnumerableByteStream GetStreamAsAsyncEnumerable(this Url url, int bufferSize = ByteStreamAsyncEnumerator.DefaultBufferSize, CancellationToken cancellationToken = default)
            => new FlurlRequest(url).GetStreamAsAsyncEnumerable(bufferSize, cancellationToken);

        /// <summary>
        /// Initialize the async enumerable byte stream from a Flurl Request.
        /// </summary>
        /// <param name="request">The Flurl Request</param>
        /// <param name="bufferSize">Buffer size in bytes. Default is 4096. Each iteration of the AsyncEnumerable will receive a byte[] buffer based on this size/chunk from the Http Stream.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An AsyncEnumerable that can be enumerated to dynamically receive buffer byte[] chunks from the stream until the full stream is read.</returns>
        public static FlurlAsyncEnumerableByteStream GetStreamAsAsyncEnumerable(this IFlurlRequest request, int bufferSize = ByteStreamAsyncEnumerator.DefaultBufferSize, CancellationToken cancellationToken = default) 
            => new FlurlAsyncEnumerableByteStream(request, bufferSize, cancellationToken);
    }
}
