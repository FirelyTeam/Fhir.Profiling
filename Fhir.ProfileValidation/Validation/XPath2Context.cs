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
using System.Xml.Xsl;
using System.Xml.XPath;

namespace Fhir.Profiling
{
    class XPath2Context : XsltContext
    {
        private readonly List<INamedXsltContextFunction> _functions = new List<INamedXsltContextFunction>();

        public XPath2Context()
        {
            _functions.Add(new UpperCaseFunction());
            _functions.Add(new ExistsFunction());
        }

        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            throw new NotImplementedException();
        }

        public override bool PreserveWhitespace(System.Xml.XPath.XPathNavigator node)
        {
            throw new NotImplementedException();
        }

        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
        {
            var func = _functions.SingleOrDefault(f => f.Name == name && f.Prefix == prefix);

            if(func != null)
                return func;
            else
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


    internal interface INamedXsltContextFunction : IXsltContextFunction
    {
        string Prefix { get; }
        string Name { get; }
    }
   
    internal class UpperCaseFunction : CustomXsltFunction
    {
        public UpperCaseFunction() : base("upper-case", new XPathResultType[1] { XPathResultType.String }, XPathResultType.String) {}

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args.Count() != 1) throw new ArgumentException("upper-case accepts only 1 argument");

            return ((string)args[0]).ToUpperInvariant();
        }
    }

    internal class DistinctValuesFunction : CustomXsltFunction
    {
        public DistinctValuesFunction() : base("distinct-values", new XPathResultType[1] { XPathResultType.NodeSet }, XPathResultType.NodeSet) { }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args.Count() != 1) throw new ArgumentException("distinct-values accepts only 1 argument");

            return null;
        }
    }

    internal class ExistsFunction : CustomXsltFunction
    {
        public ExistsFunction() : base("exists", new XPathResultType[1] { XPathResultType.NodeSet }, XPathResultType.Boolean) { }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args.Count() != 1) throw new ArgumentException("exists accepts only 1 argument");

            return false;
        }
    }
}
