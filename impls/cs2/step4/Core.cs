

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace uk.osric.mal {


    public static class Core {

        private static bool AreEqual(IMalType left, IMalType right) => left.Equals(right);

        public static Dictionary<MalSymbol, MalFunc> NS = new() {
            { new MalSymbol("*"), l => new MalNumber( ((MalNumber)l.Head).Value * ((MalNumber)l.Tail.Head).Value) },
            { new MalSymbol("+"), l => new MalNumber( ((MalNumber)l.Head).Value + ((MalNumber)l.Tail.Head).Value) },
            { new MalSymbol("-"), l => new MalNumber( ((MalNumber)l.Head).Value - ((MalNumber)l.Tail.Head).Value) },
            { new MalSymbol("/"), l => new MalNumber( Math.Floor(((MalNumber)l.Head).Value / ((MalNumber)l.Tail.Head).Value)) },
            { new MalSymbol("<"), l => ((MalNumber)l.Head).Value < ((MalNumber)l.Tail.Head).Value ? IMalType.True : IMalType.False },
            { new MalSymbol("<="), l => ((MalNumber)l.Head).Value <= ((MalNumber)l.Tail.Head).Value ? IMalType.True : IMalType.False },
            { new MalSymbol("="), l => AreEqual(l.Head, l.Tail.Head) ? IMalType.True : IMalType.False },
            { new MalSymbol(">"), l => ((MalNumber)l.Head).Value > ((MalNumber)l.Tail.Head).Value ? IMalType.True : IMalType.False },
            { new MalSymbol(">="), l => ((MalNumber)l.Head).Value >= ((MalNumber)l.Tail.Head).Value ? IMalType.True : IMalType.False },
            { new MalSymbol("count"), l => new MalNumber( l.Head is MalNil ? 0 : ((IMalSeq)l.Head).Count()) },
            { new MalSymbol("empty?"), l => ((IMalSeq)l.Head).Any() ? IMalType.False : IMalType.True },
            { new MalSymbol("list"), l => new MalList(l) },
            { new MalSymbol("list?"), l => l.Head is MalList ? IMalType.True : IMalType.False },
            { new MalSymbol("pr-str"), l => new MalString(string.Join(" ", l.Select(m => Printer.PrStr(m, true)))) },
            { new MalSymbol("str"), l => new MalString(string.Join("", l.Select(m => Printer.PrStr(m, false)))) },
            { new MalSymbol("prn"), l => {
                Console.Out.WriteLine(string.Join(" ", l.Select(m => Printer.PrStr(m, true))));
                return IMalType.Nil;
            }},
            { new MalSymbol("println"), l => {
                Console.Out.WriteLine(string.Join(" ", l.Select(m => Printer.PrStr(m, false))));
                return IMalType.Nil;
            }},
        };

        public static List<string> Mal = new() {
            "(def! not (fn* (a) (if a false true)))",
        };
    }

}