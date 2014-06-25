using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Fhir.Profiling
{
    class XPath2Context : XsltContext
    {
        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            throw new NotImplementedException();
        }

        public override bool PreserveWhitespace(System.Xml.XPath.XPathNavigator node)
        {
            throw new NotImplementedException();
        }

        public override IXsltContextFunction ResolveFunction(string prefix, string name, System.Xml.XPath.XPathResultType[] ArgTypes)
        {
            throw new InvalidOperationException("Unknown function in XPath: " + name);
        }

        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            throw new NotImplementedException();
        }

        public override bool Whitespace
        {
            get {
                return true;
            }
        }
    }
}
