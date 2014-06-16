using Fhir.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fhir.IO;
using Fhir;

namespace ProfileValidation
{
    class ProfileTester : Tester
    {

        protected override Profile LoadProfile()
        {
            Profile profile = new Profile();

            //profile.Add(ProfileFactory.MetaTypesProfile());
            //profile.Add(ProfileFactory.DataTypesProfile());
             profile.Add(ProfileFactory.PrimitiveTypesProfile());
            //profile.LoadXMLValueSets("Data\\valuesets.xml");
            //profile.LoadXmlFile("Data\\type-HumanName.profile.xml");
            //profile.LoadXmlFile("Data\\type-Identifier.profile.xml")


            profile.LoadXmlFile("Data\\profile.profile.xml");
            return profile;
        }

        protected override IEnumerable<Feed.Entry> Entries()
        {
            Feed.Entry entry = FhirFile.LoadXMLResource("Data\\profile.profile.xml");
            yield return entry;
        }
    }
}
