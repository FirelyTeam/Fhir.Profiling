using System;
/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Fhir.Profiling
{
    public class ProfileReader
    {
        XmlNamespaceManager ns;
        
        public string Value(XPathNavigator node, string xpath)
        {
            XPathNavigator n = node.SelectSingleNode(xpath, ns);
            if (n == null)
                throw new InvalidStructureException("Missing value in profile: {0}", xpath);
            return n.Value;
        }

        public string OptionalValue(XPathNavigator node, string xpath)
        {
            XPathNavigator n = node.SelectSingleNode(xpath, ns);
            return (n != null) ? n.Value : null;
        }

        public void ReadCardinality(Element element, XPathNavigator node)
        {
            Cardinality cardinality = new Cardinality();
            cardinality.Min = Value(node, "f:definition/f:min/@value");
            cardinality.Max = Value(node, "f:definition/f:max/@value");
            element.Cardinality = cardinality;
        }

        public void ReadPath(Element element, XPathNavigator node)
        {
            string s = node.SelectSingleNode("f:path/@value", ns).Value;

            element.Path = new Path(s);
            element.Name = element.Path.ElementName;
            element.Namespace = Namespace.Fhir;

            // todo: HACK! The standard doesn't provide a namespace in the profile for div
            if (element.Name == "div")
                element.Namespace = Namespace.XHtml;
        }

        public void ReadReference(Element element, XPathNavigator node)
        {
            element.BindingUri = OptionalValue(node, "f:definition/f:binding/f:referenceResource/f:reference/@value");
        }

        public TypeRef ReadTypeRef(Element element, XPathNavigator node)
        {
            TypeRef typeref = new TypeRef();
            typeref.Code = Value(node, "f:code/@value");
            typeref.ProfileName = OptionalValue(node, "f:profile/@value");
            return typeref;
        }

        public void ReadTypeRefs(Element element, XPathNavigator node)
        {
            var iterator = node.Select("f:definition/f:type", ns);
            foreach(XPathNavigator n in iterator)
            {
                TypeRef typeref = ReadTypeRef(element, n);
                element.TypeRefs.Add(typeref);
            }
        }

        public void ReadElementRef(Element element, XPathNavigator node)
        {
            element.ElementRefPath = OptionalValue(node, "f:definition/f:nameReference/@value");
        }

        private int constraintnr;
        
        public void ReadConstraints(Element element, XPathNavigator node)
        {
            foreach (XPathNavigator nav in node.Select("f:definition/f:constraint", ns))
            {
                Constraint constraint = new Constraint();
                XPathNavigator xName = nav.SelectSingleNode("f:name/@value", ns);
                // todo: Constraint naam ontbreekt soms. Dit is eigenlijk een bug in de FHIR profile generator. 
                // Maar nu moeten we er maar even omheen werken.
                constraint.Name = (xName != null) ? xName.Value : "Constraint"+(++constraintnr).ToString();
                constraint.XPath = nav.SelectSingleNode("f:xpath/@value", ns).Value;
                element.Constraints.Add(constraint);
            }   
        }

        public void ReadSlices(Element element, XPathNavigator node)
        {

        }
     
        public Element ReadElement(XPathNavigator node)
        {
            Element element = new Element();
            ReadPath(element, node);
            ReadReference(element, node);
            ReadTypeRefs(element, node);
            ReadElementRef(element, node);
            ReadCardinality(element, node);
            ReadConstraints(element, node);
            ReadSlices(element, node);
            
            return element;
        }

        public Structure ReadStructure(XPathNavigator node)
        {
            Structure structure = new Structure();
            structure.Name = node.SelectSingleNode("f:type/@value", ns).Value;
            structure.Namespace = Namespace.Fhir;
            XPathNodeIterator xElements = node.Select("f:element", ns);
            foreach (XPathNavigator xElement in xElements)
            {
                structure.Elements.Add(ReadElement(xElement));
            }
            structure.BuildHierarchy();
            return structure;
        }

        public List<Structure> ReadProfile(XPathNavigator node)
        {
            List<Structure> structures = new List<Structure>();
            XPathNodeIterator xStructures = node.Select("f:Profile/f:structure", ns);
            foreach (XPathNavigator xStructure in xStructures)
            {
                Structure s = ReadStructure(xStructure);
                structures.Add(s);
            }
            return structures;
        }

        public List<Structure> Read(IXPathNavigable navigable)
        {
            XPathNavigator navigator = navigable.CreateNavigator();
            ns = Namespace.GetManager(navigator);
            return ReadProfile(navigator);
        }

        public ValueSet ReadValueSet(XPathNavigator node)
        {
            ValueSet valueset = new ValueSet();
            
            XPathNavigator systemnode = node.SelectSingleNode("f:define/f:system/@value", ns);

            if (systemnode == null) return null;
            valueset.System = systemnode.ToString();

            XPathNodeIterator iterator = node.Select("f:define/f:concept", ns);
            foreach(XPathNavigator n in iterator)
            {
                string s = n.SelectSingleNode("f:code/@value", ns).ToString();
                valueset.codes.Add(s);
            }
            return valueset;
        }

        public List<ValueSet> ReadValueSets(IXPathNavigable navigable)
        {
            List<ValueSet> valuesets = new List<ValueSet>();
            XPathNavigator navigator = navigable.CreateNavigator();
            ns = Namespace.GetManager(navigator);

            foreach(XPathNavigator node in navigator.Select("atom:feed/atom:entry/atom:content/f:ValueSet", ns))
            {
                ValueSet set = ReadValueSet(node);
                if (set != null) valuesets.Add(set);
            }
            return valuesets;
        }
    }

}
