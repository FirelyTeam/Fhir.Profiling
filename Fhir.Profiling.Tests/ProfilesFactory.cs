using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fhir.IO;
using System.Xml.XPath;
using System.Xml;

namespace Fhir.Profiling.Tests
{
    public static class Profiles
    {
        public static Profile Patient
        {
            get
            {
                ProfileBuilder builder = new ProfileBuilder();
                builder.Add(StructureFactory.PrimitiveTypes());
                builder.Add(StructureFactory.NonFhirNamespaces());
                builder.LoadXMLValueSets("TestData\\valuesets.xml");
                builder.LoadXmlFile("TestData\\type-Extension.profile.xml");
                builder.LoadXmlFile("TestData\\type-Contact.profile.xml");
                builder.LoadXmlFile("TestData\\type-HumanName.profile.xml");
                builder.LoadXmlFile("TestData\\type-Identifier.profile.xml");
                builder.LoadXmlFile("TestData\\type-Narrative.profile.xml");
                builder.LoadXmlFile("TestData\\patient.profile.xml");
                return builder.ToProfile();
            }
        }

        public static Profile Lipid
        {
            get
            {
                ProfileBuilder builder = new ProfileBuilder();
                builder.Add(StructureFactory.PrimitiveTypes());
                builder.Add(StructureFactory.NonFhirNamespaces());
                
                //builder.LoadXMLValueSets("Data\\valuesets.xml");
                //builder.LoadXmlFile("Data\\type-Extension.profile.xml");
                //builder.LoadXmlFile("Data\\type-Contact.profile.xml");
                //builder.LoadXmlFile("Data\\type-HumanName.profile.xml");
                //builder.LoadXmlFile("Data\\type-Identifier.profile.xml");
                //builder.LoadXmlFile("Data\\type-Narrative.profile.xml");
                builder.LoadXmlFile("TestData\\valueset.profile.xml");
                builder.LoadXmlFile("TestData\\lipid.profile.xml");
                return builder.ToProfile();
            }
        }
    }

}
