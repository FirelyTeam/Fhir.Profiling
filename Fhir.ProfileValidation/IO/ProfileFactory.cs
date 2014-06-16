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

        public static Structure DivStructure()
        {
            Structure structure = new Structure();
            structure.Name = "div";
            structure.IsPrimitive = true;
            structure.Namespace = Namespace.XHtml;
            return structure;
        }

        public static Profile PrimitiveProfileFor(IEnumerable<string> list)
        {
            Profile profile = new Profile();
            foreach (string s in list)
            {
                profile.Add(Primitive(s));
            }

            return profile;
        }

        public static Profile MetaTypesProfile()
        {
            string[] list = { "Structure", "Extension", "Resource", "Narrative" };
            // NB. Narrative bevat <status> en <div>, en <div> mag html bevatten. 
            // Dit blijkt niet uit profiles.
            return PrimitiveProfileFor(list);
        }

        public static Profile DataTypesProfile()
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

        public static Profile PrimitiveTypesProfile()
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
    }
}
