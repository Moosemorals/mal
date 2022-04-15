
using System;
using System.Collections;
using System.Collections.Generic;

namespace uk.osric.mal
{
    public abstract class MalType
    {
        public static readonly MalTrue True = new MalTrue();
        public static readonly MalFalse False = new MalFalse();
        public static readonly MalNil Nil = new MalNil();
    }

    public class MalList : MalType
    {
        private readonly List<MalType> backingList = new();

        public int Count => backingList.Count;

        public MalType this[int index]
        {
            get => backingList[index];
            set => backingList[index] = value;
        }
        public void Add(MalType item) => backingList.Add(item);
    }

    public class MalSymbol : MalType
    {
        public string Value { get; private set; }
        public MalSymbol(string Value) => this.Value = Value;
    }

    public class MalString : MalType
    {
        public string Value { get; private set; }
        public MalString(string Value) => this.Value = Value;
    }

    public class MalNumber : MalType
    {
        public double Value { get; private set; }
        public MalNumber(double Value) => this.Value = Value;

        public MalNumber(string raw)
        {

            if (double.TryParse(raw, out double d))
            {
                Value = d;
            }
            else
            {
                throw new ArgumentException("Not a number", nameof(raw));
            }
        }
    }

    public class MalTrue : MalType { }
    public class MalFalse : MalType { }
    public class MalNil : MalType { }
}