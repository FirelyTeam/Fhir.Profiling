/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using Fhir.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Fhir.IO
{
    public static class FhirFile
    {
        public static List<Structure> LoadXmlFile(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            ProfileReader reader = new ProfileReader();
            return reader.Read(document);
        }

        public static void LoadXmlFile(this Profile profile, string filename)
        {
            List<Structure> structures = LoadXmlFile(filename);
            profile.Add(structures);
        }

        public static Feed LoadXMLFeed(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            return new Feed(document);
        }

        public static Feed.Entry LoadXMLResource(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            
            XPathNavigator navigator;
            XmlNamespaceManager manager;

            Feed.Entry entry = new Feed.Entry();
            entry.Title = filename;
            entry.Id = "unknown";

            navigator = document.CreateNavigator();
            manager = new XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("f", "http://hl7.org/fhir");
            entry.ResourceNode = navigator.SelectSingleNode("*");
            /*
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            return new Feed(document);
            */
            return entry;
        }

        public static Profile LoadXMLValueSets(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            ProfileReader reader = new ProfileReader();
            List<ValueSet> valuesets = reader.ReadValueSets(document);
            Profile profile = new Profile();
            profile.Add(valuesets);
            return profile;
        }

        public static void LoadXMLValueSets(this Profile profile, string filename)
        {
            Profile p = LoadXMLValueSets(filename);
            profile.Add(p);
        }

    }
}
