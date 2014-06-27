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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using Fhir.IO;

namespace Fhir.Profiling
{

    public class ResourceValidator
    {
        private Profile Profile = new Profile();
        private Report report = new Report();

        public ResourceValidator(Profile profile)
        {
            this.Profile = profile;
        }

        public bool ValidateCoding(string name, string code)
        {
            ValueSet valueset = Profile.GetValueSetByUri(name);
            if (valueset != null)
            {
                return valueset.Contains(code);
            }
            else return false;
        }

        public void ValidateCode(Vector vector)
        {
            if (vector.Element.Binding == null)
                return;

            string value = vector.GetValue("@value");
            bool exists = vector.Element.Binding.Contains(value);
            
            if (exists)
            {
                report.Add("Coding", Kind.Valid, vector, "Code [{0}] ({1}) is valid [{2}]",
                    vector.Element.Name, vector.Element.Binding.System, value);
            }
            else
            {
                report.Add("Coding", Kind.Invalid, vector, "Code [{0}] ({1}) contains a nonexisting value [{2}]",
                    vector.Element.Name, vector.Element.Binding.System, value);
            }
            
        }

        public void ValidateCardinality(Vector vector)
        {   
            int count = vector.Count(); 
            if (vector.Element.Cardinality.InRange(count))
            {
                report.Add("Cardinality", Kind.Valid, vector, "Cardinality ({1}) of element [{0}] is valid", vector.Element.Name, count);
            }
            else 
            {
                if (count == 0)
                {
                    report.Add("Cardinality", Kind.Invalid, vector,
                     "Node [{0}] has missing child node [{1}] ",
                     vector.NodePath(), vector.Element.Name);
                }
                else
                {
                    report.Add("Cardinality", Kind.Invalid, vector,
                        "The occurence {0} of node [{1}] under [{2}] is out of range ({3})",
                        count, vector.NodePath(), vector.Element.Name, vector.Element.Cardinality);
                }
            }
        }

        public void ValidateConstraint(Vector vector, Constraint constraint)
        {
            if (constraint.IsValid)
            {
                try
                {
                    bool valid = vector.Evaluate(constraint);
                    if (valid)
                        report.Add("Constraint", Kind.Valid, vector, "Node [{0}] conforms to constraint [{1}]", vector.Node.Name, constraint.Name);
                    else
                        report.Add("Constraint", Kind.Invalid, vector, "Node [{0}] does not conform to constraint [{1}]: {2} ", vector.Node.Name, constraint.Name, constraint.HumanReadable);
                }
                catch (XPathException e)
                {
                    report.Add("Constraint", Kind.Invalid, vector, "Evaluation of constraint [{0}] evaluation failed: {1}", constraint.Name, e.Message);
                }
            }
        }

        public void ValidateConstraints(Vector vector)
        {
            foreach (Constraint constraint in vector.Element.Constraints)
            {
                ValidateConstraint(vector, constraint);
            }
        }

        public void ValidateForMissingStructures(Vector vector)
        {
            IEnumerable<string> missing = vector.Element.TypeRefs.Where(t => t.Structure == null).Select(t => t.Code);
            foreach (string s in missing)
            {
                report.Add("Structure", Kind.Skipped, vector, "Profile misses structure [{0}]. Evaluation is skipped.", s);
            }
        }
        
        public void ValidateStructures(Vector vector)
        {
            foreach(Vector v in vector.ElementStructures)
            {
                ValidateStructure(v);
            }
        }

        public void ValidateElementChildren(Vector vector)
        {
            foreach (Vector v in vector.ElementChildren)
            {
                ValidateCardinality(v);

                foreach(Vector u in v.Matches)
                {
                    ValidateElement(u);
                }
            }
        }

        public void ValidateElementRef(Vector vector)
        {
            if (vector.Element.ElementRef != null)
            {
                Vector clone = vector.Clone();
                clone.Element = vector.Element.ElementRef;
                ValidateElement(clone);
            }
        }

        public void ValidateNodeChild(Vector vector)
        {
            string name = vector.Node.Name;
            if (!vector.Element.HasChild(name))
                report.Add("Element", Kind.Unknown, vector, "Element [{0}] contains undefined element [{1}]", vector.Element.Name, name);
        }

        public void ValidateNodeChildren(Vector vector)
        {
            if (vector.Element.HasTypeRef) //element has a reference, so there are no Element children to validate to. 
                return;

            if (vector.Element.NameSpacePrefix != FhirNamespaceManager.Fhir)
                return;

            foreach(Vector v in vector.NodeChildren)
            {
                ValidateNodeChild(v);
            }
        }

        public void ValidateSlices(Vector vector)
        {
             
        }
        public void ValidateElement(Vector vector)
        {
            report.Start("element", vector);
            ValidateCode(vector);
            ValidateConstraints(vector);
            ValidateStructures(vector);
            ValidatePrimitive(vector);
            ValidateForMissingStructures(vector);
            ValidateNodeChildren(vector);
            ValidateElementChildren(vector);
            ValidateElementRef(vector);
            ValidateSlices(vector);
            report.End();
        }

        public void ValidatePrimitive(Vector vector)
        {
            // fail. validation of primites should be done at the root element
            // this should be the root element of a structure
            if (!vector.Element.IsPrimitive)
                return;

            if (vector.Element.NameSpacePrefix != FhirNamespaceManager.Fhir)
                return;

            try
            {
                string value = vector.GetContent();
                string pattern = vector.Element.PrimitivePattern;
                if (Regex.IsMatch(value, pattern))
                {   
                    report.Add("Primitive", Kind.Valid, vector, "The value format ({0}) of primitive [{1}] is valid. ", vector.Element.Name, vector.Node.Name);
                }
                else
                {
                    report.Add("Primitive", Kind.Invalid, vector, "The value format ({0}) of primitive [{1}] not valid: '{2}'", vector.Element.Name, vector.Node.Name, value);
                }
            }
            catch
            {
                report.Add("Primitive", Kind.Invalid, vector, "The value of primitive [{0}] was not present.", vector.Node.Name);
            }
        }
     
        public void ValidateStructure(Vector vector)
        {
            if (vector.Structure == null)
            {
                report.Add("Structure", Kind.Unknown, vector, "Node [{0}] does not match a known structure. Further evaluation is impossible.", vector.Node.Name);
            }
            else
            {
                report.Start("structure", vector);
                ValidateElement(vector);
                report.End();
            }
        }

        public Vector GetVector(XPathNavigator root)
        {
            XmlNamespaceManager nsm = FhirNamespaceManager.CreateManager(root);
            Structure structure = Profile.GetStructureByName(root.Name);
            Vector vector = Vector.Create(structure, root, nsm);
            return vector;
        }

        public Report Validate(IXPathNavigable navigable)
        {
            report.Clear();
            XPathNavigator root = navigable.CreateNavigator();
            Vector vector = GetVector(root);
            
            ValidateStructure(vector);

            return this.report;
        }

    }
}
