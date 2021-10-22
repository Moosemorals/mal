
using System.Linq;

namespace Mal {

    internal class Printer {

        public static string PrStr(MalType o) => o switch {
            MalList l => $"({string.Join(" ", l.Values.Select(v => PrStr(v)))})",
            MalNumber n => n.Value.ToString(),
            MalSymbol s => s.Value,
            MalFunction f => $"#{f.Name}#",
            _ => throw new MalError($"Unknown mal type {o.GetType().Name}")
        };

    }

}
