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
            ProfileBuilder builder = new ProfileBuilder();
            
            //profile.Add(ProfileFactory.MetaTypesProfile());
            //profile.Add(ProfileFactory.DataTypesProfile());
            builder.Add(StructureFactory.PrimitiveTypes());
            builder.Add(StructureFactory.NonFhirNamespaces());
            builder.LoadXMLValueSets("Data\\valuesets.xml");
            builder.LoadXmlFile("Data\\type-Extension.profile.xml");
            builder.LoadXmlFile("Data\\type-HumanName.profile.xml");
            builder.LoadXmlFile("Data\\type-Identifier.profile.xml");
            builder.LoadXmlFile("Data\\type-Narrative.profile.xml");
            builder.LoadXmlFile("Data\\patient.profile.xml");

            return builder.ToProfile();
        }

        protected override IEnumerable<Feed.Entry> Entries()
        {
            Feed feed = LoadResources("patient-examples");
            return feed.Entries();
        }
    }
     
}
