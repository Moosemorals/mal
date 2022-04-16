
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace uk.osric.mal {

    public class Program {

        private readonly Env repl_env = new(null);
        private readonly Reader reader = new Reader();
        private static MalFuncHolder OpBuilder(Func<MalNumber, MalNumber, MalNumber> op) {
            return new MalFuncHolder((MalList l) => {
                IMalType a = l.Head;
                IMalType b = l.Tail?.Head ?? IMalType.Nil;

                if (a is MalNumber left && b is MalNumber right) {
                    return op(left, right);
                }
                return IMalType.Nil;
            });
        }

        private void FillEnvironment() {
            repl_env.Set("+", OpBuilder((a, b) => new MalNumber(a.Value + b.Value)));
            repl_env.Set("-", OpBuilder((a, b) => new MalNumber(a.Value - b.Value)));
            repl_env.Set("*", OpBuilder((a, b) => new MalNumber(a.Value * b.Value)));
            repl_env.Set("/", OpBuilder((a, b) => new MalNumber(Math.Floor(a.Value / b.Value))));
        }

        private IMalType Read(string input) {
            return reader.ReadStr(input);
        }

        private IMalType EvalAst(IMalType ast, Env env) {
            if (ast is MalSymbol s) {
                return env.Get(s.Value);
            } else if (ast is MalList l) {
                return new MalList(l.Select(m => Eval(m, env)));
            } else if (ast is MalVector v) {
                MalVector result = new MalVector();
                for (int i = 0; i < v.Count; i += 1) {
                    result.Add(Eval(v[i], env));
                }
                return result;
            } else if (ast is MalHash h) {
                MalHash result = new();
                foreach (var kv in h) {
                    IMalType key = kv.Key;
                    IMalType value = Eval(kv.Value, env);
                    result.Add(key, value);
                }
                return result;
            } else {
                return ast;
            }
        }

        private IMalType Apply(Env env, MalSymbol symbol, MalList rest) {
            switch (symbol.Value) {
                case "def!":
                    return env.Set(((MalSymbol)rest.Head).Value, Eval(rest.Tail, env));
                case "let*": {
                        Env inner = new Env(env);
                        if (rest.Head is MalList bindingList) {

                            while (!bindingList.IsEmpty) {
                                MalSymbol key = (MalSymbol)bindingList.Head;
                                bindingList = bindingList.Tail;
                                IMalType value = bindingList.Head;
                                bindingList = bindingList.Tail;
                                inner.Set(key.Value, Eval(value, inner));
                            }
                            return Eval(rest.Tail, inner);
                        } else if (rest.Head is MalVector bindingVector) {
                            for (int i = 0; i < bindingVector.Count; i += 2) {
                                MalSymbol key = (MalSymbol)bindingVector[i];
                                IMalType value = bindingVector[i + 1];
                                inner.Set(key.Value, Eval(value, inner));
                            }
                            return Eval(rest.Tail, inner);
                        } else {
                            throw new Exception("Unknown type for 'let*' bindings");
                        }
                    }
                default: {
                        MalFuncHolder f = (MalFuncHolder)EvalAst(symbol, env);
                        MalList r = (MalList)EvalAst(rest, env);
                        return f.Apply(r);
                    }
            }
        }

        private IMalType Eval(IMalType ast, Env env) {
            if (ast is MalList l) {
                if (l.IsEmpty) {
                    return l;
                }

                IMalType key = l.Head;
                if (key is MalSymbol s) {
                    return Apply(env, s, l.Tail);
                }
                throw new Exception("Couldn't apply list");
            } else {
                return EvalAst(ast, env);
            }
        }

        private string Print(IMalType input) {
            return Printer.PrStr(input);
        }

        private string Rep(string input) {
            return Print(Eval(Read(input), repl_env));
        }

        public void Repl(string[] args) {
            FillEnvironment();
            while (true) {
                Console.Write("user> ");
                string? input = Console.ReadLine();
                if (input != null) {
                    try {
                        Console.WriteLine(Rep(input));
                    } catch (Exception ex) {
                        Console.Error.WriteLine($"There was a problem {ex.Message}");
                        FormatException(ex, Console.Error);
                    }
                }
            }
        }

        private static void FormatException(Exception ex, TextWriter output) {
            List<Exception> exceptions = new();

            Exception? inner = ex;
            while (inner != null) {
                exceptions.Add(inner);
                inner = ex.InnerException;
            }

            exceptions.Reverse();

            for (int i = 0; i < exceptions.Count; i += 1) {
                Exception e = exceptions[i];
                if (i > 0) {
                    output.WriteLine("Wrapped by");
                }
                output.WriteLine($"{e.GetType().FullName}: {ex.Message}");
                output.WriteLine(e.StackTrace);
            }
        }

        public static void Main(string[] args) {
            Program mal = new Program();
            mal.Repl(args);
        }
    }
}