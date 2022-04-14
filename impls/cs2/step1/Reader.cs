
using System;
using System.Collections.Generic;

namespace uk.osric.mal
{
    internal class Reader
    {

        private const string Specials = " \t\r\n[]{}('\"`,;)";
        private readonly string source;
        private readonly List<Token> tokens = new();

        private int start = 0;
        private int current = 0;

        internal Reader(string source)
        {
            this.source = source;
        }

        internal List<Token> ScanTokens()
        {
            while (!IsAtEnd)
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, ""));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '@': AddToken(TokenType.AT); break;
                case '[': AddToken(TokenType.LEFT_SQUARE); break;
                case '\'': AddToken(TokenType.SINGLE_QUOTE); break;
                case ']': AddToken(TokenType.RIGHT_SQUARE); break;
                case '^': AddToken(TokenType.HAT); break;
                case '`': AddToken(TokenType.BACKTICK); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case '~':
                    AddToken(Match('@') ? TokenType.TILDE_AT : TokenType.TILDE);
                    break;
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
                case '"': String(); break;
                default:
                    Other();
                    break;
            }
        }


        private char Advance()
        {
            return source[current++];
        }

        private void AddToken(TokenType type)
        {
            tokens.Add(new Token(type, source.Substring(start, current - start)));
        }

        private bool IsAtEnd => current >= source.Length;

        private bool Match(char expected)
        {
            if (Peek() != expected)
            {
                return false;
            }
            Advance();
            return true;
        }

        private void Other() {
            string lexeme = "";
            while (!IsAtEnd && Specials.IndexOf(Peek()) == -1) {
                lexeme += Advance();
            }

            if (lexeme == "true") {
                AddToken(TokenType.TRUE);
            } else if (lexeme == "false") {
                AddToken(TokenType.FALSE);
            } else if (lexeme == "nil") {
                AddToken(TokenType.NIL);
            } else {
                AddToken(TokenType.SYMBOL);
            }
        }

        private char Peek()
        {
            return IsAtEnd ? '\0' : source[current];
        }

        private void String()
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

            tokens.Add(new Token(TokenType.STRING, lexeme));
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