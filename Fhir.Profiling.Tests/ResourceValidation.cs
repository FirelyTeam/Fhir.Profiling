﻿/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.Profiling;
using Fhir.IO;
using System.Collections.Generic;
using System.Linq;

namespace Fhir.Profiling.Tests
{
    [TestClass]
    public class ResourceValidation
    {

        [TestMethod]
        public void ValidResource()
        {
            var profile = Profiles.Patient;
            var resource = FhirFile.LoadResource("TestData\\Patient.Valid.xml");
            Report report = profile.Validate(resource);
            Assert.IsTrue(report.IsValid);
        }
        
        [TestMethod]
        public void ValueSetUnknown()
        {
            var profile = Profiles.Patient;
            var resource = FhirFile.LoadResource("TestData\\Patient.ErrorUse.xml");
            Report report = profile.Validate(resource);

            Assert.IsFalse(report.IsValid);
            Assert.IsTrue(report.Contains(Group.Coding, Status.Failed));
            Assert.AreEqual(1, report.ErrorCount);
        }

        [TestMethod]
        public void CardinalityTooMuch()
        {
            var profile = Profiles.Patient;
            var resource = FhirFile.LoadResource("TestData\\Patient.CardinalityPlus.xml");
            Report report = profile.Validate(resource);

            Assert.IsFalse(report.IsValid);
            Assert.IsTrue(report.Contains(Group.Cardinality, Status.Failed));
            Assert.AreEqual(1, report.ErrorCount);
        }

        [TestMethod]
        public void CardinalityTooLittle()
        {
            var profile = Profiles.Patient;
            var resource = FhirFile.LoadResource("TestData\\Patient.CardinalityMinus.xml");
            Report report = profile.Validate(resource);

            Assert.IsFalse(report.IsValid);
            Assert.IsTrue(report.Contains(Group.Cardinality, Status.Failed));
            Assert.AreEqual(1, report.ErrorCount);
        }

        [TestMethod]
        public void Constraint()
        {
            // <constraint value="f:name or f:telecom or f:address or f:organization"/>

            var profile = Profiles.Patient;
            var resource = FhirFile.LoadResource("TestData\\Patient.ConstraintError.xml");
            Report report = profile.Validate(resource);

            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.IsTrue(report.Contains(Group.Constraint, Status.Failed));
        }

        [TestMethod]
        public void ReachDanglingElement()
        {
            // tests if elements that do not have direct parents are reached.
            throw new AssertInconclusiveException("There is no solution for this yet");
        }

        [TestMethod]
        public void WrongRootElement()
        {
            var profile = Profiles.Patient;
            var resource = FhirFile.LoadResource("TestData\\invalidroot.xml");
            Report report = profile.Validate(resource);

            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.IsTrue(report.Contains(Group.Structure, Status.Unknown));
        }

        [TestMethod]
        public void CorrectFixedValue()
        {
            var profile = Profiles.Lipid;
            var resource = FhirFile.LoadResource("TestData\\lipid.fixvalue.xml");
            Report report = profile.Validate(resource);
            Assert.IsTrue(report.IsValid);
        }

        [TestMethod]
        public void IncorrectFixedValue()
        {
            var profile = Profiles.Lipid;
            var resource = FhirFile.LoadResource("TestData\\lipid.fixvalue.wrong.xml");
            Report report = profile.Validate(resource);

            Assert.IsFalse(report.IsValid);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.IsTrue(report.Contains(Group.Value, Status.Failed));
        }

        [TestMethod]
        public void NamespaceXHtml()
        {
            var profile = Profiles.Patient;
            var resource = FhirFile.LoadResource("TestData\\Patient.Narrative.correct.xml");
            Report report = profile.Validate(resource);
            report.Errors.ToConsole();

            Assert.IsTrue(report.IsValid);

            // In this narrative node, the div element does not contain a xhtml namespace and should not be found by the validator
            resource = FhirFile.LoadResource("TestData\\Patient.Narrative.wrong.xml");
            report = profile.Validate(resource);
            report.Errors.ToConsole();

            Assert.IsFalse(report.IsValid);
            Assert.IsTrue(report.Contains(Group.Cardinality, Status.Failed));
            Assert.AreEqual(1, report.ErrorCount);
        }

    }
}
