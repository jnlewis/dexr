using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core.Model
{
    public class GenericResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public GenericResponse() { }
        public GenericResponse(object data, string code, string message)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }
}
