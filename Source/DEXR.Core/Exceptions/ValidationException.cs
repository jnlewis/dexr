using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core
{
    public class ValidationException : System.Exception
    {
        public string Code { get; }
        public override string Message { get; }

        public ValidationException(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
