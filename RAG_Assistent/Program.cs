using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Test_RAG_System.Models.Entities;
using Test_RAG_System.Services;

//  Конфигурация
string groqApiKey = "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";
string hfApiKey = "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";
string hfEmbeddingModel = "BAAI/bge-base-en-v1.5"; // 768 dimensions

//  ИНИЦИАЛИЗАЦИЯ СЕРВИСОВ
EmbeddingService embeddingService = new EmbeddingService(hfApiKey, hfEmbeddingModel);
GroqAPIService groqApiService = new GroqAPIService(groqApiKey);
ChunkService chunkService = new ChunkService(embeddingService);
PromtAndDocumentService prmService = new PromtAndDocumentService();
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

Console.WriteLine("=== RAG С HUGGING FACE API (768 dimensions) ===\n");

// =====================================================
//  ЗАГРУЗКА И ПРЕПРОЦЕССИНГ ДОКУМЕНТА
// =====================================================
var messyDoc = prmService.GetMessyDocument();
Console.WriteLine($"Исходный документ: {messyDoc.Length} символов");

var cleanPrompt = prmService.GetCleaningPrompt();
var cleanedDoc = await groqApiService.CallGroqAsync(cleanPrompt, messyDoc);

Console.WriteLine("\n ОЧИЩЕННЫЙ ДОКУМЕНТ:");
Console.WriteLine(cleanedDoc);

// =====================================================
// ЧАНКИНГ И ЭМБЕДДИНГИ ЧЕРЕЗ HUGGING FACE
// =====================================================
var chunks = chunkService.ChunkDocument(cleanedDoc);
Console.WriteLine($"\n📦 Создано {chunks.Count} чанков");

var chunkEmbeddings = new List<ChunkEmbedding>();
foreach (var chunk in chunks)
{
    var embedding = await embeddingService.GetHuggingFaceEmbeddingAsync(chunk);
    chunkEmbeddings.Add(new ChunkEmbedding { Text = chunk, Vector = embedding });
    Console.WriteLine($"  ✓ Чанк {chunkEmbeddings.Count}: {chunk.Length} символов");
}

// =====================================================
//  ОСНОВНОЙ ЦИКЛ ВОПРОС-ОТВЕТ
// =====================================================
while (true)
{
    Console.WriteLine("\n Введите вопрос (или 'exit'):");
    var question =  Console.ReadLine();
    if ( question == "exit") break;
    if (question == "")
    {
        Console.WriteLine("Поля вопрос не должно быть null, Введите повторно");
        Console.WriteLine();
    }
    else
    {
        // Получаем релевантные чанки
        var relevantChunks = await chunkService.FindRelevantChunksAsync(question, chunkEmbeddings);

        if (!relevantChunks.Any())
        {
            Console.WriteLine(" В документации нет информации по этому вопросу");
            continue;
        }

        var context = string.Join("\n\n", relevantChunks.Select(c => c.Text));
        var answerPrompt = prmService.GetAnswerPrompt();
        var answer = await groqApiService.CallGroqAsync(answerPrompt, $"КОНТЕКСТ:\n{context}\n\nВОПРОС: {question}");

        Console.WriteLine("\n ОТВЕТ:");
        Console.WriteLine("=========");
        Console.WriteLine(answer);
        Console.WriteLine("\n ИСПОЛЬЗОВАННЫЕ ЧАНКИ:");
        foreach (var chunk in relevantChunks)
        {
            Console.WriteLine($"Score: {chunk.Score:F3} - {chunk.Text[..50]}...");
        }
    }
    
}

