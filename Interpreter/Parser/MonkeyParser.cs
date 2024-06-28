using Interpreter.Lexer;
using Interpreter.Parser.Parser;

namespace Interpreter.Parser;

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
            { TokenType.ASTERISK, Precedence.PRODUCT },
            { TokenType.LPAREN, Precedence.CALL }
        };

    private List<string> _errors;
    public IEnumerable<string> Errors
    {
        get => _errors;
    }
    private readonly MonkeyLexer _lexer;
    private Token _currentToken;
    private Token _peekToken;

    public Dictionary<TokenType, Func<IExpression?>> PrefixParseFuncs { get; init; } = [];
    public Dictionary<TokenType, Func<IExpression, IExpression>> InfixParseFuncs { get; init; } =
        [];

    public MonkeyParser(MonkeyLexer lexer)
    {
        _lexer = lexer;
        _errors = [];

        PrefixParseFuncs = new()
        {
            { TokenType.IDENT, ParseIdentifier },
            { TokenType.INT, ParseIntegerLiteral },
            { TokenType.MINUS, ParsePrefixExpression },
            { TokenType.BANG, ParsePrefixExpression },
            { TokenType.TRUE, ParseBoolean },
            { TokenType.FALSE, ParseBoolean },
            { TokenType.LPAREN, ParseGroupedExpression },
            { TokenType.IF, ParseIfExpression },
            { TokenType.FUNCTION, ParseFunctionLiteral },
        };

        InfixParseFuncs = new()
        {
            { TokenType.PLUS, ParserInfixExpression },
            { TokenType.MINUS, ParserInfixExpression },
            { TokenType.SLASH, ParserInfixExpression },
            { TokenType.ASTERISK, ParserInfixExpression },
            { TokenType.EQ, ParserInfixExpression },
            { TokenType.NOT_EQ, ParserInfixExpression },
            { TokenType.LT, ParserInfixExpression },
            { TokenType.GT, ParserInfixExpression },
            { TokenType.LPAREN, ParserCallExpression },
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

    private bool IsNextToken(Token tokenType)
    {
        if (_peekToken.TokenType == tokenType.TokenType)
        {
            NextToken();
            return true;
        }

        PeekError(tokenType);
        return false;
    }

    private void PeekError(Token tokenType) =>
        _errors.Add(
            $"expected next token to be {tokenType.Literal}, got {_peekToken.TokenType} instead"
        );

    private LetStatement? ParseLetStatement()
    {
        if (IsNextToken(Token.IDENT) is false)
            return null;

        var identifier = new Identifier() { Token = _currentToken, Value = _currentToken.Literal };

        if (IsNextToken(Token.ASSIGN) is false)
            return null;

        NextToken();

        var expression = ParseExpression(Precedence.LOWEST);

        if (_peekToken == TokenType.SEMICOLON)
            NextToken();

        return new LetStatement() { Name = identifier, Value = expression };
    }

    private ReturnStatement? ParseReturnStatement()
    {
        NextToken();

        var expression = ParseExpression(Precedence.LOWEST);

        if (_peekToken == TokenType.SEMICOLON)
            NextToken();

        return new ReturnStatement() { ReturnExpression = expression };
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
        var prefixFunc = PrefixParseFuncs.GetValueOrDefault(_currentToken.TokenType, null);

        if (prefixFunc is null)
        {
            _errors.Add($"no prefix parser function from {_currentToken.TokenType} found");
            return null;
        }

        var leftExpresion = prefixFunc();

        while (_peekToken != TokenType.SEMICOLON && precedence < PeekPrecedence())
        {
            var infixFunc = InfixParseFuncs.GetValueOrDefault(_peekToken.TokenType, null);

            if (infixFunc is null)
            {
                return leftExpresion;
            }

            NextToken();

            leftExpresion = infixFunc(leftExpresion);
        }

        return leftExpresion;
    }

    private IExpression ParseIdentifier() =>
        new Identifier() { Token = _currentToken, Value = _currentToken.Literal };

    private IExpression ParseBoolean() =>
        new BooleanLiteral() { Token = _currentToken, Value = _currentToken == TokenType.TRUE };

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
        var expression = new PrefixExpression()
        {
            Operator = _currentToken.Literal,
            Token = _currentToken,
        };

        NextToken();

        expression.Right = ParseExpression(Precedence.PREFIX);
        return expression;
    }

    private IExpression ParserInfixExpression(IExpression leftExpression)
    {
        var expression = new InfixExpression()
        {
            Token = _currentToken,
            Operator = _currentToken.Literal,
            Left = leftExpression
        };

        var precedence = CurrentPrecedence();

        NextToken();
        expression.Right = ParseExpression(precedence);
        return expression;
    }

    private IExpression? ParseGroupedExpression()
    {
        NextToken();

        var expression = ParseExpression(Precedence.LOWEST);

        if (IsNextToken(Token.RPAREN) is false)
            return null;

        return expression;
    }

    private IExpression? ParseIfExpression()
    {
        if (IsNextToken(Token.LPAREN) is false)
            return null;

        NextToken();
        var condition = ParseExpression(Precedence.LOWEST);

        if (IsNextToken(Token.RPAREN) is false)
            return null;

        if (IsNextToken(Token.LBRACE) is false)
            return null;

        var expression = new IfExpression()
        {
            Condition = condition,
            Consequence = ParseBlockStatement()
        };

        if (_peekToken == TokenType.ELSE)
        {
            NextToken();

            if (IsNextToken(Token.LBRACE) is false)
            {
                return null;
            }

            expression.Alternative = ParseBlockStatement();
        }

        return expression;
    }

    private BlockStatement ParseBlockStatement() //Starts on { character
    {
        var block = new BlockStatement();
        NextToken();

        while (_currentToken != TokenType.RBRACE && _currentToken != TokenType.EOF)
        {
            var statement = ParseStatement();
            if (statement is not null)
            {
                block.Statements.Add(statement);
            }

            NextToken();
        }

        return block;
    }

    private IExpression? ParseFunctionLiteral() // starts on fn token
    {
        if (!IsNextToken(Token.LPAREN))
            return null;

        var parameters = ParseFunctionParameters();

        NextToken();

        return new FunctionLiteral()
        {
            Parameters = parameters ?? [],
            Body = ParseBlockStatement()
        };
    }

    private List<Identifier>? ParseFunctionParameters() // starts on { character
    {
        if (_peekToken == TokenType.RPAREN)
        {
            NextToken();
            return [];
        }

        NextToken();

        var parameters = new List<Identifier> { new Identifier(_currentToken) };

        while (_peekToken == TokenType.COMMA)
        {
            NextToken();
            NextToken();
            parameters.Add(new Identifier(_currentToken));
        }

        if (IsNextToken(Token.RPAREN) is false)
            return null;

        return parameters;
    }

    private IExpression ParserCallExpression(IExpression function)
    {
        return new CallExpression()
        {
            Token = _currentToken,
            Function = function,
            Arguments = ParserCallArguments() ?? []
        };
    }

    private List<IExpression>? ParserCallArguments()
    {
        if (_peekToken == TokenType.RPAREN)
        {
            NextToken();
            return [];
        }

        NextToken();
        var args = new List<IExpression>() { ParseExpression(Precedence.LOWEST) };

        while (_peekToken == TokenType.COMMA)
        {
            NextToken();
            NextToken();
            args.Add(ParseExpression(Precedence.LOWEST));
        }

        if (IsNextToken(Token.RPAREN) is false)
            return null;

        return args;
    }

    private Precedence PeekPrecedence() =>
        Precedences.GetValueOrDefault(_peekToken.TokenType, Precedence.LOWEST);

    private Precedence CurrentPrecedence() =>
        Precedences.GetValueOrDefault(_currentToken.TokenType, Precedence.LOWEST);
}
