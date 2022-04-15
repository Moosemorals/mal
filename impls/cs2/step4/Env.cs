
using System;
using System.Collections.Generic;

namespace uk.osric.mal {

    internal class Env {

        private readonly Env? outer;

        private readonly Dictionary<string, IMalType> data = new();

        public Env(Env? outer) {
            this.outer = outer;
        }

        public IMalType Set(string key, IMalType value) {
            data[key] = value;
            return value;
        }

        public Env? Find(string key) {
            if (data.ContainsKey(key)) {
                return this;
            } else if (outer != null) {
                return outer.Find(key);
            } else {
                return null;
            }
        }

        public IMalType Get(string key) {
            Env? e = Find(key);
            if (e != null) {
                return e.data[key];
            }
            throw new Exception($"Key {key} not found in evironment");
        }
    }
}