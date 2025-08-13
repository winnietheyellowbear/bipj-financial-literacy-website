using System.Collections.Generic;
using Newtonsoft.Json;

namespace bipj
{
    /// <summary>
    /// Represents the entire structured response for the policy comparison from the Gemini API.
    /// </summary>
    public class InsurancePolicyComparisonResponse
    {
        [JsonProperty("comparisonTable")]
        public List<InsurancePolicyComparison> ComparisonTable { get; set; }

        [JsonProperty("bestFitExplanation")]
        public string BestFitExplanation { get; set; }
    }
}
