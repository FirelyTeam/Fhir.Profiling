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
using System.Xml.XPath;

namespace Fhir.Profiling
{

    public enum SlicingRules { OpenAtEnd };
    
    /// <summary>
    /// FHIR Profile. Basis for validation of FHIR Resources.
    /// Profile should be constructed from a ProfileBuilder
    /// </summary>
    public class Profile
    {
        private List<Structure> _structures = new List<Structure>();
        private List<ValueSet> _valueSets = new List<ValueSet>();
        
        internal Profile()
        {
            
        }
        
        public IEnumerable<Structure> Structures
        {
            get
            {
                return _structures;
            }
        }

        public IEnumerable<ValueSet> ValueSets
        {
            get
            {
                return _valueSets;
            }
        }

        public IEnumerable<Element> Elements
        {
            get
            {
                return Structures.SelectMany(s => s.Elements);
            }
        }

        public IEnumerable<Constraint> Constraints
        {
            get
            {
                return Elements.SelectMany(e => e.Constraints);
            }
        }

        internal void Add(IEnumerable<ValueSet> valuesets)
        {
            _valueSets.AddRange(valuesets);
        }
        
        internal void Add(IEnumerable<Structure> structures)
        {
            _structures.AddRange(structures);
        }

        public Structure GetStructureByName(string name)
        {
            return Structures.FirstOrDefault(s => s.Name == name);
        }

        public ValueSet GetValueSetByUri(string uri)
        {
            return ValueSets.FirstOrDefault(v => v.System == uri);
        }
       
        public Element GetElementByPath(Structure structure, string path)
        {
            return structure.Elements.FirstOrDefault(element => element.Path.ToString() == path);
        }

        public Structure StructureOf(Element element)
        {
            foreach (Structure structure in Structures)
            {
                if (structure.Elements.Contains(element))
                    return structure;
            }
            return null;
        }

        public Element ParentOf(Structure structure, Element element)
        {
            Path path = element.Path.Parent();
            Element parent = structure.Elements.Find(e => e.Path.Equals(path));
            return parent;
        }

        public Element ParentOf(Element element)
        {
            Structure structure = StructureOf(element);
            return ParentOf(structure, element);
        }
    }
   
}
