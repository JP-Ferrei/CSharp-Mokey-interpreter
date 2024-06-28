namespace Interpreter.Evaluation;

public class Environment
{
    public Dictionary<string, IObject> Store { get; init; } = new();
}
