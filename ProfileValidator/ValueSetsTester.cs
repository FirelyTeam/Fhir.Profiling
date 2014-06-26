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
    class ValueSetsTester : Tester
    {
        protected override Profile LoadProfile()
        {
            ProfileBuilder builder = new ProfileBuilder();

            //profile.Add(ProfileFactory.MetaTypesProfile());
            //profile.Add(ProfileFactory.DataTypesProfile());
            builder.Add(ProfileFactory.PrimitiveTypesProfile());

            builder.LoadXMLValueSets("Data\\valuesets.xml");
            //profile.LoadXmlFile("Data\\type-HumanName.profile.xml");
            //profile.LoadXmlFile("Data\\type-Identifier.profile.xml");
            builder.LoadXmlFile("Data\\type-Narrative.profile.xml");
            builder.LoadXmlFile("Data\\type-Contact.profile.xml");
            builder.LoadXmlFile("Data\\valueset.profile.xml");

            return builder.ToProfile();
        }

        protected override IEnumerable<Feed.Entry> Entries()
        {
           Feed feed = LoadResources("valueset.example");
           return feed.Entries();
        }
    }
}
