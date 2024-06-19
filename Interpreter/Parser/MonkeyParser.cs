using Interpreter.Parser.Parser;

namespace Interpreter.Parser;

using InfixParseFunc = Func<IExpression, IExpression>;
using PrefixParseFunc = Func<IExpression?>;

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
    public Dictionary<TokenType, Precedence> Precedences =
        new()
        {
            { TokenType.EQ, Precedence.EQUALS },
            { TokenType.NOT_EQ, Precedence.EQUALS },
            { TokenType.LT, Precedence.LESSGREATER },
            { TokenType.GT, Precedence.LESSGREATER },
            { TokenType.PLUS, Precedence.SUM },
            { TokenType.MINUS, Precedence.SUM },
            { TokenType.SLASH, Precedence.PRODUCT },
            { TokenType.ASTERISK, Precedence.PRODUCT }
        };

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
            { TokenType.IDENT, ParseIdentifier },
            { TokenType.INT, ParseIntegerLiteral },
            { TokenType.MINUS, ParsePrefixExpression },
            { TokenType.BANG, ParsePrefixExpression },
        };

        InfixParseFuncs = new(){
            { TokenType.PLUS, ParserInfixExpression},
            { TokenType.MINUS, ParserInfixExpression},
            { TokenType.SLASH, ParserInfixExpression},
            { TokenType.ASTERISK, ParserInfixExpression},
            { TokenType.EQ, ParserInfixExpression},
            { TokenType.NOT_EQ, ParserInfixExpression},
            { TokenType.LT, ParserInfixExpression},
            { TokenType.GT, ParserInfixExpression},
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
        var prefix = PrefixParseFuncs[_currentToken.TokenType];

        if (prefix is null)
        {
            _errors.Add($"no prefix parser function from {_currentToken.TokenType} found");
            return null;
        }

        var leftExpresion = prefix();

        while (_peekToken != TokenType.SEMICOLON && precedence < PeekPrecedenc())
        {
            var infix = InfixParseFuncs[_peekToken.TokenType];

            if (infix is null)
            {
                return leftExpresion;
            }

            NextToken();

            return infix(leftExpresion);
        }

        return leftExpresion;
    }

    private IExpression ParseIdentifier() =>
        new Identifier() { Token = _currentToken, Value = _currentToken.Literal };

    private IExpression? ParseIntegerLiteral()
    {
        if (int.TryParse(_currentToken.Literal, out var intValue))
        {
            return new IntegerLiteral() { Token = _currentToken, Value = intValue };
        }

        _errors.Add($"could not parse {_currentToken.Literal}, as integer");
        return null;
    }

    private IExpression ParsePrefixExpression()
    {
        var token = _currentToken;
        var operatorLiteral = _currentToken.Literal;

        NextToken();

        return new PrefixExpression()
        {
            Operator = operatorLiteral,
            Token = token,
            Right = ParseExpression(Precedence.PREFIX)
        };
    }

    private IExpression ParserInfixExpression(IExpression leftExpression)
    {
        var expression = new InfixExpression()
        {
            Token = _currentToken,
            Operator = _currentToken.Literal,
            Left = leftExpression
        };

        var precedence = CurrentPrecedenc();

        NextToken();
        expression.Right = ParseExpression(precedence);
        return expression;
    }

    private Precedence PeekPrecedenc()
    {
        if (Precedences.TryGetValue(_peekToken.TokenType, out var precedence))
        {
            return precedence;
        }

        return Precedence.LOWEST;
    }

    private Precedence CurrentPrecedenc()
    {
        if (Precedences.TryGetValue(_currentToken.TokenType, out var precedence))
        {
            return precedence;
        }

        return Precedence.LOWEST;
    }
}
