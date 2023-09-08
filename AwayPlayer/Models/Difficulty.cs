using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwayPlayer.Models
{
    internal class Difficulty
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("difficultyName")]
        public string Name { get; set; }
        [JsonProperty("modeName")]
        public string ModeName { get; set; }
    }
}
