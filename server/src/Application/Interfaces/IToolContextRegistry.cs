using Application.AI.Contracts;

namespace Application.AI;

public interface IToolContextRegistry
{
    AiToolContextType GetContextType(string toolName);
}