using Newtonsoft.Json;

namespace bipj
{
    /// <summary>
    /// Represents the details of a single insurance policy for comparison.
    /// </summary>
    public class InsurancePolicyComparison
    {
        [JsonProperty("feature")]
        public string Feature { get; set; }

        [JsonProperty("integratedShield")]
        public string IntegratedShield { get; set; }

        [JsonProperty("termLife")]
        public string TermLife { get; set; }

        [JsonProperty("criticalIllness")]
        public string CriticalIllness { get; set; }
    }
}