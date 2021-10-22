
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
    }

    public class MalNumber : MalAtom<double> {
        public MalNumber(double value) : base(value) { }
    }

    public class MalSymbol : MalAtom<string> {
        public MalSymbol(string value) : base(value) { }
    }

    public class MalFunction : MalType {

        private readonly Func<double, double, double> _fn;
        public MalSymbol Name {get; init;}

        public MalFunction(MalSymbol name, Func<double, double, double> fn ) {
            Name = name;
            _fn = fn;
        }

        public MalType Eval(MalList args) {
            MalNumber a = args[0] as MalNumber ?? throw new MalError("Argument isn't a number");
            MalNumber b = args[1] as MalNumber ?? throw new MalError("Argument isn't a number");

            return new MalNumber(_fn(a.Value, b.Value));
        }
    }
}
