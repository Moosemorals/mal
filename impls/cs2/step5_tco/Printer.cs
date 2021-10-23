
using System.Linq;
using System.Text;

namespace Mal {

    internal class Printer {

        public static string PrStr(MalValue o, bool printReadably) {
            string output = o switch {
                MalBool b => b.Value ? "true" : "false",
                MalFunction f => "#<function>",
                MalList l => '(' + string.Join(" ", l.Select(i => PrStr(i, printReadably))) + ')',
                MalNil => "nil",
                MalNumber n => n.Value.ToString(),
                MalString str => printReadably ? Interpolate(str.Value) : str.Value,
                MalSymbol s => s.Value,
                MalVector v => '[' + string.Join(" ", v.Select(i => PrStr(i, printReadably))) + ']',
                _ => throw new MalError($"Unknown mal type {o.GetType().Name}")
            };
            return output;
        }

        private static string Interpolate(string raw) {
            StringBuilder result = new();

            result.Append('"');
            for (int i = 0 ; i < raw.Length ; i += 1) {
                char c = raw[i];
                switch (c) {
                    case '\n':
                        result.Append("\\n");
                        break;
                    case '"':
                        result.Append("\\\"");
                        break;
                    case '\\':
                        result.Append("\\\\");
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }
            result.Append('"');
            return result.ToString();
        }
    }



}
