using System;
using System.Collections.Generic;
using System.Text;

namespace CommonNetStandard.Common
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message)
            : base(message)
        {
        }
    }
}
