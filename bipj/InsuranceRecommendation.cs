// InsuranceRecommendation.cs
using Newtonsoft.Json;

namespace bipj
{
    /// <summary>
    /// Represents a single, structured insurance recommendation returned by the Gemini API.
    /// </summary>
    public class InsuranceRecommendation
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coverage")]
        public string Coverage { get; set; }

        [JsonProperty("explanation")]
        public string Explanation { get; set; }

        [JsonProperty("budgetPercentage")]
        public int BudgetPercentage { get; set; }
    }
}
