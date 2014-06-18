using System;
using System.IO;
using System.Xml;
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
            Assert.AreEqual("f:test", nav.Name);
            Assert.AreEqual("test", nav.LocalName);
            Assert.AreEqual(JsonXPathNavigator.FHIR_NS , nav.NamespaceURI);
            Assert.AreEqual(JsonXPathNavigator.FHIR_PREFIX, nav.Prefix);
        }

        [TestMethod]
        public void TestMoveToChild()
        {
            var nav = buildNav();
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.IsTrue(nav.MoveToFirstChild());
            
            Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
            Assert.IsFalse(nav.IsEmptyElement);
            Assert.AreEqual("f:nodeA", nav.Name);

            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
            Assert.AreEqual("5", nav.Value);

            Assert.IsFalse(nav.MoveToNext());       // text node does not have siblings
            Assert.AreEqual(XPathNodeType.Text, nav.NodeType);  // should not have moved
            Assert.AreEqual("5", nav.Value);

            Assert.IsTrue(nav.MoveToParent());      // move up to nodeA
            Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
            Assert.AreEqual("f:nodeA", nav.Name);

            Assert.IsTrue(nav.MoveToNext());        // move to first element of nodeB
            Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
            Assert.AreEqual("f:nodeB", nav.Name);

            Assert.IsTrue(nav.MoveToNext());        // move to second element of nodeB
            Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
            Assert.AreEqual("f:nodeB", nav.Name);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(XPathNodeType.Text, nav.NodeType);
            Assert.AreEqual("hoi", nav.Value);
            Assert.IsTrue(nav.MoveToParent());

            Assert.IsTrue(nav.MoveToNext());        // move to third element of nodeB
            Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
            Assert.AreEqual("f:nodeB", nav.Name);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.IsTrue(nav.IsEmptyElement);
            Assert.IsTrue(nav.MoveToParent());

            Assert.IsTrue(nav.MoveToNext());        // move to nodeC
            Assert.AreEqual(XPathNodeType.Element, nav.NodeType);
            Assert.AreEqual("f:nodeC", nav.Name);

        }


        [TestMethod]
        public void ParentChild()
        {
            var nav = buildNav();
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.IsTrue(nav.MoveToParent());
            Assert.IsTrue(nav.MoveToParent());
            Assert.IsTrue(nav.MoveToParent());

            Assert.IsFalse(nav.MoveToParent());  // one time too many
        }

        [TestMethod]
        public void ForwardBackwards()
        {
            var nav = buildNav();
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.IsTrue(nav.MoveToFirstChild());      // nodeA
            Assert.IsTrue(nav.MoveToNext());    // nodeB[0]
            Assert.IsTrue(nav.MoveToNext());    // nodeB[1]
            Assert.IsTrue(nav.MoveToPrevious());
            Assert.IsTrue(nav.MoveToPrevious());

            Assert.IsFalse(nav.MoveToPrevious());  // one time too many
        }


        [TestMethod]
        public void CloneIsAtSamePosition()
        {
            var nav = buildNav();
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.IsTrue(nav.MoveToFirstChild()); // nodeA
            Assert.IsTrue(nav.MoveToNext()); // nodeB[0]
            Assert.IsTrue(nav.MoveToNext()); // nodeB[1]

            var nav2 = new JsonXPathNavigator(nav);
            Assert.IsTrue(nav.IsSamePosition(nav2));
        }


        [TestMethod]
        public void TestSelect()
        {
            var nav = buildNav();
            var mgr = new XmlNamespaceManager(nav.NameTable);
            mgr.AddNamespace("f", JsonXPathNavigator.FHIR_NS);
            var result = nav.Select("/f:test/f:nodeB", mgr);
            Assert.AreEqual(3, result.Count);
        }

        private static JsonXPathNavigator buildNav()
        {
            JsonReader r = new JsonTextReader(new StringReader(@"{ test : { nodeA: 5, nodeB: [4,'hoi',null], nodeC: 'text'} }"));
            var nav = new JsonXPathNavigator(r);
            return nav;
        }
    }
}
