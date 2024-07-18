using System.IO;
using FlurlAsyncEnumerableStreaming.Tests.TestConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlurlGraphQL.Tests
{
    public abstract class BaseTestCase
    {
        protected BaseTestCase()
        {
            TestsConfiguration.InitializeConfig();
            CurrentDirectory = Directory.GetCurrentDirectory();
        }

        public string CurrentDirectory { get; }

        public TestContext TestContext { get; set; } = null!;
    }
}