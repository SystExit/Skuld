using System;
using System.Collections.Generic;
using System.Text;

namespace Skuld.Exceptions
{
    public class EmptyResponceException : Exception
    {
        public EmptyResponceException() : base()
        { }
        public EmptyResponceException(string message) : base(message)
        { }
        public EmptyResponceException(string message, Exception inner) : base(message, inner)
        { }
    }
}
