using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace GameForm
{
    class AugmentOptions
    {
        [Option('p', "port", Required = true, HelpText = "Agent listening port")]
        public ushort agentPort { get; set; } = 30000;

        [Option('d', "debugLevel", Required = false, HelpText = "Debug level")]
        public ushort debugLevel { get; set; } = 0;

        [Option('t', "talent", Required = true, HelpText = "Talent")]
        public ushort talent { get; set; } = 0;

        [Option('b', "isPlayBack", Required = false, HelpText = "is PlayBack")]
        public int isPlayBack { get; set; } = 0;
    }
}
