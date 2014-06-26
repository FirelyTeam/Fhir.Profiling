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
    
    public class Profile
    {
        public List<ValueSet> ValueSets = new List<ValueSet>();
        public List<Structure> Structures = new List<Structure>();

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
