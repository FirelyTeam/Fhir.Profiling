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

    public class Slicing
    {
        public Path Discriminator { get; set; }
        public bool Ordered { get; set; }
        public SlicingRules Rule { get; set; }
        public List<Element> Elements = new List<Element>();
    }

    public class Profile
    {
        public List<ValueSet> ValueSets = new List<ValueSet>();
        public List<Structure> Structures = new List<Structure>();

        public void Add(List<Structure> structures)
        {
            this.Structures.AddRange(structures);
            Surrect();
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

        public IEnumerable<Constraint> Constraints
        {
            get
            {
                return Elements.SelectMany(e => e.Constraints);
            }
        }

        public IEnumerable<TypeRef> NewTypeRefs
        {
            get 
            {
                return Elements.SelectMany(e => e.TypeRefs).Where(r => r.Structure == null);
            }
        }

        /// <summary>
        /// Make the profile complete and usable by linking all internal structures and perform precompilation
        /// </summary>
        private void Surrect()
        {
            _linkBindings();
            _linkStructures();
            _linkElements();
            _compileConstraints();
        }

        private void _linkBindings()
        {
            foreach (Element element in Elements)
            {
                if (element.BindingUri != null)
                    element.Binding = this.GetValueSetByUri(element.BindingUri);
            }
        }

        private void _linkStructures()
        {
            foreach (TypeRef typeref in NewTypeRefs)
            {
                typeref.Structure = this.GetStructureByName(typeref.Code);
            }

        }

        private void _linkElements()
        {
            foreach (Structure structure in Structures)
            {
                foreach (Element element in Elements)
                {
                    element.ElementRef = this.GetElementByName(structure, element.ElementRefPath);
                }
            }
        }

        private void _compileConstraints()
        {
            foreach(Constraint c in Constraints)
            {
                if (!c.Compiled) 
                    ConstraintCompiler.Compile(c);
            }
        }

        public Structure GetStructureByName(string name)
        {
            return Structures.FirstOrDefault(s => s.Name == name);
        }

        public ValueSet GetValueSetByUri(string uri)
        {
            return ValueSets.FirstOrDefault(v => v.System == uri);
        }
       
        public Element GetElementByName(Structure structure, string path)
        {
            return structure.Elements.FirstOrDefault(element => element.Path.ToString() == path);
        }
    }

}
