

using System;
using System.Collections.Generic;

namespace uk.osric.mal {

    public class Printer {

        public static string PrStr(IMalType item, bool readably) {
            if (item is MalList l) {
                return FormatList(l, readably);
            } else if (item is MalVector v) {
                return FormatVector(v, readably);
            } else if (item is MalHash h) {
                return FormatHash(h, readably);
            } else if (item is MalTrue) {
                return "true";
            } else if (item is MalFalse) {
                return "false";
            } else if (item is MalNil) {
                return "nil";
            } else if (item is MalNumber n) {
                return n.Value.ToString();
            } else if (item is MalString str) {
                return '"' + FormatString(str.Value, readably) + '"';
            } else if (item is MalSymbol sym) {
                return sym.Value;
            } else {
                throw new Exception($"Unexpected MalType {item.GetType().Name}");
            }
        }

        private static string FormatString(string str, bool readably) {
            if (readably) {
                string output = "";
                foreach (char c in str) {
                    switch (c) {
                        case '\\':
                            output += "\\\\";
                            break;
                        case '\n':
                            output += "\\n";
                            break;
                        case '"':
                            output += "\\\"";
                            break;
                        default:
                            output += c;
                            break;
                    }
                }
                return output;
            } else {
                return str;
            }
        }

        private static string FormatList(MalList list, bool readably) {
            List<string> strings = new();
            foreach (IMalType m in list) {
                strings.Add(PrStr(m, readably));
            }
            return $"({string.Join(" ", strings)})";
        }
        private static string FormatVector(MalVector vector, bool readably) {
            List<string> strings = new();
            foreach (IMalType m in vector) {
                strings.Add(PrStr(m, readably));
            }
            return $"({string.Join(" ", strings)})";
        }
        private static string FormatHash(MalHash hash, bool readably) {
            List<string> strings = new();
            foreach (var kv in hash) {
                strings.Add(PrStr(kv.Key, readably));
                strings.Add(PrStr(kv.Value, readably));
            }
            return $"{{{string.Join(" ", strings)}}}";

        }
    }
}