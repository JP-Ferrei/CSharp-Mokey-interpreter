namespace Interpreter;

public class Lexer
{
    private const char END_OF_FILE = '\0';
    public string Input { get; private set; }
    public int Position { get; private set; } = 0;
    public int ReadPosition { get; private set; } = 0;
    public char Data { get; private set; }

    public Lexer(string input)
    {
        Input = input;
        MoveFoward();
    }

    private void MoveFoward()
    {
        Data = PeekChar();
        Position = ReadPosition;
        ReadPosition++;
    }

    private bool CanMoveFoward() => ReadPosition < Input.Length;

    private char PeekChar() => CanMoveFoward() ? Input[ReadPosition] : END_OF_FILE;

    public Token NextToken()
    {
        SkipNextToken();

        return Data switch
        {
            var d when IsLetter(d) => ResolveLetter(),
            var d when char.IsDigit(d) => ResolveNumber(),
            _ => ResolveOperators(),
        };
    }

    private void SkipNextToken()
    {
        while (Data is ' ' or '\t' or '\n' or '\r')
        {
            MoveFoward();
        }
    }

    private static bool IsLetter(char data) => char.IsLetter(data) || data == '_';

    private Token ResolveNumber() => new(TokenType.INT, ReadNumber());

    private string ReadNumber()
    {
        var startPosition = Position;
        while (char.IsDigit(Data))
        {
            MoveFoward();
        }
        return Input[startPosition..Position];
    }

    private Token ResolveLetter()
    {
        var word = ReadIdentifier();
        return new Token(Token.LookupIdent(word), word);
    }

    private string ReadIdentifier()
    {
        var position = Position;
        while (IsLetter(Data))
        {
            MoveFoward();
        }
        return Input[position..Position];
    }

    private Token ResolveOperators()
    {
        Token token = CanBeDoubleOperator(Data)
            ? ResolveDoubleOperators(Data)
            : ResolveSingleOperators();

        MoveFoward();
        return token;
    }

    private static bool CanBeDoubleOperator(char data) => data is '=' or '!' or '>' or '<';

    private Token ResolveSingleOperators()
    {
        return Data switch
        {
            '*' => new Token(TokenType.ASTERISK, Data),
            '+' => new Token(TokenType.PLUS, Data),
            '-' => new Token(TokenType.MINUS, Data),
            '/' => new Token(TokenType.SLASH, Data),
            '\\' => new Token(TokenType.BACKSLASH, Data),
            ';' => new Token(TokenType.SEMICOLON, Data),
            '(' => new Token(TokenType.LPAREN, Data),
            ')' => new Token(TokenType.RPAREN, Data),
            ',' => new Token(TokenType.COMMA, Data),
            '{' => new Token(TokenType.LBRACE, Data),
            '}' => new Token(TokenType.RBRACE, Data),
            _ => new Token(TokenType.EOF, ""),
        };
    }

    private Token ResolveDoubleOperators(char data)
    {
        return (data, PeekChar()) switch
        {
            ('=', '=') => CreateToken(TokenType.EQ, Data),
            ('=', _) => CreateToken(TokenType.ASSIGN),
            ('!', '=') => CreateToken(TokenType.NOT_EQ, Data),
            ('!', _) => CreateToken(TokenType.BANG),
            ('>', '=') => CreateToken(TokenType.GT_EQ, Data),
            ('>', _) => CreateToken(TokenType.GT),
            ('<', '=') => CreateToken(TokenType.LT_EQ, Data),
            ('<', _) => CreateToken(TokenType.LT),
            _ => CreateToken(TokenType.ILLEGAL)
        };
    }

    private Token CreateToken(TokenType tokenType)
    {
        return new Token(tokenType, Data);
    }

    private Token CreateToken(TokenType tokenType, char data)
    {
        var character = data;
        MoveFoward();
        return new Token(tokenType, string.Concat(character, Data));
    }
}
