using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Fhir.Profiling.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Fhir.Profiling.Tests
{
    [TestClass]
    public class JTokenExtensionTests
    {
        private Newtonsoft.Json.Linq.JObject getPatientExample()
        {
            Stream example = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Fhir.Profiling.Tests.TestPatient.json");

            return (JObject)JObject.ReadFrom(new JsonTextReader(new StreamReader(example)));
        }

        [TestMethod]
        public void TestBasics()
        {
            var root = getPatientExample().AsElementRoot();

            Assert.IsNotNull(root);
            Assert.IsTrue(root.IsRoot());

            var children = root.ElementChildren();
                    
            Assert.IsTrue(children.Any(c => c.Name == "resourceType"));
            Assert.AreEqual(1,children.Count(c => c.Name == "identifier"));
            Assert.AreEqual(2,children.Count(c => c.Name == "name"));
            Assert.AreEqual(1, children.Count(c => c.Name == "birthDate"));
        }

        [TestMethod]
        public void TestPrimitives()
        {
            var root = getPatientExample().AsElementRoot();
            var children = root.ElementChildren();

            var bd = children.Single(c => c.Name == "birthDate");
            Assert.IsTrue(bd.Value is JObject);     // primitive has been turned into a JObject
            var bdVal = (JObject)bd.Value;
            var prim = bdVal.Properties().Single();
            Assert.IsTrue(prim.IsPrimitive());
            Assert.AreEqual("1974-12", prim.Value);
        }

        [TestMethod]
        public void TestExtendedProp()
        {
            throw new NotImplementedException();
            //TODO: test normal case
                //TODO: Moet 1 zijn!     Assert.AreEqual(0,children.Count(c => c.Name == "_active"));
        }

        [TestMethod]
        public void TestArrays()
        {
            var root = getPatientExample().AsElementRoot();
            var children = root.ElementChildren();

            var names = children.Where(c => c.Name == "name");
            Assert.AreEqual(2,names.Count());
            Assert.IsTrue(names.All(n => n.Value is JObject));

            // move into first given name
            var name1 = names.First();
            var name1Children = name1.ElementChildren();
            var firstNames = name1Children.Where(c => c.Name == "given");
            Assert.AreEqual(2, firstNames.Count());

            Assert.IsTrue(firstNames.All(n => n.Value is JObject));
            Assert.AreEqual("Peter",firstNames.First().PrimitiveValue());
            Assert.AreEqual("James",firstNames.Skip(1).First().PrimitiveValue());
        }
    }
}
