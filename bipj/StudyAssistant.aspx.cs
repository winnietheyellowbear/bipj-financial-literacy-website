using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace bipj
{
    public partial class StudyAssistant : System.Web.UI.Page
    {
        private static readonly string openaiApiKey = "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"; // Replace with secure storage in production

        [WebMethod]
        public static string GetAIResponse(string question, string topic)
        {
            string apiKey = "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";

            string prompt = $"You are assisting a learner in the topic of \"{topic}\". " +
                            $"Only answer questions that are related to this topic. " +
                            $"If the question is not related to this topic, remind the student it's out of scope — but still give a brief answer. " +
                            $"Here is the student's question: \"{question}\"";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = $"You are a helpful tutor restricted to the topic: {topic}" },
            new { role = "user", content = prompt }
        },
                temperature = 0.7
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync("https://api.openai.com/v1/chat/completions", content).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;
                dynamic result = JsonConvert.DeserializeObject(responseString);

                return result.choices[0].message.content.ToString();
            }
        }

    }
}