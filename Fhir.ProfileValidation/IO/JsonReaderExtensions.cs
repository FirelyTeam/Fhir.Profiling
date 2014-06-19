using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fhir.Profiling.IO;

namespace Fhir.Profiling
{
    public static class JsonReaderExtensions
    {
        public static JsonXPathNavigator GetNavigator(this JsonReader reader)
        {
            return new JsonXPathNavigator(reader);
        }
            
    }
}
