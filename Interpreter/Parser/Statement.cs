using System.Diagnostics.CodeAnalysis;
using System.Text;
using Interpreter.Lexer;
using Interpreter.Parser.Parser;

namespace Interpreter.Parser;

public class LetStatement : IStatement
{
    public Token Token { get; } = Token.LET;
    public required Identifier Name { get; init; }
    public required IExpression Value { get; init; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() =>
        $"{Token.Literal} {Name} = {Value?.ToString() ?? string.Empty};";
}

public class Identifier : IExpression
{
    public Identifier() { }

    [SetsRequiredMembers]
    public Identifier(Token token)
    {
        Token = token;
        Value = token.Literal;
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

public class BooleanLiteral : IExpression
{
    public Token Token { get; init; }
    public bool Value { get; init; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() => Value.ToString().ToLower();
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

public class IfExpression : IExpression
{
    public Token Token = Token.IF;
    public required IExpression Condition { get; init; }
    public required BlockStatement Consequence { get; set; }
    public BlockStatement? Alternative { get; set; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() =>
        $"(if {Condition} {Consequence} {Alternative?.ToString() ?? string.Empty})";
}

public class BlockStatement : IStatement
{
    public Token Token { get; set; }
    public List<IStatement> Statements { get; set; } = new List<IStatement>();

    public string TokenLiteral => Token.Literal;

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var statement in Statements)
        {
            sb.Append(statement.ToString());
        }
        return sb.ToString();
    }
}

public class FunctionLiteral : IExpression
{
    public Token Token = Token.FUNCTION;
    public required List<Identifier> Parameters { get; set; } = [];
    public required BlockStatement Body { get; set; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() =>
        $"{TokenLiteral}({string.Join(", ", Parameters.Select(it => it.ToString()))}) {Body}";
}

public class CallExpression : IExpression
{
    public Token Token { get; init; }
    public required List<IExpression> Arguments { get; set; } = [];
    public required IExpression Function { get; set; }

    public string TokenLiteral => Token.Literal;

    public override string ToString() =>
        $"{Function}({string.Join(", ", Arguments.Select(it => it.ToString()))})";
}

