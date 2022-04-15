

using System;
using System.Collections.Generic;

namespace uk.osric.mal {

    public class Printer {

        public static string PrStr(MalType item) {
            if (item is MalList l) {
                List<string> strings = new ();
                for (int i =0 ; i < l.Count; i += 1) {
                    strings.Add(PrStr(l[i]));
                }
                return $"({string.Join(" ", strings)})";
            } else if (item is MalTrue) {
                return "true";
            } else if (item is MalFalse) {
                return "false";
            } else if (item is MalNil) {
                return "nil";
            } else if (item is MalNumber n) {
                return n.Value.ToString();
            } else if (item is MalString str) {
                return str.Value;
            } else if (item is MalSymbol sym) {
                return sym.Value;
            } else {
                throw new Exception($"Unexpected MalType {item.GetType().Name}");
            }
        }
    }
}