using Newtonsoft.Json;

namespace MAP_TEST.Models
{
    public class Plane
    {
        [JsonProperty("lat")]
        public float iha_enlem { get; set; }

        [JsonProperty("lng")]
        public float iha_boylam { get; set; }
        [JsonProperty("yaw")]
        public float iha_yonelme { get; set; }
        [JsonProperty("alt")]
        public float iha_irtifa { get; set; }

    }
}
