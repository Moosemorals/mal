
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

        internal MalValue ReadList(TokenType endList) {
            MalSequence seq = endList == TokenType.RightParen ? new MalList() : new MalVector();
            while (true) {
                Token next = Peek();
                if (next.Type == TokenType.EOF) {
                    throw new MalError($"Missing closing bracket line {next.Line}");
                } else if (next.Type == endList) {
                    // Consume close token
                    Advance();
                    return seq;
                } else {
                    seq.Add(ReadForm());
                }
            }
        }

        internal MalValue ReadAtom() {
            Token next = Advance();
            if (next.Type == TokenType.Number) {
                if (next.Literal is double d) {
                    return new MalNumber(d);
                } else {
                    throw new MalError("Missing number literal");
                }
            } else if (next.Type == TokenType.String) {
                if (next.Literal is string s) {
                    return new MalString(s);
                } else {
                    throw new MalError("Missing string literal");
                }
            } else if (next.Type == TokenType.True) {
                return MalBool.True;
            } else if (next.Type == TokenType.False) {
                return MalBool.False;
            } else if (next.Type == TokenType.Nil) {
                return MalNil.Nil;
            } else {
                return new MalSymbol((string) next.Lexeme);
            }
        }

        internal MalValue ReadForm() {
            if (Peek().Type == TokenType.LeftParen) {
                Advance();
                return ReadList(TokenType.RightParen);
            } else if (Peek().Type == TokenType.LeftSquare) {
                Advance();
                return ReadList(TokenType.RightSquare);
            } else {
                return ReadAtom();
            }
        }

        internal static MalValue ReadStr(string input) {
            return new Reader(new Scanner(input).ScanTokens()).ReadForm();

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
            string lexeme = _source[_start.._current];
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

            if (double.TryParse(_source[_start.._current], out double literal)) {
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
                    Advance();
                    if (IsAtEnd) {
                        continue;
                    }
                    char c = Advance();
                    if (c == 'n') {
                        literal.Append('\n');
                    } else if (c == '\\' || c == '"') {
                        literal.Append(c);
                    } else {
                        literal.Append(new char[] { '\\', c} );
                    }
                    continue;
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

            string lexeme = _source[_start.._current];
            return lexeme switch {
                "true" => MakeToken(TokenType.True),
                "false" => MakeToken(TokenType.False),
                "nil" => MakeToken(TokenType.Nil),
                _ => MakeToken(TokenType.Symbol),
            };
        }


        private char Advance() => _source[_current++];
        private bool IsAtEnd => _current >= _source.Length;
        private static bool IsSpecial(char c) => Specials.Contains(c);
        private static bool IsNotSpecial(char c) => !IsSpecial(c) && !IsWhitespace(c);
        private static bool IsWhitespace(char c) => c == ' ' || c == '\r' || c == '\n' || c == '\t';
        private static bool IsDigit(char c) => c >= '0' && c <= '9';
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
