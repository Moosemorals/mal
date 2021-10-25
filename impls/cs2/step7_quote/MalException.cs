

using System;

namespace Mal {

    public class MalError : Exception {


        public MalError(string? message) : base(message) { }

        public MalError(string? message, Exception? innerException) : base(message, innerException) { }
    }

}
