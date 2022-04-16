
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace uk.osric.mal {
    public interface IMalType {
        public static readonly MalTrue True = new MalTrue();
        public static readonly MalFalse False = new MalFalse();
        public static readonly MalNil Nil = new MalNil();
    }

    public delegate IMalType MalFunc(MalList args);

    public interface IMalSeq : IEnumerable<IMalType>, IMalType { }

    public class MalList : IMalSeq {
        private class Cell {
            public IMalType First { get; private init; }
            public Cell? Next { get; set; }

            public Cell(IMalType First, Cell? Next) {
                this.First = First;
                this.Next = Next;
            }
        }

        private Cell? cons;

        private MalList(Cell? cons) {
            this.cons = cons;
        }

        private MalList() { cons = null; }

        internal MalList(IEnumerable<IMalType> list) {
            Cell? x = null;
            foreach (IMalType m in list) {
                if (x == null) {
                    cons = new Cell(m, null);
                    x = cons;
                } else {
                    x.Next = new Cell(m, null);
                    x = x.Next;
                }
            }
        }

        public static MalList Empty() => new MalList();

        public IMalType Head => cons?.First ?? IMalType.Nil;

        public MalList Tail => new MalList(cons?.Next);

        public MalList Cons(IMalType item) => new MalList(new Cell(item, cons));

        public IEnumerator<IMalType> GetEnumerator() {
            Cell? head = cons;
            while (head != null) {
                yield return head.First;
                head = head.Next;
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool IsEmpty => cons == null;
    }

    public class MalVector : List<IMalType>, IMalSeq { }

    public class MalHash : Dictionary<IMalType, IMalType>, IMalType { }

    public class MalSymbol : IEquatable<MalSymbol>, IMalType {
        public string Value { get; private set; }
        public MalSymbol(string Value) => this.Value = Value;

        public override bool Equals(object? obj) => this.Equals(obj as MalSymbol);

        public override int GetHashCode() => Value.GetHashCode();

        public override string? ToString() => Value;

        public bool Equals(MalSymbol? other) => other != null && other.Value.Equals(Value);

        public static bool operator ==(MalSymbol? left, MalSymbol? right) {
            if (left is null) {
                if (right is null) {
                    return true;
                }
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(MalSymbol? left, MalSymbol? right) => !(left == right);
    }

    public class MalString : IMalType {
        public string Value { get; private set; }
        public MalString(string Value) => this.Value = Value;
    }

    public class MalNumber : IMalType {
        public double Value { get; private set; }
        public MalNumber(double Value) => this.Value = Value;

        public MalNumber(string raw) {

            if (double.TryParse(raw, out double d)) {
                Value = d;
            } else {
                throw new ArgumentException("Not a number", nameof(raw));
            }
        }
    }

    public class MalFuncHolder : IMalType {
        public MalFunc Value { get; private set; }
        public MalFuncHolder(MalFunc Value) => this.Value = Value;

        public IMalType Apply(MalList args) {
            return Value(args);
        }

    }

    public class MalTrue : IMalType { }
    public class MalFalse : IMalType { }
    public class MalNil : IMalType { }
}