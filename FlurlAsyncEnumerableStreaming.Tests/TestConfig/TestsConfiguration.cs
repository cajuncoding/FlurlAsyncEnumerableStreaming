using System;
using Flurl;

namespace FlurlAsyncEnumerableStreaming.Tests.TestConfig
{
    internal class TestsConfiguration
    {
        public static bool IsInitialized { get; private set; } = false;

        public static Url LargeFileDownloadUrl => GetConfigValue(nameof(LargeFileDownloadUrl))
            .RemoveQueryParam("dl")
            .AppendQueryParam("raw", 1);

        public static string TargetDownloadPath => GetConfigValue(nameof(TargetDownloadPath));

        private static Exception CreateMissingConfigException(string configName) => new Exception($"The configuration value for [{configName}] could not be loaded.");

        public static string GetConfigValue(string configName)
        {
            InitializeConfig();
            return Environment.GetEnvironmentVariable(configName) ?? throw CreateMissingConfigException(configName);
        }

        public static bool InitializeConfig()
        {
            if (!IsInitialized)
                ConfigHelpers.InitEnvironmentFromLocalSettingsJson();

            return IsInitialized = true;
        }
    }
}
