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

        public void Add(Structure structure)
        {
            Structures.Add(structure);
        }
        public void Add(List<Structure> structures)
        {
            addReferencesTo(structures);
            this.Structures.AddRange(structures);
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

        private void addReferencesTo(List<Structure> structures)
        {
            foreach (Structure s in structures)
            {
                foreach (Element e in s.Elements)
                {
                    foreach (TypeRef r in e.TypeRefs)
                    {
                        r.Structure = this.GetStructureByName(r.Code);
                    }
                    if (e.ReferenceUri != null)
                    {
                        e.Reference = this.GetValueSetByUri(e.ReferenceUri);
                    }
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
    }
}
