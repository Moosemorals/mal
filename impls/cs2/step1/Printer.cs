

using System;
using System.Collections.Generic;

namespace uk.osric.mal {

    public class Printer {

        public static string PrStr(MalType item) {
            if (item is MalList l) {
                return FormatList(l, '(', ')');
            } else if (item is MalVector v) {
                return FormatList(v, '[', ']');
            } else if (item is MalHash h) {
                return FormatList(h, '{', '}');
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
            for (int i = 0; i < list.Count; i += 1) {
                strings.Add(PrStr(list[i]));
            }
            return $"{left}{string.Join(" ", strings)}{right}";
        }
    }
}