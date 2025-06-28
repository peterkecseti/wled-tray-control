using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLEDProfiles
{
    public class Preset
    {
        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("bri")]
        public int? Brightness { get; set; }

        [JsonProperty("on")]
        public bool? On { get; set; }

        [JsonProperty("fx")]
        public int? Effect { get; set; }

        [JsonProperty("sx")]
        public int? Speed { get; set; }

        [JsonProperty("ix")]
        public int? Intensity { get; set; }

        [JsonProperty("col")]
        public int[][] Colors { get; set; }
    }
}
