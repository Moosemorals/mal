
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mal {

    internal abstract class MalValue { }

    internal abstract class MalSequence : MalValue, IEnumerable<MalValue> {
        public abstract int Count { get; }
        public abstract void Add(MalValue item);
        public abstract void CopyTo(MalValue[] array, int arrayIndex);

        public abstract MalSequence Rest();

        public MalValue[] ToArray() {
            MalValue[] array = new MalValue[Count];
            CopyTo(array, 0);
            return array;
        }

        public abstract IEnumerator<MalValue> GetEnumerator();

        public abstract MalValue this[int index] { get; }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }


    internal class MalList : MalSequence {

        private readonly LinkedList<MalValue> _backingList;
        public MalList() : base() {
            _backingList = new();
        }

        public MalList(IEnumerable<MalValue> items) : this() {
            if (items is MalList l) {
                _backingList = l._backingList;
            } else {
                foreach (MalValue v in items) {
                    _backingList.AddLast(v);
                }
            }
        }

        public override IEnumerator<MalValue> GetEnumerator() => _backingList.GetEnumerator();
        public override int Count => _backingList.Count;

        public override MalValue this[int index] => _backingList.ElementAt(index);

        public override MalList Rest() => new(_backingList.Skip(1));

        public override void Add(MalValue item) => _backingList.AddLast(item);
        public override void CopyTo(MalValue[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);
    }

    internal class MalVector : MalSequence {

        private readonly List<MalValue> _backingList;

        public MalVector() : base() {
            _backingList = new();
        }

        public MalVector(IEnumerable<MalValue> items) : this() {
            if (items is MalVector v) {
                _backingList = v._backingList;
            } else {
                _backingList.AddRange(items);
            }
        }

        public override MalVector Rest() => new( _backingList.Skip(1));

        public override IEnumerator<MalValue> GetEnumerator() => _backingList.GetEnumerator();
        public override int Count => _backingList.Count;

        public override MalValue this[int index] => _backingList[index];

        public override void Add(MalValue item) => _backingList.Add(item);
        public override void CopyTo(MalValue[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);

    }

    internal abstract class MalAtom<T> : MalValue {
        public MalAtom(T value) => Value = value;
        public T Value { get; set; }

        public override bool Equals(object? obj) {
            return obj is MalAtom<T> atom &&
                   EqualityComparer<T>.Default.Equals(Value, atom.Value);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Value);
        }
    }

    internal class MalNumber : MalAtom<double> {
        public MalNumber(double value) : base(value) { }
    }

    internal class MalSymbol : MalAtom<string> {
        public MalSymbol(string value) : base(value) { }
    }

    internal class MalString : MalAtom<string> {
        public MalString(string value) : base(value) { }
    }

    internal abstract class MalFunction : MalValue {
        public abstract MalValue Eval(Program prog, MalValue[] args);
    }

    internal class MalForeignFunction : MalFunction {
        private readonly Func<MalValue[], Program, MalValue> _fn;
        public MalForeignFunction(Func<MalValue[], Program, MalValue> fn)
            => _fn = fn;
        public override MalValue Eval(Program program, MalValue[] args)
            => _fn(args, program);
    }

    internal class MalNativeFunction : MalFunction {
        internal Env Env { get; init; }
        internal MalSymbol[] Param { get; init; }
        internal MalValue Body { get; init; }

        public MalNativeFunction(Env env, MalSymbol[] param, MalValue body) {
            Env = env;
            Param = param;
            Body = body;
        }

        public override MalValue Eval(Program prog, MalValue[] args) {
            Env local = new(Env, Param, args);
            return prog.Eval(Body, local);
        }
    }

    internal class MalBool : MalAtom<bool> {
        public static readonly MalBool True = new(true);
        public static readonly MalBool False = new(false);
        private MalBool(bool value) : base(value) { }
    }

    internal class MalNil : MalValue {
        public static readonly MalNil Nil = new();
        private MalNil() { }
    }

    internal class MalState : MalAtom<MalValue> {
        public MalState(MalValue value) : base(value) {
        }
    }
}
