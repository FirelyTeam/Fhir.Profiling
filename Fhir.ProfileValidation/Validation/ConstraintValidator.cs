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

namespace Fhir.Profiling
{
    public class ConstraintValidator
    {
        XmlDocument dummydoc = new XmlDocument();
        XPathNavigator navigator;
        Report report;

        public ConstraintValidator(Report report)
        {
            navigator = dummydoc.CreateNavigator();
            this.report = report;
        }

        public bool Validate(Constraint constraint, out string message)
        {
            try
            {
                XPathExpression expr = navigator.Compile(constraint.XPath);
                message = "Constraint is valid";
                return true;
            }
            catch (XPathException e)
            {
                message = string.Format("Constraint [{0}] has an invalid XPath: {1}", constraint.Name, e.Message);
                return false;
            }
        }
    }
}
