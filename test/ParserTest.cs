using Interpreter;
using Interpreter.Parser;

namespace test;

[TestFixture]
public class ParserTest
{
    [Test]
    public void TestLetStatements()
    {
        var input = """
                let x = 5;
                let y = 10;
                let foobar = 838383;
            """;

        var lexer = new Lexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();

        var expectedIdentifiers = new List<string>() { "x", "y", "foobar" };
        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(3));

            for (int i = 0; i < program.Statements.Count; i++)
            {
                var expecetd = expectedIdentifiers[i];
                var item = program.Statements[i];

                Assert.That(item.TokenLiteral, Is.EqualTo("let"));
                Assert.That(item, Is.TypeOf(typeof(LetStatement)));
                if (item is LetStatement statement)
                {
                    Assert.That(statement.Name.Value, Is.EqualTo(expecetd));
                    Assert.That(statement.Name.TokenLiteral, Is.EqualTo(expecetd));
                }
            }

            CollectionAssert.IsEmpty(parser.Errors);
        });
    }

    [Test]
    public void TestReturnStatements()
    {
        var input = """
                return 5;
                return 10;
                return 993322;
            """;

        var lexer = new Lexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();

        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(3));
            CollectionAssert.IsEmpty(parser.Errors);

            for (int i = 0; i < program.Statements.Count; i++)
            {
                var item = program.Statements[i];

                Assert.That(item.TokenLiteral, Is.EqualTo("return"));
                Assert.That(item, Is.TypeOf(typeof(ReturnStatement)));
                if (item is ReturnStatement statement) { }
            }
        });
    }

    [Test]
    public void TestString()
    {
        var program = new Program()
        {
            Statements =
            [
                new LetStatement()
                {
                    Name = new Identifier()
                    {
                        Token = new(TokenType.IDENT, "myVar"),
                        Value = "myVar"
                    },
                    Value = new Identifier()
                    {
                        Token = new(TokenType.IDENT, "anotherVar"),
                        Value = "anotherVar"
                    }
                }
            ]
        };

        Assert.That(program.ToString(), Is.EqualTo("let myVar = anotherVar;"));
    }

    [Test]
    public void TestIdentifierExpression()
    {

        var input = "foobar;";

        var lexer = new Lexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.Multiple(() =>
            {
                Assert.That(program, Is.Not.Null);
                Assert.That(program.Statements, Has.Count.EqualTo(1));
                CollectionAssert.IsEmpty(parser.Errors);

                for (int i = 0; i < program.Statements.Count; i++)
                {
                    var item = program.Statements[i];

                    Assert.That(item, Is.TypeOf(typeof(ExpressionStatement)));
                    if (item is ExpressionStatement statement)
                    {
                        if (statement.Expression is Identifier ident)
                        {
                            Assert.That(ident.Value, Is.EqualTo("foobar"));
                            Assert.That(ident.TokenLiteral, Is.EqualTo("foobar"));
                        }

                    }
                }
            });
    }
}
