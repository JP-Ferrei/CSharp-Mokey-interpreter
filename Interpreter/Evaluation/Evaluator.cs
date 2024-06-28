using Interpreter.Exceptions;
using Interpreter.Parser;
using Interpreter.Parser.Parser;

namespace Interpreter.Evaluation;

public class Evaluator
{
    public static IObject? Eval(INode? node)
    {
        var result = node switch
        {
            Parser.Program p => EvalProgram(p.Statements),
            IntegerLiteral n => new IntegerObject(n.Value),
            BooleanLiteral n => BooleanObject.NativeBoolToBoolean(n),
            ExpressionStatement expression => Eval(expression.Expression),
            PrefixExpression expression => EvalPrefixExpression(expression),
            InfixExpression expression => EvalInfixExpression(expression),
            BlockStatement block => EvalBlockStatement(block),
            IfExpression expression => EvalIfExpression(expression),
            ReturnStatement returnStatement => EvalReturnStatement(returnStatement),
            _ => NullObject.Instance
        };

        return result;
    }

    private static IObject? EvalProgram(List<IStatement> statements)
    {
        IObject? result = null;
        foreach (var statement in statements)
        {
            result = Eval(statement);

            if (result is ReturnObject resultObject)
                return resultObject.Value;
        }

        return result;
    }

    private static IObject? EvalBlockStatement(BlockStatement block)
    {
        IObject? result = null;
        foreach (var statement in block.Statements)
        {
            result = Eval(statement);

            if (result is not null and ReturnObject)
                return result;
        }

        return result;
    }

    private static IObject? EvalInfixExpression(InfixExpression expression) =>
        EvalInfixExpression(expression.Operator, Eval(expression.Left), Eval(expression.Right));

    private static IObject? EvalInfixExpression(string @operator, IObject? left, IObject? right)
    {
        ArgumentNullException.ThrowIfNull(@operator);
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        Console.WriteLine($"operator: {@operator}, left: {left}, right: {right}");
        return (@operator, left, right) switch
        {
            (_, IntegerObject leftInteger, IntegerObject rightInteger)
                => EvalIntegerInfixOperator(@operator, leftInteger, rightInteger),
            ("==", BooleanObject l, BooleanObject r)
                => BooleanObject.NativeBoolToBoolean(l.Value == r.Value),
            ("!=", BooleanObject l, BooleanObject r)
                => BooleanObject.NativeBoolToBoolean(l.Value != r.Value),
            (var o, var l, var r) when l.Type() != r.Type()
                => throw new TypeMissMatchException(
                    @operator: o.ToString(),
                    leftValue: l!,
                    rightValue: r!
                ),
            (var o, var l, var r)
                => throw new UnkwownOperatorException(
                    @operator: o.ToString(),
                    leftValue: l!,
                    rightValue: r!
                ),
        };
    }

    private static IObject? EvalIntegerInfixOperator(
        string @operator,
        IntegerObject left,
        IntegerObject right
    )
    {
        return @operator switch
        {
            "+" => new IntegerObject(left.Value + right.Value),
            "-" => new IntegerObject(left.Value - right.Value),
            "*" => new IntegerObject(left.Value * right.Value),
            "/" => new IntegerObject(left.Value / right.Value),
            "<" => BooleanObject.NativeBoolToBoolean(left.Value < right.Value),
            ">" => BooleanObject.NativeBoolToBoolean(left.Value > right.Value),
            "==" => BooleanObject.NativeBoolToBoolean(left.Value == right.Value),
            "!=" => BooleanObject.NativeBoolToBoolean(left.Value != right.Value),
            var o
                => throw new UnkwownOperatorException(
                    @operator: o.ToString(),
                    leftValue: left,
                    rightValue: right
                ),
        };
    }

    private static IObject? EvalPrefixExpression(PrefixExpression expression) =>
        EvalPrefixExpression(expression.Operator, Eval(expression.Right));

    private static IObject? EvalPrefixExpression(string operatorValue, IObject? right)
    {
        ArgumentNullException.ThrowIfNull(right);
        return operatorValue switch
        {
            "!" => EvalBangOperatorExpression(right),
            "-" => EvalMinusPrefixOperatorExpression(right),
            _ => throw new UnkwownOperatorException(operatorValue, right)
        };
    }

    private static IObject? EvalMinusPrefixOperatorExpression(IObject? right)
    {
        ArgumentNullException.ThrowIfNull(right);

        if (right is not null and IntegerObject integer)
        {
            return new IntegerObject(-integer.Value);
        }

        throw new UnkwownOperatorException(right!);
    }

    private static IObject EvalBangOperatorExpression(IObject? right)
    {
        return right switch
        {
            BooleanObject b when b is { Value: true } => BooleanObject.False,
            BooleanObject b when b is { Value: false } => BooleanObject.True,
            NullObject x => BooleanObject.True,
            null => BooleanObject.True,
            _ => BooleanObject.False
        };
    }

    private static IObject? EvalIfExpression(IfExpression expression)
    {
        var condition = Eval(expression.Condition);

        var isTruthy = condition switch
        {
            NullObject _ => false,
            BooleanObject b when b is { Value: false } => false,
            _ => true
        };

        return (isTruthy, expression.Alternative) switch
        {
            (true, _) => Eval(expression.Consequence),
            (false, not null) => Eval(expression.Alternative),
            _ => null
        };
    }

    private static IObject? EvalReturnStatement(ReturnStatement returnStatement)
    {
        var value = Eval(returnStatement.ReturnExpression);
        return new ReturnObject(value);
    }
}
