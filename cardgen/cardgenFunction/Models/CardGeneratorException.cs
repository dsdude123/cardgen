using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cardgenFunction.Models
{
    class CardGeneratorException : Exception
    {

        public CardGeneratorException()
        {
        }

        public CardGeneratorException(string message)
            : base(message)
        {
        }

        public CardGeneratorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
