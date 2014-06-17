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
        private const string FHIR_NS = "http://hl7.org/fhir";
        private const string XHTML_NS = "http://www.w3.org/1999/xhtml";

        private readonly NameTable _nameTable = new NameTable();

        private readonly Stack<NavigatorState> _state = new Stack<NavigatorState>();

        private NavigatorState current { get { return _state.Peek(); } }


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
                return current.SamePos(xpn.current);
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
            throw new NotImplementedException();
        }

        public override bool MoveToFirstChild()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToId(string id)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToNext()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToNextAttribute()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToParent()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToPrevious()
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
                if (current.OnRoot) return false;

                else
                    throw new NotSupportedException();

            }
        }

        public override string Name
        {
            get
            {
                if (NodeType == XPathNodeType.Root)
                    return nt(String.Empty);
                else
                    throw new NotImplementedException();
            }
        }

        public override string LocalName
        {
            get
            {
                if (NodeType == XPathNodeType.Root)
                    return nt(String.Empty);
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
            get { return nt(String.Empty); }
        }

        public override string Prefix
        {
            get { return nt(String.Empty); }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                if (current.OnRoot)
                    return XPathNodeType.Root;
                else if (current.OnElement)
                    return XPathNodeType.Element;
                else if (current.OnTextNode)
                    return XPathNodeType.Text;
                else
                {
                    throw new NotSupportedException("Internal logic error. Underlying source is at " + current.ToString());
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
                ElementPos = -1;
                ArrayPos = -1;
                AttributePos = -1;
            }

            public JObject Root { get; set; }
            public JObject Current { get; set; }
            public int? ElementPos { get; set; }
            public int? ArrayPos { get; set; }
            public int? AttributePos { get; set; }
            public bool OnTextNode { get; set;}

            public bool OnRoot { get { return Root != null && Current == null;  } }

            public bool OnElement
            {
                get { return Current != null && !OnAttribute && !OnTextNode; }                
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

                result.Current = Current;
                result.ElementPos = ElementPos;
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
                       Current == other.Current &&
                       ElementPos == other.ElementPos &&
                       ArrayPos == other.ArrayPos &&
                       AttributePos == other.AttributePos &&
                       OnTextNode == other.OnTextNode;
            }

            public override string ToString()
            {
                var result = new StringBuilder();
                if (Root == null) result.Append("[Uninitialized]");
                if (Root != null) result.Append("[Root]");
                if (Current != null) result.AppendFormat("[Current: {0}]",Current.Path);
                if (ElementPos != null) result.AppendFormat("[ElementIx: {0}]", ElementPos.Value);
                if (ArrayPos != null) result.AppendFormat("[ArrayIx: {0}]", ArrayPos.Value);
                if (AttributePos != null) result.AppendFormat("[AttributeIx: {0}]", AttributePos.Value);
                if (OnTextNode) result.Append("[OnTextNode]");
                return result.ToString();
            }
        }
    }
}
