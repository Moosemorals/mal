using System;
using System.Collections.Generic;
using System.Linq;

namespace Mal {
    class Program {

        private readonly Dictionary<string, Func<double, double, double>> _env = new() {
            { "+", (a, b) => a + b },
            { "-", (a, b) => a - b },
            { "*", (a, b) => a * b },
            { "/", (a, b) => (int) (a / b) },
        };


        private MalType Eval_Ast(MalType ast, Dictionary<string, Func<double, double, double>> env) => ast switch {
            MalSymbol s => env.ContainsKey(s.Value) ? new MalFunction(s, env[s.Value]) : throw new MalError("Unknown symbol"),
            MalList l => new MalList(l.Select(i => Eval(i, env))),
            _ => ast
        };


        private MalType Read(string input) {
            return Reader.ReadStr(input);
        }

        private MalType Eval(MalType input, Dictionary<string, Func<double, double, double>> env) {
            if (input is MalList l) {
                if (l.Count == 0) {
                    return l;
                }
                MalList evald = Eval_Ast(l, env) as MalList ?? throw new MalError("Eval_Ast didn't return MalList");
                MalFunction fn = evald[0] as MalFunction ?? throw new MalError("Was expecting a function");
                return fn.Eval(new MalList(evald.Skip(1)));
            } else {
                return Eval_Ast(input, env);
            }

        }

        private string Print(MalType input) {
            return Printer.PrStr(input);
        }

        private string Rep(string input) {
            return Print(Eval(Read(input), _env));
        }

        static void Main(string[] args) {
            Program p = new Program();
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
