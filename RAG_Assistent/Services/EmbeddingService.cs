using System.Text;
using System.Text.Json;
using Test_RAG_System.Models.Entities;
using Test_RAG_System.Services.Interfaces;

namespace Test_RAG_System.Services;

public class EmbeddingService : IEmbeddingService
{
    string HfApiKey = "";
    string HfEmbeddingModel = "";

    public EmbeddingService(string hfApiKey, string hfEmbeddingModel)
    {
        HfApiKey = hfApiKey;
        HfEmbeddingModel = hfEmbeddingModel;
    }
// =====================================================
//  МЕТОДЫ ДЛЯ API
// =====================================================
    public async Task<float[]> GetHuggingFaceEmbeddingAsync(string text)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {HfApiKey}");
            client.Timeout = TimeSpan.FromSeconds(30);

            var request = new
            {
                model = HfEmbeddingModel,
                inputs = text,
                options = new { wait_for_model = true }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://router.huggingface.co/hf-inference/models/" + HfEmbeddingModel,
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"HuggingFace API error {response.StatusCode}: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            // HuggingFace возвращает массив чисел напрямую
            return doc.RootElement
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Ошибка HuggingFace: {ex.Message}");
            throw;
        }
    }

    public float CosineSimilarity(float[] v1, float[] v2)
    {
        float dot = 0, mag1 = 0, mag2 = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        mag1 = (float)Math.Sqrt(mag1);
        mag2 = (float)Math.Sqrt(mag2);
        return mag1 == 0 || mag2 == 0 ? 0 : dot / (mag1 * mag2);
    }
}