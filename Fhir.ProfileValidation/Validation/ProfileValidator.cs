/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Fhir.Profiling
{

    public class ProfileValidator
    {
        private Report report = new Report();
        private Profile profile;
        private List<string> missingStructureNames = new List<string>();
        

        public ProfileValidator(Profile profile)
        {
            report.Clear();
            this.profile = profile;
        }

        public void ValidateCardinality(Element element)
        {
            if (element.Cardinality.Min == null || element.Cardinality.Max == null)
            report.Add("Element", Kind.Incomplete,
                "Element [{0}] does not define it's cardinality", element.Path);

        }
        public void ValidateConstraint(Element element, Constraint constraint)
        {
            if (constraint.IsValid)
            {
                report.Add("Constraint", Kind.Valid, "Constraint is valid");
            }
            else 
            {
                 report.Add("Constraint", Kind.Invalid, 
                     "Constraint [{0}] has an invalid XPath: {1}", 
                     constraint.Name, constraint.CompilerError.Message);
            }
        }

        public void ValidateConstraints(Element element)
        {
            foreach(Constraint c in element.Constraints)
            {
                ValidateConstraint(element, c);
            }
        }

        public void ValidateTypeRef(Element element, TypeRef typeref)
        {

            // Test if the Surrect was able to link to the target structure.
            if (typeref.Structure != null)
            {
                report.Add("Reference", Kind.Valid, "Type reference to structure [{0}] is valid", typeref.Code);
                
                // Genest structuren valideren hoeft niet. Want alle structures worden al op hoofdniveau gevalideerd
                //ValidateStructure(typeref.Structure);

            }

            // Test if there is a reference at all
            else if (typeref.Code == null)
            {
                report.Add("Reference", Kind.Invalid, "Missing a reference to a structure in element [{0}]", element.Name);
            }

            // Test if code is itself valid? If so, the reference valid but the target is missing.
            else if (Regex.IsMatch(typeref.Code, "[A-Za-z][A-Za-z0-9]*"))
            {
                // Collect first to avoid duplicates
                missingStructureNames.Add(typeref.Code);
            }

            // The code contains invalid characters
            else
            {
                report.Add("Reference", Kind.Invalid, "Invalid structure reference '{0}' in {1}", typeref.Code, element.Path);
            }
        }

        public void ValidateTypeRefs(Element element)
        {
            foreach (TypeRef t in element.TypeRefs)
            {
                ValidateTypeRef(element, t);
            }
        }

        public void ValidateElement(Element element)
        {
            ValidateCardinality(element);
            ValidateConstraints(element);
            ValidateTypeRefs(element);
        }

        public void ValidateElementRefs(Structure structure)
        {
            foreach(Element element in structure.Elements)
            {
                if (element.ElementRefPath != null && element.ElementRef == null)
                {
                    report.Add("Reference", Kind.Invalid, 
                        "Element [{0}] Name reference to different element [{1}] is unresolved", 
                        element.Path, element.ElementRefPath);
                }
            }
        }

        public void ValidateStructure(Structure structure)
        {
            //if (structure.IsPrimitive)
             //   return; // there is no more detail.

            foreach (Element element in structure.Elements)
            {
                ValidateElement(element);
            }

            ValidateElementRefs(structure);
        }

        private void ValidateStructures()
        {
            foreach (Structure structure in profile.Structures)
            {
                ValidateStructure(structure);
            }
        }

        private void AddStructureNamesToReport()
        {
            foreach (string s in missingStructureNames.Distinct())
                report.Add("Profile", Kind.Incomplete, "Missing structure definition [{0}]", s);
            
        }

        public Report Validate()
        {
            report.Clear();
            ValidateStructures();
            
            AddStructureNamesToReport();

            return report;
        }
    }
}
