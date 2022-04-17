
using System;
using System.Collections.Generic;
using System.Linq;

namespace uk.osric.mal {

    internal class Env {

        private readonly Env? outer;

        private readonly Dictionary<MalSymbol, IMalType> data = new();

        public Env(Env? outer) : this(outer, null, null) { }

        public Env(Env? outer, IEnumerable<IMalType>? binds, IEnumerable<IMalType>? exprs) {
            this.outer = outer;

            if (binds != null && exprs != null) {
                var b = binds.GetEnumerator();
                var e = exprs.GetEnumerator();

                while (b.MoveNext()) {
                    var key = (MalSymbol)b.Current;
                    if (key.Value == "&") {
                        b.MoveNext();
                        key = (MalSymbol)b.Current;
                        data[key] = new MalList(e);
                        return;
                    }
                    e.MoveNext();
                    var value = e.Current;
                    data[key] = value;
                }
            }
        }

        public IMalType Set(MalSymbol key, IMalType value) {
            data[key] = value;
            return value;
        }

        public Env? Find(MalSymbol key) {
            if (data.ContainsKey(key)) {
                return this;
            } else if (outer != null) {
                return outer.Find(key);
            } else {
                return null;
            }
        }

        public IMalType Get(MalSymbol key) {
            if (key.Value.StartsWith(':')) {
                return key;
            }
            Env? e = Find(key);
            if (e != null) {
                return e.data[key];
            }
            throw new Exception($"Key {key} not found in evironment");
        }
    }
}