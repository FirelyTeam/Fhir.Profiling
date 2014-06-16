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
            Profile profile = new Profile();
            
            //profile.Add(ProfileFactory.MetaTypesProfile());
            //profile.Add(ProfileFactory.DataTypesProfile());
            // profile.Add(ProfileFactory.PrimitiveTypesProfile());
            //profile.LoadXMLValueSets("Data\\valuesets.xml");
            //profile.LoadXmlFile("Data\\type-HumanName.profile.xml");
            //profile.LoadXmlFile("Data\\type-Identifier.profile.xml")
            
            ;
            profile.LoadXmlFile("Data\\type-Coding.profile.xml");
            profile.LoadXmlFile("Data\\type-Extension.profile.xml");
            profile.LoadXmlFile("Data\\valueset.profile.xml");
            profile.LoadXmlFile("Data\\lipid-profile.xml");

            return profile;
        }

        protected override IEnumerable<Feed.Entry> Entries()
        {
            Feed feed = LoadResources("lipid-interpretation");
            return feed.Entries();
            
        }
    }
}
