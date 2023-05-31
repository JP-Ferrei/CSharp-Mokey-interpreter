namespace Interpreter
{
    public class Lexer
    {
        public string Input { get; private set; }
        public int Position { get; private set; }
        public int ReadPosition { get; private set; }
        public char Data { get; private set; }

        public Lexer(string input)
        {
            Input = input;
            Position = 0;
            ReadPosition = 0;
            ReadChar();
        }

        private void ReadChar()
        {
            if (ReadPosition >= Input.Length)
                Data = '\0';
            else
                Data = Input[ReadPosition];

            Position = ReadPosition;
            ReadPosition++;
        }

        private char PeekChar()
        {
            if (ReadPosition >= Input.Length)
                return '\0';
            else
                return Input[ReadPosition];

        }

        public Token NextToken()
        {
            SkipNextToken();
            if (IsLetter(Data))
                return ResolveLetter();

            if (IsDigit(Data))
                return ResolveNumber();

            return ResolveOperators();
        }

        private void SkipNextToken()
        {
            while (Data == ' ' || Data == '\t' || Data == '\n' || Data == '\r')
            {
                ReadChar();
            }
        }

        private bool CanBeDoubleOperator(char data) => (data == '=' || data == '!' || data == '>' || data == '<');

        private bool IsLetter(char data) => 'a' <= data && data <= 'z' || 'A' <= data && data <= 'Z' || data == '_';

        private bool IsDigit(char data) => '0' <= data && data <= '9';

        private Token ResolveNumber() => new(TokenType.INT, ReadNumber());

        private Token ResolveLetter()
        {
            var word = ReadIdentifier();
            return new Token(Token.LookupIdent(word), word);
        }

        private string ReadIdentifier()
        {
            var position = Position;
            while (IsLetter(Data))
            {
                ReadChar();
            }
            return Input[position..Position];
        }

        private Token ResolveOperators()
        {
            Token token;
            if (CanBeDoubleOperator(Data))
                token = ResolveDoubleOperators(Data);
            else
                token = ResolveSingleOperators();

            ReadChar();
            return token;
        }

        private string ReadNumber()
        {
            var position = Position;
            while (IsDigit(Data))
            {
                ReadChar();
            }
            return Input[position..Position];
        }

        private Token ResolveSingleOperators()
        {
            return Data switch
            {
                '*' => new Token(TokenType.ASTERISK, Data),
                '+' => new Token(TokenType.PLUS, Data),
                '-' => new Token(TokenType.MINUS, Data),
                '/' => new Token(TokenType.SLASH, Data),
                '\\' => new Token(TokenType.BACKSLASH, Data),
                ';' => new Token(TokenType.SEMICOLON, Data),
                '(' => new Token(TokenType.LPAREN, Data),
                ')' => new Token(TokenType.RPAREN, Data),
                ',' => new Token(TokenType.COMMA, Data),
                '{' => new Token(TokenType.LBRACE, Data),
                '}' => new Token(TokenType.RBRACE, Data),
                _ => new Token(TokenType.EOF, ""),
            };
        }

        private Token ResolveDoubleOperators(char data)
        {
            Token token;
            switch (data)
            {
                case '=':
                    if (PeekChar() == '=')
                    {
                        var character = Data;
                        ReadChar();
                        token = new Token(TokenType.EQ, string.Concat(character, Data));
                    }
                    else
                        token = new Token(TokenType.ASSIGN, Data);
                    break;
                case '!':
                    if (PeekChar() == '=')
                    {
                        var character = Data;
                        ReadChar();
                        token = new Token(TokenType.NOT_EQ, string.Concat(character, Data));
                    }
                    else
                        token = new Token(TokenType.BANG, Data);
                    break;
                case '>':
                    if (PeekChar() == '=')
                    {
                        var character = Data;
                        ReadChar();
                        token = new Token(TokenType.GT_EQ, string.Concat(character, Data));
                    }
                    else
                        token = new Token(TokenType.GT, Data);
                    break;
                case '<':
                    if (PeekChar() == '=')
                    {
                        var character = Data;
                        ReadChar();
                        token = new Token(TokenType.LT_EQ, string.Concat(character, Data));
                    }
                    else
                        token = new Token(TokenType.LT, Data);
                    break;
                default:
                    token = new Token(TokenType.ILLEGAL, data);
                    break;
            }
            return token;
        }
    }
}