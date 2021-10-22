using System;
using System.Collections.Generic;
using System.Linq;

namespace Mal {
    class Program {

        private readonly Env _outer = new Env(null);

        private MalFunction MathFunctionBuilder(string name, Func<double, double, double> fn) {
            return new MalFunction(
                new MalSymbol(name),
                args => {
                    MalNumber a = args[0] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    MalNumber b = args[1] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    return new MalNumber(fn(a.Value, b.Value));
                }
            );
        }

        private void Init() {

            MalFunction add = MathFunctionBuilder("+", (a, b) => a + b);
            MalFunction sub = MathFunctionBuilder("-", (a, b) => a - b);
            MalFunction mul = MathFunctionBuilder("*", (a, b) => a * b);
            MalFunction div = MathFunctionBuilder("/", (a, b) => (int)(a / b));

            _outer.Set(add.Name, add);
            _outer.Set(sub.Name, sub);
            _outer.Set(mul.Name, mul);
            _outer.Set(div.Name, div);
        }

        private MalType Eval_Ast(MalType ast, Env env) => ast switch {
            MalSymbol s => env.Get(s),
            MalList l => new MalList(l.Select(i => Eval(i, env))),
            _ => ast
        };

        private MalType Read(string input) {
            return Reader.ReadStr(input);
        }

        private MalType Eval(MalType input, Env env) {
            if (input is MalList l) {
                if (l.Count == 0) {
                    return l;
                }
                if (l[0] is MalSymbol s) {
                    switch (s.Value) {
                        case "def!": {
                                MalSymbol key = l[1] as MalSymbol ?? throw new MalError("Can only define symbols");
                                MalType value = Eval(l[2], env);
                                env.Set(key, value);
                                return value;
                            }
                        case "let*": {
                                Env c = new Env(env);
                                MalList bindings = l[1] as MalList ?? throw new MalError("First argument to let must be list");
                                if (bindings.Count % 2 != 0) {
                                    throw new MalError("Bindings must come in pairs");
                                }
                                for (int i = 0 ; i < bindings.Count ; i += 2) {
                                    MalSymbol key = bindings[i] as MalSymbol ?? throw new MalError("Can only bind to symbols");
                                    MalType value = Eval(bindings[i + 1], c);
                                    c.Set(key, value);
                                }
                                return Eval(l[2], c);
                            }
                    }
                }
                MalList evald = Eval_Ast(l, env) as MalList ?? throw new MalError("Eval_Ast didn't return MalList");
                MalFunction fn = evald[0] as MalFunction ?? throw new MalError("Was expecting a function");
                return fn.Eval(evald.Skip(1).ToArray());
            } else {
                return Eval_Ast(input, env);
            }

        }

        private string Print(MalType input) {
            return Printer.PrStr(input);
        }

        private string Rep(string input) {
            return Print(Eval(Read(input), _outer));
        }

        static void Main(string[] args) {
            Program p = new Program();
            p.Init();
            while (true) {
                Console.Write("user> ");
                string? input = Console.ReadLine();
                if (input == null) {
                    break;
                }
                try {
                    string output = p.Rep(input);
                    Console.WriteLine(output);
                } catch (MalError ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
