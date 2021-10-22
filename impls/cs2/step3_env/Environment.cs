
using System.Collections.Generic;

namespace Mal {


    public class Env {

        private readonly Env? _outer;

        private readonly Dictionary<MalSymbol, MalType> _data = new();

        public Env(Env? outer) {
            _outer = outer;
        }

        public void Set(MalSymbol key, MalType value) {
            if (!_data.ContainsKey(key)) {
                _data.Add(key, value);
            } else {
                _data[key] = value;
            }
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

        public MalType Get(MalSymbol key) {

            Env? env = Find(key);
            if (env != null) {
                return env._data[key];
            }
            throw new MalError($"Key {key.Value} not found");
        }

    }

}
