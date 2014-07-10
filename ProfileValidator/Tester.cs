using Fhir;
using Fhir.IO;
using Fhir.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileValidation
{
    
    public abstract class Tester
    {
        static ReportPrinter printer = new ReportPrinter(@"c:\temp\report");
        public Profile profile;

        public Tester()
        {
            profile = this.LoadProfile();
        }

        protected Feed LoadResources(string filename)
        {
            return FhirFile.LoadXMLFeed("Data\\" + filename + ".xml");
        }

        protected abstract Profile LoadProfile();

        protected abstract IEnumerable<Feed.Entry> Entries();

        public void ValidateEntries(Profile profile, IEnumerable<Feed.Entry> entries, ReportMode mode)
        {
            ResourceValidator validator = new ResourceValidator(profile);

            foreach (Feed.Entry entry in entries)
            {
                printer.Title(string.Format("Validating resource '{0}'", entry.Title));
                Report report = validator.Validate(entry.ResourceNode);
                printer.Print(report, mode);
                Console.Write(".");
            }
        }

        public void ValidateProfile(Profile profile)
        {
            ProfileValidator pv = new ProfileValidator(profile);
            printer.Title("Checking the profile for use in resource validation");
            Report report = pv.Validate();
            printer.PrintSummary(report);
        }

        public void Run(ReportMode mode)
        {
            profile = LoadProfile();
            IEnumerable<Feed.Entry> entries = Entries();
            
            ValidateProfile(profile);
            ValidateEntries(profile, entries, mode);
        }
    }
}
