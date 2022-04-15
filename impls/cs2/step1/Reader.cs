
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace uk.osric.mal
{
    internal class Reader
    {
        private readonly Regex NumberMatch = new Regex(@"^\d+(\.\d+)?$");
        private IEnumerator<Token>? tokens;
        internal MalType ReadStr(string input)
        {
            Scanner s = new Scanner(input);
            tokens = s.Tokenize().GetEnumerator();
            Advance();
            return ReadForm();
        }

        internal MalType ReadForm()
        {
            if (Peek().Type == TokenType.LEFT_PAREN)
            {
                return ReadList();
            }
            else if (Peek().Type != TokenType.EOF)
            {
                return ReadAtom();
            }
            else
            {
                return MalType.Nil;
            }
        }

        internal MalList ReadList()
        {
            MalList result = new MalList();

            while (!IsAtEnd)
            {
                Token next = Advance();
                if (next.Type == TokenType.RIGHT_PAREN)
                {
                    break;
                }
                result.Add(ReadForm());
            }
            return result;
        }

        internal MalType ReadAtom()
        {
            Token token = Peek();
            switch (token.Type)
            {
                case TokenType.TRUE:
                    return MalType.True;
                case TokenType.FALSE:
                    return MalType.False;
                case TokenType.NIL:
                    return MalType.Nil;
                case TokenType.STRING:
                    return new MalString(token.Lexeme);
                case TokenType.SYMBOL:
                    if (NumberMatch.IsMatch(token.Lexeme))
                    {
                        return new MalNumber(token.Lexeme);
                    }
                    else
                    {
                        return new MalSymbol(token.Lexeme);
                    }
                default:
                    throw new ArgumentException($"Unknown token {token.Type} ('{token.Lexeme}')");
            }
        }

        private bool IsAtEnd => tokens == null || tokens.Current.Type == TokenType.EOF;
        private Token Peek() => tokens != null ? tokens.Current : throw new NullReferenceException();

        private Token Advance()
        {
            if (tokens != null && tokens.MoveNext())
            {
                return Peek();
            }
            throw new Exception("Ran out of tokens");
        }
    }


    internal class Scanner
    {
        private const string Specials = "\0 \t\r\n[]{}('\"`,;)";
        private int start = 0;
        private int current = 0;
        private readonly string source;

        public Scanner(string source) => this.source = source;

        internal IEnumerable<Token> Tokenize()
        {
            while (!IsAtEnd)
            {
                yield return ScanToken();
            }
            yield break;
        }

        private Token ScanToken()
        {
            while (!IsAtEnd)
            {
                start = current;
                char c = Advance();
                switch (c)
                {
                    case '(': return MakeToken(TokenType.LEFT_PAREN);
                    case ')': return MakeToken(TokenType.RIGHT_PAREN);
                    case '@': return MakeToken(TokenType.AT);
                    case '[': return MakeToken(TokenType.LEFT_SQUARE);
                    case '\'': return MakeToken(TokenType.SINGLE_QUOTE);
                    case ']': return MakeToken(TokenType.RIGHT_SQUARE);
                    case '^': return MakeToken(TokenType.HAT);
                    case '`': return MakeToken(TokenType.BACKTICK);
                    case '{': return MakeToken(TokenType.LEFT_BRACE);
                    case '}': return MakeToken(TokenType.RIGHT_BRACE);
                    case '~':
                        return MakeToken(Match('@') ? TokenType.TILDE_AT : TokenType.TILDE);
                    case ';':
                        while (!IsAtEnd && Peek() != '\n')
                        {
                            Advance();
                        }
                        break;
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                    case ',':
                        // ignored
                        break;
                    case '"': return String();
                    default:
                        if (!IsSpecial(c))
                        {
                            return Other();
                        }
                        else
                        {
                            throw new Exception($"Unexpected character {c}");
                        }
                }
            }
            return MakeToken(TokenType.EOF);
        }

        private char Advance()
        {
            return source[current++];
        }

        private Token MakeToken(TokenType type)
        {
            return new Token(type, source.Substring(start, current - start));
        }

        private bool IsAtEnd => current >= source.Length;

        private bool IsSpecial(char c) => Specials.IndexOf(c) != -1;

        private bool Match(char expected)
        {
            if (Peek() != expected)
            {
                return false;
            }
            Advance();
            return true;
        }

        private Token Other()
        {
            while (!IsSpecial(Peek()))
            {
                Advance();
            }

            string lexeme = source.Substring(start, current - start);

            if (lexeme == "true")
            {
                return MakeToken(TokenType.TRUE);
            }
            else if (lexeme == "false")
            {
                return MakeToken(TokenType.FALSE);
            }
            else if (lexeme == "nil")
            {
                return MakeToken(TokenType.NIL);
            }
            else
            {
                return MakeToken(TokenType.SYMBOL);
            }
        }

        private char Peek()
        {
            return IsAtEnd ? '\0' : source[current];
        }

        private Token String()
        {
            string lexeme = "";
            while (!IsAtEnd && Peek() != '"')
            {
                if (Peek() == '\\')
                {
                    Advance();
                    if (!IsAtEnd)
                    {
                        lexeme += Advance();
                    }
                }
                else
                {
                    lexeme += Advance();
                }
            }

            if (IsAtEnd)
            {
                throw new Exception("Unterminated string");
            }

            // Skip closing quote
            Advance();

            return new Token(TokenType.STRING, lexeme);
        }



    }


    internal record Token(TokenType Type, string Lexeme);


    internal enum TokenType
    {

        AT,
        BACKTICK,
        EOF,
        FALSE,
        HAT,
        LEFT_BRACE,
        LEFT_PAREN,
        LEFT_SQUARE,
        NIL,
        NUMBER,
        RIGHT_BRACE,
        RIGHT_PAREN,
        RIGHT_SQUARE,
        SINGLE_QUOTE,
        STRING,
        SYMBOL,
        TILDE,
        TILDE_AT,
        TRUE,
    }
}