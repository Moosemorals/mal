

using System;

namespace Mal {

    public class MalError : Exception {
        public MalError(string? message) : base(message) {
        }
    }

}
