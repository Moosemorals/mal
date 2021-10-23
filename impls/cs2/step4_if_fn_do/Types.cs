
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mal {

    public abstract class MalType { }

    public class MalList : MalType, IEnumerable<MalType> {

        public MalList() {
            _backingList = new();
        }

        public MalList(IEnumerable<MalType> items) {
            _backingList = items.ToList();
        }

        private readonly List<MalType> _backingList;

        public void Add(MalType o) => _backingList.Add(o);

        public int Count => _backingList.Count;

        public MalType this[int index] => _backingList[index];

        public IEnumerable<MalType> Values => _backingList;

        public override string? ToString() => Printer.PrStr(this);

        public IEnumerator<MalType> GetEnumerator() {
            return ((IEnumerable<MalType>) _backingList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) _backingList).GetEnumerator();
        }
    }

    public abstract class MalAtom<T> : MalType {
        public MalAtom(T value) => Value = value;
        public T Value {get;set;}

        public override bool Equals(object? obj) {
            return obj is MalAtom<T> atom &&
                   EqualityComparer<T>.Default.Equals(Value, atom.Value);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Value);
        }
    }

    public class MalNumber : MalAtom<double> {
        public MalNumber(double value) : base(value) { }
    }

    public class MalSymbol : MalAtom<string> {
        public MalSymbol(string value) : base(value) { }
    }

    public class MalFunction : MalType {

        private readonly Func<MalType[], MalType> _fn;
        public MalSymbol Name {get; init;}

        public MalFunction(MalSymbol name, Func<MalType[], MalType> fn ) {
            Name = name;
            _fn = fn;
        }

        public MalType Eval(MalType[] args) {
            return _fn(args);
        }
    }
}
