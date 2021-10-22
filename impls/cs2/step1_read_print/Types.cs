
using System.Collections.Generic;

namespace Mal {

    public abstract class MalType { }

    public class MalList : MalType {
        private readonly List<MalType> _backingList = new();

        public void Add(MalType o) => _backingList.Add(o);

        public int Length => _backingList.Count;

        public MalType this[int index] => _backingList[index];

        public IEnumerable<MalType> Values => _backingList;

        public override string? ToString() => Printer.PrStr(this);
    }

    public abstract class MalAtom<T> : MalType {
        public MalAtom(T value) => Value = value;
        public T Value {get;set;}
    }

    public class MalNumber : MalAtom<double> {}
}
