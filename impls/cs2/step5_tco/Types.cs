
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mal {

    internal abstract class MalValue { }

    internal abstract class MalSequence : MalValue, IList<MalValue> {

        private readonly List<MalValue> _backingList = new();

        public MalSequence() => _backingList = new();
        public MalSequence(IEnumerable<MalValue> items) => _backingList = items.ToList();

        IEnumerator IEnumerable.GetEnumerator() => _backingList.GetEnumerator();
        public IEnumerator<MalValue> GetEnumerator() => _backingList.GetEnumerator();
        public MalValue this[int index] { get => _backingList[index]; set => _backingList[index] = value; }
        public bool Contains(MalValue item) => _backingList.Contains(item);
        public bool IsReadOnly => false;
        public bool Remove(MalValue item) => _backingList.Remove(item);
        public int Count => _backingList.Count;
        public int IndexOf(MalValue item) => _backingList.IndexOf(item);
        public void Add(MalValue item) => _backingList.Add(item);
        public void Clear() => _backingList.Clear();
        public void CopyTo(MalValue[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);
        public void Insert(int index, MalValue item) => _backingList.Insert(index, item);
        public void RemoveAt(int index) => _backingList.RemoveAt(index);
    }

    internal class MalList : MalSequence {

        public MalList() : base() {}
        public MalList(IEnumerable<MalValue> items) : base(items) { }
    }

    internal class MalVector: MalSequence {

        public MalVector() : base() { }

        public MalVector(IEnumerable<MalValue> items) : base(items) {}
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
        private readonly Func<Program, MalValue[], MalValue> _fn;
        public MalForeignFunction(Func<Program, MalValue[], MalValue> fn)
            => _fn = fn;
        public override MalValue Eval(Program prog, MalValue[] args)
            => _fn(prog, args);
    }

    internal class MalNativeFunction : MalFunction {
        internal Env Env {get;init;}
        internal MalSymbol[] Param {get; init;}
        internal MalValue Body {get; init;}

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
}
