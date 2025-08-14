// PolicyCategory.cs
using System.Collections.Generic;
using Newtonsoft.Json;

namespace bipj
{
    /// <summary>
    /// Represents a category of insurance (e.g., "Life Insurance") and holds a list of recommended policies for that category.
    /// </summary>
    public class PolicyCategory
    {
        [JsonProperty("insuranceType")]
        public string InsuranceType { get; set; }

        [JsonProperty("recommendedPolicies")]
        public List<RecommendedPolicy> RecommendedPolicies { get; set; }
    }
}
