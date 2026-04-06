using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ecommerce_Common
{
    public abstract class Result
    {
        public List<Errors> Errors { get; set; } = new();
        public bool isError => Errors != null && Errors.Any();
    }
    public class Result<T> : Result
    {
        public T Response { get; set; }
        public string Message { get; set; }
        public string WarningMessage { get; set; }
        public string Token { get; set; }

    }
}
