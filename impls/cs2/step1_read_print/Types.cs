
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

    public class MalAtom : MalType {
        public MalAtom(object value) => Value = value;
        public object Value {get;set;}
    }


}
