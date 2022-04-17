
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace uk.osric.mal {
    internal class Reader {
        private readonly Regex NumberMatch = new Regex(@"^-?\d+(\.\d+)?$");

        private readonly Dictionary<TokenType, string> ReaderMacros = new() {
            {TokenType.SINGLE_QUOTE, "quote"},
            {TokenType.BACKTICK, "quasiquote"},
            {TokenType.TILDE, "unquote"},
            {TokenType.TILDE_AT, "splice-unquote"},
            {TokenType.AT, "deref"},
        };

        private IEnumerator<Token>? tokens;
        internal IMalType ReadStr(string input) {
            Scanner s = new Scanner(input);
            tokens = s.Tokenize().GetEnumerator();
            Advance();
            return ReadForm();
        }

        internal IMalType ReadForm() {
            TokenType tt = Peek().Type;
            if (tt == TokenType.LEFT_PAREN) {
                return ReadList();
            } else if (tt == TokenType.LEFT_SQUARE) {
                return ReadVector();
            } else if (tt == TokenType.LEFT_BRACE) {
                return ReadHash();
            } else if (ReaderMacros.ContainsKey(tt)) {
                MalList result = MalList.Empty();
                result.Cons(new MalSymbol(ReaderMacros[tt]));
                Advance();
                result.Cons(ReadForm());
                return result;
            } else if (tt != TokenType.EOF) {
                return ReadAtom();
            } else {
                return IMalType.Nil;
            }
        }

        private MalList ReadList() {
            List<IMalType> items = new();

            while (!IsAtEnd) {
                Token next = Advance();
                if (next.Type == TokenType.RIGHT_PAREN) {
                    break;
                }
                items.Add(ReadForm());
            }
            return new MalList(items);
        }

        private MalVector ReadVector() {
            MalVector result = new();
            while(!IsAtEnd) {
                Token next = Advance();
                if (next.Type == TokenType.RIGHT_SQUARE) {
                    break;
                }
                result.Add(ReadForm());
            }
            return result;
        }

        private MalHash ReadHash() {
            MalHash result = new MalHash();

            while (!IsAtEnd) {
                Token KeyToken = Advance();
                if (KeyToken.Type == TokenType.RIGHT_BRACE) {
                    break;
                }
                IMalType key = ReadForm();
                Token ValueToken = Advance();
                if (ValueToken.Type == TokenType.RIGHT_BRACE) {
                    throw new ArgumentException("Hash literals need to come in pairs");
                }
                IMalType value = ReadForm();
                result.Add(key, value);
            }
            return result;
        }

        internal IMalType ReadAtom() {
            Token token = Peek();
            switch (token.Type) {
                case TokenType.TRUE:
                    return IMalType.True;
                case TokenType.FALSE:
                    return IMalType.False;
                case TokenType.NIL:
                    return IMalType.Nil;
                case TokenType.STRING:
                    return new MalString(token.Lexeme);
                case TokenType.SYMBOL:
                    if (NumberMatch.IsMatch(token.Lexeme)) {
                        return MalNumber.Parse(token.Lexeme);
                    } else {
                        return new MalSymbol(token.Lexeme);
                    }
                default:
                    throw new ArgumentException($"Unknown token {token.Type} ('{token.Lexeme}')");
            }
        }

        private bool IsAtEnd => tokens == null || tokens.Current.Type == TokenType.EOF;
        private Token Peek() => tokens != null ? tokens.Current : throw new NullReferenceException();

        private Token Advance() {
            if (tokens != null && tokens.MoveNext()) {
                return Peek();
            }
            throw new Exception("Unexpeced EOF");
        }
    }


    internal class Scanner {
        private const string Specials = "\0 \t\r\n[]{}('\"`,;)";
        private int start = 0;
        private int current = 0;
        private readonly string source;

        public Scanner(string source) => this.source = source;

        internal IEnumerable<Token> Tokenize() {
            while (!IsAtEnd) {
                yield return ScanToken();
            }
            yield break;
        }

        private Token ScanToken() {
            while (!IsAtEnd) {
                start = current;
                char c = Advance();
                switch (c) {
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
                        while (!IsAtEnd && Peek() != '\n') {
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
                        if (!IsSpecial(c)) {
                            return Other();
                        } else {
                            throw new Exception($"Unexpected character {c}");
                        }
                }
            }
            return MakeToken(TokenType.EOF);
        }

        private char Advance() {
            return source[current++];
        }

        private Token MakeToken(TokenType type) {
            return new Token(type, source.Substring(start, current - start));
        }

        private bool IsAtEnd => current >= source.Length;

        private bool IsSpecial(char c) => Specials.IndexOf(c) != -1;

        private bool Match(char expected) {
            if (Peek() != expected) {
                return false;
            }
            Advance();
            return true;
        }

        private Token Other() {
            while (!IsSpecial(Peek())) {
                Advance();
            }

            string lexeme = source.Substring(start, current - start);

            if (lexeme == "true") {
                return MakeToken(TokenType.TRUE);
            } else if (lexeme == "false") {
                return MakeToken(TokenType.FALSE);
            } else if (lexeme == "nil") {
                return MakeToken(TokenType.NIL);
            } else {
                return MakeToken(TokenType.SYMBOL);
            }
        }

        private char Peek() {
            return IsAtEnd ? '\0' : source[current];
        }

        private char PeekBehind() {
            if (current > 0) {
                return source[current - 1];
            } else {
                return '\0';
            }
        }

        private Token String() {
            string str = "";
            bool escaped = false;
            while (!IsAtEnd) {
                char c = Peek();
                // If we read a slash character then the next character is
                // escaped unless we're already in an escaped state
                if (!escaped && c == '"') {
                    break;
                }
                if (!escaped && c == '\\') {
                    escaped = true;
                } else {
                    escaped = false;
                }

                str += c;
                Advance();
            }

            if (IsAtEnd) {
                throw new Exception("Unterminated string at EOF");
            }

            // Skip closing quote
            Advance();

            return new Token(TokenType.STRING, InterpretSlashes(str));
        }

        private static string InterpretSlashes(string input) {
            // replace backslash escapes
            string output = "";
            for (int i = 0; i < input.Length; i += 1) {
                char c = input[i];
                if (c == '\\') {
                    if (i != input.Length - 1) {
                        char n = input[i + 1];
                        switch (n) {
                            case 'n':
                                output += '\n';
                                break;
                            case '\\':
                                output += '\\';
                                break;
                            case '"':
                                output += '"';
                                break;
                            default:
                                output += c;
                                output += n;
                                break;
                        }
                        i += 1;
                        continue;
                    }
                }
                output += c;
            }
            return output;
        }
    }


    internal record Token(TokenType Type, string Lexeme);


    internal enum TokenType {

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