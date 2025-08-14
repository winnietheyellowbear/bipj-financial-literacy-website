// PolicyAnalysis.cs
using Newtonsoft.Json;

namespace bipj
{
    /// <summary>
    /// Represents the final analysis from the Gemini API, identifying the single best policy in a given category.
    /// </summary>
    public class PolicyAnalysis
    {
        [JsonProperty("insuranceType")]
        public string InsuranceType { get; set; }

        [JsonProperty("bestPolicyName")]
        public string BestPolicyName { get; set; }

        [JsonProperty("justification")]
        public string Justification { get; set; }
    }
}
