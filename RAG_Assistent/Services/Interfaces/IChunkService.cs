using Test_RAG_System.Models.Entities;

namespace Test_RAG_System.Services.Interfaces;

public interface IChunkService
{
    List<string> ChunkDocument(string document, int maxChunkSize = 500, int overlap = 100);
    List<string> SplitChunkWithOverlap(string text, int chunkSize, int overlap);
    Task<List<ScoredChunk>> FindRelevantChunksAsync(string question, List<ChunkEmbedding> chunks, int topK = 5, float threshold = 0.2f);
}