using Interpreter.Lexer;
using Interpreter.Parser;
using Interpreter.Parser.Parser;

namespace test;

[TestFixture]
public class ParserTest
{
    [TestCase("let x = 5;", "x", 5)]
    [TestCase("let y = true;", "y", true)]
    [TestCase("let foobar = y;", "foobar", "y")]
    public void TestLetStatements(string input, string expectedIdentifier, object expectedValue)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);
        var program = parser.ParserProgram();

        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(1));
            CollectionAssert.IsEmpty(parser.Errors);

            var item = program.Statements.First();

            TestLetStatement(item, expectedIdentifier);
            AssertExtensions.AssertReturnType(item, out LetStatement letStatement);
            TestLiteralExpression(letStatement.Value, expectedValue);
        });
    }

    [TestCase("return 5;", 5)]
    [TestCase("return 10;", 10)]
    [TestCase("return true;", true)]
    public void TestReturnStatements(string input, object expectedValue)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();

        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(1));
            CollectionAssert.IsEmpty(parser.Errors);

            var item = program.Statements.First();

            Assert.That(item.TokenLiteral, Is.EqualTo("return"));
            AssertExtensions.AssertReturnType(item, out ReturnStatement returnStatement);
            TestLiteralExpression(returnStatement.ReturnExpression, expectedValue);
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

        var lexer = new MonkeyLexer(input);
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

    [TestCase("5;", 5)]
    public void TestIntegerLiteralExpression(string input, int expected)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(1));
            CollectionAssert.IsEmpty(parser.Errors);

            foreach (var item in program.Statements)
            {
                Assert.That(item, Is.TypeOf(typeof(ExpressionStatement)));
                if (item is ExpressionStatement statement)
                {
                    TestLiteralExpression(statement.Expression, expected);
                }
            }
        });
    }

    [TestCase("true;", true)]
    [TestCase("false;", false)]
    // [TestCase("let foobar = true;", true)]
    // [TestCase("let barfoo = false;", false)]
    public void TestBooleanLiteralExpression(string input, bool expected)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(1));
            CollectionAssert.IsEmpty(parser.Errors);

            foreach (var item in program.Statements)
            {
                Assert.That(item, Is.TypeOf(typeof(ExpressionStatement)));
                if (item is ExpressionStatement statement)
                {
                    TestLiteralExpression(statement.Expression, expected);
                }
            }
        });
    }

    [TestCase("!5;", "!", 5)]
    [TestCase("-15", "-", 15)]
    [TestCase("!true;", "!", true)]
    [TestCase("!false;", "!", false)]
    public void TestParsingPrefixExpressions(string input, string operatorValue, object expected)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(1));
            CollectionAssert.IsEmpty(parser.Errors);

            foreach (var item in program.Statements)
            {
                Assert.That(item, Is.TypeOf(typeof(ExpressionStatement)));
                if (item is ExpressionStatement statement)
                {
                    if (statement.Expression is PrefixExpression prefix)
                    {
                        Assert.That(prefix.Operator, Is.EqualTo(operatorValue));
                        TestLiteralExpression(prefix.Right, expected);
                    }
                }
            }
        });
    }

    [TestCase("5 + 5;", 5, "+", 5)]
    [TestCase("5 - 5;", 5, "-", 5)]
    [TestCase("5 * 5;", 5, "*", 5)]
    [TestCase("5 / 5;", 5, "/", 5)]
    [TestCase("5 > 5;", 5, ">", 5)]
    [TestCase("5 < 5;", 5, "<", 5)]
    [TestCase("5 == 5;", 5, "==", 5)]
    [TestCase("5 != 5;", 5, "!=", 5)]
    [TestCase("true == true", true, "==", true)]
    [TestCase("true != false", true, "!=", false)]
    [TestCase("false == false", false, "==", false)]
    public void TestParsingInfixExpressions(
        string input,
        object leftValue,
        string operatorValue,
        object rightValue
    )
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.Statements, Has.Count.EqualTo(1));
            CollectionAssert.IsEmpty(parser.Errors);

            foreach (var item in program.Statements)
            {
                Assert.That(item, Is.TypeOf(typeof(ExpressionStatement)));
                if (item is ExpressionStatement statement)
                {
                    TestInfixExpression(statement.Expression, leftValue, operatorValue, rightValue);
                }
            }
        });
    }

    [TestCase("1 + 2 + 3", "((1 + 2) + 3)")]
    [TestCase("-a * b", "((-a) * b)")]
    [TestCase("!-a", "(!(-a))")]
    [TestCase("a + b + c", "((a + b) + c)")]
    [TestCase("a + b - c", "((a + b) - c)")]
    [TestCase("a * b * c", "((a * b) * c)")]
    [TestCase("a * b / c", "((a * b) / c)")]
    [TestCase("a + b / c", "(a + (b / c))")]
    [TestCase("a + b * c + d / e - f", "(((a + (b * c)) + (d / e)) - f)")]
    [TestCase("3 + 4; -5 * 5", "(3 + 4)((-5) * 5)")]
    [TestCase("5 > 4 == 3 < 4", "((5 > 4) == (3 < 4))")]
    [TestCase("5 < 4 != 3 > 4", "((5 < 4) != (3 > 4))")]
    [TestCase("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))")]
    [TestCase("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))")]
    [TestCase("true", "true")]
    [TestCase("false", "false")]
    [TestCase("3 > 5 == false", "((3 > 5) == false)")]
    [TestCase("3 < 5 == true", "((3 < 5) == true)")]
    [TestCase("1 + (2 + 3) + 4", "((1 + (2 + 3)) + 4)")]
    [TestCase("(5 + 5) * 2", "((5 + 5) * 2)")]
    [TestCase("2 / (5 + 5)", "(2 / (5 + 5))")]
    [TestCase("-(5 + 5)", "(-(5 + 5))")]
    [TestCase("!(true == true)", "(!(true == true))")]
    [TestCase("(5 + (5+2)) * 2", "((5 + (5 + 2)) * 2)")]
    [TestCase("a + add(b * c) + d", "((a + add((b * c))) + d)")]
    [TestCase(
        "add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))",
        "add(a, b, 1, (2 * 3), (4 + 5), add(6, (7 * 8)))"
    )]
    [TestCase("add(a + b + c * d / f + g)", "add((((a + b) + ((c * d) / f)) + g))")]
    public void TestOperatorPrecedenceParsing(string input, string expected)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.Multiple(() =>
        {
            Assert.That(program, Is.Not.Null);
            Assert.That(program.ToString(), Is.EqualTo(expected));
            CollectionAssert.IsEmpty(parser.Errors);
        });
    }

    [TestCase("if (x < y) { x }")]
    [TestCase("if (x < y) { x }")]
    public void TestIfExpression(string input)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.That(program, Is.Not.Null);
        Assert.That(program.Statements, Has.Count.EqualTo(1));
        CollectionAssert.IsEmpty(parser.Errors);

        foreach (var item in program.Statements)
        {
            AssertExtensions.AssertReturnType(item, out ExpressionStatement statement);
            AssertExtensions.AssertReturnType(statement.Expression, out IfExpression ifExpression);
            Assert.That(TestInfixExpression(ifExpression.Condition, "x", "<", "y"), Is.True);
            Assert.That(ifExpression.Consequence.Statements, Has.Count.EqualTo(1));

            AssertExtensions.AssertReturnType(
                ifExpression.Consequence.Statements.First(),
                out ExpressionStatement firtsExpressionStatement
            );
            AssertExtensions.AssertReturnType(
                firtsExpressionStatement.Expression,
                out IExpression firtsExpression
            );
            Assert.That(TestIdentifier(firtsExpression, "x"), Is.True);
            Assert.That(ifExpression.Alternative, Is.Null);
        }
    }

    [TestCase("if (x < y) { x } else { y }")]
    public void TestIfElseExpression(string input)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.That(program, Is.Not.Null);
        Assert.That(program.Statements, Has.Count.EqualTo(1));
        CollectionAssert.IsEmpty(parser.Errors);

        foreach (var item in program.Statements)
        {
            AssertExtensions.AssertReturnType(item, out ExpressionStatement statement);
            AssertExtensions.AssertReturnType(statement.Expression, out IfExpression ifExpression);
            Assert.That(TestInfixExpression(ifExpression.Condition, "x", "<", "y"), Is.True);
            Assert.That(ifExpression.Consequence.Statements, Has.Count.EqualTo(1));

            AssertExtensions.AssertReturnType(
                ifExpression.Consequence.Statements.First(),
                out ExpressionStatement firtsExpressionStatement
            );
            AssertExtensions.AssertReturnType(
                firtsExpressionStatement.Expression,
                out IExpression firtsExpression
            );
            Assert.That(TestIdentifier(firtsExpression, "x"), Is.True);
            Assert.That(ifExpression.Alternative, Is.Not.Null);

            Assert.That(ifExpression.Consequence.Statements, Has.Count.EqualTo(1));

            AssertExtensions.AssertReturnType(
                ifExpression.Alternative.Statements.First(),
                out ExpressionStatement firtsAlternativeExpressionStatement
            );
            AssertExtensions.AssertReturnType(
                firtsAlternativeExpressionStatement.Expression,
                out IExpression firtsAlternativeExpression
            );
            Assert.That(TestIdentifier(firtsAlternativeExpression, "y"), Is.True);
        }
    }

    [TestCase("fn(x, y) { x + y; }", "x", "y")]
    [TestCase("fn() { x + y; }")]
    public void TestFunctionLiteralParsing(string input, params string[] parameters)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.That(program, Is.Not.Null);
        Assert.That(program.Statements, Has.Count.EqualTo(1));
        CollectionAssert.IsEmpty(parser.Errors);

        var first = program.Statements.First();
        AssertExtensions.AssertReturnType(first, out ExpressionStatement statement);
        AssertExtensions.AssertReturnType(
            statement.Expression,
            out FunctionLiteral functionLiteral
        );

        Assert.That(functionLiteral.Parameters, Has.Count.EqualTo(parameters.Length));
        for (int i = 0; i < parameters.Length; i++)
        {
            TestLiteralExpression(functionLiteral.Parameters[i], parameters[i]);
        }

        Assert.That(functionLiteral.Body.Statements, Has.Count.EqualTo(1));
        AssertExtensions.AssertReturnType(
            functionLiteral.Body.Statements[0],
            out ExpressionStatement bodyStatement
        );
        AssertExtensions.AssertReturnType(bodyStatement.Expression, out IExpression bodyExpression);
        TestInfixExpression(bodyExpression, "x", "+", "y");
    }

    [TestCase("fn() {};")]
    [TestCase("fn(x) {};", "x")]
    [TestCase("fn(x, y, z) {};", "x", "y", "z")]
    public void TestFunctionLiteralParsingWithEmptyBody(string input, params string[] parameters)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.That(program, Is.Not.Null);
        Assert.That(program.Statements, Has.Count.EqualTo(1));
        CollectionAssert.IsEmpty(parser.Errors);

        var first = program.Statements.First();
        AssertExtensions.AssertReturnType(first, out ExpressionStatement statement);
        AssertExtensions.AssertReturnType(
            statement.Expression,
            out FunctionLiteral functionLiteral
        );

        Assert.That(functionLiteral.Parameters, Has.Count.EqualTo(parameters.Length));
        for (int i = 0; i < parameters.Length; i++)
        {
            TestLiteralExpression(functionLiteral.Parameters[i], parameters[i]);
        }
    }

    [TestCase("add(1, 2 * 3, 4 + 5)")]
    public void TestCallExpressionParsing(string input)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);

        var program = parser.ParserProgram();
        Assert.That(program, Is.Not.Null);
        Assert.That(program.Statements, Has.Count.EqualTo(1));
        CollectionAssert.IsEmpty(parser.Errors);

        var first = program.Statements.First();
        AssertExtensions.AssertReturnType(first, out ExpressionStatement statement);
        AssertExtensions.AssertReturnType(statement.Expression, out CallExpression callExpression);

        TestIdentifier(callExpression.Function, "add");
        Assert.That(callExpression.Arguments, Has.Count.EqualTo(3));
        TestLiteralExpression(callExpression.Arguments[0], 1);
        TestInfixExpression(callExpression.Arguments[1], 2, "*", 3);
        TestInfixExpression(callExpression.Arguments[2], 4, "+", 5);
    }

    #region Assertions

    public bool TestLetStatement(IStatement statement, string name)
    {
        try
        {
            Assert.That(statement.TokenLiteral, Is.EqualTo("let"));
            Assert.That(statement, Is.TypeOf(typeof(LetStatement)));
            if (statement is LetStatement letStatement)
            {
                Assert.That(letStatement.Name.Value, Is.EqualTo(name));
                Assert.That(letStatement.Name.TokenLiteral, Is.EqualTo(name));
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TestLiteralExpression(IExpression? expression, object expected)
    {
        try
        {
            Assert.That(expression, Is.Not.Null);
            return expected switch
            {
                int d => TestIntegerLiteral(expression, d),
                string d => TestIdentifier(expression, d),
                bool d => TestBooleanLiteral(expression, d),
                _
                    => throw new Exception(
                        $"type of expression not handled. got {expected.GetType()}"
                    )
            };
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TestIntegerLiteral(IExpression? expression, int intValue)
    {
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(expression, Is.Not.Null);
                Assert.That(expression, Is.TypeOf(typeof(IntegerLiteral)));
                if (expression is IntegerLiteral integerLiteral)
                {
                    Assert.That(integerLiteral.Value, Is.EqualTo(intValue));
                    Assert.That(integerLiteral.TokenLiteral, Is.EqualTo(intValue.ToString()));
                }
            });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TestBooleanLiteral(IExpression? expression, bool boolValue)
    {
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(expression, Is.Not.Null);
                Assert.That(expression, Is.TypeOf(typeof(BooleanLiteral)));
                if (expression is BooleanLiteral booleanLiteral)
                {
                    Assert.That(booleanLiteral.Value, Is.EqualTo(boolValue));
                    Assert.That(
                        booleanLiteral.TokenLiteral,
                        Is.EqualTo(boolValue.ToString().ToLower())
                    );
                }
            });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TestIdentifier(IExpression? expression, string value)
    {
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(expression, Is.Not.Null);
                Assert.That(expression, Is.TypeOf(typeof(Identifier)));
                if (expression is Identifier identifier)
                {
                    Assert.That(identifier.Value, Is.EqualTo(value));
                    Assert.That(identifier.TokenLiteral, Is.EqualTo(value));
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
            return false;
        }
    }

    public bool TestInfixExpression(
        IExpression? expression,
        object left,
        string operatorValue,
        object right
    )
    {
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(expression, Is.Not.Null);
                Assert.That(expression, Is.TypeOf(typeof(InfixExpression)));
                if (expression is InfixExpression infix)
                {
                    TestLiteralExpression(infix.Left, left);
                    Assert.That(infix.Operator, Is.EqualTo(operatorValue));
                    TestLiteralExpression(infix.Right, right);
                }
            });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    #endregion
}
