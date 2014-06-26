using Fhir.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    public static class ProfileFactory
    {
       
        static ProfileFactory()
        {
        }

        public static void AddExtensionElement(Structure structure, Element parent = null)
        {
            parent = parent  ?? structure.Root;
            string path = string.Format("{0}.extension", parent.Path); 
            Element element = new Element();
            element.Path = new Path(path);
            element.Name = "extension";
            element.Cardinality = new Cardinality { Min = "0", Max = "*" };
            element.TypeRefs.Add(new TypeRef { Code = "Extension" });
            structure.Elements.Add(element);
        }

        public static Structure Primitive(string name, string pattern)
        {
            Structure structure = new Structure();
            structure.Name = name;

            Element element = new Element();
            element.Path = new Path(name);
            element.Name = name;
            element.Cardinality = new Cardinality { Min = "1", Max = "1" };
            structure.Elements.Add(element);

            AddExtensionElement(structure, element);
            structure.IsPrimitive = true;
            structure.ValuePattern = pattern;
            return structure;
        }

        public static Structure XhtmlStructure()
        {
            Structure structure = new Structure();
            structure.Name = "xhtml";
            structure.IsPrimitive = true;
            structure.Namespace = Namespace.XHtml;
            return structure;
        }

        public static List<Structure> PrimitiveProfileFor(IEnumerable<string> list)
        {
            List<Structure> structures = new List<Structure>();
            foreach (string s in list)
            {
                structures.Add(Primitive(s, ""));
            }

            return structures;
        }

        public static List<Structure> MetaTypesProfile()
        {
            string[] list = { "Structure", "Extension", "Resource", "Narrative" };
            // NB. Narrative bevat <status> en <div>, en <div> mag html bevatten. 
            // Dit blijkt niet uit profiles.
            return PrimitiveProfileFor(list);
        }

        public static List<Structure> DataTypesProfile()
        {
            string[] list = {
                "ratio",
                "Period",
                "range",
                "Attachment",
                "Identifier",
                "schedule",
                "HumanName",
                "Coding",
                "Address", 
                "CodeableConcept",
                "Quantity",
                "SampledData",
                "Contact",
                "Age",
                "Distance",
                "Duration",
                "Count",
                "Money"};
            return PrimitiveProfileFor(list);
        }

        public static List<Structure> PrimitiveTypesProfile()
        {
            List<Structure> list = new List<Structure>
            { 
                Primitive("instant", @".*"),
                Primitive("date", @".*"),
                Primitive("dateTime", @".*"),
                Primitive("decimal", @"\d+"),
                //Primitive("element", ".*"),
                Primitive("boolean", @"(true|false)"),
                Primitive("integer", @"\d+"),
                Primitive("string", @".*"),
                Primitive("uri", @"http"),
                Primitive("base64Binary", @".*"),
                Primitive("code", @".*"),
                Primitive("id", @"\d+"),
                Primitive("oid", @".*"),
                Primitive("uuid" , @".*")
            };

            return list;
        }

        public static List<Structure> ExceptionsProfile()
        {
            List<Structure> list = new List<Structure>();
            list.Add(XhtmlStructure());
            return list;
        }
    }
}
