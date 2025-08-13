// RecommendedPolicy.cs
using Newtonsoft.Json;

namespace bipj
{
    /// <summary>
    /// Represents a single recommended insurance policy with its details.
    /// </summary>
    public class RecommendedPolicy
    {
        [JsonProperty("policyName")]
        public string PolicyName { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }
    }
}
