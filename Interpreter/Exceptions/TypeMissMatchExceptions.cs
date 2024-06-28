using System.Runtime.Serialization;
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

[Serializable]
public class UnkwownOperatorException : Exception
{
    public UnkwownOperatorException(string @operator, IObject leftValue, IObject rightValue)
        : base($"unkwown operator: {leftValue.Type()}{@operator}{rightValue.Type()}") { }

    public UnkwownOperatorException(string @operator, IObject rightValue)
        : base($"unkwown operator: {@operator}{rightValue.Type()}") { }

    public UnkwownOperatorException(IObject rightValue)
        : base($"unkwown operator: -{rightValue.Type()}") { }

    public UnkwownOperatorException(string message)
        : base(message) { }

    public UnkwownOperatorException(string message, System.Exception inner)
        : base(message, inner) { }
}
