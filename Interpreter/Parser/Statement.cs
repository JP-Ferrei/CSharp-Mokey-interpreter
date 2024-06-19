using System.Text;
using Interpreter.Parser.Parser;

namespace Interpreter.Parser;

public class LetStatement : IStatement
{
    public Token Token { get; } = Token.LET;
    public required Identifier Name { get; init; }
    public IExpression? Value { get; init; }

    public string TokenLiteral => Token.Literal;

    public void StatementNode()
    {
    }

    public override string ToString()
    {
        return $"{Token.Literal} {Name} = {Value?.ToString() ?? string.Empty};";
    }
}
public class Identifier : IExpression
{
    public Identifier() { }

    public Identifier(Token token, string value)
    {
        Token = token;
        Value = value;
    }

    public required Token Token { get; init; }
    public required string Value { get; init; }

    public string TokenLiteral => Token.Literal;

    public void ExpressionNode() { }

    public override string ToString()
    {
        return Value;
    }
}

public class ReturnStatement : IStatement
{
    public Token Token { get; } = Token.RETURN;
    public IExpression? ReturnExpression { get; init; }

    public string TokenLiteral => Token.Literal;

    public void StatementNode() { }

    public override string ToString()
    {
        return $"{TokenLiteral} {ReturnExpression?.ToString() ?? string.Empty};";
    }
}

public class ExpressionStatement : IStatement, IExpression
{
    public Token Token { get; set; }
    public IExpression? Expression { get; init; }

    public string TokenLiteral => Token.Literal;

    public void ExpressionNode() { }

    public void StatementNode() { }

    public override string ToString()
    {
        return Expression?.ToString() ?? string.Empty;

    }
}
