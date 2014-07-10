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
using System.Xml.XPath;

namespace Fhir.Profiling
{
    public static class ValidateExtensions
    {
        public static Report Validate(this Profile profile)
        {
            ProfileValidator pv = new ProfileValidator(profile);
            return pv.Validate();
        }

        public static Report Validate(this Profile profile, XPathNavigator root)
        {
            ResourceValidator rv = new ResourceValidator(profile);
            Report report = rv.Validate(root);
            return report; 
        }

        

    }
}
