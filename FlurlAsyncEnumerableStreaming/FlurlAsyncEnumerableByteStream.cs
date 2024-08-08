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
    /// 
    /// Inspired and adapted from Flurl's built-in DownloadFileAsync() method: https://github.com/tmenier/Flurl/blob/master/src/Flurl.Http/DownloadExtensions.cs
    /// Also noted in the related Stack Overflow Post: https://stackoverflow.com/a/46859554/7293142
    /// </summary>
    public class FlurlAsyncEnumerableByteStream : IAsyncEnumerable<byte[]>, IAsyncDisposable
    {
        private readonly ByteStreamAsyncEnumerator _byteStreamAsyncEnumerator;

        public bool IsStreamOpen => _byteStreamAsyncEnumerator?.IsStreamOpen ?? false;

        public FlurlAsyncEnumerableByteStream(IFlurlRequest flurlRequest, int bufferSize = ByteStreamAsyncEnumerator.DefaultBufferSize, CancellationToken cancellationToken = default)
        {
            _byteStreamAsyncEnumerator = new ByteStreamAsyncEnumerator(flurlRequest, bufferSize, cancellationToken);
        }

        public IAsyncEnumerator<byte[]> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => _byteStreamAsyncEnumerator;

        public ValueTask<HttpResponseContentHeaders> GetHeadersAsync(CancellationToken cancellationToken = default)
            => _byteStreamAsyncEnumerator.GetHeadersAsync();

        public async ValueTask DisposeAsync()
        {
            if (_byteStreamAsyncEnumerator != null) await _byteStreamAsyncEnumerator.DisposeAsync();
        }
    }

    public sealed class ByteStreamAsyncEnumerator : IAsyncEnumerator<byte[]>, IDisposable
    {
        public const int DefaultBufferSize = 4096;

        private readonly int _bufferSize;
        private readonly IFlurlRequest _flurlRequest;
        private readonly CancellationToken _cancellationToken;
        private bool _isInitialized = false;
        private IFlurlResponse _flurlResponse;
        private Stream _responseStream;
        private byte[] _byteBuffer;
        private Memory<byte> _memoryBuffer;

        public bool IsStreamOpen => _responseStream?.CanRead ?? false;

        public byte[] Current { get; private set; }

        public ByteStreamAsyncEnumerator(IFlurlRequest flurlRequest, int bufferSize = DefaultBufferSize, CancellationToken cancellationToken = default)
        {
            _flurlRequest = flurlRequest ?? throw new ArgumentNullException(nameof(flurlRequest));
            _cancellationToken = cancellationToken;
            _bufferSize = bufferSize <= 0 ? DefaultBufferSize : bufferSize;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            //NOTE: It's important to check the IsInitialized flag and allow the Async Enumerator to be re-initialized as needed to support multiple enumerations...
            //      Of course the Request will be re-sent and re-initialized but this is expected for a Streaming process where we do
            //          not buffer contents into memory; not buffering 100% of all content is the Primary Benefit of streaming!
            if (!_isInitialized) await InitializeStreamAndBuffersAsync().ConfigureAwait(false);

            //NOTE: Correctly filling our Buffer is non-trivial and is handled properly by our FillBufferAsync() method.
            //      Simply call Stream.ReadAsync() can result in only partially filled buffer even though there is still more data in the stream.
            var readCount = await FillBufferAsync(_memoryBuffer, _bufferSize, _responseStream, _cancellationToken).ConfigureAwait(false);
            if (readCount > 0)
            {
                //NOTE: We need to Trim the Buffer when at End of Stream (e.g. Buffer was not completely filled) to ensure we don't have extraneous bytes from prior fills left-over...
                Current = readCount == _bufferSize
                    ? _byteBuffer
                    : _memoryBuffer.Slice(0, readCount).ToArray();

                return true;
            }

            Current = null;
            return false;
        }

        public async ValueTask<HttpResponseContentHeaders> GetHeadersAsync()
        {
            if (!_isInitialized) await InitializeStreamAndBuffersAsync().ConfigureAwait(false);
            return new HttpResponseContentHeaders(_flurlResponse);
        }

        private async Task InitializeStreamAndBuffersAsync()
        {
            _flurlResponse = await _flurlRequest.SendAsync(HttpMethod.Get, null, HttpCompletionOption.ResponseHeadersRead, _cancellationToken).ConfigureAwait(false);
            _responseStream = await _flurlResponse.GetStreamAsync().ConfigureAwait(false);

            if (!_responseStream.CanRead)
                throw new InvalidOperationException("The response stream for the request could not be initialized and/or cannot be read.");

            _byteBuffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
            _memoryBuffer = _byteBuffer.AsMemory();
            _isInitialized = true;
            _isDisposed = false;
        }

        //NOTE: It is (not documented well) behaviour of Stream.Read() and Stream.ReadAsync() that it may not read all the requested bytes and fill the buffer. 
        //      Therefore, to ensure our buffer is filled as expected we must loop and fetch incrementally as needed to correctly fill the buffer.
        private async ValueTask<int> FillBufferAsync(Memory<byte> memBuffer, int bufferSize, Stream stream, CancellationToken cancellationToken)
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

        private bool _isDisposed = false;
        public ValueTask DisposeAsync()
        {
            Dispose();
            
            #if NET6_0_OR_GREATER
            return ValueTask.CompletedTask;
            #else
            //Same as .NET6+ ValueTask.CompletedTask (which netstandard2.1 does not have)...
            return default;
            #endif
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            Current = null;
            _memoryBuffer = null;
            ArrayPool<byte>.Shared.Return(_byteBuffer);
            _byteBuffer = null;
            _responseStream?.Dispose();
            _flurlResponse?.Dispose();
            //NOTE: It's important to clear the IsInitialized flag to allow the Async Enumerator to be enumerated multiple times...
            //      Of course the Request will be re-sent and re-initialized but this is expected for a Streaming process where we do
            //          not buffer contents into memory; not buffering 100% of all content is the Primary Benefit of streaming!
            _isInitialized = false;

            _isDisposed = true;
        }
    }
}
