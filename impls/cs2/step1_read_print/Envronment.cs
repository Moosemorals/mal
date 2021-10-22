using System;
using System.Collections.Generic;

namespace Mal {

    internal class Env {

        private readonly Dictionary<string, Func<int, int, int>> _data = new();
        private readonly Env? _outer;

        public Env(Env? outer) {
            _outer = outer;
        }

        public



    }

}
