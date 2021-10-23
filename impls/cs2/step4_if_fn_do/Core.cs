
using System;
using System.Linq;

namespace Mal {


    internal static class Core {

        public static void Init(Env env) {

            // Basic maths
            env.Set(new MalSymbol("+"), MathFunctionBuilder((a, b) => a + b));
            env.Set(new MalSymbol("-"), MathFunctionBuilder((a, b) => a - b));
            env.Set(new MalSymbol("*"), MathFunctionBuilder((a, b) => a * b));
            env.Set(new MalSymbol("/"), MathFunctionBuilder((a, b) => (int) (a / b)));
            env.Set(new MalSymbol("<"), CompFunctionBuilder((a, b) => a < b));
            env.Set(new MalSymbol("<="), CompFunctionBuilder((a, b) => a <= b));
            env.Set(new MalSymbol(">"), CompFunctionBuilder((a, b) => a > b));
            env.Set(new MalSymbol(">="), CompFunctionBuilder((a, b) => a >= b));

            // Print
            env.Set(new MalSymbol("pr-str"), PrintFunctionBuilder(readable: true, sep: " ", toScreen: false));
            env.Set(new MalSymbol("str"), PrintFunctionBuilder(readable: false, sep: "", toScreen: false));
            env.Set(new MalSymbol("prn"), PrintFunctionBuilder(readable: true, sep: " ", toScreen: true));
            env.Set(new MalSymbol("println"), PrintFunctionBuilder(readable: false, sep: " ", toScreen: true));

            // Tools
            env.Set(new MalSymbol("list"), List);
            env.Set(new MalSymbol("list?"), IsList);
            env.Set(new MalSymbol("empty?"), IsEmpty);
            env.Set(new MalSymbol("count"), Count);
            env.Set(new MalSymbol("="), AreEqual);
        }

        private static MalForeignFunction List => new((prog, args) => {
            return new MalList(args);
        });

        private static MalForeignFunction IsList => new((prog, args) =>
           args[0] is MalList ? MalBool.True : MalBool.False
        );

        private static MalForeignFunction IsEmpty => new((prog, args) => {
            if (args[0] is MalSequence l) {
                return l.Count == 0 ? MalBool.True : MalBool.False;
            } else {
                throw new MalError("Expecting sequence");
            }
        });

        private static MalForeignFunction Count => new((prog, args) => {
            if (args[0] is MalSequence l) {
                return new MalNumber(l.Count);
            } else {
                return new MalNumber(0);
            }
        });

        private static MalForeignFunction AreEqual => new((prog, args) => {
            MalValue a = args[0];
            MalValue b = args[1];

            return _AreEqual(a, b) ? MalBool.True : MalBool.False;
        });

        private static bool _AreEqual(MalValue a, MalValue b) {
            if (a is MalSequence A && b is MalSequence B) {
                if (A.Count != B.Count) {
                    return false;
                }

                for (int i = 0 ; i < A.Count ; i += 1) {
                    if (!_AreEqual(A[i], B[i])) {
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
            new((ignored, args) => {
                string output = string.Join(sep, args.Select(a => Printer.PrStr(a, readable)));
                if (toScreen) {
                    Console.WriteLine(output);
                    return MalNil.Nil;
                }
                return new MalString(output);
            });

        private static MalFunction MathFunctionBuilder(Func<double, double, double> fn) {
            return new MalForeignFunction(
                (ignored, args) => {
                    MalNumber a = args[0] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    MalNumber b = args[1] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    return new MalNumber(fn(a.Value, b.Value));
                }
            );
        }
        private static MalFunction CompFunctionBuilder(Func<double, double, bool> fn) {
            return new MalForeignFunction(
                (ignored, args) => {
                    MalNumber a = args[0] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    MalNumber b = args[1] as MalNumber ?? throw new MalError("Can only operate on numbers");
                    return fn(a.Value, b.Value) ? MalBool.True : MalBool.False;
                }
            );
        }
    }

}
