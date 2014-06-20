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
    internal class NavigatorState
    {
        private const string SPEC_NODE_ROOT = "(root)";

        public NavigatorState(JObject root)
        {
            Element = new JProperty(SPEC_NODE_ROOT, root);
        }

        public NavigatorState(JProperty pos, string parentPath)
        {
            Element = pos;
            ParentPath = parentPath;
        }

        private NavigatorState()
        {
        }

        public JProperty Element { get; private set; }
        public int? ChildPos { get; private set; }
        public int? AttributePos { get; private set; }
        public string ParentPath { get; private set; }

        // Transient variable containing cached list of children,
        // so we safe time recompiling these when navigating back and forth
        private List<JProperty> _children;

        public IEnumerable<JProperty> Children
        {
            get
            {
                if (_children == null) _children = Element.ElementChildren().ToList();
                return _children;
            }
        }

        public bool OnRootElement
        {
            get { return Element.Name == SPEC_NODE_ROOT; }
        }

        public bool IsSameState(NavigatorState other)
        {
            return ChildPos == other.ChildPos &&
                    AttributePos == other.AttributePos &&
                    Path == other.Path;  // Can't compare Element, it's a reference
        }

        public NavigatorState Copy()
        {
            var result = new NavigatorState();
            result.Element = Element;
            result.ChildPos = ChildPos;
            result.AttributePos = AttributePos;
            result.ParentPath = ParentPath;

            return result;
        }

        public string Name
        {
            get { return Element.Name; }
        }

        public string Path
        {
            get { return ParentPath + "/" + Name; }
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
        