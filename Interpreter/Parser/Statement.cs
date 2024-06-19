using System.Text;
using Interpreter.Parser.Parser;

namespace Interpreter.Parser;

public class LetStatement : IStatement
{
    public Token Token { get; } = Token.LET;
    public required Identifier Name { get; init; }
    public IExpression? Value { get; init; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() =>
        $"{Token.Literal} {Name} = {Value?.ToString() ?? string.Empty};";
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

    public override string ToString() => Value;
}

public class ReturnStatement : IStatement
{
    public Token Token { get; } = Token.RETURN;
    public IExpression? ReturnExpression { get; init; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() =>
        $"{TokenLiteral} {ReturnExpression?.ToString() ?? string.Empty};";
}

public class ExpressionStatement : IStatement, IExpression
{
    public Token Token { get; set; }
    public IExpression? Expression { get; init; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() => Expression?.ToString() ?? string.Empty;
}

public class IntegerLiteral : IExpression
{
    public Token Token { get; set; }
    public int Value { get; init; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() => Value.ToString();
}

public class PrefixExpression : IExpression
{
    public Token Token { get; set; }
    public required string Operator { get; init; }
    public IExpression? Right { get; set; }

    public string TokenLiteral => Token.Literal;
    public override string ToString() => $"({Operator}{Right})";
}

public class InfixExpression : IExpression
{
    public Token Token { get; set; }
    public required string Operator { get; init; }
    public IExpression? Left { get; set; }
    public IExpression? Right { get; set; }

    public string TokenLiteral => Token.Literal;
    public override string ToString() => $"({Left} {Operator} {Right})";
}