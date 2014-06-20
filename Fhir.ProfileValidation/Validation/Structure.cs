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
    public class Structure
    {
        public string Name { get; set; }
        public List<Element> Elements = new List<Element>();
        public Structure()
        {
            IsPrimitive = false;
        }
        public Element Root
        {
            get
            {
                return Elements.FirstOrDefault(e => e.Path.Count == 1);
            }
        }
        public Element FindParent(Element element)
        {
            Path p = element.Path.Parent();
            Element parent = Elements.Find(e => e.Path.Equals(p));
            return parent;
        }
        public void BuildHierarchy()
        {
            foreach (Element e in Elements)
            {
                Element parent = FindParent(e);
                if (parent != null)
                {
                    parent.Children.Add(e);
                }
            }
        }
        public override string ToString()
        {
            return string.Format("{0} ({1} elements)", Name, Elements.Count);
        }
        public Boolean IsPrimitive { get; set; }
        public string ValuePattern { get; set; }
        public string Namespace { get; set; }
    }
}
