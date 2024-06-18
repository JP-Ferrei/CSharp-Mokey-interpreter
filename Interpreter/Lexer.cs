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

    public IEnumerable<Token> Iterate()
    {
        while (CanMoveFoward())
        {
            var token = NextToken();
            yield return token;
        }
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
        SkipDeadChars();

        return Data switch
        {
            var d when IsLetter(d) => ReadIdentifier(),
            var d when char.IsDigit(d) => ReadNumber(),
            _ => ReadOperators(),
        };
    }

    private void SkipDeadChars()
    {
        while (Data is ' ' or '\t' or '\n' or '\r')
        {
            MoveFoward();
        }
    }

    private static bool IsLetter(char data) => char.IsLetter(data) || data == '_';

    private Token ReadNumber()
    {
        var startPosition = Position;
        while (char.IsDigit(Data))
        {
            MoveFoward();
        }
        var number = Input[startPosition..Position];
        return new(TokenType.INT, number);
    }

    private Token ReadIdentifier()
    {
        var position = Position;
        while (IsLetter(Data))
        {
            MoveFoward();
        }
        var word = Input[position..Position];
        return new Token(Token.LookupIdent(word), word);
    }

    private Token ReadOperators()
    {
        var token = Data switch
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
            '=' => PeekChar() switch
            {
                '=' => CreateToken(TokenType.EQ, Data),
                _ => new Token(TokenType.ASSIGN, Data),
            },
            '!' => PeekChar() switch
            {
                '=' => CreateToken(TokenType.NOT_EQ, Data),
                _ => new Token(TokenType.BANG, Data),
            },
            '>' => PeekChar() switch
            {
                '=' => CreateToken(TokenType.GT_EQ, Data),
                _ => new Token(TokenType.GT, Data),
            },
            '<' => PeekChar() switch
            {
                '=' => CreateToken(TokenType.LT_EQ, Data),
                _ => new Token(TokenType.LT, Data),
            },
            _ => new Token(TokenType.EOF, ""),
        };
        MoveFoward();
        return token;
    }

    private Token CreateToken(TokenType tokenType, char data)
    {
        var character = data;
        MoveFoward();
        return new Token(tokenType, string.Concat(character, Data));
    }
}
