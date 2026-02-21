using Test_RAG_System.Models.Entities;

namespace Test_RAG_System.Services.Interfaces;

public interface IEmbeddingService
{
    public Task<float[]> GetHuggingFaceEmbeddingAsync(string text);
    public float CosineSimilarity(float[] v1, float[] v2);

    
}