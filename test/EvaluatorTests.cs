using Interpreter.Evaluation;
using Interpreter.Exceptions;
using Interpreter.Lexer;
using Interpreter.Parser;
using Environment = Interpreter.Evaluation.Environment;

namespace test;

[TestFixture]
public class EvaluatorTests
{
    [TestCase("let a = 5; a;", 5)]
    [TestCase("let a = 5 * 5; a;", 25)]
    [TestCase("let a = 5; let b = a; b;", 5)]
    [TestCase("let a = 5; let b = a; let c = a + b + 5; c;", 15)]
    public void TestLetStatements(string input, int expected)
    {
        var evaluated = TestEval(input);
        TestIntegerObject(evaluated, expected);
    }

    [TestCase("5", 5)]
    [TestCase("10", 10)]
    [TestCase("-5", -5)]
    [TestCase("-10", -10)]
    [TestCase("5 + 5 + 5 + 5 - 10", 10)]
    [TestCase("2 * 2 * 2 * 2 * 2", 32)]
    [TestCase("-50 + 100 + -50", 0)]
    [TestCase("5 * 2 + 10", 20)]
    [TestCase("5 + 2 * 10", 25)]
    [TestCase("20 + 2 * -10", 0)]
    [TestCase("50 / 2 * 2 + 10", 60)]
    [TestCase("2 * (5 + 10)", 30)]
    [TestCase("3 * 3 * 3 + 10", 37)]
    [TestCase("3 * (3 * 3) + 10", 37)]
    [TestCase("(5 + 10 * 2 + 15 / 3) * 2 + -10", 50)]
    public void TestEvalIntegerExpression(string input, int expected)
    {
        var evaluated = TestEval(input);
        TestIntegerObject(evaluated, expected);
    }

    [TestCase("true", true)]
    [TestCase("false", false)]
    [TestCase("true == true", true)]
    [TestCase("false == false", true)]
    [TestCase("true == false", false)]
    [TestCase("true != false", true)]
    [TestCase("false != true", true)]
    [TestCase("(1 < 2) == true", true)]
    [TestCase("(1 < 2) == false", false)]
    [TestCase("(1 > 2) == true", false)]
    [TestCase("(1 > 2) == false", true)]
    public void TestEvalBooleanExpression(string input, bool expected)
    {
        var evaluated = TestEval(input);
        TestBooleanObject(evaluated, expected);
    }

    [TestCase("!true", false)]
    [TestCase("!false", true)]
    [TestCase("!5", false)]
    [TestCase("!!true", true)]
    [TestCase("!!false", false)]
    [TestCase("!!5", true)]
    [TestCase("1 < 2", true)]
    [TestCase("1 > 2", false)]
    [TestCase("1 < 1", false)]
    [TestCase("1 > 1", false)]
    [TestCase("1 == 1", true)]
    [TestCase("1 != 1", false)]
    [TestCase("1 == 2", false)]
    [TestCase("1 != 2", true)]
    public void TestBangOperator(string input, bool expected)
    {
        var evaluated = TestEval(input);
        TestBooleanObject(evaluated, expected);
    }

    [TestCase("if (true) { 10 }", 10)]
    [TestCase("if (false) { 10 }", null)]
    [TestCase("if (1) { 10 }", 10)]
    [TestCase("if (1 < 2) { 10 }", 10)]
    [TestCase("if (1 > 2) { 10 }", null)]
    [TestCase("if (1 > 2) { 10 } else { 20 }", 20)]
    [TestCase("if (1 < 2) { 10 } else { 20 }", 10)]
    public void TestIfElseExpressions(string input, object expected)
    {
        var evaluated = TestEval(input);

        if (expected is int number)
            TestIntegerObject(evaluated, number);
        else
            TestNullObject(evaluated);
    }

    [TestCase("return 10;", 10)]
    [TestCase("return 10; 9;", 10)]
    [TestCase("return 2 * 5; 9;", 10)]
    [TestCase("9; return 2 * 5; 9;", 10)]
    [TestCase(" if (10 > 1) { if (10 > 1) { return 10; }return 1; }", 10)]
    public void TestReturnExpressions(string input, int expected)
    {
        var evaluated = TestEval(input);
        TestIntegerObject(evaluated, expected);
    }

    [TestCase("fn(x) { x + 2; };")]
    public void TestReturnExpressions(string input)
    {
        var evaluated = TestEval(input);
        AssertExtensions.AssertReturnType(evaluated, out FunctionObject fn);

        Assert.That(fn.Parameters.Count, Is.EqualTo(1));
        Assert.That(fn.Parameters[0].ToString(), Is.EqualTo("x"));

        var expectedBody = "(x + 2)";
        Assert.That(fn.Body.ToString(), Is.EqualTo(expectedBody));
    }

    [TestCase("5 + true;", typeof(TypeMissMatchException), "type mismatch: INTEGER + BOOLEAN")]
    [TestCase("5 + true; 5;", typeof(TypeMissMatchException), "type mismatch: INTEGER + BOOLEAN")]
    [TestCase("-true", typeof(UnkwownOperatorException), "unknown operator: -BOOLEAN")]
    [TestCase(
        "true + false;",
        typeof(UnkwownOperatorException),
        "unknown operator: BOOLEAN + BOOLEAN"
    )]
    [TestCase(
        "5; true + false; 5",
        typeof(UnkwownOperatorException),
        "unknown operator: BOOLEAN + BOOLEAN"
    )]
    [TestCase(
        "if (10 > 1) { true + false; }",
        typeof(UnkwownOperatorException),
        "unknown operator: BOOLEAN + BOOLEAN"
    )]
    [TestCase(
        "if (10 > 1) { true + false; }",
        typeof(UnkwownOperatorException),
        "unknown operator: BOOLEAN + BOOLEAN"
    )]
    [TestCase(
        "if (10 > 1) { if (10 > 1) { return true + false; } return 1; }",
        typeof(UnkwownOperatorException),
        "unknown operator: BOOLEAN + BOOLEAN"
    )]
    [TestCase("foobar", typeof(IdenfierNotFoundException), "identifier not found: foobar")]
    public void TestErrorHandling(string input, Type exceptionType, string errorMessage)
    {
        Assert.Throws(
            exceptionType,
            () =>
            {
                TestEval(input);
            },
            errorMessage
        );
    }

    public IObject TestEval(string input)
    {
        var lexer = new MonkeyLexer(input);
        var parser = new MonkeyParser(lexer);
        var program = parser.ParserProgram();
        var env = new Environment();

        return Evaluator.Eval(program, env);
    }

    private bool TestIntegerObject(object evaluated, int expected)
    {
        try
        {
            AssertExtensions.AssertReturnType(evaluated, out IntegerObject integer);

            Assert.That(integer.Value, Is.EqualTo(expected));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool TestBooleanObject(object evaluated, bool expected)
    {
        try
        {
            AssertExtensions.AssertReturnType(evaluated, out BooleanObject boolean);

            Assert.That(boolean.Value, Is.EqualTo(expected));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool TestNullObject(IObject evaluated)
    {
        try
        {
            Assert.That(evaluated, Is.Null);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
