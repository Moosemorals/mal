
using System;
using System.Collections.Generic;
using System.Text;

namespace Mal {

    internal class Reader {

        private readonly List<Token> _tokens;
        private int _current = 0;

        private Reader(IEnumerable<Token> tokens) {
            _tokens = new List<Token>(tokens);
        }


        private Token Peek() {
            return _tokens[_current];
        }

        private Token Advance() {
            return _tokens[_current++];
        }

        internal MalType ReadList() {
            MalList list = new();
            while (true) {
                Token next = Peek();
                // Console.WriteLine($"List {next} {list}");
                if (next.Type == TokenType.EOF) {
                    throw new MalError($"Missing closing bracket line {next.Line}");
                } else if (next.Type == TokenType.RightParen) {
                    // Consume close bracket
                    Advance();
                    return list;
                } else {
                    list.Add(ReadForm());
                }
            }
        }

        internal MalType ReadAtom() {
            Token next = Advance();
            // Console.WriteLine($"Atom {next}");
            if (next.Type == TokenType.Number) {
                return new MalAtom(next.Literal ?? throw new MalError("Missing literal value"));
            } else {
                return new MalAtom(next.Lexeme);
            }
        }

        internal MalType ReadForm() {
            // Console.WriteLine($"For1 {Peek()}");
            if (Peek().Type == TokenType.LeftParen) {
                Token next = Advance();
                // Console.WriteLine($"For2 {next}");
                return ReadList();
            } else {
                return ReadAtom();
            }
        }

        internal static MalType ReadStr(string input) {
            Reader reader = new Reader(new Scanner(input).ScanTokens());

            return reader.ReadForm();
        }
    }

    internal class Scanner {

        private const string Specials = "[]{}()`'~^@,;\"";

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private readonly string _source;

        internal Scanner(string source) {
            _source = source;
        }

        internal IEnumerable<Token> ScanTokens() {
            while (!IsAtEnd) {
                _start = _current;
                yield return ScanToken();
            }
            yield return MakeToken(TokenType.EOF);
        }

        private Token ScanToken() {
            while (!IsAtEnd) {
                char c = Advance();
                switch (c) {
                    case '(': return MakeToken(TokenType.LeftParen);
                    case ')': return MakeToken(TokenType.RightParen);
                    case '{': return MakeToken(TokenType.LeftBrace);
                    case '}': return MakeToken(TokenType.RightBrace);
                    case '[': return MakeToken(TokenType.LeftSquare);
                    case ']': return MakeToken(TokenType.RightSquare);
                    case '@': return MakeToken(TokenType.At);
                    case '^': return MakeToken(TokenType.Hat);
                    case '`': return MakeToken(TokenType.Backtick);
                    case '\'': return MakeToken(TokenType.SingleQuote);
                    case '~': return MakeToken(Match('@') ? TokenType.TildeAt : TokenType.Tilde);
                    case '"': return String();
                    case ';':
                        // Comments - ignore to EOL
                        while (!IsAtEnd && Peek() != '\n') {
                            Advance();
                        }
                        break;
                    case ' ':
                    case '\r':
                    case '\t':
                        // step over whitespace
                        _start += 1;
                        break;
                    case '\n':
                        _start += 1;
                        _line += 1;
                        break;

                    default:
                        if (IsDigit(c) || (c == '-' && IsDigit(Peek()))) {
                            return Number();
                        } else if (IsNotSpecial(c)) {
                            return Symbol();
                        } else {
                            Console.WriteLine($"Unexpected character {c} at line {_line}");
                        }
                        break;
                }
            }
            return MakeToken(TokenType.EOF);
        }

        private Token MakeToken(TokenType type) => MakeToken(type, null);
        private Token MakeToken(TokenType type, object? literal) {
            string lexeme = _source.Substring(_start, _current - _start);
            return new Token(type, lexeme, literal, _line);
        }
        private Token Number() {
            if (Peek() == '-') {
                Advance();
            }

            while (IsDigit(Peek())) {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext())) {
                // Consume decimal point
                Advance();

                while (IsDigit(Peek())) {
                    Advance();
                }
            }

            if (double.TryParse(_source.Substring(_start, _current - _start), out double literal)) {
                return MakeToken(TokenType.Number, literal);
            }

            throw new MalError("Unparsable Number");
        }
        private Token String() {
            StringBuilder literal = new();
            while (!IsAtEnd && Peek() != '"') {
                if (Peek() == '\n') {
                    _line += 1;
                } else if (Peek() == '\\') {
                    // Consume backslash
                    Advance();
                    // Add escaped character to string
                }

                // Add next character to string
                literal.Append(Advance());
                if (IsAtEnd) {
                    throw new MalError($"Unterminated string at line {_line}");
                }
            }
            if (IsAtEnd) {
                throw new MalError($"Unterminated string at line {_line}");
            }
            // Consume close double quote
            Advance();

            return MakeToken(TokenType.String, literal.ToString());
        }

        private Token Symbol() {
            while (!IsAtEnd && IsNotSpecial(Peek())) {
                Advance();
            }

            string lexeme = _source.Substring(_start, _current - _start);
            switch (lexeme) {
                case "true": return MakeToken(TokenType.True);
                case "false": return MakeToken(TokenType.False);
                case "nil": return MakeToken(TokenType.Nil);
                default:
                    return MakeToken(TokenType.Symbol);
            }
        }


        private char Advance() => _source[_current++];
        private bool IsAtEnd => _current >= _source.Length;
        private bool IsSpecial(char c) => Specials.Contains(c);
        private bool IsNotSpecial(char c) => !IsSpecial(c) && !IsWhitespace(c);
        private bool IsWhitespace(char c) => c == ' ' || c == '\r' || c == '\n' || c == '\t';
        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool Match(char expected) {
            if (IsAtEnd || Peek() != expected) {
                return false;
            }
            Advance();
            return true;
        }
        private char Peek() => IsAtEnd ? '\0' : _source[_current];
        private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    }

    internal record Token(TokenType Type, string Lexeme, object? Literal, int Line);

    internal enum TokenType {
        At, Backtick, EOF, False, Hat, LeftBrace, LeftParen, LeftSquare, Nil, Number,
        RightBrace, RightParen, RightSquare, SingleQuote, String, Symbol, Tilde,
        TildeAt, True,
    }


}
