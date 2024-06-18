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

        foreach (var token in lexer.Iterate())
        {
            Console.WriteLine(token);
        }
    }
}
