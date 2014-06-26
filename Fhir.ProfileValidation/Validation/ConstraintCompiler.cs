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
using System.Xml.Xsl;

namespace Fhir.Profiling
{
    public static class ConstraintCompiler
    {
        static XmlDocument dummydoc = new XmlDocument();
        static XPathNavigator navigator;
        static XsltContext context = new XPath2Context();
        //Report report;

        static ConstraintCompiler()
        {
            navigator = dummydoc.CreateNavigator();
            //this.report = report;
        }

        public static void Compile(Constraint constraint)
        {
            try
            {
                constraint.Compiled = true;
                XPathExpression expr = navigator.Compile(constraint.XPath);
                expr.SetContext(context);
                constraint.Expression = expr;
                constraint.IsValid = true;
            }
            catch (Exception e)
            {
                constraint.CompilerError = e;
                constraint.IsValid = false;
            }
        }

        public static object Eval(string xpath)
        {
            return navigator.Evaluate(xpath, context);
        }
    }
}
