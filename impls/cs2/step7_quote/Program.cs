using System;
using System.Collections.Generic;
using System.Linq;

namespace Mal {
    internal class Program {

        public Program(IEnumerable<string> argv) {
            Core.Init(_outer, argv);
        }

        private readonly Env _outer = new(null, null, null);

        private MalValue EvalAst(MalValue ast, Env env) => ast switch {
            MalSymbol s => env.Get(s),
            MalList l => new MalList(l.Select(i => Eval(i, env))),
            MalVector v => new MalVector(v.Select(i => Eval(i, env))),
            _ => ast
        };

        private static MalValue Read(string input) {
            return Reader.ReadStr(input);
        }

        internal MalValue Eval(MalValue ast, Env env) {
            while (true) {
                //          Console.WriteLine($"eval> {Printer.PrStr(ast, true)}");

                if (ast is not MalList l) {
                    return EvalAst(ast, env);
                }
                if (l.Count == 0) {
                    return ast;
                }

                switch (l.First()) {
                    case MalSymbol cmd when cmd.Value == "def!": {
                        MalSymbol key = l[1] as MalSymbol ?? throw new MalError("Can only define symbols");
                        MalValue value = Eval(l[2], env);
                        return env.Set(key, value);
                    }
                    case MalSymbol cmd when cmd.Value == "let*": {
                        Env c = new(env, null, null);
                        MalSequence bindings = l[1] as MalSequence ?? throw new MalError("First argument to let must be sequence");
                        if (bindings.Count % 2 != 0) {
                            throw new MalError("Bindings must come in pairs");
                        }
                        for (int i = 0 ; i < bindings.Count ; i += 2) {
                            MalSymbol key = bindings[i] as MalSymbol ?? throw new MalError("Can only bind to symbols");
                            MalValue value = Eval(bindings[i + 1], c);
                            c.Set(key, value);
                        }
                        // TCO
                        ast = l[2];
                        env = c;
                        continue;
                    }
                    case MalSymbol cmd when cmd.Value == "do": {
                        // Drop the 'do'
                        List<MalValue> rest = l.Skip(1).ToList();
                        switch (rest.Count) {
                            case 0:
                                ast = MalNil.Nil;
                                break;
                            case 1:
                                ast = rest[0];
                                break;
                            default:
                                EvalAst(new MalList(rest.SkipLast(1)), env);
                                ast = rest.Last();
                                break;
                        }
                        continue;
                    }
                    case MalSymbol cmd when cmd.Value == "if": {
                        MalValue expr = Eval(l[1], env);
                        MalValue trueBranch = l[2];
                        MalValue falseBranch = l.Count > 3 ? l[3] : MalNil.Nil;
                        if (expr is not MalNil && !(expr is MalBool b && b == MalBool.False)) {
                            // TCO
                            ast = trueBranch;
                        } else {
                            ast = falseBranch;
                        }
                        continue;
                    }
                    case MalSymbol cmd when cmd.Value == "fn*": {
                        MalSymbol[] param = (l[1] as MalSequence)?.Cast<MalSymbol>().ToArray() ?? throw new MalError("Missing argument list");
                        MalValue body = l[2];
                        return new MalNativeFunction(env, param, body);
                    }
                    default: {
                        MalList evald = EvalAst(l, env) as MalList ?? throw new MalError("Eval_Ast didn't return MalList");
                        MalFunction fn = evald.First() as MalFunction ?? throw new MalError("Was expecting a function");

                        MalValue[] fnArgs = evald.Skip(1).ToArray();
                        // TCO
                        if (fn is MalForeignFunction) {
                            return fn.Eval(this, fnArgs);
                        } else if (fn is MalNativeFunction n) {
                            ast = n.Body;
                            env = new Env(n.Env, n.Param, fnArgs);
                            continue;
                        } else {
                            throw new MalError("Unknown function type");
                        }
                    }
                }
            }
        }

        private static string Print(MalValue input) => Printer.PrStr(input, true);

        private string Rep(string input) => Print(Eval(Read(input), _outer));

        static void Main(string[] argv) {
            Program p;
            if (argv.Length > 1) {
                p = new(argv.Skip(1));
            } else {
                p = new(Array.Empty<string>());
            }

            p.Rep("(def! not (fn* (a) (if a false true)))");
            p.Rep("(def! load-file (fn* (f) (eval (read-string (str \"(do \" (slurp f) \"\nnil)\")))))");

            if (argv.Length > 0) {
                Console.WriteLine(p.Rep($"(load-file \"{argv[0]}\")"));
                return;
            }

            while (true) {
                Console.Write("user> ");
                string? input = Console.ReadLine();
                if (input == null) {
                    break;
                }
                try {
                    string output = p.Rep(input);
                    Console.Write(output + "\n");
                } catch (MalError ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
