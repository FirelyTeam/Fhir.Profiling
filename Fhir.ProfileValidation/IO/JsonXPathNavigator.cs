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

        public override bool MoveToFirstAttribute()
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else
                throw new NotImplementedException();
        }

        public override bool MoveToFirstChild()
        {
            if (NodeType == XPathNodeType.Root)
            {
                position.Element = position.Root.Properties().First();
                return true;
            }
            else
                throw new NotImplementedException();
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

        public override bool MoveToNext()
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

        public override bool MoveToParent()
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else
                throw new NotImplementedException();
        }

        public override bool MoveToPrevious()
        {
            if (NodeType == XPathNodeType.Root)
                return false;
            else
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
                if (NodeType == XPathNodeType.Element)
                {
                    return position.Element.Value == null || position.Element.Value.Type == JTokenType.Null;
                }
                else
                    return false;
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
            get { return nt(FHIR_NS); }
        }

        public override string Prefix
        {
            get { return nt(FHIR_PREFIX); }
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
                throw new NotImplementedException(); 
                // Navigate over all child elements and append Value
            }   // Probably only works on OnTextNode?
        }

        private class NavigatorState
        {
            public NavigatorState(JObject root)
            {
                Root = root;
            }

            public JObject Root { get; set; }
            public JProperty Element { get; set; }
            public int? ChildPos { get; set; }
            public int? ArrayPos { get; set; }
            public int? AttributePos { get; set; }
            public bool OnTextNode { get; set;}

            public bool OnRoot { get { return Root != null && Element == null;  } }

            public bool OnElement
            {
                get { return Element != null && !OnAttribute && !OnTextNode; }                
            }

            public bool OnAttribute
            {
                get { return AttributePos != null; }
            }

            public IEnumerable<JProperty> Elements { get; set; }
            public IEnumerable<JProperty> Attributes { get; set; }

            public NavigatorState Copy()
            {
                var result = new NavigatorState(this.Root);

                result.Element = Element;
                result.ChildPos = ChildPos;
                result.ArrayPos = ArrayPos;
                result.AttributePos = AttributePos;
                result.OnTextNode = OnTextNode;

                // Also copy the cached list of attributes and elements
                result.Elements = Elements;
                result.Attributes = Attributes;

                return result;
            }

            public bool SamePos(NavigatorState other)
            {
                return Root == other.Root &&
                       Element == other.Element &&
                       ChildPos == other.ChildPos &&
                       ArrayPos == other.ArrayPos &&
                       AttributePos == other.AttributePos &&
                       OnTextNode == other.OnTextNode;
            }

            public override string ToString()
            {
                var result = new StringBuilder();
                if (Root == null) result.Append("[Uninitialized]");
                if (Root != null) result.Append("[Root]");
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
