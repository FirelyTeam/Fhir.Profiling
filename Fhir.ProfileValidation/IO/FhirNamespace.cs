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

namespace Fhir.Profiling
{
    public static class Namespace
    {
        public const string Fhir = "f";
        public const string Atom = "atom";
        public const string XHtml = "xhtml";

        public static XmlNamespaceManager GetManager(XPathNavigator navigator)
        {
            XmlNamespaceManager nsm = new XmlNamespaceManager(navigator.NameTable);
            nsm.AddNamespace(Namespace.Fhir, "http://hl7.org/fhir");
            nsm.AddNamespace(Namespace.Atom, "http://www.w3.org/2005/Atom");
            nsm.AddNamespace(Namespace.XHtml, "http://www.w3.org/1999/xhtml");
            return nsm;

        }
    }
}
