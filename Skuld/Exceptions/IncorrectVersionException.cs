using System;

namespace Skuld.Exceptions
{
    public class IncorrectVersionException : Exception
    {
        public IncorrectVersionException() : base()
        {

        }
        public IncorrectVersionException(string message) : base (message)
        {

        }
        public IncorrectVersionException(string message, Exception inner) : base(message, inner)
        {

        }
    }    
}
