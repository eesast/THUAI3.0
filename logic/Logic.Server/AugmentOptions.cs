using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace Logic.Server
{
    class AugmentOptions
    {
        [Option('p', "port", Required = true, HelpText = "Server listening port")]
        public ushort serverPort { get; set; } = 10086;

        [Option('c', "playerCount", Required = false, HelpText = "Player count in each CommunicationID.Item1")]
        public ushort playerCount { get; set; } = 1;

        [Option('a', "agentCount", Required = false, HelpText = "Agent count")]
        public ushort agentCount { get; set; } = 1;

        [Option('t', "gameTime", Required = false, HelpText = "Max game time in second")]
        public uint totalGameTimeSeconds { get; set; } = 10000;

        [Option('d', "debugLevel", Required = false, HelpText = "Debug level")]
        public ushort debugLevel { get; set; } = 0;
    }
}
