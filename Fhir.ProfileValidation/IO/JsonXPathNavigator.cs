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
                var root = (JObject)JObject.Load(reader);
                _state.Push(new NavigatorState(root.AsElementRoot()));
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


        private IEnumerable<JProperty> elementChildren()
        {
            return position.Children.Where(c => !isAttribute(c));
        }

        private IEnumerable<JProperty> attributeChildren()
        {
            return position.Children.Where(c => isAttribute(c));
        }

        private bool isAttribute(JProperty property)
        {
            return property.IsPrimitive() || property.Name == SPEC_CHILD_ID;
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
                return position.IsSameState(xpn.position);
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

            throw new NotImplementedException();

            //TODO: need position.NewChildNavigator(index);
            //position.ChildPos = index;

            //var newState = new NavigatorState(child);
            //_state.Push(newState);

            //return true;
        }

        private bool canMoveToAttributeChild(int index)
        {
            var count = attributeChildren().Count();
            return index >= 0 && index < count;
        }

        private bool moveToAttributeChild(int index)
        {
            throw new NotImplementedException();

            //var child = attributeChildren().Skip(index).FirstOrDefault();
            //if (child == null) return false;

            //position.AttributePos = index;

            //return true;
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
                return !position.Children.Any();
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


        //public bool OnValueElement
        //{
        //    get { return Element.IsPrimitive(); }
        //}

        //public bool OnNormalElement
        //{
        //    get { return !OnRootElement && !OnAttribute && !OnValueElement; }
        //}

        //public bool OnNullValueElement
        //{
        //    get { return OnValueElement && Element.Value.Type == JTokenType.Null; }
        //}

        //public bool OnAttribute
        //{
        //    get { return AttributePos != null; }
        //}


        public override XPathNodeType NodeType
        {
            get
            {
                throw new NotImplementedException();
                //if (position.OnRootElement)
                //    return XPathNodeType.Root;
                //else if (position.OnNormalElement)
                //    return XPathNodeType.Element;
                //else if (position.OnValueElement)
                //    return XPathNodeType.Text;
                //else
                //{
                //    throw new NotSupportedException("Internal logic error. Can't figure out NodeType. Underlying source is at " + position.ToString());
                //}
            }
        }

     
        public override string Value
        {
            get
            {
                return position.Element.ElementText();
            }   
        }

        public override string ToString()
        {
            return String.Join("/", _state);
        }
    }
}
