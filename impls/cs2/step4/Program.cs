
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace uk.osric.mal {

    public class Program {

        private readonly Env repl_env = new(null);
        private readonly Reader reader = new Reader();

        private IMalType EvalAst(IMalType ast, Env env) {
            if (ast is MalSymbol s) {
                return env.Get(s);
            } else if (ast is MalList l) {
                return new MalList(l.Select(m => Eval(env, m)));
            } else if (ast is MalVector v) {
                return new MalVector(v.Select(m => Eval(env, m)));
            } else if (ast is MalHash h) {
                MalHash result = new();
                foreach (var kv in h) {
                    IMalType key = kv.Key;
                    IMalType value = Eval(env, kv.Value);
                    result.Add(key, value);
                }
                return result;
            } else {
                return ast;
            }
        }

        private IMalType Def(Env env, MalList rest) {
            return env.Set((MalSymbol)rest.Head, Eval(env, rest.Tail.Head));
        }

        private IMalType Let(Env env, MalList rest) {
            Env inner = new Env(env);
            if (rest.Head is MalList bindingList) {
                // via https://stackoverflow.com/a/6888263
                using (var iterator = bindingList.GetEnumerator()) {
                    while (iterator.MoveNext()) {
                        IMalType key = iterator.Current;
                        IMalType value = iterator.MoveNext() ? iterator.Current : throw new ArgumentException("Bindings must come in paris");
                        inner.Set((MalSymbol)key, Eval(inner, value));
                    }
                }
                return Eval(inner, rest.Tail.Head);
            } else if (rest.Head is MalVector bindingVector) {
                for (int i = 0; i < bindingVector.Count; i += 2) {
                    MalSymbol key = (MalSymbol)bindingVector[i];
                    IMalType value = bindingVector[i + 1];
                    inner.Set(key, Eval(inner, value));
                }
                return Eval(inner, rest.Tail.Head);
            } else {
                throw new Exception("Unknown type for 'let*' bindings");
            }
        }

        private IMalType If(Env env, MalList rest) {
            IMalType testResult = Eval(env, rest.Head);
            if (testResult != IMalType.Nil && testResult != IMalType.False) {
                return Eval(env, rest.Tail.Head);
            } else {
                return Eval(env, rest.Tail.Tail.Head);
            }
        }

        private IMalType Func(Env env, MalList rest) {
            return new MalFuncHolder(args => {
                Env inner = new Env(env, (IEnumerable<IMalType>)rest.Head, args);
                return Eval(inner, rest.Tail.Head);
            });
        }

        private IMalType Call(Env env, MalList ast) {
            MalList list = (MalList)EvalAst(ast, env);
            MalFuncHolder f = (MalFuncHolder)list.Head;
            return f.Apply(list.Tail);
        }

        private IMalType Eval(Env env, IMalType ast) {
            if (ast is MalList l) {
                if (l.IsEmpty) {
                    return l;
                }
                return l.Head switch {

                    MalSymbol { Value: "def!" } => Def(env, l.Tail),
                    MalSymbol { Value: "do" } => l.Tail.Select(m => Eval(env, m)).Last(),
                    MalSymbol { Value: "if" } => If(env, l.Tail),
                    MalSymbol { Value: "let*" } => Let(env, l.Tail),
                    MalSymbol { Value: "fn*" } => Func(env, l.Tail),

                    _ => Call(env, l),
                };
            } else {
                return EvalAst(ast, env);
            }
        }

        private IMalType Read(string input) {
            return reader.ReadStr(input);
        }

        private string Print(IMalType input) {
            return Printer.PrStr(input, true);
        }

        private string Rep(string input) {
            return Print(Eval(repl_env, Read(input)));
        }


        private void LoadStdLib(Env env) {
            // Load standard lib native functions
            foreach ((MalSymbol key, MalFunc value) in Core.NS) {
                env.Set(key, new MalFuncHolder(value));
            }

            // Load standard lib mal functions
            foreach (string def in Core.Mal) {
                Rep(def);
            }
        }

        public void Repl(string[] args) {
            LoadStdLib(repl_env);

            Readline readline = new();

            while (true) {
                try {
                    string? input = readline.WaitForInput("user> ", basic: true);
                    if (input != null) {
                        Console.WriteLine(Rep(input));
                    }
                } catch (Exception ex) {
                    Console.Error.WriteLine($"There was a problem {ex.Message}");
                    Console.Error.WriteLine(Printer.FormatException(ex));
                }
            }
        }

        public static void Main(string[] args) {
            Program mal = new Program();
            mal.Repl(args);
        }
    }
}