using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Models
{
    public class ResultMessage
    {
        public ErrorType Error = ErrorType.NoError;
        public string Msgs = string.Empty;
        public object Result = null;

        public ResultMessage(ErrorType err, string msg)
        {
            Error = err;
            Msgs = msg;
        }

        public ResultMessage(object ret)
        {
            Result = ret;
        }
    }
}
