
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mal {


    internal static class Core {
        private static void Add(Env env, string name, MalValue fn) {
            env.Set(new MalSymbol(name), fn);
            }

        public static void Init(Env env, IEnumerable<string> argv) {

            // Basic maths
            Add(env, "+", MathFunctionBuilder((a, b) => a + b));
            Add(env, "-", MathFunctionBuilder((a, b) => a - b));
            Add(env, "*", MathFunctionBuilder((a, b) => a * b));
            Add(env, "/", MathFunctionBuilder((a, b) => (int) (a / b)));
            Add(env, "<", CompFunctionBuilder((a, b) => a < b));
            Add(env, "<=", CompFunctionBuilder((a, b) => a <= b));
            Add(env, "=", AreEqual);
            Add(env, ">", CompFunctionBuilder((a, b) => a > b));
            Add(env, ">=", CompFunctionBuilder((a, b) => a >= b));

            // Print
            Add(env, "pr-str", PrintFunctionBuilder(readable: true, sep: " ", toScreen: false));
            Add(env, "str", PrintFunctionBuilder(readable: false, sep: "", toScreen: false));
            Add(env, "prn", PrintFunctionBuilder(readable: true, sep: " ", toScreen: true));
            Add(env, "println", PrintFunctionBuilder(readable: false, sep: " ", toScreen: true));

            // Tools
            Add(env, "list", List);
            Add(env, "list?", IsList);
            Add(env, "empty?", IsEmpty);
            Add(env, "count", Count);
            Add(env, "eval", Eval(env));
            Add(env, "read-string", ReadString);
            Add(env, "slurp", Slurp);

            Add(env, "atom", Atom);
            Add(env, "atom?", IsAtom);
            Add(env, "deref", Deref);
            Add(env, "reset!", Reset);
            Add(env, "swap!", Swap);

            Add(env, "*ARGV*", new MalList(argv.Select(a => new MalString(a))));
        }

        private static MalForeignFunction Atom => new((args, _) =>
           args.Length > 0 ? new MalState(args[0]) : new MalState(MalNil.Nil)
        );

        private static MalForeignFunction IsAtom => new((args, _) =>
           args.Length > 0 && args[0] is MalState ? MalBool.True : MalBool.False
        );

        private static MalForeignFunction Deref => new ((args, _) =>
            args.Length > 0 && args[0] is MalState s ? s.Value : MalNil.Nil
        );

        private static MalForeignFunction Reset => new ((args, _) => {
            if (args.Length > 1 && args[0] is MalState s) {
                s.Value = args[1];
                return args[1];
            }
            return MalNil.Nil;
        });

        private static MalForeignFunction Swap => new ((args, prog) => {
            if (args.Length > 1 && args[0] is MalState s && args[1] is MalFunction f) {
                List<MalValue> fnArgs = new(args.Length > 2 ? args.Skip(2) : Array.Empty<MalValue>());
                fnArgs.Insert(0, s.Value);

                s.Value = f.Eval(prog, fnArgs.ToArray());
                return s.Value;
            }
            return MalNil.Nil;
        });

        private static MalForeignFunction Eval(Env env) => new((args, program) =>
           args.Length > 0 ? program.Eval(args[0], env) : MalNil.Nil
        );

        private static MalForeignFunction List => new((args, _) => {
            return new MalList(args);
        });

        private static MalForeignFunction IsList => new((args, _) =>
           args[0] is MalList ? MalBool.True : MalBool.False
        );

        private static MalForeignFunction IsEmpty => new((args, _) => {
            if (args[0] is MalSequence l) {
                return l.Count == 0 ? MalBool.True : MalBool.False;
            } else {
                throw new MalError("Expecting sequence");
            }
        });

        private static MalForeignFunction Count => new((args, _) => {
            if (args[0] is MalSequence l) {
                return new MalNumber(l.Count);
            } else {
                return new MalNumber(0);
            }
        });

        private static MalForeignFunction ReadString => new((args, _) => {
            if (args.Length > 0 && args[0] is MalString str) {
                return Reader.ReadStr(str.Value);
            }
            return MalNil.Nil;
        });

        private static MalForeignFunction Slurp => new((args, _) => {
            if (args.Length > 0 && args[0] is MalString str) {
                string path = str.Value;
                try {
                    return new MalString(File.ReadAllText(path));
                } catch (Exception ex) {
                    throw new MalError($"Can't read file at {path}: {ex.Message}", ex);
                }
            }
            return MalNil.Nil;
        });

        private static MalForeignFunction AreEqual => new((args, _) => {
            MalValue a = args[0];
            MalValue b = args[1];

            return AreEqualImpl(a, b) ? MalBool.True : MalBool.False;
        });

        private static bool AreEqualImpl(MalValue a, MalValue b) {
            if (a is MalSequence A && b is MalSequence B) {
                if (A.Count != B.Count) {
                    return false;
                }

                for (int i = 0 ; i < A.Count ; i += 1) {
                    if (!AreEqualImpl(A[i], B[i])) {
                        return false;
                    }
                }
                return true;
            }

            if (a.GetType() != b.GetType()) {
                return false;
            }

            return a.Equals(b);
        }

        private static MalForeignFunction PrintFunctionBuilder(string sep, bool readable, bool toScreen) =>
            new((args, _) => {
                string output = string.Join(sep, args.Select(a => Printer.PrStr(a, readable)));
                if (toScreen) {
                    Console.WriteLine(output);
                    return MalNil.Nil;
                }
                return new MalString(output);
            });

        private static MalFunction MathFunctionBuilder(Func<double, double, double> fn) {
            return new MalForeignFunction(
                (args, _) => {
                    MalNumber a = args[0] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    MalNumber b = args[1] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    return new MalNumber(fn(a.Value, b.Value));
                }
            );
        }
        private static MalFunction CompFunctionBuilder(Func<double, double, bool> fn) {
            return new MalForeignFunction(
                (args, _) => {
                    MalNumber a = args[0] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    MalNumber b = args[1] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    return fn(a.Value, b.Value) ? MalBool.True : MalBool.False;
                }
            );
        }
    }

}
