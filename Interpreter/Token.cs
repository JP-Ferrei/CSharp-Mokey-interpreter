namespace Interpreter
{
    public class Token
    {
        private static readonly Dictionary<string, TokenType> _keyWords = new()
        {
            { "fn", TokenType.FUNCTION },
            { "let", TokenType.LET },
            { "true", TokenType.TRUE },
            { "false", TokenType.FALSE},
            { "if", TokenType.IF},
            { "else", TokenType.ELSE},
            { "return", TokenType.RETURN},

        };

        public TokenType TokenType { get; set; }
        public string Literal { get; set; }

        public Token()
        {
        }

        public Token(TokenType tokenType, string literal)
        {
            TokenType = tokenType;
            Literal = literal;
        }

        public Token(TokenType token, char literal) : this(token, literal.ToString()) { }

        public static TokenType LookupIdent(string ident)
        {
            if (_keyWords.TryGetValue(ident, out TokenType token))
                return token;

            return TokenType.IDENT;
        }

        public override string ToString() => string.Format("[ Type: {0}, Literal: {1} ]", TokenType.ToString(), Literal);
    }

    public enum TokenType
    {
        ILLEGAL,
        EOF,
        //Identifiers
        INT,
        IDENT,
        //Delimiters
        COMMA,
        SEMICOLON,
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        //Operators
        ASSIGN,
        PLUS,
        MINUS,
        BANG,
        ASTERISK,
        SLASH,
        BACKSLASH,
        EQ,
        NOT_EQ,
        LT,
        GT,
        LT_EQ,
        GT_EQ,
        //keywords
        FUNCTION,
        LET,
        RETURN,
        TRUE,
        FALSE,
        IF,
        ELSE
    }
}