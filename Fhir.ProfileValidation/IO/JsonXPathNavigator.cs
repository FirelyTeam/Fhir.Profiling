﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fhir.Profiling.IO
{
    public class JsonXPathNavigator : XPathNavigator
    {
        public const string XHTML_NS = "http://www.w3.org/1999/xhtml";
        public const string FHIR_NS = "http://hl7.org/fhir";
        public const string FHIR_PREFIX = "f";
        public const string SPEC_CHILD_ID = "id";

        private readonly NameTable _nameTable = new NameTable();

        private readonly Stack<NavigatorState> _state = new Stack<NavigatorState>();

        private NavigatorState position { get { return _state.Peek(); } }

        public JsonXPathNavigator(JsonReader reader)
        {
            reader.DateParseHandling = DateParseHandling.None;
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            try
            {
                _state.Push(new NavigatorState(JObject.Load(reader)));
            }
            catch (Exception e)
            {
                throw new FormatException("Cannot parse json: " + e.Message);
            }

            _nameTable.Add(FHIR_NS);
            _nameTable.Add(XHTML_NS);
            _nameTable.Add(String.Empty);
            _nameTable.Add(FHIR_PREFIX);
        }

        public JsonXPathNavigator(JsonXPathNavigator source)
        {
            copyState(source._state);

            _nameTable = source._nameTable;
        }

        private void copyState(IEnumerable<NavigatorState> other)
        {
            _state.Clear();

            foreach (var state in other.Reverse())
            {
                _state.Push(state.Copy());
            }
        }

        public override string BaseURI
        {            
            get { return _nameTable.Get(String.Empty); }
        }

        public override XPathNavigator Clone()
        {
            return new JsonXPathNavigator(this);
        }
     
        public override bool IsSamePosition(XPathNavigator other)
        {
            var xpn = other as JsonXPathNavigator;

            if (xpn != null)
                return position.SamePos(xpn.position);
            else
                throw new NotSupportedException("The other navigator must also be a JsonXPathNavigator");
        }

        public override bool MoveTo(XPathNavigator other)
        {
            var xpn = other as JsonXPathNavigator;

            if (xpn != null)
            {
                copyState(xpn._state);
                return true;
            }
            else
                return false;
        }

        public override bool MoveToFirstChild()
        {          
            if (NodeType == XPathNodeType.Element || NodeType == XPathNodeType.Root)
            {
                return moveToElementChild(0);
            }
            else
                return false;
        }

        public override bool MoveToNext()
        {
            if (NodeType == XPathNodeType.Root)
                return false;

            return tryMoveToSibling(1);
        }

        public override bool MoveToPrevious()
        {
            if (NodeType == XPathNodeType.Root)
                return false;

            return tryMoveToSibling(-1);
        }

        public override bool MoveToParent()
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else
            {
                _state.Pop();
                return true;
            }
        }

        private bool tryMoveToSibling(int delta)
        {
            // Move to the next/prev sibling. This means moving up to the parent, and continue with the next/prev node
            if (!_state.Any()) return false;

            // Keep the current state, when we cannot move after we've moved up to the parent, we'll need to stay
            // where we are
            var currentState = _state.Pop();
            
            // Can we move?
            if (NodeType == XPathNodeType.Element)
            {
                var newPos = position.ChildPos.Value + delta;
                if (canMoveToElementChild(newPos))
                {
                    moveToElementChild(newPos);
                    return true;
                }
                else
                {
                    // we cannot, roll back to old state
                    _state.Push(currentState);
                    return false;
                }
            }
            else if (NodeType == XPathNodeType.Root)
            {
                // we cannot, roll back to old state
                _state.Push(currentState);
                return false;
            }
            else
                throw new InvalidOperationException(
                    "Internal logic error: popping state on tryMove does not end up on element");
        }

        private bool canMoveToElementChild(int index)
        {
            var count = elementChildren().Count();
            return index >= 0 && index < count;
        }

        private bool moveToElementChild(int index)
        {
            var child = elementChildren().Skip(index).FirstOrDefault();
            if (child == null) return false;

            position.ChildPos = index;

            var newState = new NavigatorState(child);
            _state.Push(newState);

            return true;
        }

        private bool canMoveToAttributeChild(int index)
        {
            var count = attributeChildren().Count();
            return index >= 0 && index < count;
        }

        private bool moveToAttributeChild(int index)
        {
            var child = attributeChildren().Skip(index).FirstOrDefault();
            if (child == null) return false;

            position.AttributePos = index;

            return true;
        }


        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else
                throw new NotImplementedException();
        }

        public override bool MoveToId(string id)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToFirstAttribute()
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else if (NodeType == XPathNodeType.Element)

            else
                throw new NotImplementedException();
        }

        public override bool MoveToNextAttribute()
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else
                throw new NotImplementedException();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            throw new NotImplementedException();
        }

        private string nt(string val)
        {
            return _nameTable.Get(val);
        }

        public override bool IsEmptyElement
        {
            get
            {
                return !position.Children.Any() || position.OnNullValueElement;
            }
        }

        public override string Name
        {
            get
            {
                var pref = Prefix != String.Empty ? Prefix + ":" : String.Empty;
                return _nameTable.Add(pref + LocalName);
            }
        }

        public override string LocalName
        {
            get
            {
                if (NodeType == XPathNodeType.Root)
                    return nt(String.Empty);
                else if (NodeType == XPathNodeType.Element)
                {
                    //var name = position.Element.Name;
                    //if (name.StartsWith("_")) name = name.Substring(1);
                    return _nameTable.Add(position.Element.Name);
                }
                else
                    throw new NotImplementedException();
            }
        }

        public override System.Xml.XmlNameTable NameTable
        {
            get { return _nameTable; }
        }

        public override string NamespaceURI
        {
            get { return (NodeType == XPathNodeType.Root) ? nt(String.Empty) : nt(FHIR_NS); }
        }

        public override string Prefix
        {
            get { return (NodeType == XPathNodeType.Root) ? nt(String.Empty) : nt(FHIR_PREFIX); }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                if (position.OnRootElement)
                    return XPathNodeType.Root;
                else if (position.OnNormalElement)
                    return XPathNodeType.Element;
                else if (position.OnValueElement)
                    return XPathNodeType.Text;
                else
                {
                    throw new NotSupportedException("Internal logic error. Can't figure out NodeType. Underlying source is at " + position.ToString());
                }
            }
        }

     
        public override string Value
        {
            get
            {
                return position.Text;
            }   
        }

        public override string ToString()
        {
            return String.Join("/", _state);
        }

        private class NavigatorState
        {
            private const string SPEC_NODE_ROOT = "(root)";


            public NavigatorState(JObject root)
            {
                Element = new JProperty(SPEC_NODE_ROOT, root);
            }

            public NavigatorState(JProperty pos)
            {
                Element = pos;
            }

            public JProperty Element { get; private set; }
            public int? ChildPos { get; set; }
            public int? AttributePos { get; set; }

            // Transient variable containing cached list of children
            private List<JProperty> _children;

            private IEnumerable<JProperty> allChildren
            {
                get
                {
                    if (_children == null) _children = getChildren(Element).ToList();
                    return _children;
                }
            }

            public IEnumerable<JProperty> ElementChildren()
            {
                return allChildren.Where(c => !isAttribute(c));
            }

            public IEnumerable<JProperty> AttributeChildren()
            {
                return allChildren.Where(c => isAttribute(c));
            }

            private bool isAttribute(JProperty property)
            {
                return property.Name == NavigatorState.SPEC_NODE_VALUE || property.Name == SPEC_CHILD_ID;
            }

            public bool OnRootElement
            {
                get { return Element.Name == SPEC_NODE_ROOT; }
            }

            public bool OnValueElement
            {
                get { return Element.Name == SPEC_NODE_VALUE; }
            }

            public bool OnNormalElement
            {
                get { return !OnRootElement && !OnAttribute && !OnValueElement; }
            }

            public bool OnNullValueElement
            {
                get { return OnValueElement && Element.Value.Type == JTokenType.Null; }
            }

            public bool OnAttribute
            {
                get { return AttributePos != null; }
            }

            public string Text
            {
                get
                {
                    if (OnAttribute)
                    {
                        // This means position.Element is a JProperty pointing to a primitive
                        return 
                    }
                    else
                    {
                        return String.Join("", position.GetChildren().Select(c => new NavigatorState(c).Text));
                    }
                }
            }
        

            private static IEnumerable<JProperty> getChildren(JProperty parent)
            {
                var parentObject = parent.Value as JObject;

                if (parentObject == null)
                    throw new ArgumentException("Can only get children for a JObject parent", "parent");

                //if (parent.Name == SPEC_NODE_VALUE)
                //    throw new ArgumentException("Cannot get children for pseudo-primitive (value) node", "parent");

              
            }


            public NavigatorState Copy()
            {
                var result = new NavigatorState(Element);
                result.ChildPos = ChildPos;
                result.AttributePos = AttributePos;

                return result;
            }

            public bool SamePos(NavigatorState other)
            {
                return Element == other.Element &&
                       ChildPos == other.ChildPos &&
                       AttributePos == other.AttributePos;
            }

            public override string ToString()
            {               
                if (Element == null) return ("[Uninitialized]");

                var result = new StringBuilder();

                result.Append(Element.Name);

                if (ChildPos != null || AttributePos != null)
                {
                    result.Append("{");
                    if (ChildPos != null) result.AppendFormat("Child: {0},", ChildPos.Value);
                    if (AttributePos != null) result.AppendFormat("Attr: {0},", AttributePos.Value);
                    result.Append("}");
                }

                return result.ToString();
            }
        }
    }
}
