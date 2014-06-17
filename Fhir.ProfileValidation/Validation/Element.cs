﻿/*
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
        public List<TypeRef> TypeRefs = new List<TypeRef>();
        public bool HasTypeRef
        {
            get
            {
                return TypeRefs.Count > 0;
            }
        }
        public List<Element> Children = new List<Element>();
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
        public string ReferenceUri;
        public ValueSet Reference;
        public string Namespace { get; set; } // namespace key (f=fhir, xhtml, etc.)
        public override string ToString()
        {
            return string.Format("{0} ({1})", Path.ToString(), Cardinality);
        }
    }
}