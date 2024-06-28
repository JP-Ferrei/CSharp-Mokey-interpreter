namespace Interpreter.Lexer;

public class MonkeyLexer
{
    private const char END_OF_FILE = '\0';
    public string Input { get; private set; }
    public int Position { get; private set; } = 0;
    public int ReadPosition { get; private set; } = 0;
    public char Data { get; private set; }

    public MonkeyLexer(string input)
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

        return word switch
        {
            "fn" => Token.FUNCTION,
            "let" => Token.LET,
            "true" => Token.TRUE,
            "false" => Token.FALSE,
            "if" => Token.IF,
            "else" => Token.ELSE,
            "return" => Token.RETURN,
            _ => new Token(TokenType.IDENT, word)
        };
    }

    private Token ReadOperators()
    {
        var token = Data switch
        {
            '*' => Token.ASTERISK,
            '+' => Token.PLUS,
            '-' => Token.MINUS,
            '/' => Token.SLASH,
            '\\' => Token.BACKSLASH,
            ';' => Token.SEMICOLON,
            '(' => Token.LPAREN,
            ')' => Token.RPAREN,
            ',' => Token.COMMA,
            '{' => Token.LBRACE,
            '}' => Token.RBRACE,
            '='
                => PeekChar() switch
                {
                    '=' => Token.EQ,
                    _ => Token.ASSIGN,
                },
            '!'
                => PeekChar() switch
                {
                    '=' => Token.NOT_EQ,
                    _ => Token.BANG,
                },
            '>'
                => PeekChar() switch
                {
                    '=' => Token.GT_EQ,
                    _ => Token.GT,
                },
            '<'
                => PeekChar() switch
                {
                    '=' => Token.LT_EQ,
                    _ => Token.LT,
                },
            _ => Token.EOF,
        };
        if (
            token.TokenType
            is TokenType.EQ
                or TokenType.NOT_EQ
                or TokenType.GT_EQ
                or TokenType.LT_EQ
        )
            MoveFoward();

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
