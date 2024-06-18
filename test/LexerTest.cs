using Interpreter;

namespace test
{
    [TestFixture]
    public class LexerTest
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ShouldReadAllNonLettersCharactersCorrectly()
        {
            var input = "=+(){},;";
            var expectedTokens = new List<Token>
            {
                new(TokenType.ASSIGN, "="),
                new(TokenType.PLUS, "+"),
                new(TokenType.LPAREN, "("),
                new(TokenType.RPAREN, ")"),
                new(TokenType.LBRACE, "{"),
                new(TokenType.RBRACE, "}"),
                new(TokenType.COMMA, ","),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.EOF, ""),
            };
            AssertTokens(input, expectedTokens);
        }

        [Test]
        public void ShouldReadAllCharactersCorrectly()
        {
            var input = """
                let five = 5;
                let ten = 10;
                let add = fn(x, y) {
                    x + y;
                };
                let result = add(five, ten);
            """;
            var expectedTokens = new List<Token>
            {
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "five"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.INT, "5"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "ten"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.INT, "10"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "add"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.FUNCTION, "fn"),
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "x"),
                new(TokenType.COMMA, ","),
                new(TokenType.IDENT, "y"),
                new(TokenType.RPAREN, ")"),
                new(TokenType.LBRACE, "{"),
                new(TokenType.IDENT, "x"),
                new(TokenType.PLUS, "+"),
                new(TokenType.IDENT, "y"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.RBRACE, "}"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "result"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.IDENT, "add"),
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "five"),
                new(TokenType.COMMA, ","),
                new(TokenType.IDENT, "ten"),
                new(TokenType.RPAREN, ")"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.EOF, ""),
            };

            AssertTokens(input, expectedTokens);
        }

        [Test]
        public void test3()
        {
            var input = """
                    let five = 5;
                    let ten = 10;
                    let add = fn(x, y) {
                        x + y;
                    };
                    let result = add(five, ten);
                    !-/*5;
                    5 < 10 > 5;

                    if (5 < 10) {
                    return true;
                    } else {
                    return false;
                    }
                    10 == 10;
                    10 != 9;
                    """;
            var expectedTokens = new List<Token>
            {
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "five"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.INT, "5"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "ten"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.INT, "10"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "add"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.FUNCTION, "fn"),
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "x"),
                new(TokenType.COMMA, ","),
                new(TokenType.IDENT, "y"),
                new(TokenType.RPAREN, ")"),
                new(TokenType.LBRACE, "{"),
                new(TokenType.IDENT, "x"),
                new(TokenType.PLUS, "+"),
                new(TokenType.IDENT, "y"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.RBRACE, "}"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.LET, "let"),
                new(TokenType.IDENT, "result"),
                new(TokenType.ASSIGN, "="),
                new(TokenType.IDENT, "add"),
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "five"),
                new(TokenType.COMMA, ","),
                new(TokenType.IDENT, "ten"),
                new(TokenType.RPAREN, ")"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.BANG, "!"),
                new(TokenType.MINUS, "-"),
                new(TokenType.SLASH, "/"),
                new(TokenType.ASTERISK, "*"),
                new(TokenType.INT, "5"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.INT, "5"),
                new(TokenType.LT, "<"),
                new(TokenType.INT, "10"),
                new(TokenType.GT, ">"),
                new(TokenType.INT, "5"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.IF, "if"),
                new(TokenType.LPAREN, "("),
                new(TokenType.INT, "5"),
                new(TokenType.LT, "<"),
                new(TokenType.INT, "10"),
                new(TokenType.RPAREN, ")"),
                new(TokenType.LBRACE, "{"),
                new(TokenType.RETURN, "return"),
                new(TokenType.TRUE, "true"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.RBRACE, "}"),
                new(TokenType.ELSE, "else"),
                new(TokenType.LBRACE, "{"),
                new(TokenType.RETURN, "return"),
                new(TokenType.FALSE, "false"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.RBRACE, "}"),
                new(TokenType.INT, "10"),
                new(TokenType.EQ, "=="),
                new(TokenType.INT, "10"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.INT, "10"),
                new(TokenType.NOT_EQ, "!="),
                new(TokenType.INT, "9"),
                new(TokenType.SEMICOLON, ";"),
                new(TokenType.EOF, ""),
            };

            AssertTokens(input, expectedTokens);
        }

        private void AssertTokens(string input, IEnumerable<Token> expectedTokens)
        {
            var lexer = new Lexer(input);
            foreach (var expectedToken in expectedTokens)
            {
                Token token = lexer.NextToken();
                Assert.Multiple(() =>
                {
                    Assert.That(token.TokenType, Is.EqualTo(expectedToken.TokenType));
                    Assert.That(token.Literal, Is.EqualTo(expectedToken.Literal));
                });
            }

        }
    }
}