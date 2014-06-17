using System;
using System.IO;
using System.Xml.XPath;
using Fhir.Profiling.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Fhir.Profiling.Tests
{
    [TestClass]
    public class JsonNavigatorTests
    {
        [TestMethod]
        public void TestConstruct()
        {
            var nav = buildNav();

            // At root;
            Assert.AreEqual(XPathNodeType.Root,nav.NodeType );
            Assert.IsFalse(nav.IsEmptyElement);
            Assert.AreEqual(String.Empty, nav.Name);
            Assert.AreEqual(String.Empty, nav.LocalName);
            Assert.AreEqual(String.Empty, nav.NamespaceURI);
            Assert.AreEqual(String.Empty, nav.Prefix);
         //   Assert.AreEqual("5text", nav.Value);
        }


        [TestMethod]
        public void TestRootToChild()
        {
            var nav = buildNav();
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
            Assert.IsFalse(nav.IsEmptyElement);
            Assert.AreEqual("f:prop1", nav.Name);
            Assert.AreEqual("prop1", nav.LocalName);
            Assert.AreEqual(JsonXPathNavigator.FHIR_NS , nav.NamespaceURI);
            Assert.AreEqual(JsonXPathNavigator.FHIR_PREFIX, nav.Prefix);
        }

        private static JsonXPathNavigator buildNav()
        {
         //   JsonReader r = new JsonTextReader(new StringReader(@"{ prop1: 5, prop2: 'text'}"));
            JsonReader r = new JsonTextReader(new StringReader(@"{ prop1: 5, prop2: 'text'}"));
            var nav = new JsonXPathNavigator(r);
            return nav;
        }
    }
}
