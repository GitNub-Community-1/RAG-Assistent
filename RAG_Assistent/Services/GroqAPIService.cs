using System.Text;
using System.Text.Json;
using Test_RAG_System.Models.Entities;
using Test_RAG_System.Services.Interfaces;

namespace Test_RAG_System.Services;

public class GroqAPIService : IGroqAPIService
{
    string GroqApiKey = "";

    public GroqAPIService(string groqApiKey)
    {
        GroqApiKey = groqApiKey;
    }
// =====================================================
//  ИСПРАВЛЕННЫЙ CallGroqAsync
// =====================================================


    public async Task<string> CallGroqAsync(string systemPrompt, string userMessage)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GroqApiKey}");
            client.DefaultRequestHeaders.Add("User-Agent", "RAG-App/1.0");

            var request = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.2,
                max_tokens = 2000
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API error {response.StatusCode}: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка Groq: {ex.Message}");
            throw;
        }
    }
   
}