using System;
using System.Collections;
using System.Collections.Generic;
using Communication.Proto;
using Communication.Server;
using THUnity2D;
using static THUnity2D.Tools;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
namespace Logic.Constant
{
    public static class Constant
    {
        public const double FrameRate = 20;
        public const double TimeInterval = 1 / FrameRate;
        public const double TimeIntervalInMillisecond = 1000 * 1 / FrameRate;
        public const double HalfTimeIntervalInMillisecond = TimeIntervalInMillisecond / 2;

        private static JObject _configs;
        public static JObject Configs
        {
            get
            {
                _configs = _configs ?? (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(@"Config/Config.json")));
                return _configs;
            }
        }
    }

}
