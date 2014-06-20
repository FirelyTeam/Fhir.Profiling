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
            return position.Children.Where(c => isAttribute(c) && !c.IsNullPrimitive());
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
            throw new NotImplementedException();
            //var xpn = other as JsonXPathNavigator;

            //if (xpn != null)
            //    return position.IsSameState(xpn.position);
            //else
            //    throw new NotSupportedException("The other navigator must also be a JsonXPathNavigator");
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
            if (NodeType == XPathNodeType.Element)
                return tryMoveToSibling(1);
            else
                return false;
        }

        public override bool MoveToPrevious()
        {
            if (NodeType == XPathNodeType.Element)
                return tryMoveToSibling(-1);
            else
                return false;
        }

        public override bool MoveToParent()
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else if (NodeType == XPathNodeType.Element)
            {
                _state.Pop();
                return true;
            }
            else if (NodeType == XPathNodeType.Attribute)
            {
                position.AttributePos = null;
                return true;
            }
            else
                return false;
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

        private JProperty currentAttribute
        {
            get
            {
                if (position.AttributePos != null)
                {
                    attributeChildren().Skip(position.AttributePos.Value);
                }

                return null;
            }
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
            if (NodeType == XPathNodeType.Element)
                return moveToAttributeChild(0);
            else
                return false;
        }

        public override bool MoveToNextAttribute()
        {
            if (NodeType == XPathNodeType.Attribute)
                return moveToAttributeChild(position.AttributePos.Value + 1);
            else
                return false;
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
                    return _nameTable.Add(position.Element.Name);
                else if (NodeType == XPathNodeType.Attribute)
                    return _nameTable.Add(currentAttribute.Name);
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
            get { return (NodeType == XPathNodeType.Root || NodeType == XPathNodeType.Attribute) ? nt(String.Empty) : nt(FHIR_NS); }
        }

        public override string Prefix
        {
            get { return (NodeType == XPathNodeType.Root || NodeType == XPathNodeType.Attribute) ? nt(String.Empty) : nt(FHIR_PREFIX); }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                if (position.Element.IsRoot())
                    return XPathNodeType.Root;
                else if (position.AttributePos == null)
                    return XPathNodeType.Element;
                else if (position.AttributePos != null)
                    return XPathNodeType.Attribute;
                else
                {
                    throw new NotSupportedException("Internal logic error. Can't figure out NodeType. Underlying source is at " + position.ToString());
                }
                //TODO: Simulate Binary.content as NoteType.Text
            }
        }

     
        public override string Value
        {
            get
            {
                if (NodeType == XPathNodeType.Attribute)
                {
                    JValue primitive;
                    if (currentAttribute.IsPrimitive())
                        primitive = (JValue)currentAttribute.Value;
                    else
                    {
                        // This is a named attribute (like id and contentType) for which only the
                        // primitive value is relevant, they cannot be extended
                        primitive = currentAttribute.PrimitivePropertyValue();
                    }

                    // We accept four primitive json types, convert them to the correct xml string representations
                    if (primitive.Type == JTokenType.Integer)
                        return XmlConvert.ToString((Int64)primitive.Value);
                    else if (primitive.Type == JTokenType.Float)
                        return XmlConvert.ToString((Decimal)primitive.Value);
                    else if (primitive.Type == JTokenType.Boolean)
                        return XmlConvert.ToString((bool)primitive.Value);
                    else if (primitive.Type == JTokenType.String)
                        return (string)primitive.Value;
                    else
                        throw new FormatException("Only integer, float, boolean and string primitives are allowed in FHIR Json");
                }
                else
                    return position.Element.ElementText();
            }   
        }

        public override string ToString()
        {
            return String.Join("/", _state);
        }
    }
}
