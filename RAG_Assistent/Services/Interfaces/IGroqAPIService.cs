using Test_RAG_System.Models.Entities;

namespace Test_RAG_System.Services.Interfaces;

public interface IGroqAPIService
{
    public Task<string> CallGroqAsync(string systemPrompt, string userMessage);
}