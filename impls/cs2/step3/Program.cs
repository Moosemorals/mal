
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace uk.osric.mal {
    public class Program {

        private readonly Reader reader = new Reader();
        private static MalFunc OpBuilder(Func<MalNumber, MalNumber, MalNumber> op) {
            return a => {
                if (a.Length == 2 && a[0] is MalNumber left && a[1] is MalNumber right) {
                    return op(left, right);
                }
                return IMalType.Nil;
            };
        }

        private readonly Dictionary<string, MalFunc> repl_env = new() {
            {"+", OpBuilder((a,b) => new MalNumber(a.Value + b.Value))},
            {"-", OpBuilder((a,b) => new MalNumber(a.Value - b.Value))},
            {"*", OpBuilder((a,b) => new MalNumber(a.Value * b.Value))},
            {"/", OpBuilder((a,b) => new MalNumber(Math.Floor(a.Value / b.Value)))},
        };

        private IMalType Read(string input) {
            return reader.ReadStr(input);
        }

        private IMalType EvalAst(IMalType ast, Dictionary<string, MalFunc> env) {
            if (ast is MalSymbol s) {
                if (env.ContainsKey(s.Value)) {
                    return new MalFuncHolder(env[s.Value]);
                } else {
                    throw new Exception($"Unknown symbol {s.Value}");
                }
            } else if (ast is MalList l) {
                MalList result = new MalList();
                for (int i = 0; i < l.Count; i += 1) {
                    result.Add(Eval(l[i], env));
                }
                return result;
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

        private IMalType Eval(IMalType ast, Dictionary<string, MalFunc> env) {
            if (ast is MalList l) {
                if (l.Count == 0) {
                    return l;
                }
                IMalType r = EvalAst(l, env);
                if (r is MalList evaledList && evaledList[0] is MalFuncHolder f) {
                    return f.Apply(evaledList.Skip(1).ToArray());
                }
                throw new Exception("Problem evalating symbol as function");
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

        public static void Main(string[] args) {
            Program mal = new Program();
            while (true) {
                Console.Write("user> ");
                string? input = Console.ReadLine();
                if (input != null) {
                    try {
                        Console.WriteLine(mal.Rep(input));
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
    }
}