

using System;
using System.Collections.Generic;
using System.Text;

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
                return FormatString(str.Value, readably);
            } else if (item is MalSymbol sym) {
                return sym.Value;
            } else if (item is MalFuncHolder func) {
                return "#<function>";
            } else {
                throw new Exception($"Unexpected MalType {item.GetType().Name}");
            }
        }

        private static string FormatString(string str, bool readably) {
            if (readably) {
                StringBuilder output = new StringBuilder(str.Length);
                output.Append('"');
                foreach (char c in str) {
                    switch (c) {
                        case '\\':
                            output.Append("\\\\");
                            break;
                        case '\n':
                            output.Append("\\n");
                            break;
                        case '"':
                            output.Append("\\\"");
                            break;
                        default:
                            output.Append(c);
                            break;
                    }
                }
                output.Append('"');
                return output.ToString();
            } else {
                return str;
            }
        }

        public static string FormatException(Exception ex) {
            StringBuilder output = new StringBuilder();
            List<Exception> exceptions = new();

            Exception? inner = ex;
            while (inner != null) {
                exceptions.Add(inner);
                inner = ex.InnerException;
            }

            exceptions.Reverse();

            for (int i = 0; i < exceptions.Count; i += 1) {
                Exception e = exceptions[i];
                if (i > 0) {
                    output.AppendLine("Wrapped by");
                }
                output.AppendLine($"{e.GetType().FullName}: {ex.Message}");
                output.AppendLine(e.StackTrace);
            }
            return output.ToString();
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
            return $"[{string.Join(" ", strings)}]";
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