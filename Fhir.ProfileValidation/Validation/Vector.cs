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
using System.Xml;
using System.Xml.XPath;

namespace Fhir.Profiling
{

    public struct Vector
    {
        public Structure Structure;
        public Element Element;
        public XPathNavigator Node;
        public XmlNamespaceManager NSM;

        public static Vector Create(Structure structure, XPathNavigator node, XmlNamespaceManager nsm)
        {
            Vector vector;
            vector.Structure = structure;
            vector.Element = (structure != null) ? structure.Root : null;
            vector.Node = node;
            vector.NSM = nsm;
            return vector;
        }
        
        public static Vector Void()
        {
            return Create(null, null, null);
        }
        
        public Vector Clone()
        {
            Vector clone;
            clone.Structure = this.Structure;
            clone.Element = this.Element;
            clone.Node = this.Node.CreateNavigator();
            clone.NSM = this.NSM;
            return clone;
        }

        public Vector MoveTo(Element element)
        {
            Vector clone = this.Clone();
            clone.Element = element;
            return clone;
        }

        public Vector MoveTo(Element element, XPathNavigator node)
        {
            Vector clone = this.Clone();
            clone.Element = element;
            clone.Node = node;
            return clone;
        }
        
        public Vector MoveTo(Structure structure)
        {
            Vector clone = this.Clone();
            clone.Structure = structure;
            clone.Element = structure.Root;
            return clone;
        }

        public Vector MoveTo(XPathNavigator node)
        {
            Vector clone = this.Clone();
            clone.Node = node;
            return clone;
        }

        public bool Evaluate(Constraint constraint)
        {
            return (bool)Node.Evaluate(constraint.XPath, NSM);
        }
        
        public bool Exists(string xpath)
        {
            XPathNavigator node = Node.SelectSingleNode(xpath, NSM);
            return (node != null);
        }

        public int Count()
        {
            string xpath = string.Format("./{0}:{1}", Element.Namespace, Element.Name); //"./f:" + element.Name;
            XPathNodeIterator iterator = Node.Select(xpath, NSM);
            return iterator.Count;
        }

        public string GetValue(string xpath)
        {
            return Node.SelectSingleNode(xpath, NSM).ToString();
        }
        
        public string NodePath()
        {
            XPathNavigator n = Node.CreateNavigator();
            string s = n.Name;
            while (n.MoveToParent())
            {
                if (!string.IsNullOrEmpty(n.Name))
                    s = n.Name + "." + s;
            }
            return s;
        }
        
        public IEnumerable<Vector> ElementChildren
        {
            get
            {
                foreach (Element element in Element.Children)
                {
                    yield return this.MoveTo(element);
                }
            }
        }

        public IEnumerable<Vector> Matches
        {
            get
            {
                string xpath = this.Element.Path.NodeMatch;
               
                XPathNodeIterator nodes = Node.Select(xpath, NSM);
                foreach (XPathNavigator node in nodes)
                {
                    yield return this.MoveTo(node);
                }
            }
        }

        public IEnumerable<Vector> NodeChildren
        {
            get
            {
                foreach (XPathNavigator node in Node.SelectChildren(XPathNodeType.Element))
                {
                    yield return this.MoveTo(node);
                }
            }
        }
        
        public IEnumerable<Vector> ElementStructures
        {
            get
            {
                foreach (TypeRef t in Element.TypeRefs)
                {
                    if (t.Structure != null)
                    {
                        yield return this.MoveTo(t.Structure);

                    }
                }
            }
        }
        
        public bool ElementHasChild(string name)
        {
            return Element.Children.FirstOrDefault(c => c.Name == name) != null;
        }
    }

}
