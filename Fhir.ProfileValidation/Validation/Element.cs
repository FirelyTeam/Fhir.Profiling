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

namespace Fhir.Profiling
{
    public class Element
    {
        public string Name;
        public Path Path { get; set; }
        public Segment Segment
        {
            get
            {
                return Path.Last;
            }
        }
        public List<TypeRef> TypeRefs = new List<TypeRef>();
        public bool HasTypeRef
        {
            get
            {
                return TypeRefs.Count > 0;
            }
        }
        public string ElementRefPath { get; set; }
        public Element ElementRef { get; set; }
        public List<Element> Children = new List<Element>();
        public bool Multi
        {
            get 
            {
                return Path.Segments.Last().Multi;
            }
        }
        public bool IsRoot
        {
            get
            {
                return Path.Count == 1;
            }
        }
        public string NodeMatch
        {
            get
            {
                string xpath;

                if (Segment.Multi)
                {
                    xpath = string.Format("./*[starts-with(name(),'{0}')]", Segment.Name);
                }
                else
                {
                    xpath = string.Format("./{0}:{1}", this.Namespace, Segment.Name);
                }
                return xpath;
            }
        }

        public bool HasChild(string name)
        {
            return this.Children.FirstOrDefault(c => c.Name == name) != null;
        }
        public bool HasChildren
        {
            get
            {
                return Children.Count > 0;
            }
        }
        public Cardinality Cardinality;
        public List<Constraint> Constraints = new List<Constraint>();
        public string BindingUri;
        public ValueSet Binding;
        public string Namespace { get; set; } // namespace key (f=fhir, xhtml, etc.)
        public override string ToString()
        {
            return string.Format("{0} ({1})", Path.ToString(), Cardinality);
        }
    }
}
