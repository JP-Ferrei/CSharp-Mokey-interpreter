using Interpreter.Parser.Parser;

namespace Interpreter.Parser;

using InfixParseFunc = Func<IExpression, IExpression>;
using PrefixParseFunc = Func<IExpression>;

public enum Precedence
{
    LOWEST = 1,
    EQUALS, // ==
    LESSGREATER, // > or <
    SUM, // +
    PRODUCT, // *
    PREFIX, // -X or !X
    CALL,
}

public class MonkeyParser
{
    private List<string> _errors;
    public IEnumerable<string> Errors
    {
        get => _errors;
    }
    private readonly Lexer _lexer;
    private Token _currentToken;
    private Token _peekToken;

    public Dictionary<TokenType, PrefixParseFunc> PrefixParseFuncs { get; init; } = [];
    public Dictionary<TokenType, InfixParseFunc> InfixParseFuncs { get; init; } = [];

    public MonkeyParser(Lexer lexer)
    {
        _lexer = lexer;
        _errors = [];

        PrefixParseFuncs = new()
        {
            { TokenType.IDENT, ParseIdentifier }
        };

        NextToken();
        NextToken();
    }

    public void NextToken()
    {
        _currentToken = _peekToken;
        _peekToken = _lexer.NextToken();
    }

    public Program ParserProgram()
    {
        var program = new Program();

        foreach (var statement in Iterate())
        {
            if (statement is not null)
                program.Statements.Add(statement);
        }
        return program;
    }

    private IEnumerable<IStatement?> Iterate()
    {
        while (_currentToken != TokenType.EOF)
        {
            yield return ParseStatement();
            NextToken();
        }
    }

    private IStatement? ParseStatement()
    {
        return _currentToken.TokenType switch
        {
            TokenType.LET => ParseLetStatement(),
            TokenType.RETURN => ParseReturnStatement(),
            _ => ParseExpressionStatement()
        };
    }

    private bool IsNextToken(TokenType tokenType)
    {
        if (_peekToken.TokenType == tokenType)
        {
            NextToken();
            return true;
        }

        PeekError(tokenType);
        return false;
    }

    private void PeekError(TokenType tokenType) =>
        _errors.Add($"expected next token to be {tokenType}, got {_peekToken.TokenType} instead");

    private LetStatement? ParseLetStatement()
    {
        if (IsNextToken(TokenType.IDENT) is false)
            return null;

        var identifier = new Identifier() { Token = _currentToken, Value = _currentToken.Literal };

        if (IsNextToken(TokenType.ASSIGN) is false)
            return null;

        // TODO: We're skipping the expressions until we encounter a semicolon
        while (_currentToken != TokenType.SEMICOLON)
        {
            NextToken();
        }

        return new LetStatement() { Name = identifier };
    }

    private ReturnStatement? ParseReturnStatement()
    {
        NextToken();
        // TODO: We're skipping the expressions until we encounter a semicolon
        while (_currentToken != TokenType.SEMICOLON)
        {
            NextToken();
        }

        return new ReturnStatement() { ReturnExpression = null };
    }

    private ExpressionStatement? ParseExpressionStatement()
    {
        var statement = new ExpressionStatement()
        {
            Expression = ParseExpression(Precedence.LOWEST)
        };

        if (_peekToken == TokenType.SEMICOLON)
            NextToken();

        return statement;
    }

    private IExpression? ParseExpression(Precedence precedence)
    {
        if (PrefixParseFuncs.TryGetValue(_currentToken.TokenType, out var prefix))
        {
            return prefix();
        }

        return null;
    }

    private IExpression ParseIdentifier() =>
        new Identifier() { Token = _currentToken, Value = _currentToken.Literal };
}
