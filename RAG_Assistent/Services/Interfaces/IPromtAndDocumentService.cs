namespace Test_RAG_System.Services.Interfaces;

public interface IPromtAndDocumentService
{
    string GetMessyDocument();
    string GetCleaningPrompt();
    string GetAnswerPrompt();
}