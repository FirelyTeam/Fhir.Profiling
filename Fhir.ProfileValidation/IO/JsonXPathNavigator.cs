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
        public const string FHIR_NS = "http://hl7.org/fhir";
        public const string XHTML_NS = "http://www.w3.org/1999/xhtml";
        public const string FHIR_PREFIX = "f";

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

            foreach (var state in other)
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
            //if (NodeType == XPathNodeType.Root)
            //{
            //    position.Element = position.Children.First();
            //    return true;
            //}
            
            if (NodeType == XPathNodeType.Element || NodeType == XPathNodeType.Root)
            {
                position.ChildPos = 0;
                return moveToChild(0);

                //if (position.Element.Value is JObject)
                //{
                //    position.ChildPos = 0;
                //    return moveToChild(0);
                //}
                //else if (position.Element.Value is JValue)
                //{
                //    position.OnTextNode = true;
                //    return true;
                //}
                //else
                //{
                //    throw new NotImplementedException();
                //}
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
                if (canMoveTo(newPos))
                {
                    position.ChildPos = newPos;
                    moveToChild(newPos);
                    return true;
                }
                else
                {
                    // we cannot, roll back to old state
                    _state.Push(currentState);
                    return false;
                }
            }
            else
                throw new InvalidOperationException(
                    "Internal logic error: popping state on tryMove does not end up on element");
        }

        private bool canMoveTo(int index)
        {
            var count = position.Children.Count();
            return index >= 0 && index < count;
        }

        private bool moveToChild(int index)
        {
            var child = position.Children.Skip(index).FirstOrDefault();
            if (child == null) return false;

            var newState = new NavigatorState(child);
            _state.Push(newState);

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
                //if (NodeType == XPathNodeType.Element)
                //{
                //    if(position.Element.Value == null) return true;
                //    if(position.Element.Value.Type == JTokenType.Null) return true;
                //    if (position.Element.Value is JObject && !position.Element.Value.Any()) return true;
                //    if (position.Element.Value is JArray && !position.Element.Value.Any()) return true;

                //    return false;
                //}
                //else
                //    return false;

                return !position.Children.Any() || position.Children.Single().Value.Type == JTokenType.Null;
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
                if (position.OnRoot)
                    return XPathNodeType.Root;
                else if (position.OnElement)
                    return XPathNodeType.Element;
                else if (position.OnTextNode)
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
                if (position.OnTextNode)
                {
                    // This means position.Element is a JProperty pointing to a primitive
                    return position.Element.Value.ToString();
                }
                else
                {
                    // TODO: Navigate over all child elements and append Value
                    throw new NotImplementedException();
                }
            }   
        }

        private class NavigatorState
        {
            private const string SPEC_NODE_ROOT = "(root)";
            private const string SPEC_NODE_TEXT = "(text)";

            public NavigatorState(JObject root)
            {
                Element = new JProperty(SPEC_NODE_ROOT, root);
            }

            public NavigatorState(JProperty pos)
            {
                Element = pos;
            }

            public JProperty Element { get; set; }
            public int? ChildPos { get; set; }
            public int? ArrayPos { get; set; }
            public int? AttributePos { get; set; }

            // Transient variable containing cached list of children
            private IEnumerable<JProperty> _children;

            public IEnumerable<JProperty> Children
            {
                get
                {
                    if (_children == null)
                        _children = NavigatorState.GetChildren(Element);
                    return _children;
                }

            }

            public bool OnRoot
            {
                get { return Element.Name == SPEC_NODE_ROOT; }
            }

            public bool OnElement
            {
                get { return !OnRoot && !OnAttribute && !OnTextNode; }                
            }

            public bool OnTextNode
            {
                get { return Element.Name == SPEC_NODE_TEXT; }
            }

            public bool OnAttribute
            {
                get { return AttributePos != null; }
            }

            public static IEnumerable<JProperty> GetChildren(JProperty parent)
            {
                if (parent.Value == null) yield break;

                // Simulate anonymous "text node" within the parent
                if (parent.Value is JValue)
                    yield return new JProperty(SPEC_NODE_TEXT, parent.Value);

                else if (parent.Value is JObject)
                {
                    var props = ((JObject) parent.Value).Properties();
                    foreach (var prop in props)
                    {
                        // If the property is an Array, return it as sibling properties
                        if (prop.Value is JArray)
                        {
                            foreach (var elem in prop.Value.Children())
                            {
                                yield return new JProperty(prop.Name, elem);
                            }
                        }
                        else
                            yield return prop;
                    }
                }                

                else
                    throw new InvalidOperationException(
                        "Internal logic error. Was not expecting to find children for a " + parent.Value.GetType().Name);
            }


            public NavigatorState Copy()
            {
                var result = new NavigatorState(Element);
                result.ChildPos = ChildPos;
                result.ArrayPos = ArrayPos;
                result.AttributePos = AttributePos;

                return result;
            }

            public bool SamePos(NavigatorState other)
            {
                return Element == other.Element &&
                       ChildPos == other.ChildPos &&
                       ArrayPos == other.ArrayPos &&
                       AttributePos == other.AttributePos;
            }

            public override string ToString()
            {
                var result = new StringBuilder();
                if (Element == null) result.Append("[Uninitialized]");
                if (OnRoot) result.Append("[Root]");
                if (Element != null) result.AppendFormat("[Element: {0}]",Element.Path);
                if (ChildPos != null) result.AppendFormat("[ElementIx: {0}]", ChildPos.Value);
                if (ArrayPos != null) result.AppendFormat("[ArrayIx: {0}]", ArrayPos.Value);
                if (AttributePos != null) result.AppendFormat("[AttributeIx: {0}]", AttributePos.Value);
                if (OnTextNode) result.Append("[OnTextNode]");
                return result.ToString();
            }
        }
    }
}
