
using System.Collections.Generic;

namespace Mal {

   internal class Env {

        private readonly Env? _outer;

        private readonly Dictionary<MalSymbol, MalValue> _data = new();

        public Env(Env? outer, MalSymbol[]? binds, MalValue[]? exprs) {
            _outer = outer;

            if (binds != null && exprs != null) {
                int i =0;
                while (i < binds.Length) {
                    MalSymbol b = binds[i];
                    if (b.Value == "&") {
                        if (i + 1 == binds.Length) {
                            throw new MalError("Missing bind name");
                        }
                        Set(binds[i+1], new MalList(exprs[i..]));
                        break;
                    }
                    Set(b, exprs[i]);
                    i += 1;
                }
           }
        }

        public MalValue Set(MalSymbol key, MalValue value) {
            if (!_data.ContainsKey(key)) {
                _data.Add(key, value);
            } else {
                _data[key] = value;
            }
            return value;
        }

        public Env? Find(MalSymbol key) {
            if (_data.ContainsKey(key)) {
                return this;
            } else if (_outer != null) {
                return _outer.Find(key);
            } else {
                return null;
            }
        }

        public MalValue Get(MalSymbol key) {

            Env? env = Find(key);
            if (env != null) {
                return env._data[key];
            }
            throw new MalError($"Key {key.Value} not found");
        }

    }

}
