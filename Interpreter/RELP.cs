using System.Text;
using Interpreter.Parser;

namespace Interpreter;

public static class RELP
{
    public static string MonkeyFace()
    {
        var sb = new StringBuilder();
        sb.AppendLine("""           __,__""");
        sb.AppendLine("""  .--. .-\"     \"-. .--.""");
        sb.AppendLine(""" / .. \/  .-. .-.  \/ .. \""");
        sb.AppendLine("""| |  '|  /   Y   \  |'  | |""");
        sb.AppendLine("""| \   \  \ 0 | 0 /  /   / |""");
        sb.AppendLine(@" \ '- ,\.-""""""""""""""-./, -' /");
        sb.AppendLine("""  ''-' /_   ^ ^   _\ '-''""");
        sb.AppendLine("""      |  \._   _./  |""");
        sb.AppendLine("""      \   \ '~' /   /""");
        sb.AppendLine("""       '._ '-=-' _.'""");
        sb.AppendLine("""          '-----'""");

        return sb.ToString();
    }



    public static void Start()
    {
        while (true)
        {

            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Empty");
                return;
            }

            var lexer = new Lexer(input);
            var parser = new MonkeyParser(lexer);
            var program = parser.ParserProgram();

            if (parser.Errors.Any())
            {
                PrintParserErros(parser.Errors);
            }

            foreach (var statement in program.Statements)
            {
                Console.WriteLine(statement);
            }
        }
    }

    public static void PrintParserErros(IEnumerable<string> erros)
    {
        Console.WriteLine(MonkeyFace());
        Console.WriteLine("Woops!! We ran into some monkey business here");
        Console.WriteLine(" parser erros");
        foreach (var item in erros)
        {
            Console.WriteLine(item);
        }
    }
}
