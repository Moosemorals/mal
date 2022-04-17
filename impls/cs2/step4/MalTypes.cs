
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace uk.osric.mal {
    public interface IMalType {
        public static readonly MalTrue True = new MalTrue();
        public static readonly MalFalse False = new MalFalse();
        public static readonly MalNil Nil = new MalNil();
    }

    public delegate IMalType MalFunc(MalList args);
    public abstract class MalValueType : IMalType { }

    public abstract class MalValueType<T> : MalValueType, IEquatable<MalValueType<T>> where T : notnull {

        public T Value { get; protected set; }

        public MalValueType(T Value) => this.Value = Value;

        public bool Equals(MalValueType<T>? other) => other != null && Value.Equals(other.Value);

        public override bool Equals(object? obj) =>
        (obj != null && obj.GetType() == this.GetType())
         ? Equals((MalValueType<T>)obj)
          : false;

        public override int GetHashCode() => Value.GetHashCode();

        public override string? ToString() => Value.ToString();

        public static bool operator ==(MalValueType<T>? left, MalValueType<T>? right) {
            if (left is null) {
                if (right is null) {
                    return true;
                }
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(MalValueType<T>? left, MalValueType<T>? right) => !(left == right);
    }

    public class MalSymbol : MalValueType<string> {
        public MalSymbol(string Value) : base(Value) { }
    }

    public class MalString : MalValueType<string> {
        public MalString(string Value) : base(Value) { }
    }

    public class MalNumber : MalValueType<double> {
        public MalNumber(double Value) : base(Value) { }
        public static MalNumber Parse(string raw) {
            if (double.TryParse(raw, out double value)) {
                return new MalNumber(value);
            }
            throw new ArgumentException("Can't parse string value");
        }
    }


    public class MalList : IEnumerable<IMalType>, IEquatable<MalList>, IMalType {
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

        public bool Equals(MalList? other) => other != null && this.SequenceEqual(other);

        public override bool Equals(object? obj) => this.Equals(obj as MalList);

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                foreach (IMalType item in this) {
                    hash = hash * 23 + item.GetHashCode();
                }
                return hash;
            }
        }

        public override string? ToString() {
            return base.ToString();
        }

        public bool IsEmpty => cons == null;
    }

    public class MalVector : List<IMalType>, IEquatable<MalVector>, IMalType {

        public MalVector() : base() {}
        public MalVector(IEnumerable<IMalType> list) : base(list) {}


        public bool Equals(MalVector? other) => other != null && this.SequenceEqual(other);

        public override bool Equals(object? obj) => this.Equals(obj as MalVector);

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                foreach (IMalType item in this) {
                    hash = hash * 23 + item.GetHashCode();
                }
                return hash;
            }
        }
    }

    public class MalHash : Dictionary<IMalType, IMalType>, IMalType { }

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