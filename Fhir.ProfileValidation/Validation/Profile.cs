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

    public class Profile
    {
        public List<ValueSet> ValueSets = new List<ValueSet>();
        
        public List<Structure> Structures = new List<Structure>();

        public void Add(List<Structure> structures)
        {
            this.Structures.AddRange(structures);
            AddInternalReferences();
        }
        
        public void Add(IEnumerable<ValueSet> valuesets)
        {
            this.ValueSets.AddRange(valuesets);
        }
        
        public void Add(Profile profile)
        {
            Add(profile.Structures);
            Add(profile.ValueSets);
        }

        public IEnumerable<Element> Elements
        {
            get 
            {
                return Structures.SelectMany(s => s.Elements);
            }
        }

       
        public IEnumerable<TypeRef> NewTypeRefs
        {
            get 
            {
                return Elements.SelectMany(e => e.TypeRefs).Where(r => r.Structure == null);
            }
        }

        private void AddInternalReferences()
        {
            foreach(Element element in Elements)
            {
                if (element.BindingUri != null)
                    element.Binding = this.GetValueSetByUri(element.BindingUri);
            }

            foreach (TypeRef typeref in NewTypeRefs)
            {
                typeref.Structure = this.GetStructureByName(typeref.Code);
            }

            foreach (Structure structure in Structures)
            {
                foreach (Element element in Elements)
                {
                    element.ElementRef = this.GetElementByName(structure, element.ElementRefPath);
                }
            }
        }

        public Structure GetStructureByName(string name)
        {
            foreach (Structure s in Structures)
            {
                if (s.Name == name)
                    return s;
            }
            return null;
        }

        public ValueSet GetValueSetByUri(string uri)
        {
            foreach (ValueSet valueset in ValueSets)
            {
                if (valueset.System == uri)
                    return valueset;
            }
            return null;
        }
       
        public Element GetElementByName(Structure structure, string path)
        {
            foreach(Element element in structure.Elements)
            {
                if (element.Path.ToString() == path)
                {
                    return element;
                }
            }
            return null;
        }
    }
}
