using Interpreter.Evaluation;

namespace Interpreter.Exceptions;

[Serializable]
public class TypeMissMatchException : Exception
{
    public TypeMissMatchException(string @operator, IObject leftValue, IObject rightValue)
        : base($"type mismatch: {leftValue}{@operator}{rightValue}") { }

    public TypeMissMatchException(string message)
        : base(message) { }

    public TypeMissMatchException(string message, Exception inner)
        : base(message, inner) { }
}
