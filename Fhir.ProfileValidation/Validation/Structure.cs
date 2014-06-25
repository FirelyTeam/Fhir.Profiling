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
        public IEnumerable<Element> SlicingElements
        {
            get
            {
                return Elements.Where(e => e.Slicing != null);
            }
        }
        public Element FindParent(Element element)
        {
            Path p = element.Path.Parent();
            Element parent = Elements.Find(e => e.Path.Equals(p));
            return parent;
        }

        public bool TryLinkToSlice(Element element)
        {
            Path p = element.Path;
            Element slicer = SlicingElements.FirstOrDefault(e => e.Path.Equals(p));
            if (slicer != null)
            {
                slicer.Slicing.Elements.Add(element);
                return true;
            }
            return false;
        }

        public bool TryLinkToParent(Element element)
        {
            Element parent = FindParent(element);
            if (parent != null)
            {
                parent.Children.Add(element);
                return true;
            }
            return false;
        }
        
        public void LinkToElement(Element e)
        {
            TryLinkToSlice(e);
            TryLinkToParent(e);
        }

        public void LinkElements()
        {
            foreach (Element e in Elements)
            {
                LinkToElement(e);
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
