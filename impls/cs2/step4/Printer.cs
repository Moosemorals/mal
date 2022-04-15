

using System;
using System.Collections.Generic;

namespace uk.osric.mal {

    public class Printer {

        public static string PrStr(IMalType item) {
            if (item is MalList l) {
                return FormatList(l, '(', ')');
            } else if (item is MalVector v) {
                return FormatList(v, '[', ']');
            } else if (item is MalHash h) {
                return FormatHash(h);
            } else if (item is MalTrue) {
                return "true";
            } else if (item is MalFalse) {
                return "false";
            } else if (item is MalNil) {
                return "nil";
            } else if (item is MalNumber n) {
                return n.Value.ToString();
            } else if (item is MalString str) {
                return '"' + str.Value + '"';
            } else if (item is MalSymbol sym) {
                return sym.Value;
            } else {
                throw new Exception($"Unexpected MalType {item.GetType().Name}");
            }
        }

        private static string FormatList(MalSeq list, char left, char right) {
            List<string> strings = new();
            foreach (IMalType m in list) {
                strings.Add(PrStr(m));
            }
            return $"{left}{string.Join(" ", strings)}{right}";
        }

        private static string FormatHash(MalHash hash) {
            List<string> strings = new();
            foreach (var kv in hash) {
                strings.Add(PrStr(kv.Key));
                strings.Add(PrStr(kv.Value));
            }
            return $"{{{string.Join(" ", strings)}}}";

        }
    }
}