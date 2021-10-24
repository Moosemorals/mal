using System;
using System.Collections.Generic;
using System.Linq;

namespace Mal {
    internal class Program {

        public Program() {
            Core.Init(_outer);
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
                if (ast is not MalList l) {
                    return EvalAst(ast, env);
                }
                if (l.Count == 0) {
                    return ast;
                }

                switch (l[0]) {
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
                            _ = rest.SkipLast(1).Select(i => Eval(i, env)).ToList();
                            ast = rest.Last();
                            break;
                        }
                        continue;
                    }
                    case MalSymbol cmd when cmd.Value == "if": {
                        MalValue expr = Eval(l[1], env);
                        if (expr is not MalNil && !(expr is MalBool b && b == MalBool.False)) {
                            // True branch
                            // TCO
                            ast = l[2];
                        } else {
                            // False branch
                            if (l.Count >= 4) {
                                ast = l[3];
                            } else {
                                ast = MalNil.Nil;
                            }
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
                        MalFunction fn = evald[0] as MalFunction ?? throw new MalError("Was expecting a function");
                        // TCO
                        if (fn is MalForeignFunction ff) {
                            return fn.Eval(this, evald.Skip(1).ToArray());
                        } else if (fn is MalNativeFunction n) {
                            ast = n.Body;
                            env = new Env(n.Env, n.Param, evald.Skip(1).ToArray());
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

        static void Main() {
            Program p = new();

            p.Rep("(def! not (fn* (a) (if a false true)))");
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
