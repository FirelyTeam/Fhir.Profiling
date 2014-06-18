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
        public static Structure Primitive(string name)
        {
            Structure structure = new Structure();
            structure.Name = name;
            structure.IsPrimitive = true;
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
                structures.Add(Primitive(s));
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
            string[] list = { 
                "instant",
                "date",
                "dateTime",
                "decimal",
                "element",
                "boolean",
                "integer",
                "string",
                "uri",
                "base64Binary",
                "code",
                "id",
                "oid",
                "uuid" };

            return PrimitiveProfileFor(list);
        }

        public static List<Structure> ExceptionsProfile()
        {
            List<Structure> list = new List<Structure>();
            list.Add(XhtmlStructure());
            return list;
        }
    }
}
