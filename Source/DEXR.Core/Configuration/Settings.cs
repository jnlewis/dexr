using System;
using System.IO;

namespace DEXR.Core.Configuration
{
    internal static class Settings
    {
        private static ProtocolConfiguration _protocolConfiguration;
        public static ProtocolConfiguration ProtocolConfiguration
        {
            get
            {
                if (_protocolConfiguration == null)
                {
                    var jsonConfig = File.ReadAllText("config.json");
                    _protocolConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<ProtocolConfiguration>(jsonConfig);

                    //validate configuration
                    if(_protocolConfiguration.ServerPort == 0 || 
                        _protocolConfiguration.SeedList == null)
                    {
                        throw new Exception("The config.json file configuration is incorrectly formatted.");
                    }

                    if(_protocolConfiguration.ServerHostType != "default" && 
                        _protocolConfiguration.ServerHostType != "localhost" &&
                        _protocolConfiguration.ServerHostType != "default_https" &&
                        _protocolConfiguration.ServerHostType != "localhost_https")
                    {
                        throw new Exception("The config.json ServerHostType is invalid.");
                    }
                }

                return _protocolConfiguration;
            }
        }

        private static NewChainSetup _newChainSetup;
        public static NewChainSetup NewChainSetup
        {
            get
            {
                if (_newChainSetup == null)
                {
                    var jsonConfig = File.ReadAllText("deploy.json");
                    _newChainSetup = Newtonsoft.Json.JsonConvert.DeserializeObject<NewChainSetup>(jsonConfig);

                    //validate configuration
                    if (_newChainSetup.InitialSupply < 0 || 
                        _newChainSetup.Decimals < 0 ||
                        string.IsNullOrEmpty(_newChainSetup.NativeTokenSymbol) ||
                        string.IsNullOrEmpty(_newChainSetup.NativeTokenName))
                    {
                        throw new Exception("The deploy.json file configuration is incorrectly formatted.");
                    }
                }

                return _newChainSetup;
            }
        }
    }
}
