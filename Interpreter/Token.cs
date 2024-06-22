using System.Reflection.Metadata.Ecma335;

namespace Interpreter;

public struct Token
{
    public TokenType TokenType { get; set; }
    public string Literal { get; set; }

    public Token(TokenType tokenType, string literal)
    {
        TokenType = tokenType;
        Literal = literal;
    }

    public Token(TokenType token, char literal) : this(token, literal.ToString()) { }

    public bool IsTypeEqual(TokenType type)
    {
        return TokenType == type;
    }

    public override string ToString() => string.Format("[ Type: {0}, Literal: {1} ]", TokenType.ToString(), Literal);


    public static bool operator ==(Token left, TokenType right) => left.TokenType == right;
    public static bool operator !=(Token left, TokenType right) => left.TokenType != right;
    public static Token ILLEGAL => new(TokenType.ILLEGAL, "ILLEGAL");
    public static Token EOF => new(TokenType.EOF, "EOF");
    public static Token IDENT => new(TokenType.IDENT, "");

    public static Token COMMA => new(TokenType.COMMA, ",");
    public static Token SEMICOLON => new(TokenType.SEMICOLON, ";");
    public static Token LPAREN => new(TokenType.LPAREN, "(");
    public static Token RPAREN => new(TokenType.RPAREN, ")");
    public static Token LBRACE => new(TokenType.LBRACE, "{");
    public static Token RBRACE => new(TokenType.RBRACE, "}");

    public static Token ASSIGN => new(TokenType.ASSIGN, "=");
    public static Token PLUS => new(TokenType.PLUS, "+");
    public static Token MINUS => new(TokenType.MINUS, "-");
    public static Token BANG => new(TokenType.BANG, "!");
    public static Token ASTERISK => new(TokenType.ASTERISK, "*");
    public static Token SLASH => new(TokenType.SLASH, "/");
    public static Token BACKSLASH => new(TokenType.BACKSLASH, "\\");
    public static Token EQ => new(TokenType.EQ, "==");
    public static Token NOT_EQ => new(TokenType.NOT_EQ, "!=");
    public static Token LT => new(TokenType.LT, "<");
    public static Token GT => new(TokenType.GT, ">");
    public static Token LT_EQ => new(TokenType.LT_EQ, "<=");
    public static Token GT_EQ => new(TokenType.GT_EQ, ">=");

    public static Token FUNCTION => new(TokenType.FUNCTION, "fn");
    public static Token LET => new(TokenType.LET, "let");
    public static Token RETURN => new(TokenType.RETURN, "return");
    public static Token TRUE => new(TokenType.TRUE, "true");
    public static Token FALSE => new(TokenType.FALSE, "false");
    public static Token IF => new(TokenType.IF, "if");
    public static Token ELSE => new(TokenType.ELSE, "else");
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