using System.Text;
using Interpreter.Parser.Parser;

namespace Interpreter.Parser;

public class Program : INode
{
    public List<IStatement> Statements { get; set; } = [];

    public string TokenLiteral => Statements.FirstOrDefault()?.TokenLiteral ?? "";


    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var statement in Statements)
        {
            sb.Append(statement.ToString());
        }
        return sb.ToString();
    }

}
