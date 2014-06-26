using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Fhir.Profiling.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Fhir.Profiling.Tests
{
    [TestClass]
    public class XsltContextTests
    {
        [TestMethod]
        public void TestUppercase()
        {
            var result = ConstraintCompiler.Eval("upper-case('hoi!')");
            Assert.AreEqual("HOI!", result);
        }
    }
}
