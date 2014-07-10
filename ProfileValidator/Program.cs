/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using System;
using System.Diagnostics;
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

    // todo: fixed value testen (er is nog geen testdata)
    
    // todo: ValueSet vs. CodeSystem (is dit hetzelfde als een binding?)

    // todo: merge declaring profile with restricting profile (declaring profile is not a standard yet)
    // BESPREKEN: een profile is een restrictie op een basis profiel. Als we dus het basis profiel eerst inladen,
    // dan moet een profiel hiervan delen overschrijven. Maar hoe werkt dit met Slicing, etc.
    // - cardinaliteit: replace
    // - structure - ignore, kan niet veranderen
    // - primitive - ignore, kan niet veranderen
    // - fixed value - zou heel raar zijn
    // - slicing: replace. maar zou op zich al gek zijn om een nieuwe slice te introduceren.
    // 

    // todo: resourceReference can refer to <profile> which is a structure.
    // todo: adding JSON tests
    // todo: Element names are profile URI specific (names can overlap with other profiles)
    // todo: Merge Profile class (when possible) with Api.Introspection and Api.ModelInfo
    // todo: When an element type is a ResourceReference, then the path does not contain [x]. The restriction is content of the refered resource.
    // todo: Slices
    // todo: Layering (adding the defining resource to the profile, then adding a restricting resource to the same profile)
    // todo: Aggregation (resource references) (resource, feed and world scope)
    
    // done: parse meaning of [x].
    // done: name reference (recursion)
    // done: extensions

    public class Params
    {
        public string[] args;
        
        public Params(string[] args)
        {
            this.args = args;
        }

        public int IndexOf(string arg)
        {
            return Array.IndexOf(args, arg);
        }

        public string Switch(string option)
        {
            return "-" + option;
        }

        public bool Exists(string option)
        {
            return (IndexOf(Switch(option)) >= 0);
        }

        public string this[int idx]
        {
            get
            {
                if (args.Count() < idx) 
                    return string.Empty;
                else
                    return args[idx];
            }
        }

        public bool Exists(int idx)
        {
            return idx < args.Count();
        }

        public int Count
        {
            get
            {
                return args.Count();
            }
        }

    }

    

    class Program
    {
        public static void Execute(Params parameters)
        {
            string file_profile, file_resource;

            if (parameters.Count == 0)
            {
                Console.WriteLine("FHIR Profile based resource validation version 0.5 -- DSTU-1");
                Console.WriteLine("(c) Copyright 2014 Furore - http://www.furore.com");
                Console.WriteLine();
                Console.WriteLine("Use: approve <profile>.xml <resource>.xml <options>");
                Console.WriteLine("Options: ");
                Console.WriteLine("  -r     = Raw output (default)");
                Console.WriteLine("  -f     = Output in html with full details");
                Console.WriteLine("  -e     = Output in html with errors only");
                Console.WriteLine("  -open  = Open report directly");

                return;
            }

            if (parameters.Exists(0))
            {

                file_profile = parameters[0];
                if (!File.Exists(file_profile))
                {
                    Console.WriteLine("Profile file not found: {0}", file_profile);
                    return;
                }
            }
            else
            {
                Console.WriteLine("You have to provide a Profile file");
                return;
            }

            if (parameters.Exists(1))
            {
                file_resource = parameters[1];
                if (!File.Exists(file_resource))
                {
                    Console.WriteLine("Resource file not found: {0}", file_resource);
                    return;
                }
            }
            else
            {
                Console.WriteLine("You have to provide a resource file");
                return;
            }


            ReportMode mode = ReportMode.Raw;
            if (parameters.Exists("f")) mode = ReportMode.Full;
            if (parameters.Exists("e")) mode = ReportMode.Errors;

            ProfileBuilder builder = new ProfileBuilder();
            builder.LoadXmlFile(file_profile);
            Profile profile = builder.ToProfile();

            XPathNavigator resource = FhirFile.LoadResource(file_resource);
            Report report = profile.Validate(resource);
            
            if (report.IsValid)
            {
                Console.WriteLine("Your resource is valid");
            }
            else
            {
                Console.WriteLine("Your resource was not valid.");

                if (mode == ReportMode.Raw)
                {
                    report.Errors.ToConsole();
                }
                else 
                {
                    ReportPrinter printer = new ReportPrinter("report");
                    if (mode == ReportMode.Errors)
                    {
                        printer.PrintSummary(report);
                        Console.WriteLine("Errors were saved to 'report.html'");
                    }
                    else
                    {
                        printer.PrintFull(report);
                        Console.WriteLine("Validation analysis was saved to 'report.html'");
                    }
  
                    if (parameters.Exists("open"))
                    {
                        Console.WriteLine("Opening report.html");
                        Process.Start("report.html");
                    }
  
                }

            }
        }

        static void Main(string[] args)
        {
            Params parameters = new Params(args);
            try
            {
                Execute(parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            
            
            if (parameters.Exists("wait")) Console.ReadKey();


        }
    }
}
