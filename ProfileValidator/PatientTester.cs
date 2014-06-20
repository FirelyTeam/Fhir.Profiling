using Fhir;
using Fhir.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fhir.IO;

namespace ProfileValidation
{
    
    class PatientTester : Tester
    {
        protected override Profile LoadProfile()
        {
            Profile profile = new Profile();
            
            //profile.Add(ProfileFactory.MetaTypesProfile());
            //profile.Add(ProfileFactory.DataTypesProfile());
            profile.Add(ProfileFactory.PrimitiveTypesProfile());
            profile.Add(ProfileFactory.ExceptionsProfile());
            profile.LoadXMLValueSets("Data\\valuesets.xml");
            profile.LoadXmlFile("Data\\type-Extension.profile.xml");
            profile.LoadXmlFile("Data\\type-HumanName.profile.xml");
            profile.LoadXmlFile("Data\\type-Identifier.profile.xml");
            profile.LoadXmlFile("Data\\type-Narrative.profile.xml");
            profile.LoadXmlFile("Data\\patient.profile.xml");

            return profile;
        }

        protected override IEnumerable<Feed.Entry> Entries()
        {
            Feed feed = LoadResources("patient-examples");
            return feed.Entries();
        }
    }
     
}
