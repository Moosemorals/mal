
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

    public delegate IMalType MalFunc(params IMalType[] args);

    public class MalSeq : List<IMalType>, IMalType {
        public MalSeq() : base() { }
        public MalSeq(IEnumerable<IMalType> list) : base(list) { }
    }

    public class MalList : MalSeq {
        public MalList() : base() { }

        public MalList(IEnumerable<IMalType> list) : base(list) { }

        public MalList Rest() => new MalList(this.Skip(1));
    }

    public class MalVector : MalSeq { }

    public class MalHash : Dictionary<IMalType, IMalType>, IMalType { }

    public class MalSymbol : IMalType {
        public string Value { get; private set; }
        public MalSymbol(string Value) => this.Value = Value;
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

        public IMalType Apply(params IMalType[] args) => Value(args);

    }

    public class MalTrue : IMalType { }
    public class MalFalse : IMalType { }
    public class MalNil : IMalType { }
}