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
using System.Text.RegularExpressions;
using System.IO;
using Fhir.IO;
using Fhir.Profiling;
using Fhir;

namespace ProfileValidation
{

    // merge declaring profile with restricting profile (declaring profile is not a standard yet)
    // todo: ValueSet vs. CodeSystem
    
    // todo: resourceReference can refer to <profile> which is a structure.

    // todo: Element names are profile URI specific (names can overlap with other profiles)
    // todo: Merge Profile class (when possible) with Api.Introspection and Api.ModelInfo
    // todo: When an element type is a ResourceReference, then the path does not contain [x]. The restriction is content of the refered resource.
    // todo: Slices
    // todo: Layering (adding the defining resource to the profile, then adding a restricting resource to the same profile)
    // todo: Aggregation (resource references) (resource, feed and world scope)
    
    // done: parse meaning of [x].
    // done: name reference (recursion)
    // done: extensions

    class Program
    {
        public static void Run<T>(ReportMode mode) where T:Tester, new()
        {
            Tester tester = new T();
            tester.Run(mode);
        }

        static void Main(string[] args)
        {
            //Run<PatientTester>(ReportMode.Full);
            //Run<ValueSetsTester>();
            //Run<LipidTester>(ReportMode.Summary); // deze faalt vw. invalide lipid-profile
            //Run<ProfileTester>(ReportMode.Summary);
            Run<LipidProfileTester>(ReportMode.Summary);

            

        }
    }
}
