using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    class InvalidStructureException : Exception
    {
        public InvalidStructureException(string message, params object[] args) : base(string.Format(message, args))
        {
            
        }
    }
}
