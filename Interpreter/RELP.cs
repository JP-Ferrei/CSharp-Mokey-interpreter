namespace Interpreter;

public static class RELP
{

    public static void Start()
    {
        var input = Console.ReadLine();
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Empty");
            return;
        }

        var lexer = new Lexer(input);

        for (var token = lexer.NextToken(); token.TokenType != TokenType.EOF; token = lexer.NextToken())
        {
            Console.WriteLine(token);
        }
    }
}
