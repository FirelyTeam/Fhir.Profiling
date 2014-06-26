using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fhir.IO;
using Fhir.Profiling;
using Fhir;

namespace ProfileValidation
{
    class LipidTester : Tester
    {
  
        protected override Profile LoadProfile()
        {
            ProfileBuilder builder = new ProfileBuilder();
            
            //profile.Add(ProfileFactory.MetaTypesProfile());
            //profile.Add(ProfileFactory.DataTypesProfile());
            builder.Add(ProfileFactory.PrimitiveTypesProfile());
            //profile.LoadXMLValueSets("Data\\valuesets.xml");
            //profile.LoadXmlFile("Data\\type-HumanName.profile.xml");
            //profile.LoadXmlFile("Data\\type-Identifier.profile.xml")
            
            ;
            builder.LoadXmlFile("Data\\type-Coding.profile.xml");
            builder.LoadXmlFile("Data\\type-Extension.profile.xml");
            builder.LoadXmlFile("Data\\valueset.profile.xml");
            builder.LoadXmlFile("Data\\lipid-profile.xml");

            return builder.ToProfile();
        }

        protected override IEnumerable<Feed.Entry> Entries()
        {
            Feed feed = LoadResources("lipid-interpretation");
            return feed.Entries();
            
        }
    }
}
