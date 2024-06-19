namespace Interpreter.Parser.Parser;

public interface INode
{
    string TokenLiteral { get; }
}

public interface IStatement : INode
{
    void StatementNode();
}

public interface IExpression : INode
{
    void ExpressionNode();
}