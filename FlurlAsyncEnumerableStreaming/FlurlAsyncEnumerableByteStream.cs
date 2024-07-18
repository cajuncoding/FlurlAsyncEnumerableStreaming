using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Flurl.Http
{
    /// <summary>
    /// An AsyncEnumerable&lt;byte[]&gt; that can be enumerated to dynamically receive chunks (based on the buffer size provided) of the
    ///     raw Http Stream until the stream is completely read.
    /// This provides and easy-to-use api for efficient streaming of the raw http data (e.g. large http binary file streams) without
    ///     the need to read the entire stream into memory or store it in an intermediate location. And does so with very minimal
    ///     memory utilization or overhead.
    /// NOTE: Flurl provides a DownloadFileAsync() API but that requires storing of the file to a physical location that this avoids.
    /// </summary>
    public class FlurlAsyncEnumerableByteStream : IAsyncEnumerable<byte[]>
    {
        public const int DefaultBufferSize = 4096;
        protected IFlurlRequest FlurlRequest { get; }
        protected int BufferSize { get; }

        public FlurlAsyncEnumerableByteStream(IFlurlRequest flurlRequest, int bufferSize = DefaultBufferSize)
        {
            FlurlRequest = flurlRequest ?? throw new ArgumentNullException(nameof(flurlRequest));
            BufferSize = bufferSize <= 0 ? DefaultBufferSize : bufferSize;
        }

        public async IAsyncEnumerator<byte[]> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            //Inspired and adapted from Flurl's built-in DownloadFileAsync() method: https://github.com/tmenier/Flurl/blob/master/src/Flurl.Http/DownloadExtensions.cs
            //Also noted in the related Stack Overflow Post: https://stackoverflow.com/a/46859554/7293142
            using var resp = await FlurlRequest.SendAsync(HttpMethod.Get, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            await using var httpStream = await resp.GetStreamAsync().ConfigureAwait(false);

            if (!httpStream.CanRead)
                yield break;

            var byteBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            var memBuffer = byteBuffer.AsMemory();

            try
            {
                int readCount;

                //NOTE: Correctly filling our Buffer is non-trivial and is handled properly by our FillBufferAsync() method.
                //      Simply call Stream.ReadAsync() can result in only partially filled buffer even though there is still more data in the stream.
                while ((readCount = await FillBufferAsync(memBuffer, BufferSize, httpStream, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    //NOTE: We only need to Trim the Buffer when at End of Stream (e.g. Buffer was not completely filled)...
                    yield return readCount == BufferSize
                        ? byteBuffer
                        : memBuffer.Slice(0, readCount).ToArray();
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(byteBuffer);
            }
        }

        //NOTE: It is (not documented well) behaviour of Stream.Read() and Stream.ReadAsync() that it may not read all the requested bytes and fill the buffer. 
        //      Therefore, to ensure our buffer is filled as expected we must loop and fetch incrementally as needed to correctly fill the buffer.
        private async Task<int> FillBufferAsync(Memory<byte> memBuffer, int bufferSize, Stream stream, CancellationToken cancellationToken)
        {
            //Fill the Buffer as much as possible or until the end of the stream...
            var totalRead = 0;
            while (totalRead < bufferSize)
            {
                var targetMemBuffer = totalRead == 0 ? memBuffer : memBuffer.Slice(totalRead);
                var read = await stream.ReadAsync(targetMemBuffer, cancellationToken).ConfigureAwait(false);
                if (read == 0) break;
                totalRead += read;
            }

            return totalRead;
        }
    }
}
