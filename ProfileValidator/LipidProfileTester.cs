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
    class LipidProfileTester : Tester
    {
        protected override Profile LoadProfile()
        {
            ProfileBuilder builder = new ProfileBuilder();

            //profile.Add(ProfileFactory.MetaTypesProfile());
            //profile.Add(ProfileFactory.DataTypesProfile());
            // profile.Add(ProfileFactory.PrimitiveTypesProfile());
            //profile.LoadXMLValueSets("Data\\valuesets.xml");
            //profile.LoadXmlFile("Data\\type-HumanName.profile.xml");
            //profile.LoadXmlFile("Data\\type-Identifier.profile.xml")
            
            builder.LoadXmlFile("Data\\profile.profile.xml");
            return builder.ToProfile();
        }

        protected override IEnumerable<Feed.Entry> Entries()
        {
            //Feed feed = LoadResources("lipid-profile");
            Feed.Entry entry = FhirFile.LoadXMLResource("Data\\lipid-profile.xml");
            yield return entry;

        }
    }

}
