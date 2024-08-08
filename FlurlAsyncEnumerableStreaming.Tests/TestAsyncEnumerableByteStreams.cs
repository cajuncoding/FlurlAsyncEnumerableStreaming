using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Flurl.Http;
using FlurlAsyncEnumerableStreaming.Tests.TestConfig;
using FlurlGraphQL.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlurlAsyncEnumerableStreaming
{
    [TestClass]
    public class TestAsyncEnumerableByteStreams : BaseTestCase
    {
        private const int BytesPerMegabyte = 1024 * 1024;

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TestRealWorldExample()
        {
            var tempDirectory = TestsConfiguration.TargetDownloadPath;
            var downloadUrl = TestsConfiguration.LargeFileDownloadUrl;

            var fileInfo = new FileInfo(Path.Combine(tempDirectory, $"GetEnumeratedBytesBytes_{Path.GetFileName(downloadUrl.Path)}"));
            if (fileInfo.Exists) fileInfo.Delete();

            var downloadTimer = Stopwatch.StartNew();
            var initialMemoryConsumption = GetCurrentMemoryUsage();

            await using var asyncEnumerableStream = downloadUrl.GetStreamAsAsyncEnumerable();
            var contentHeaders = await asyncEnumerableStream.GetHeadersAsync();

            var byteSize = 0L;
            await using (var fileStream = fileInfo.OpenWrite())
            {
                await foreach (var byteChunk in asyncEnumerableStream)
                {
                    byteSize += byteChunk.Length;
                    await fileStream.WriteAsync(byteChunk);
                }
            }

            downloadTimer.Stop();
            var finalMemoryConsumption = GetCurrentMemoryUsage();

            Assert.AreEqual(contentHeaders.ContentLength, byteSize);

            fileInfo.Refresh();

            TestContext.WriteLine(string.Empty);
            TestContext.WriteLine("---- Enumerate All Bytes ----");
            TestContext.WriteLine($"Downloaded in [{downloadTimer.Elapsed.TotalSeconds}]");
            TestContext.WriteLine($"Content-Length = [{contentHeaders.ContentLength}]");
            TestContext.WriteLine($"Content-Type = [{contentHeaders.ContentType}]");
            TestContext.WriteLine($"Actual Size = [{byteSize}], [{(byteSize / BytesPerMegabyte)} MB]");
            TestContext.WriteLine($"Filesystem Size = [{fileInfo.Length}], [{(fileInfo.Length / BytesPerMegabyte)} MB]");
            TestContext.WriteLine($"Memory Usage = [{finalMemoryConsumption - initialMemoryConsumption}] MB");

            Process.Start("explorer.exe", $"/open, \"{tempDirectory}\"");
        }

        [TestMethod]
        public async Task TestMultipleEnumerations()
        {
            var tempDirectory = TestsConfiguration.TargetDownloadPath;
            var downloadUrl = TestsConfiguration.SmallFileDownloadUrl;

            await using var asyncEnumerableStream = downloadUrl.GetStreamAsAsyncEnumerable();

            for (var i = 0; i < 2; i++)
            {
                //Stream should never be open when first instantiated or after an iteration (e.g. Disposal)!
                Assert.IsFalse(asyncEnumerableStream.IsStreamOpen);

                var contentHeaders = await asyncEnumerableStream.GetHeadersAsync();
                Assert.IsTrue(asyncEnumerableStream.IsStreamOpen);

                var fileInfo = new FileInfo(Path.Combine(tempDirectory, $"GetEnumeratedBytesBytes_{Path.GetFileName(downloadUrl.Path)}"));
                if (fileInfo.Exists) fileInfo.Delete();

                var byteSize = 0L;
                await using (var fileStream = fileInfo.OpenWrite())
                {
                    await foreach (var byteChunk in asyncEnumerableStream)
                    {
                        Assert.IsTrue(asyncEnumerableStream.IsStreamOpen);

                        byteSize += byteChunk.Length;
                        await fileStream.WriteAsync(byteChunk);
                    }
                }

                Assert.IsFalse(asyncEnumerableStream.IsStreamOpen);
                Assert.AreEqual(contentHeaders.ContentLength, byteSize);

                fileInfo.Refresh();

                TestContext.WriteLine(string.Empty);
                TestContext.WriteLine($"---- Iteration [{i}] ----");
                TestContext.WriteLine($"Content-Length = [{contentHeaders.ContentLength}]");
                TestContext.WriteLine($"Content-Type = [{contentHeaders.ContentType}]");
                TestContext.WriteLine($"Actual Size = [{byteSize}], [{(byteSize / BytesPerMegabyte)} MB]");
                TestContext.WriteLine($"Filesystem Size = [{fileInfo.Length}], [{(fileInfo.Length / BytesPerMegabyte)} MB]");
            }

            Process.Start("explorer.exe", $"/open, \"{tempDirectory}\"");
        }

        [TestMethod]
        public async Task TestDownloadOfLargeFilesAsync()
        {
            var tempDirectory = TestsConfiguration.TargetDownloadPath;
            var downloadUrl = TestsConfiguration.LargeFileDownloadUrl;

            //Retrieve using built-in GetBytesAsync()...
            var downloadTimer = Stopwatch.StartNew();
            var fullRequestBytes = await downloadUrl.GetBytesAsync().ConfigureAwait(false);
            downloadTimer.Stop();

            //Retrieve using Async Enumerable Streaming...
            //NOTE: For the Unit Test we read the full file into Memory but this approach does not require that the full file be loaded 😁
            downloadTimer.Restart();
            using var memoryStream = new MemoryStream();
            await foreach (var byteChunk in downloadUrl.GetStreamAsAsyncEnumerable())
                await memoryStream.WriteAsync(byteChunk);
            downloadTimer.Stop();
            var enumeratedRequestBytes = memoryStream.ToArray();

            TestContext.WriteLine("---- Get All Bytes ----");
            TestContext.WriteLine($"Downloaded in [{downloadTimer.Elapsed.TotalSeconds}]");
            TestContext.WriteLine($"Size = [{fullRequestBytes.Length}], [{(fullRequestBytes.Length / BytesPerMegabyte)} MB]");
            //await File.WriteAllBytesAsync(Path.Combine(tempDirectory, $"GetFullBytes_{Path.GetFileName(url.Path)}"), fullRequestBytes);

            TestContext.WriteLine(string.Empty);
            TestContext.WriteLine("---- Enumerate All Bytes ----");
            TestContext.WriteLine($"Downloaded in [{downloadTimer.Elapsed.TotalSeconds}]");
            TestContext.WriteLine($"Size = [{fullRequestBytes.Length}], [{(fullRequestBytes.Length / BytesPerMegabyte)} MB]");
            //await File.WriteAllBytesAsync(Path.Combine(tempDirectory, $"GetEnumeratedBytesBytes_{Path.GetFileName(url.Path)}"), enumeratedRequestBytes);

            //Process.Start("explorer.exe", $"/open, \"{tempDirectory}\"");
            Assert.AreEqual(fullRequestBytes.Length, enumeratedRequestBytes.Length);
        }


        [TestMethod]
        public async Task TestMemoryConsumptionOfDownloadingLargeFilesAsync()
        {
            var downloadUrl = TestsConfiguration.LargeFileDownloadUrl;

            // The proc.PrivateMemorySize64 will return the private memory usage in byte.
            var memoryInitially = GetCurrentMemoryUsage();

            //Retrieve using built-in GetBytesAsync()...
            var fullRequestBytes = await downloadUrl.GetBytesAsync().ConfigureAwait(false);
            Assert.IsTrue(fullRequestBytes.Length > 0);

            var memoryAfterGetBytesAsync = GetCurrentMemoryUsage();
            var memoryChangeAfterGetBytesAsync = memoryAfterGetBytesAsync - memoryInitially;

            //Retrieve using Async Enumerable Streaming...
            //NOTE: For the Unit Test we read the full file into Memory but this approach does not require that the full file be loaded 😁
            var enumerationCounter = 0;
            await foreach (var byteChunk in downloadUrl.GetStreamAsAsyncEnumerable())
            {
                Assert.IsTrue(byteChunk.Length > 0);
                enumerationCounter++;
            }
            Assert.IsTrue(enumerationCounter > 0);

            var memoryAfterAsyncEnumeration = GetCurrentMemoryUsage();
            var memoryChangeAfterAsyncEnumeration = memoryAfterAsyncEnumeration - memoryAfterGetBytesAsync;

            TestContext.WriteLine($"Initial Memory = [{memoryInitially}] MB");
            TestContext.WriteLine($"Memory Change after GetBytesAsync() = [{memoryChangeAfterGetBytesAsync}] MB");
            TestContext.WriteLine($"Memory Change after GetStreamAsAsyncEnumerable() = [{memoryChangeAfterAsyncEnumeration}] MB");

            Assert.IsTrue(memoryChangeAfterAsyncEnumeration < memoryChangeAfterGetBytesAsync);
        }

        private decimal GetCurrentMemoryUsage()
        {
            using Process proc = Process.GetCurrentProcess();
            return (decimal)proc.PrivateMemorySize64 / BytesPerMegabyte;
        }
    }
}
