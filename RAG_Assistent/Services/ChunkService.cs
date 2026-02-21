using System.Text;
using Test_RAG_System.Models.Entities;
using Test_RAG_System.Services.Interfaces;

namespace Test_RAG_System.Services;

public class ChunkService : IChunkService
{
    private readonly EmbeddingService _embeddingService;

    public ChunkService(EmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    // =====================================================
    //  ЧАНКИНГ (ручной, без библиотек)
    // =====================================================
    public List<string> ChunkDocument(string document, int maxChunkSize = 500, int overlap = 100)
    {
        var chunks = new List<string>();
        var lines = document.Split('\n');
        var currentBlock = "";

        foreach (var line in lines)
        {
            if (line.StartsWith("###") && currentBlock != "")
            {
                chunks.Add(currentBlock);
                currentBlock = line + "\n";
            }
            else if (line.StartsWith("###"))
            {
                currentBlock = line + "\n";
            }
            else
            {
                currentBlock += line + "\n";
            }
        }
        
        if (!string.IsNullOrWhiteSpace(currentBlock))
            chunks.Add(currentBlock);
        
        var finalChunks = new List<string>();
        foreach (var chunk in chunks)
        {
            if (chunk.Length <= maxChunkSize)
            {
                finalChunks.Add(chunk);
            }
            else
            {
                finalChunks.AddRange(SplitChunkWithOverlap(chunk, maxChunkSize, overlap));
            }
        }

        return finalChunks;
    }

    public List<string> SplitChunkWithOverlap(string text, int chunkSize, int overlap)
    {
        var chunks = new List<string>();
        int step = chunkSize - overlap;
        
        for (int i = 0; i < text.Length; i += step)
        {
            int end = Math.Min(i + chunkSize, text.Length);
            chunks.Add(text.Substring(i, end - i));
        }
        
        return chunks;
    }
    
    // =====================================================
    //  ПОИСК И СРАВНЕНИЕ
    // =====================================================
    public async Task<List<ScoredChunk>> FindRelevantChunksAsync(
        string question, 
        List<ChunkEmbedding> chunks, 
        int topK = 5,
        float threshold = 0.5f)
    {
        var questionEmbedding = await _embeddingService.GetHuggingFaceEmbeddingAsync(question);

        var similarities = chunks
            .Select(c => new ScoredChunk
            {
                Text = c.Text,
                Score = _embeddingService.CosineSimilarity(questionEmbedding, c.Vector)
            })
            .Where(x => x.Score > threshold)
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();

        return similarities;
    }
}