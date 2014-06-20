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
        public NavigatorState(JProperty pos)
        {
            Element = pos;
        }

        private NavigatorState()
        {
        }

        public JProperty Element { get; private set; }
        public int? ChildPos { get; set; }
        public int? AttributePos { get; set; }

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

        public NavigatorState Copy()
        {
            var result = new NavigatorState();
            result.Element = Element;
            result.ChildPos = ChildPos;
            result.AttributePos = AttributePos;

            return result;
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
        