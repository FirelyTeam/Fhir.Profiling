/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using Fhir.Profiling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    public enum ReportMode {  Full, Errors, Raw };

    public class ReportPrinter
    {
        private string filename;

        public ReportPrinter(string name)
        {
            filename = name + ".html";
            File.Delete(filename);
            Write("<style> h1 {{ font: 15pt Arial; margin-bottom: 0px; }} div {{ font: 10pt courier; white-space: pre-wrap; }} b {{ color: red; }} i {{ color: green; }} u {{ color: blue; text-decoration:none; }} span {{ font: 8pt courier; color: grey; }} </style>");
        }

        public void Write(string s, params string[] args)
        {
            string txt = string.Format(s, args);
            File.AppendAllText(filename, txt);
        }

        public void WriteLine(string s, params string[] args)
        {
            string txt = string.Format(s, args) + "\n";
            File.AppendAllText(filename, txt);
        }

        private static string indent(int n)
        {
            return new string(' ', n * 4);
        }

        public void Title(string s)
        {
            Write("<h1>{0}</h1>", s);
        }

        public void WriteOutcome(Report.Outcome outcome)
        {
            WriteLine(OutcomeToString(outcome));

            if (outcome.Vector != null)
            if (outcome.Vector.Element != null)
                if (outcome.Vector.Element.Path != null)
                {
                    string type = 
                        outcome.Vector.Element.TypeRefs.Count > 0 ?
                        " (type: "+string.Join(".", outcome.Vector.Element.TypeRefs)+")" : null;
                    
                    WriteLine("<span>{0}{1}</span>\n",
                        string.Join(".", outcome.Vector.Element.Path),
                        type);
                }
        }

        public string OutcomeToString(Report.Outcome outcome)
        {
            string tag = "u";

            switch(outcome.Kind)
            {
                case Status.Any:
                case Status.Start:
                case Status.End: 
                case Status.Info:
                case Status.Skipped:
                    tag = "u";
                    break;

                case Status.Failed:
                case Status.Incomplete:
                case Status.Unknown:
                    tag = "b";
                    break;

                case Status.Valid:
                    tag = "i";
                    break;
            }
            return string.Format("<{0}>{1} {2}</{0}>: {3}", tag, outcome.Type, outcome.Kind.ToString().ToLower(), outcome.Message);
        }

        public void PrintAllOutcomes(Report report)
        {
            foreach (Report.Outcome outcome in report)
            {
                string i = indent(outcome.Nesting);

                switch (outcome.Kind)
                {
                    case Status.Start:
                        WriteLine(i + string.Format("<u>{0}</u> {1} ({2})", outcome.Type, outcome.Vector.Element.Name, outcome.Vector.Node.Name));
                        WriteLine(i + "{{");
                        break;
                    case Status.End:
                        WriteLine(i + "}}");
                        break;
                    default:
                        WriteLine(i + OutcomeToString(outcome));
                        break;
                }
            }
        }

        public void PrintFailedOutcomes(Report report)
        {
            foreach (Report.Outcome outcome in report)
            {
                if (outcome.Kind.Failed())
                {
                    WriteOutcome(outcome);
                }
            }
        }

        public void PrintFull(Report report)
        {
            WriteLine("<div>");
            PrintAllOutcomes(report);
            WriteLine("</div>");
        }

        public void PrintSummary(Report report)
        {
            if (report.IsValid)
            {
                WriteLine("<div>");
                WriteLine("Resource is valid");
                WriteLine("</div>");
                return;
            }
            else
            {
                WriteLine("<div>");
                PrintFailedOutcomes(report);
                WriteLine("</div>");
            }

            
        }

        public void Print(Report report, ReportMode mode)
        {
            if (mode == ReportMode.Full)
                PrintFull(report);
            else
                PrintSummary(report);
        }

    }
}
