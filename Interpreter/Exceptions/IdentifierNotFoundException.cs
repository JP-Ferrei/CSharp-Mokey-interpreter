using Interpreter.Parser;

namespace Interpreter.Exceptions;

[System.Serializable]
public class IdenfierNotFoundException : System.Exception
{
    public IdenfierNotFoundException(Identifier ident)
        : base($"identifier not found {ident.Value}") { }

    public IdenfierNotFoundException(string message)
        : base(message) { }

    public IdenfierNotFoundException(string message, System.Exception inner)
        : base(message, inner) { }
}
