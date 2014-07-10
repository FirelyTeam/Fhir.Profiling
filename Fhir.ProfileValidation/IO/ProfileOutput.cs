using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    public static class ProfileOutput
    {
        public static void ToConsole(this IEnumerable<Report.Outcome> outcomes)
        {
            foreach(Report.Outcome outcome in outcomes)
            {
                Console.WriteLine(outcome);
            }
        }
    }
}
